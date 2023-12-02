//using System;
//using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System;
using System.Linq;
//using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

using ComputeSharp;
using ComputeSharp.Sample;
using TiffLibrary1;
using CstWriterLibrary;
using System.Collections.Generic;

const int wFrame = 384;
const int hFrame = 384;
const int pixPerFrame = wFrame * hFrame;

float[] array = [.. Enumerable.Range(1, pixPerFrame)];
// Print the initial matrix
Formatting.PrintMatrix(array, 3, 3, "BEFORE");

float[] rawArray = [.. Enumerable.Range(0, pixPerFrame).Select(c => c % 1000 * (1 / 1024.0f))];
float[] pfArray = new float[pixPerFrame * 4];
const float CountScaleOutput = (float)1024.0;
const float CountScaleInput = (float)(1.0 / CountScaleOutput);
Stopwatch sw = new();

const string TiffInputFilename = @"E:\FujiData\231104\WristScan002\mA_sweep_WristCT_100mA_high_sensitivity_2023-11-04_192732_E1-100Frames.tif";
const string ReferenceInputFilename = @"E:\FujiData\231104\CalData010\mA_sweep_OpenField_30mA_high_sensitivity_2023-11-04_172515.tif";

TiffInfo a = TiffInfo.ReadTiffInfo(ReferenceInputFilename);
(float[] pix, int width, int height) refImg = TiffReaderExtensions.ReadTiffFrame(ReferenceInputFilename);
for (int ipx = 0; ipx < pixPerFrame; ipx++)
{
    refImg.pix[ipx] *= CountScaleInput;
}

string fnTest = TiffInputFilename;

float[] dummyFrame = new float[pixPerFrame];
float[] frame = new float[pixPerFrame];

(TiffInfo tiffInfo, int numberOfDirectories) ti = TiffInfo.ReadTiffInfoAndFrameCount(fnTest);

Console.WriteLine(ti.ToString());
using (TiffReader tf = TiffReader.OpenRead(fnTest, default))
{
    _ = tf.TiffReadNextFrameGeneric<float>(frame);
    for (int ipx = 0; ipx < pixPerFrame; ipx++)
    {
        frame[ipx] *= CountScaleInput;
    }
}

Console.WriteLine("Hello, World!");
const string cstPath = @"c:\tmp\coeff231104-01.csts";

CstInfo cfi = CstHeader.GetFileInfo(cstPath);
Console.WriteLine(cfi);

float[] coeffDataFrames = new float[pixPerFrame * cfi.Depth];

using (FileStream fs = File.OpenRead(cstPath))
{
    _ = fs.Seek(cfi.DataByteOffset, SeekOrigin.Begin);
    fs.ReadExactly(MemoryMarshal.AsBytes(coeffDataFrames.AsSpan()));
}

List<GraphicsDevice> gdList = GraphicsDevice.EnumerateDevices().ToList();
foreach (GraphicsDevice xgd in gdList)
{
    Console.WriteLine(xgd);
}

GraphicsDevice gd = GraphicsDevice.GetDefault();
Console.WriteLine(gd);

// Create the graphics buffers
float[] corArray = new float[pixPerFrame];
float[] corArrayDummy = new float[pixPerFrame];
using ReadOnlyBuffer<float> rawBuffer = gd.AllocateReadOnlyBuffer(frame);// rawArray);
using ReadWriteBuffer<float> corBufferDummy = gd.AllocateReadWriteBuffer(corArrayDummy);
using ReadWriteBuffer<float> corBuffer = gd.AllocateReadWriteBuffer(corArray);
using ReadOnlyBuffer<float> pfBuffer = gd.AllocateReadOnlyBuffer(coeffDataFrames);

// Warmup the shader
gd.For(pixPerFrame, new Poly(rawBuffer, pfBuffer, corBufferDummy));

sw.Restart();
// Run the shader
//loop to get more milliseconds
for (int nrep = 0; nrep < 1000; nrep++)
{
    gd.For(pixPerFrame, new Poly(rawBuffer, pfBuffer, corBuffer));
}

sw.Stop();
corBuffer.CopyTo(corArray);
TiffWriter.WriteSingleFrameFile<float>($@"c:\tmp\testPixFrameGpu.tif", frame, 384, 384);
TiffWriter.WriteSingleFrameFile<float>($@"c:\tmp\testPixCorGpu.tif", corArray, 384, 384);
TiffWriter.WriteSingleFrameFile<float>($@"c:\tmp\testPixCoefGpu.tif", coeffDataFrames.AsSpan(pixPerFrame, pixPerFrame), 384, 384);

Console.WriteLine($" {sw.ElapsedMilliseconds} ");
/// <summary>
/// A kernel that evaluates a cubic polynomial.
/// </summary>
//[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[ThreadGroupSize(256, 1, 1)]
[GeneratedComputeShaderDescriptor]
internal readonly partial struct Poly(ReadOnlyBuffer<float> rawBuffer, ReadOnlyBuffer<float> pfBuffer, ReadWriteBuffer<float> corBuffer) : IComputeShader
{
    /// <inheritdoc/>
    public void Execute()
    {
        int ppf = 384 * 384;
        float t = rawBuffer[ThreadIds.X];
        float pf0 = pfBuffer[ThreadIds.X + (0 * ppf)];
        float pf1 = pfBuffer[ThreadIds.X + (1 * ppf)];
        float pf2 = pfBuffer[ThreadIds.X + (2 * ppf)];
        float pf3 = pfBuffer[ThreadIds.X + (3 * ppf)];
        float logGain = (((((pf3 * t) + pf2) * t) + pf1) * t) + pf0;
        float gain = Hlsl.Exp(logGain);
        //gain /= gain;
        //corBuffer[ThreadIds.X] = rawBuffer[ThreadIds.X] * gain * 1024;
        corBuffer[ThreadIds.X] = t * gain * 1024;
    }
}

/// <summary>
/// The sample kernel that multiples all items by two.
/// </summary>
[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
internal readonly partial struct MultiplyByTwo(ReadWriteBuffer<float> buffer) : IComputeShader
{
    /// <inheritdoc/>
    public void Execute()
    {
        buffer[ThreadIds.X] *= 2;
    }
}