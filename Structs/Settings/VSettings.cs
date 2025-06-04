using BepInEx.Configuration;
using VAMP.Utilities;

namespace VAMP.Structs.Settings;

public readonly struct VSettings
{
    public static ConfigFile Scheduler { get; private set; }
    public static ConfigEntry<int> Interval { get; private set; }
    public static ConfigEntry<int> AcceptPeriod {get; private set; }
    public static ConfigEntry<int> Concurrent {get; private set; }

    public static void InitConfig()
    {
        Scheduler = SettingsUtil.CreateConfigFile("EventScheduler");
        Interval = SettingsUtil.InitConfigEntry(Scheduler, "Config", "Interval", 30, 
        "The interval in minutes between events.");

        AcceptPeriod = SettingsUtil.InitConfigEntry(Scheduler, "Config", "AcceptPeriod", 5, 
        "The amount of time in minutes for an event to be accepted.");

        Concurrent = SettingsUtil.InitConfigEntry(Scheduler, "Config", "Concurrent", 1, 
        "The amount of events that can be running at once.");
    }
}