﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ComputeSharp.Core.Extensions;
using ComputeSharp.Graphics.Commands;
using ComputeSharp.Graphics.Extensions;
using ComputeSharp.Shaders.Extensions;
using ComputeSharp.Shaders.Renderer;
using ComputeSharp.Shaders.Translation;
using ComputeSharp.Shaders.Translation.Interop;
using ComputeSharp.Shaders.Translation.Models;
using TerraFX.Interop;
using static TerraFX.Interop.D3D12_COMMAND_LIST_TYPE;

namespace ComputeSharp.Shaders
{
    /// <summary>
    /// A <see langword="class"/> responsible for performing all the necessary operations to compile and run a compute shader.
    /// </summary>
    /// <typeparam name="T">The type of compute shader to run.</typeparam>
    internal static class ShaderRunner<T>
        where T : struct, IComputeShader
    {
        /// <summary>
        /// The mapping used to cache and reuse compiled shaders.
        /// </summary>
        private static readonly Dictionary<ShaderKey, CachedShader<T>> ShadersCache = new();

        /// <summary>
        /// Compiles and runs the input shader on a target <see cref="GraphicsDevice"/> instance, with the specified parameters.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> to use to run the shader.</param>
        /// <param name="x">The number of iterations to run on the X axis.</param>
        /// <param name="y">The number of iterations to run on the Y axis.</param>
        /// <param name="z">The number of iterations to run on the Z axis.</param>
        /// <param name="shader">The input <typeparamref name="T"/> instance representing the compute shader to run.</param>
        public static void Run(GraphicsDevice device, int x, int y, int z, in T shader)
        {
            // Calculate the optimized thread num values
            int threadsX, threadsY = 1, threadsZ = 1;
            if (y == 1 && z == 1) threadsX = (int)device.WavefrontSize;
            else if (z == 1) threadsX = threadsY = 8;
            else threadsX = threadsY = threadsZ = 4;

            Run(device, x, y, z, threadsX, threadsY, threadsZ, in shader);
        }

        /// <summary>
        /// Compiles and runs the input shader on a target <see cref="GraphicsDevice"/> instance, with the specified parameters.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> to use to run the shader.</param>
        /// <param name="x">The number of iterations to run on the X axis.</param>
        /// <param name="y">The number of iterations to run on the Y axis.</param>
        /// <param name="z">The number of iterations to run on the Z axis.</param>
        /// <param name="threadsX">The number of threads in each thread group for the X axis.</param>
        /// <param name="threadsY">The number of threads in each thread group for the Y axis.</param>
        /// <param name="threadsZ">The number of threads in each thread group for the Z axis.</param>
        /// <param name="shader">The input <typeparamref name="T"/> instance representing the compute shader to run.</param>
        public static unsafe void Run(
            GraphicsDevice device,
            int x,
            int y,
            int z,
            int threadsX,
            int threadsY,
            int threadsZ,
            in T shader)
        {
            device.ThrowIfDisposed();

            // Create the shader key
            ShaderKey key = new(ShaderHashCodeProvider.GetHashCode(in shader), threadsX, threadsY, threadsZ);
            CachedShader<T> shaderData;
            PipelineData? pipelineData;

            lock (ShadersCache)
            {
                // Get or preload the shader
                if (!ShadersCache.TryGetValue(key, out shaderData))
                {
                    LoadShader(threadsX, threadsY, threadsZ, in shader, out shaderData);

                    // Cache for later use
                    ShadersCache.Add(key, shaderData);
                }

                // Create the reusable pipeline state
                if (!shaderData.CachedPipelines.TryGetValue(device, out pipelineData))
                {
                    CreatePipelineData(device, shaderData, out pipelineData);
                }
            }

            // Create the commands list and set the pipeline state
            using CommandList commandList = new(device, D3D12_COMMAND_LIST_TYPE_COMPUTE);

            commandList.D3D12GraphicsCommandList->SetComputeRootSignature(pipelineData.D3D12RootSignature);
            commandList.D3D12GraphicsCommandList->SetPipelineState(pipelineData.D3D12PipelineState);

            // Extract the dispatch data for the shader invocation
            using DispatchData dispatchData = shaderData.Loader.GetDispatchData(device, in shader, x, y, z);

            // Initialize the loop targets and the captured values
            commandList.D3D12GraphicsCommandList->SetComputeRoot32BitConstants(dispatchData.Variables);

            ReadOnlySpan<D3D12_GPU_DESCRIPTOR_HANDLE> resources = dispatchData.Resources;

            for (int i = 0; i < resources.Length; i++)
            {
                // Load the captured buffers
                commandList.D3D12GraphicsCommandList->SetComputeRootDescriptorTable((uint)i + 1, resources[i]);
            }

            // Calculate the dispatch values
            int
                groupsX = Math.DivRem(x, threadsX, out int modX) + (modX == 0 ? 0 : 1),
                groupsY = Math.DivRem(y, threadsY, out int modY) + (modY == 0 ? 0 : 1),
                groupsZ = Math.DivRem(z, threadsZ, out int modZ) + (modZ == 0 ? 0 : 1);

            // Dispatch and wait for completion
            commandList.D3D12GraphicsCommandList->Dispatch((uint)groupsX, (uint)groupsY, (uint)groupsZ);
            commandList.ExecuteAndWaitForCompletion();
        }

        /// <summary>
        /// Loads a shader with the specified parameters.
        /// </summary>
        /// <param name="threadsX">The number of threads in each thread group for the X axis.</param>
        /// <param name="threadsY">The number of threads in each thread group for the Y axis.</param>
        /// <param name="threadsZ">The number of threads in each thread group for the Z axis.</param>
        /// <param name="shader">The input <typeparamref name="T"/> instance representing the compute shader to run.</param>
        /// <param name="shaderData">The <see cref="CachedShader{T}"/> instance to return with the cached shader data.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadShader(int threadsX, int threadsY, int threadsZ, in T shader, out CachedShader<T> shaderData)
        {
            // Load the input shader
            ShaderLoader<T> shaderLoader = ShaderLoader<T>.Load(in shader);

            // Render the loaded shader
            using var shaderSource = HlslShaderRenderer.Render(threadsX, threadsY, threadsZ, shaderLoader);

            // Compile the loaded shader to HLSL bytecode
            IDxcBlobObject shaderBytecode = new(ShaderCompiler.Instance.CompileShader(shaderSource.WrittenSpan));

            // Get the cached shader data
            shaderData = new CachedShader<T>(shaderLoader, shaderBytecode);
        }

        /// <summary>
        /// Creates and caches a <see cref="PipelineData"/> instance for a given shader.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> to use to run the shader.</param>
        /// <param name="shaderData">The <see cref="CachedShader{T}"/> instance with the data on the loaded shader.</param>
        /// <param name="pipelineData">The resulting <see cref="PipelineData"/> instance to use to run the shader.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CreatePipelineData(GraphicsDevice device, in CachedShader<T> shaderData, out PipelineData pipelineData)
        {
            using ComPtr<ID3D12RootSignature> d3D12RootSignature = device.D3D12Device->CreateRootSignature(shaderData.Loader.D3D12Root32BitConstantsCount, shaderData.Loader.D3D12DescriptorRanges1);
            using ComPtr<ID3D12PipelineState> d3D12PipelineState = device.D3D12Device->CreateComputePipelineState(d3D12RootSignature.Get(), shaderData.Bytecode.D3D12ShaderBytecode);

            pipelineData = new PipelineData(d3D12RootSignature.Move(), d3D12PipelineState.Move());

            shaderData.CachedPipelines.Add(device, pipelineData);
        }
    }
}
