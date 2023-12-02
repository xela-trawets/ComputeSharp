using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace CstWriterLibrary;
//20231113					  //cst write
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 60)]
public unsafe struct CstHeaderStruct
{
    public fixed byte Signature[8];
    public byte ImageType;//0 = projection, 1 = projection set, 2 = volume
    public byte DataType;//0 = Float32, 1 = UInt16
    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public float Angle;
    public uint Checksum;
    public uint DataByteOffset;
    public uint Reserved;
    public byte StorageDirection;
    public byte BeamMode;
    public byte IsAdjusted;
    public byte BitsUsed;
    public float NormChamber;
    public float TubeVoltage;
    public float TubeCurrent;
    public float PulseWidth;
    public float AdjustmentOffset;
    public float AdjustmentScale;
}
public sealed record CstFileWriter(ushort Width, ushort Height, TypeCode TypeCode = TypeCode.Single) : IDisposable
{
    public const string CstSig = "CSTv1.00";//u8;
    int FrameCount { get; set; } = 0;
    private FileStream? fs;
    FileStream Fs { get => this.fs ?? throw new ArgumentNullException(); set => this.fs = value; }

    CstHeaderStruct cstHeader = new()
    {
        //Signature = "CSTv1.00"u8!, set on close
        ImageType = 0,
        DataType = TypeCode switch
        {
            TypeCode.Int16 => 1,
            TypeCode.UInt16 => 1,
            TypeCode.Single => 0,
            _ => throw new Exception($"Bad Image Pixel Type {TypeCode}; type must be 32 bit float or 16 bit unsigned int")
        },

        Width = Width,
        Height = Height,
        Depth = 0,//set on close
        Angle = 0,
        DataByteOffset = 60,
        StorageDirection = 4,
        BeamMode = 1,

    };
    public void OpenWrite(string fn)
    {
        Fs = File.OpenWrite(fn);
        Span<byte> headerBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan<CstHeaderStruct>(ref this.cstHeader, 1));
        ReadOnlySpan<byte> unfinishedSig = "CloseMe!"u8;
        unfinishedSig.CopyTo(headerBytes.Slice(0, 8));
        this.cstHeader.ImageType = fn.EndsWith(".cstp") ? (byte)0 : (byte)1;
        Fs.Write(headerBytes);
    }
    public int WriteFrameFloat32(Span<float> dataFrame)
    {
        //check type
        //check size
        Fs.Write(MemoryMarshal.AsBytes(dataFrame));
        FrameCount++;
        return FrameCount;
    }
    public int WriteFrameUshort(Span<ushort> dataFrame)
    {
        //check type
        //check size
        Fs.Write(MemoryMarshal.AsBytes(dataFrame));
        FrameCount++;
        return FrameCount;
    }
    public int WriteFrame<T>(T[] dataFrame)
        where T : IBinaryNumber<T>
    {
        return dataFrame switch
        {
            ushort[] a => WriteFrameUshort(a),
            float[] a => WriteFrameFloat32(a),
            _ => throw new($"unknown cst data type {typeof(T).Name}")
        };
    }

    void Close(FileStream tmpfs)
    {
        if (tmpfs is not null)
        {
            Span<byte> headerBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan<CstHeaderStruct>(ref this.cstHeader, 1));
            ReadOnlySpan<byte> sig = "CSTv1.00"u8;
            sig.CopyTo(headerBytes.Slice(0, 8));
            this.cstHeader.Depth = (ushort)FrameCount;
            //cstHeader.ImageType = (FrameCount == 1) ? (byte)0 : (byte)1;
            _ = tmpfs.Seek(0, SeekOrigin.Begin);
            tmpfs.Write(headerBytes);
        }
    }
    public void Dispose()
    {
        FileStream? tmpfs = this.fs;
        this.fs = null;
        if (tmpfs is not null)
        {
            try
            {
                Close(tmpfs);
            }
            finally
            {
                tmpfs.Dispose();
            }
        }
    }
}
public static class SpanCastExtensions
{
    public static Span<byte> AsByteSpan<T>(this ref T x)
        where T : struct
    {
        Span<T> spanT = MemoryMarshal.CreateSpan<T>(ref x, 1);
        return MemoryMarshal.AsBytes(spanT);
    }
    public static Type GetType(this TypeCode code)
    {
        return Type.GetType("System." + Enum.GetName(typeof(TypeCode), code))!;
    }
    public static int Size(this TypeCode code)
    {
        return code switch
        {
            TypeCode.UInt16 => 2,
            TypeCode.Single => 4,
            _ => throw new("Unknown data type in version 2.3.0.39 of cst doc"),
        };
    }
}

public record CstInfo(int Width, int Height, int Depth, TypeCode DataType, int DataByteOffset);
public class CstHeader
{
    public static CstInfo GetFileInfo(string fileName)
    {
        CstHeaderStruct hdrStruct = default;
        using FileStream fs = File.OpenRead(fileName);
        fs.ReadExactly(hdrStruct.AsByteSpan());
        TypeCode tc = hdrStruct.DataType switch
        {
            0 => TypeCode.Single,
            1 => TypeCode.UInt16,
            _ => throw new("Unknown data type in version 2.3.0.39 of cst doc"),
        };
        int dataByteOffset = (int)hdrStruct.DataByteOffset;
        Debug.Assert(dataByteOffset >= 0);
        if (dataByteOffset < 60)
        {
            dataByteOffset = 60;
        }

        return new(hdrStruct.Width, hdrStruct.Height, hdrStruct.Depth, tc, dataByteOffset);
    }
    public static string GetFileXml(string fileName)
    {
        CstInfo cstInfo = GetFileInfo(fileName);
        FileInfo fi = new(fileName);
        using FileStream fs = fi.OpenRead();
        long xmlStartPos = (cstInfo.DataType.Size() * (long)cstInfo.Height * cstInfo.Width * cstInfo.Depth) + cstInfo.DataByteOffset;
        long xmlSize = fi.Length - xmlStartPos;
        Debug.Assert(xmlSize >= 0);
        Debug.Assert(xmlSize < 1048576);
        byte[] buffer = new byte[xmlSize];
        _ = fs.Seek(xmlStartPos, SeekOrigin.Begin);
        fs.ReadExactly(buffer);
        string xml = Encoding.UTF8.GetString(buffer);//Xml might not be utf-8
        return xml;
    }
}