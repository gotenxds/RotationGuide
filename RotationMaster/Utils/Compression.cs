using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Dalamud.Logging;

namespace RotationMaster.Utils;

public static class Compression
{
    public static string Compress(string str)
    {
        const CompressionLevel level = CompressionLevel.SmallestSize;

        var bytes = Encoding.Unicode.GetBytes(str);
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();

        using var stream = new BrotliStream(output, level);

        input.CopyTo(stream);
        stream.Flush();
        output.Flush();
        
        var result = output.ToArray();
        
        var resultString = Convert.ToBase64String(result);

        return resultString;
    }

    public static string Decompress(string str)
    {
        var bytes = Convert.FromBase64String(str);
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using var stream = new BrotliStream(input, CompressionMode.Decompress);

        stream.Flush();
        output.Flush();
        stream.CopyTo(output);
        
        return Encoding.Unicode.GetString(output.ToArray());
    }
}
