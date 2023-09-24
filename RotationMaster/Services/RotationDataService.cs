using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.Logging;
using RotationMaster.Data;
using RotationMaster.Utils;

namespace RotationMaster.Services;

public class RotationDataService
{
    private const string RotationsFileName = "rotations.json";
    private static Rotation[] rotations;

    private RotationDataService() { }

    public static IEnumerable<Rotation> GetAll()
    {
        if (rotations == null)
        {
            LoadOrCreate();
        }

        return rotations;
    }

    public static void Save(Rotation rotation)
    {
        var findIndex = Array.FindIndex(rotations, r => r.Id == rotation.Id);

        if (findIndex == -1)
        {
            rotations = rotations.Append(rotation).ToArray();
        }
        else
        {
            rotations[findIndex] = rotation;
        }

        FileUtils.Save(RotationsFileName, rotations);
    }

    public static void Delete(Rotation rotation)
    {
        rotations = rotations.Remove(rotation);

        FileUtils.Save(RotationsFileName, rotations);
    }

    private static void LoadOrCreate()
    {
        PluginLog.Debug("Trying to load or create");
        if (!FileUtils.Exists(RotationsFileName))
        {
            PluginLog.Debug("Does not exist, creating");
            rotations = Array.Empty<Rotation>();

            FileUtils.Save(RotationsFileName, rotations);
        }
        else
        {
            PluginLog.Debug("Exists, loading");
            if (FileUtils.TryLoad<Rotation[]>(RotationsFileName, out var loadedRotations))
            {
                rotations = loadedRotations!;
                PluginLog.Debug($"Loaded, count: {rotations.Length}");
            }
        }
    }

    public static string Export(Rotation rotation)
    {
        var jsonString = JsonSerializer.Serialize(rotation);


        return Compression.Compress(jsonString);
    }

    public static void Import(string importString)
    {
        var json = Compression.Decompress(importString);

        var deserializeRotation = JsonSerializer.Deserialize<Rotation>(json);

        Save(deserializeRotation);
    }
}
