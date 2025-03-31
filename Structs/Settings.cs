using System.IO;
using BepInEx;
using BepInEx.Configuration;
using VAMP.Utilities;

namespace VAMP.Structs;

public readonly struct Settings
{
    public static ConfigEntry<int> Interval { get; private set; }
    public static ConfigEntry<int> AcceptPeriod {get; private set; }
    public static ConfigEntry<int> Concurrent {get; private set; }

    public static void InitConfig()
    {
        Interval = SettingsUtil.InitConfigEntry("Config", "Interval", 30, 
        "The interval in minutes between events.", "EventScheduler");

        AcceptPeriod = SettingsUtil.InitConfigEntry("Config", "AcceptPeriod", 5, 
        "The amount of time in minutes for an event to be accepted.", "EventScheduler");

        Concurrent = SettingsUtil.InitConfigEntry("Config", "Concurrent", 1, 
        "The amount of events that can be running at once.", "EventScheduler");
    }
}