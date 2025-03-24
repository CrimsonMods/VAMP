using System;

namespace VAMP.Structs;

public class Event
{
    public Guid id;
    public string name;
    public EventTrigger eventTrigger;

    public Event(Guid id, string name, EventTrigger trigger)
    {
        this.id = id;
        this.name = name;
        this.eventTrigger = trigger;
    }
}

public enum EventTrigger
{
    Preset,
    Daily,
    Interval
}