using System;

namespace VAMP.Structs;

/// <summary>
/// Represents an event in the system.
/// </summary>
public class Event
{
    /// <summary>
    /// The unique identifier for the event.
    /// </summary>
    public Guid id;

    /// <summary>
    /// The name of the event.
    /// </summary>
    public string name;

    /// <summary>
    /// The trigger type for the event.
    /// </summary>
    public EventTrigger eventTrigger;

    /// <summary>
    /// The minimum number of players required for the event.
    /// </summary>
    public int minPlayers;

    /// <summary>
    /// Array of schedule entries defining when the event occurs.
    /// </summary>
    public ScheduleEntry[] dateTimes;

    /// <summary>
    /// Initializes a new instance of the Event class.
    /// </summary>
    /// <param name="id">The unique identifier for the event.</param>
    /// <param name="name">The name of the event.</param>
    /// <param name="trigger">The trigger type for the event.</param>
    /// <param name="dateTimes">Optional array of schedule entries. Defaults to null.</param>
    /// <param name="minPlayers">Optional minimum number of players. Defaults to 0.</param>
    public Event(Guid id, string name, EventTrigger trigger, ScheduleEntry[] dateTimes = null, int minPlayers = 0)
    {
        this.id = id;
        this.name = name;
        this.eventTrigger = trigger;
        this.minPlayers = minPlayers;
        if (dateTimes != null)
            this.dateTimes = dateTimes;
    }
}

/// <summary>
/// Represents a scheduled time entry for an event.
/// </summary>
public class ScheduleEntry
{
    /// <summary>
    /// Gets or sets the day of the week for the schedule entry. Null for daily.
    /// </summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the hour (0-23) for the schedule entry.
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// Gets or sets the minute (0-59) for the schedule entry.
    /// </summary>
    public int Minute { get; set; }
}

/// <summary>
/// Defines the types of triggers for events.
/// </summary>
public enum EventTrigger
{
    /// <summary>
    /// Event occurs at preset times.
    /// </summary>
    Preset,

    /// <summary>
    /// Event occurs at regular intervals.
    /// </summary>
    Interval
}