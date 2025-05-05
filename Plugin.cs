using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using VAMP.Structs;

namespace VAMP;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    public static Plugin Instance { get; private set; }
    public static Harmony Harmony => Instance._harmony;
    public static ManualLogSource LogInstance => Instance.Log;
    public static VSettings Settings { get; private set;}

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

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }
}
