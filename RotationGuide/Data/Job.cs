using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Internal;
using Lumina.Excel.GeneratedSheets;

namespace RotationGuide.Data;

public static class Job
{
    private static ClassJob[] jobs;
    private static Dictionary<uint, ClassJob> jobsById;
    public static HashSet<string> ViableJobAbbreviation = new()
    {
        "PLD", "WAR", "DRK", "GNB",
        "WHM", "SCH", "AST", "SGE",
        "MNK", "DRG", "NIN", "SAM", "RPR",
        "BRD", "MCH", "DNC",
        "BLM", "SMN", "RDM"
    };

    public static IEnumerable<ClassJob> GetJobs()
    {
        if (jobs == null)
        {
            jobs = Plugin.DataManager.GetExcelSheet<ClassJob>().Where(job => ViableJobAbbreviation.Contains(job.Abbreviation))
                  .ToArray();
        }

        return jobs;
    }
    
    public static ClassJob GetJob(uint id)
    {
        if (jobsById == null)
        {
            jobsById = GetJobs().ToDictionary(job => job.RowId);
        }

        return jobsById[id];
    }

    public static IDalamudTextureWrap GetIcon(this ClassJob job)
    {
        return Plugin.TextureProvider.GetIcon(job.RowId + 100 + 62000u)!;
    }
}
