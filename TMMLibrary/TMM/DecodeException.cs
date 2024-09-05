namespace TMMLibrary.TMM;
using System.Runtime.InteropServices;

public class DecodeException : IOException
{
    public DecodeException(Type decoder, string message, Exception? cause = null) 
        : base($"Decode {decoder.Name} - {message}", cause)
    { }

    public DecodeException(Type decoder, long position, string message, Exception? cause = null)
        : base($"Decode {decoder.Name} at {position:X} - {message}", cause)
    { }

    public DecodeException(object obj, string message) : this(obj.GetType(), message)
    {}

    public static void ExpectEof(Type decoder, Stream stream)
    {
        if (stream.Position != stream.Length)
        {
            throw new DecodeException(decoder, $"expected to be at end of file, instead at 0x{stream.Position:X}");
        }
    }

    public static void ExpectEqual<T>(Type decoder, long pos, T expect, T actual)
        where T : IEquatable<T>
    {
        if (!expect.Equals(actual))
        {
            throw new DecodeException(decoder, pos, $"expected {expect} found {actual}");
        }
    }

    /// <summary>
    /// If the expect list and actual list are different, throws an exception detailing the difference
    /// and the position the difference occured.
    /// </summary>
    /// <param name="decoder">Type that is doing the decoding, makes exception messages more useful</param>
    /// <param name="endPosition">Stream position <i>after</i> the array is read</param>
    /// <param name="expect">Expected list of values</param>
    /// <param name="actual">Actual values read from the stream</param>
    /// <typeparam name="T">Element type being compared</typeparam>
    /// <exception cref="DecodeException"></exception>
    public static void ExpectEqualList<T>(Type decoder, long endPosition, IList<T> expect, IList<T> actual)
        where T : IEquatable<T>
    {
        var tSize = Marshal.SizeOf<T>();
        var startPosition = endPosition - actual.Count * tSize;
        if (actual.Count != expect.Count)
        {
            throw new DecodeException(decoder, startPosition, $"expected {expect.Count} elements, found {actual.Count}");
        }
        for (var i = 0; i < expect.Count; i++)
        {
            if (expect[i].Equals(actual[i])) continue;
            var realPos = startPosition + tSize * i;
            throw new DecodeException(decoder, realPos, $"expected {expect[i]} found {actual[i]}");
        }
    }
}