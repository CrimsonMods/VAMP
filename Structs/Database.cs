using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ProjectM.Terrain;
using Stunlock.Core;
using VAMP.Utilities;

namespace VAMP.Structs;

internal class Database 
{
    static readonly JsonSerializerOptions prettyJsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNamingPolicy = null,
        Converters = 
        {
            new JsonUtil.LongShortNamesConverter(),
            new JsonUtil.PrefabGUIDConverter()
        }
    };

    public static string RegionsFile = Path.Combine(Plugin.ConfigFiles, "Names_Regions.json");
    public static string VBloodsFile = Path.Combine(Plugin.ConfigFiles, "Names_VBloods.json");

    public Database()
    {
        CreateFiles();
        LoadDatabase();
    }

    private void CreateFiles()
    {
        if(!Directory.Exists(Plugin.ConfigFiles)) { Directory.CreateDirectory(Plugin.ConfigFiles); }

        if(!File.Exists(RegionsFile))
        {
            File.WriteAllText(RegionsFile, JsonSerializer.Serialize(Data.WorldRegions.WorldRegionToString, prettyJsonOptions));
        }

        if(!File.Exists(VBloodsFile))
        {
            var serializableDict = Data.VBloods.PrefabToNames.ToDictionary(
                pair => pair.Key.GuidHash.ToString(),
                pair => pair.Value
            );
            
            File.WriteAllText(VBloodsFile, JsonSerializer.Serialize(serializableDict, prettyJsonOptions));
        }
    }

    private void LoadDatabase()
    {
        try
        {
            string json = File.ReadAllText(RegionsFile);
            Data.WorldRegions.WorldRegionToString = JsonSerializer.Deserialize<Dictionary<WorldRegionType, (string Long, string Short)>>(json, prettyJsonOptions);
        }
        catch (Exception e)
        {
            Plugin.LogInstance.LogError("Failed loading Region Names. Please validate Names_Regions.json");
        }

        try
        {
            string json = File.ReadAllText(VBloodsFile);
            // Deserialize to a temporary dictionary with string keys
            var tempDict = JsonSerializer.Deserialize<Dictionary<string, (string Long, string Short)>>(json, prettyJsonOptions);
            
            // Convert back to PrefabGUID keys
            Data.VBloods.PrefabToNames = tempDict.ToDictionary(
                pair => new PrefabGUID(int.Parse(pair.Key)),
                pair => pair.Value
            );
        }
        catch (Exception e)
        {
            Plugin.LogInstance.LogError("Failed loading VBlood Names. Please validate Names_VBloods.json");
        }
    }
}