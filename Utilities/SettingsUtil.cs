using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VAMP.Utilities;

public static class SettingsUtil
{
    public static ConfigEntry<T> InitConfigEntry<T>(string section, string key, T defaultValue, string description)
    {
        // Bind the configuration entry and get its value
        var entry = Plugin.Instance.Config.Bind(section, key, defaultValue, description);

        // Check if the key exists in the configuration file and retrieve its current value
        var newFile = Path.Combine(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");

        if (File.Exists(newFile))
        {
            var config = new ConfigFile(newFile, true);
            if (config.TryGetEntry(section, key, out ConfigEntry<T> existingEntry))
            {
                // If the entry exists, update the value to the existing value
                entry.Value = existingEntry.Value;
            }
        }
        return entry;
    }

    public static void ReorderConfigSections(List<string> sections)
    {
        var configPath = Path.Combine(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");
        if (!File.Exists(configPath)) return;

        var lines = File.ReadAllLines(configPath).ToList();
        var sectionsContent = new Dictionary<string, List<string>>();
        string currentSection = "";

        // Parse existing file
        foreach (var line in lines)
        {
            if (line.StartsWith("["))
            {
                currentSection = line.Trim('[', ']');
                sectionsContent[currentSection] = new List<string> { line };
            }
            else if (!string.IsNullOrWhiteSpace(currentSection))
            {
                sectionsContent[currentSection].Add(line);
            }
        }

        // Rewrite file in ordered sections
        using var writer = new StreamWriter(configPath, false);
        foreach (var section in sections)
        {
            if (sectionsContent.ContainsKey(section))
            {
                foreach (var line in sectionsContent[section])
                {
                    writer.WriteLine(line);
                }
                writer.WriteLine(); // Add spacing between sections
            }
        }
    }
}
