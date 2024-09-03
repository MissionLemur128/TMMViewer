using System.Text;

namespace TMMLibrary.TMM;

public static class IoExtensions
{
    public static ushort[] ReadUint16Array(this BinaryReader br,  int v)
    {
        var array = new ushort[v];
        for (int i = 0; i < v; i++)
        {
            array[i] = br.ReadUInt16();
        }
        return array;
    }

    public static uint[] ReadUint32Array(this BinaryReader br, int v)
    {
        var array = new uint[v];
        for (int i = 0; i < v; i++)
        {
            array[i] = br.ReadUInt32();
        }
        return array;
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
        return br.ReadUtf16String(length);
    }
}