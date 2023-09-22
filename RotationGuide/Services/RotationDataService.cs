using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using RotationGuide.Data;
using RotationGuide.Utils;

namespace RotationGuide.Services;

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
}
