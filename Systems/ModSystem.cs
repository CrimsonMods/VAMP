using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using VAMP.Structs;
using VAMP.Systems;
using BepInEx.Unity.IL2CPP;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VAMP.Structs.Settings;

namespace VAMP.Services;

public class ModSystem
{
    private static Dictionary<string, PluginInfo> _cachedMods;
    private static List<ModInfo> _detailedModInfo;
    private static bool _isInitialized = false;
    private static readonly HttpClient httpClient = new HttpClient();

    /// <summary>
    /// Initializes the ModSystem and caches mod information.
    /// </summary>
    public ModSystem()
    {
        if (_isInitialized) return;

        RefreshModCache();
        _isInitialized = true;
    }

    /// <summary>
    /// Refreshes the internal mod cache.
    /// </summary>
    public static async void RefreshModCache()
    {
        _cachedMods = IL2CPPChainloader.Instance.Plugins.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _detailedModInfo = await BuildDetailedModInfo();
    }

    /// <summary>
    /// Gets information about all loaded BepInEx plugins/mods.
    /// </summary>
    /// <returns>A dictionary containing plugin GUID as key and PluginInfo as value.</returns>
    public static Dictionary<string, PluginInfo> GetLoadedMods()
    {
        return _cachedMods ?? IL2CPPChainloader.Instance.Plugins.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Gets a list of all loaded mod names and versions.
    /// </summary>
    /// <returns>A list of tuples containing mod name and version.</returns>
    public static List<ModInfo> GetLoadedModsInfo()
    {
        return _detailedModInfo;
    }

    /// <summary>
    /// Checks if a specific mod is loaded by GUID.
    /// </summary>
    /// <param name="modGuid">The GUID of the mod to check for.</param>
    /// <returns>True if the mod is loaded, false otherwise.</returns>
    public static bool IsModLoaded(string modGuid)
    {
        var mods = GetLoadedMods();
        return mods.ContainsKey(modGuid);
    }

    /// <summary>
    /// Gets a specific mod by GUID.
    /// </summary>
    /// <param name="modGuid">The GUID of the mod to retrieve.</param>
    /// <param name="modInfo">The mod information if found.</param>
    /// <returns>True if the mod was found, false otherwise.</returns>
    public static bool TryGetMod(string modGuid, out PluginInfo modInfo)
    {
        var mods = GetLoadedMods();
        return mods.TryGetValue(modGuid, out modInfo);
    }

    /// <summary>
    /// Logs all loaded mods to the console/log file.
    /// </summary>
    public static void LogLoadedMods()
    {
        var mods = GetLoadedModsInfo();
        Plugin.LogInstance?.LogInfo($"Found {mods.Count} loaded BepInEx mods:");

        foreach (var mod in mods.OrderBy(m => m.Name))
        {
            Plugin.LogInstance?.LogInfo($"- {mod.Name} v{mod.Version} ({mod.GUID})");
        }
    }

    /// <summary>
    /// Gets the latest version from Thunderstore for a given author and package name.
    /// </summary>
    /// <param name="author">The Thunderstore author/namespace</param>
    /// <param name="packageName">The package name (usually the GUID)</param>
    /// <returns>The latest version string, or empty string if not found</returns>
    private static async Task<string> GetThunderstoreVersionAsync(string author, string packageName)
    {
        try
        {
            var url = $"https://thunderstore.io/api/v1/package-metrics/{author}/{packageName}/";

            using var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var thunderstoreData = JsonSerializer.Deserialize<ThunderstoreResponse>(jsonContent);

                return thunderstoreData?.latest_version ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Plugin.LogInstance?.LogWarning($"Failed to get Thunderstore version for {author}/{packageName}: {ex.Message}");
            return string.Empty;
        }
    }

    private static async Task<List<ModInfo>> BuildDetailedModInfo()
    {
        var mods = new List<ModInfo>();

        foreach (var plugin in IL2CPPChainloader.Instance.Plugins.Values)
        {
            var modInfo = new ModInfo
            {
                Assembly = plugin.Instance?.GetType().Assembly,
                Name = plugin.Metadata.Name,
                GUID = plugin.Metadata.GUID,
                Version = plugin.Metadata.Version.ToString(),
                AssemblyName = plugin.Instance?.GetType().Assembly.GetName().Name,
                IsActive = plugin.Instance != null,
                LoadSource = "BepInEx IL2CPP Chainloader",
                Dependencies = ExtractDependencies(plugin.Instance?.GetType().Assembly)
            };

            // Try to get additional metadata
            if (plugin.Instance != null)
            {
                var assembly = plugin.Instance.GetType().Assembly;
                var pluginType = plugin.Instance.GetType();

                // Look for BepInPlugin attribute on the plugin class, not the assembly
                var bepInPlugin = pluginType.GetCustomAttribute<BepInPlugin>();

                if (bepInPlugin != null)
                {
                    modInfo.Author = GetAuthorFromAssembly(assembly);
                    modInfo.Description = GetDescriptionFromAssembly(assembly);
                }


                if (!string.IsNullOrEmpty(modInfo.Author))
                {
                    modInfo.ThunderstoreVersion = await GetThunderstoreVersionAsync(modInfo.Author, modInfo.Name);
                }
            }

            mods.Add(modInfo);
        }

        return mods;
    }

    private static string GetAuthorFromAssembly(Assembly assembly)
    {
        return assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown";
    }

    private static string GetDescriptionFromAssembly(Assembly assembly)
    {
        return assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
    }

    private static List<string> ExtractDependencies(Assembly assembly)
    {
        var dependencies = new List<string>();

        if (assembly == null) return dependencies;

        try
        {
            var bepInDependencies = assembly.GetCustomAttributes<BepInDependency>();
            dependencies.AddRange(bepInDependencies.Select(dep => dep.DependencyGUID));
        }
        catch (Exception ex)
        {
            Plugin.LogInstance?.LogWarning($"Could not extract dependencies from {assembly.FullName}: {ex.Message}");
        }

        return dependencies;
    }
}

// Response model for Thunderstore API
public class ThunderstoreResponse
{
    public int downloads { get; set; }
    public int rating_score { get; set; }
    public string latest_version { get; set; } = string.Empty;
}