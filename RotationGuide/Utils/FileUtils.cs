using System;
using System.IO;
using System.Text.Json;
using Dalamud.Logging;

namespace RotationGuide.Utils;

public class FileUtils
{
//    private static JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
    private static JsonSerializerOptions jsonSerializerOptions= new()
    {
        WriteIndented = true
    };
    public static void Save<T>(string fileName, T data)
    {
        try
        {
            var fileInfo = GetFileInfo(fileName);

            var jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(fileInfo.FullName, jsonString);

            PluginLog.Debug($"Saved {fileInfo.Name}");
        }
        catch (Exception e)
        {
            PluginLog.Error(e, $"Error saving file: {fileName}");
        }
    }

    public static bool TryLoad<T>(string fileName, out T? t)
    {
        try
        {
            var data = File.ReadAllText(GetFileInfo(fileName).FullName);

            t = JsonSerializer.Deserialize<T>(data);

            PluginLog.Debug($"loaded {fileName}");

            return true;
        }
        catch (Exception e)
        {
            PluginLog.Error(e, $"Error loading file: {fileName}");
        }

        t = default;

        return false;
    }

    public static bool Exists(string fileName)
    {
        return File.Exists(GetFileInfo(fileName).FullName);
    }

    private static FileInfo GetFileInfo(string fileName)
    {
        var configDirectory = Plugin.PluginInterface.ConfigDirectory;
        return new FileInfo(Path.Combine(configDirectory.FullName, fileName));
    }
}
