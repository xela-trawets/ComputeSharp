using System;
using ComputeSharp.D2D1;
using ComputeSharp.D2D1.Descriptors;
using ComputeSharp.D2D1.WinUI;

namespace ComputeSharp.SwapChain.Core.Shaders;

/// <summary>
/// An base effect for an animated pixel shader.
/// </summary>
public abstract class PixelShaderEffect : CanvasEffect
{
    /// <summary>
    /// The current elapsed time.
    /// </summary>
    private TimeSpan elapsedTime;

    /// <summary>
    /// The current screen width in raw pixels.
    /// </summary>
    private int screenWidth;

    /// <summary>
    /// The current screen height in raw pixels.
    /// </summary>
    private int screenHeight;

    /// <summary>
    /// Gets or sets the total elapsed time.
    /// </summary>
    public TimeSpan ElapsedTime
    {
        get => this.elapsedTime;
        set => SetAndInvalidateEffectGraph(ref this.elapsedTime, value);
    }

    /// <summary>
    /// Gets or sets the screen width in raw pixels.
    /// </summary>
    public int ScreenWidth
    {
        get => this.screenWidth;
        set => SetAndInvalidateEffectGraph(ref this.screenWidth, value);
    }

    /// <summary>
    /// Gets or sets the screen height in raw pixels.
    /// </summary>
    public int ScreenHeight
    {
        get => this.screenHeight;
        set => SetAndInvalidateEffectGraph(ref this.screenHeight, value);
    }

    /// <summary>
    /// An effect for an animated pixel shader.
    /// </summary>
    /// <typeparam name="T">The type of pixel shader to render.</typeparam>
    /// <param name="factory">The input <typeparamref name="T"/> factory.</param>
    public sealed class For<T>(For<T>.Factory factory) : PixelShaderEffect
        where T : unmanaged, ID2D1PixelShader, ID2D1PixelShaderDescriptor<T>
    {
        /// <summary>
        /// The <see cref="PixelShaderEffect{T}"/> node in use.
        /// </summary>
        private static readonly CanvasEffectNode<PixelShaderEffect<T>> Effect = new();

        /// <inheritdoc/>
        protected override void BuildEffectGraph(CanvasEffectGraph effectGraph)
        {
            effectGraph.RegisterOutputNode(Effect, new PixelShaderEffect<T>());
        }

        /// <inheritdoc/>
        protected override void ConfigureEffectGraph(CanvasEffectGraph effectGraph)
        {
            effectGraph.GetNode(Effect).ConstantBuffer = factory(ElapsedTime, ScreenWidth, ScreenHeight);
        }

        /// <summary>
        /// A factory of a given shader instance.
        /// </summary>
        /// <param name="elapsedTime">The total elapsed time.</param>
        /// <param name="screenWidth">The screen width in raw pixels.</param>
        /// <param name="screenHeight">The screen height in raw pixels.</param>
        /// <returns>A shader instance to render.</returns>
        public delegate T Factory(TimeSpan elapsedTime, int screenWidth, int screenHeight);
    }
}