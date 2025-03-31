using System;

namespace VAMP.Structs;

public class Event
{
    public Guid id;
    public string name;
    public EventTrigger eventTrigger;
    public int minPlayers;
    public DateTime[] dateTimes;

    public Event(Guid id, string name, EventTrigger trigger, DateTime[] dateTimes = null, int minPlayers = 0)
    {
        this.id = id;
        this.name = name;
        this.eventTrigger = trigger;
        this.minPlayers = minPlayers;
        if (dateTimes != null)
            this.dateTimes = dateTimes;
    }
}

public enum EventTrigger
{
    Preset,
    Daily,
    Interval
}