using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
#if !NET6_0_OR_GREATER
using UnmanagedCallersOnlyAttribute = ComputeSharp.NetStandard.System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute;
#endif

namespace ComputeSharp.D2D1.Shaders.Interop.Effects.TransformMapperManagers;

/// <inheritdoc/>
partial struct D2D1DrawInfoUpdateContextImpl
{
    /// <summary>
    /// The implementation for <see cref="ID2D1DrawInfoUpdateContex"/>.
    /// </summary>
    private static unsafe class ID2D1DrawInfoUpdateContexMethods
    {
#if !NET6_0_OR_GREATER
        /// <inheritdoc cref="GetConstantBufferSize"/>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int GetConstantBufferSizeDelegate(D2D1DrawInfoUpdateContextImpl* @this, uint* size);

        /// <inheritdoc cref="GetConstantBuffer"/>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int GetConstantBufferDelegate(D2D1DrawInfoUpdateContextImpl* @this, byte* buffer, uint bufferCount);

        /// <inheritdoc cref="SetConstantBuffer"/>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int SetConstantBufferDelegate(D2D1DrawInfoUpdateContextImpl* @this, byte* buffer, uint bufferCount);

        /// <summary>
        /// A cached <see cref="QueryInterfaceDelegate"/> instance wrapping <see cref="QueryInterface"/>.
        /// </summary>
        public static readonly QueryInterfaceDelegate QueryInterfaceWrapper = QueryInterface;

        /// <summary>
        /// A cached <see cref="AddRefDelegate"/> instance wrapping <see cref="AddRef"/>.
        /// </summary>
        public static readonly AddRefDelegate AddRefWrapper = AddRef;

        /// <summary>
        /// A cached <see cref="ReleaseDelegate"/> instance wrapping <see cref="Release"/>.
        /// </summary>
        public static readonly ReleaseDelegate ReleaseWrapper = Release;

        /// <summary>
        /// A cached <see cref="GetConstantBufferSizeDelegate"/> instance wrapping <see cref="GetConstantBufferSize"/>.
        /// </summary>
        public static readonly GetConstantBufferSizeDelegate GetConstantBufferSizeWrapper = GetConstantBufferSize;

        /// <summary>
        /// A cached <see cref="GetConstantBufferDelegate"/> instance wrapping <see cref="GetConstantBuffer"/>.
        /// </summary>
        public static readonly GetConstantBufferDelegate GetConstantBufferWrapper = GetConstantBuffer;

        /// <summary>
        /// A cached <see cref="SetConstantBufferDelegate"/> instance wrapping <see cref="SetConstantBuffer"/>.
        /// </summary>
        public static readonly SetConstantBufferDelegate SetConstantBufferWrapper = SetConstantBuffer;
#endif

        /// <inheritdoc cref="D2D1DrawInfoUpdateContextImpl.QueryInterface"/>
        [UnmanagedCallersOnly]
        public static int QueryInterface(D2D1DrawInfoUpdateContextImpl* @this, Guid* riid, void** ppvObject)
        {
            return @this->QueryInterface(riid, ppvObject);
        }

        /// <inheritdoc cref="D2D1DrawInfoUpdateContextImpl.AddRef"/>
        [UnmanagedCallersOnly]
        public static uint AddRef(D2D1DrawInfoUpdateContextImpl* @this)
        {
            return @this->AddRef();
        }

        /// <inheritdoc cref="D2D1DrawInfoUpdateContextImpl.Release"/>
        [UnmanagedCallersOnly]
        public static uint Release(D2D1DrawInfoUpdateContextImpl* @this)
        {
            return @this->Release();
        }

        /// <inheritdoc cref="ID2D1DrawInfoUpdateContex.GetConstantBufferSize"/>
        [UnmanagedCallersOnly]
        public static int GetConstantBufferSize(D2D1DrawInfoUpdateContextImpl* @this, uint* size)
        {
            // TODO

            return S.S_OK;
        }

        /// <inheritdoc cref="ID2D1DrawInfoUpdateContex.GetConstantBuffer"/>
        [UnmanagedCallersOnly]
        public static int GetConstantBuffer(D2D1DrawInfoUpdateContextImpl* @this, byte* buffer, uint bufferCount)
        {
            // TODO

            return S.S_OK;
        }

        /// <inheritdoc cref="ID2D1DrawInfoUpdateContex.SetConstantBuffer"/>
        [UnmanagedCallersOnly]
        public static int SetConstantBuffer(D2D1DrawInfoUpdateContextImpl* @this, byte* buffer, uint bufferCount)
        {
            // TODO

            return S.S_OK;
        }
    }
}