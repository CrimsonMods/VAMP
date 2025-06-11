using BepInEx.Configuration;
using VAMP.Utilities;

namespace VAMP.Structs.Settings;

public readonly struct VAMPSettings
{
    public static ConfigFile VAMP { get; private set; }
    public static ConfigEntry<bool> AutoWipeDetection { get; private set; }
    public static ConfigEntry<string> StartDateFile { get; private set; }
    
    public static ConfigEntry<bool> ModProfiler { get; private set; }

    public static void InitConfig()
    {
        VAMP = SettingsUtil.CreateConfigFile("VAMP");
        AutoWipeDetection = SettingsUtil.InitConfigEntry(VAMP, "Config", "AutoWipeDetection", true,
            "If true, VAMP will automatically delete mod files in the _WipeData folder when the server is wiped.");

        StartDateFile = SettingsUtil.InitConfigEntry(VAMP, "Config", "StartDateFile", "save-data/Saves/v4/world1/StartDate.json",
            "The path of the file that contains the start date of the server.");

        ModProfiler = SettingsUtil.InitConfigEntry(VAMP, "Config", "ModProfiler", false,
            "If true, VAMP will profile all mod methods and output the results to a file in the ModProfiler directory.");
    }
}