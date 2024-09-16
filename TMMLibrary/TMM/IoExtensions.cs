using System.Numerics;
using System.Text;

namespace TMMLibrary.TMM;


public interface IEncode
{
    void Encode(BinaryWriter bw);
}

public static class IoExtensions
{
    public static ushort[] ReadUint16Array(this BinaryReader br,  int v)
    {
        var array = new ushort[v];
        for (var i = 0; i < v; i++)
        {
            array[i] = br.ReadUInt16();
        }
        return array;
    }

    public static void Write(this BinaryWriter bw, ushort[] array)
    {
        foreach (var v in array)
        {
            bw.Write(v);
        }
    }

    public static uint[] ReadUint32Array(this BinaryReader br, int v)
    {
        var array = new uint[v];
        for (var i = 0; i < v; i++)
        {
            array[i] = br.ReadUInt32();
        }
        return array;
    }

    public static int[] ReadInt32Array(this BinaryReader br, int count)
    {
        var array = new int[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = br.ReadInt32();
        }
        return array;
    }

    public static void Write(this BinaryWriter bw, uint[] array)
    {
        foreach (var v in array)
        {
            bw.Write(v);
        }
    }

    public static float[] ReadFloat32Array(this BinaryReader br, int count)
    {
        var array = new float[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = br.ReadSingle();
        }
        return array;
    }

    public static Matrix4x4 ReadMatrix4x4(this BinaryReader br)
    {
        var matrix = Matrix4x4.Identity;
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                matrix[i, j] = br.ReadSingle();
            }
        }
        return matrix;
    }

    public static void Write(this BinaryWriter bw, float[] array)
    {
        foreach (var v in array)
        {
            bw.Write(v);
        }
    }

    public static void Write(this BinaryWriter bw, Matrix4x4 matrix)
    {
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                bw.Write(matrix[i, j]);
            }
        }
    }

    /// <summary>
    /// Read <c>length * 2</c> bytes and interpret them as a UTF-16 string.
    /// </summary>
    /// <param name="br">this</param>
    /// <param name="length">Length of the UTF-16 string in characters (half its length in bytes)</param>
    /// <returns>The decoded string</returns>
    public static string ReadUtf16String(this BinaryReader br, int length)
    {
        // utf-16 means 2 bytes per character
        var bytes = br.ReadBytes(length * 2);
        return Encoding.Unicode.GetString(bytes);
    }

    /// <summary>
    /// Read a UTF-16 encoded string which is prefixed by a 4-byte character length.
    /// </summary>
    /// <param name="br"></param>
    /// <returns>The decoded string</returns>
    public static string ReadTmString(this BinaryReader br)
    {
        var length = br.ReadInt32();
        // sanity check, remove once we have the decoding more accurate
        if (length > 100)
        {
            throw new IOException("invalid length trying to read TmString");
        }
        return length == 0 ? "" : br.ReadUtf16String(length);
    }

    public static void WriteTmString(this BinaryWriter bw, string s)
    {
        if (s.Length == 0)
        {
            bw.Write(0);
            return;
        }
        var bytes = Encoding.Unicode.GetBytes(s);
        bw.Write((uint)bytes.Length / 2);
        bw.Write(bytes);
    }

    public static T[] DecodeArray<T>(this BinaryReader br, int length, Func<BinaryReader, T> fn)
    {
        var array = new T[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = fn(br);
        }
        return array;
    }

    public static void EncodeArray<T>(this BinaryWriter bw, IEnumerable<T> array) where T : IEncode
    {
        foreach (var item in array)
        {
            item.Encode(bw);
        }
    }

    public static void PrintPosition(this BinaryReader br)
    {
        Console.WriteLine($"BinaryReader at position 0x{br.BaseStream.Position:X}");
    }
}


