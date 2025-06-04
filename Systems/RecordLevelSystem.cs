using System.Collections.Generic;
using System.IO;
using VAMP.Models;
using VAMP.Services;

namespace VAMP.Systems;

internal class RecordLevelSystem
{
    public static string RecordLevelPath = Path.Combine(FilePaths.WipeData, "vamp_record_levels.json");

    public static Dictionary<ulong, int> RecordLevels = new Dictionary<ulong, int>();

    public RecordLevelSystem()
    {
        Load();
    }

    public static void SetRecord(ulong steamId)
    {
        if(PlayerService.TryFindBySteam(steamId, out Player player))
        {
            if(RecordLevels.ContainsKey(steamId))
            {
                if(RecordLevels[steamId] < player.Level)
                {
                    RecordLevels[steamId] = player.Level;
                    Save();
                }
            }
            else
            {
                RecordLevels.Add(steamId, player.Level);
                Save();
            }
        }
    }

    public static int GetRecord(ulong steamId)
    {
        return RecordLevels.TryGetValue(steamId, out int level) ? level : 0;
    }

    public static void Save()
    {
        File.WriteAllText(RecordLevelPath, System.Text.Json.JsonSerializer.Serialize(RecordLevels));
    }

    public static void Load()
    {
        if (!File.Exists(RecordLevelPath))
        {
            File.Create(RecordLevelPath).Close();
            return;
        }

        RecordLevels = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, int>>(File.ReadAllText(RecordLevelPath));
    }
}