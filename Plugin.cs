using System;
using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using VAMP.Services;
using VAMP.Structs.Settings;
using VAMP.Systems;

namespace VAMP;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    public static Plugin Instance { get; private set; }
    public static Harmony Harmony => Instance._harmony;
    public static ManualLogSource LogInstance => Instance.Log;
    internal static VSettings Settings { get; private set;}
    internal static VAMPSettings VAMPSettings { get; private set; }

    internal static string ConfigFiles => Path.Combine(Paths.ConfigPath, "VAMP");

    /// <summary>
    /// Event that is triggered when the Core system has finished loading.
    /// This action can be subscribed to for executing code after core initialization is complete.
    /// </summary>
    [Obsolete("Use VAMP.Events.OnCoreLoaded instead. Will be removed in a future version.")]
    public static Action OnCoreLoaded;    
    public static bool SpawnDebug = false;

    public override void Load()
    {
        Instance = this;
        Settings = new();
        VSettings.InitConfig();
        VAMPSettings = new();
        VAMPSettings.InitConfig();

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        Events.OnCoreLoaded += Loaded;
    }

    private void Loaded()
    {
        FileWatcherSystem.Initialize();

        if (VAMPSettings.ModProfiler.Value) Core.StartCoroutine(DelayStart());
        
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(30f);
        foreach (var plugin in ModSystem.GetLoadedModsInfo())
        {
            if(plugin.Name == "VAMP") continue;
            ModProfiler.ProfileAssembly(plugin.Assembly);
        }

        yield return new WaitForSeconds(30f);
        while (true)
        {
            yield return new WaitForSeconds(30f);
            ModProfiler.DumpStats();
        }
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }
}
