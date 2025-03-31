using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VAMP.Utilities;

/// <summary>
/// Utility class for handling configuration settings.
/// </summary>
public static class SettingsUtil
{
    /// <summary>
    /// Initializes a configuration entry with the specified parameters.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <param name="section">The section name in the configuration file.</param>
    /// <param name="key">The key name for the configuration entry.</param>
    /// <param name="defaultValue">The default value if no configuration exists.</param>
    /// <param name="description">The description of the configuration entry.</param>
    /// <param name="settingsName">The name of the settings file. Defaults to the plugin GUID.</param>
    /// <returns>A ConfigEntry instance containing the configuration value.</returns>
    public static ConfigEntry<T> InitConfigEntry<T>(string section, string key, T defaultValue, string description, string settingsName = MyPluginInfo.PLUGIN_GUID)
    {
        var entry = Plugin.Instance.Config.Bind(section, key, defaultValue, description);

        var newFile = Path.Combine(Paths.ConfigPath, $"{settingsName}.cfg");

        if (File.Exists(newFile))
        {
            var config = new ConfigFile(newFile, true);
            if (config.TryGetEntry(section, key, out ConfigEntry<T> existingEntry))
            {
                entry.Value = existingEntry.Value;
            }
        }
        return entry;
    }

    /// <summary>
    /// Reorders sections in the configuration file according to the specified order.
    /// </summary>
    /// <param name="sections">List of section names in the desired order.</param>
    /// <param name="settingsName">The name of the settings file. Defaults to the plugin GUID.</param>
    public static void ReorderConfigSections(List<string> sections, string settingsName = MyPluginInfo.PLUGIN_GUID)
    {
        var configPath = Path.Combine(Paths.ConfigPath, $"{settingsName}.cfg");
        if (!File.Exists(configPath)) return;

        var lines = File.ReadAllLines(configPath).ToList();
        var sectionsContent = new Dictionary<string, List<string>>();
        string currentSection = "";

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

        using var writer = new StreamWriter(configPath, false);
        foreach (var section in sections)
        {
            if (sectionsContent.ContainsKey(section))
            {
                foreach (var line in sectionsContent[section])
                {
                    writer.WriteLine(line);
                }
                writer.WriteLine();
            }
        }
    }
}