# Event Scheduler Documentation

The EventScheduler service manages the scheduling and execution of game events in VAMP. It provides functionality for registering events, handling voting, and managing event lifecycles.

## Key Features

- Automatic scheduling of preset and interval-based events
- Built-in voting system for event activation
- Concurrent event management
- Event registration system

## Usage

### Creating a Preset Event

```csharp
// Create a preset event that occurs every Monday at 20:00 UTC
var scheduleEntry = new ScheduleEntry 
{
    DayOfWeek = DayOfWeek.Monday,
    Hour = 20,
    Minute = 0
};

var event = new Event(
    Guid.NewGuid(),
    "Blood Moon",
    EventTrigger.Preset,
    new[] { scheduleEntry },
    minPlayers: 5
);

// Register the event with the scheduler
EventScheduler.Register(event);
```

### Create a Daily Event

Daily events are just like Preset events with a minor difference in code to signify to the scheduler that it should repeat daily.

```csharp
var scheduleEntry = new ScheduleEntry 
{
    DayOfWeek = null, // null signifies daily
    Hour = 20,
    Minute = 0
};
```

### Creating an Interval Event

```csharp
// Create an event that can occur at random intervals
var randomEvent = new Event(
    Guid.NewGuid(),
    "Bandit Raid",
    EventTrigger.Interval,
    minPlayers: 3
);

EventScheduler.Register(randomEvent);
```

### Handling Event Start

```csharp
// Subscribe to event start notifications
EventScheduler.OnEventStarted += (Event e) => {
    Console.WriteLine($"Event {e.name} has started!");
    if(e.name == "Blood Moon")
    {
        // Add your event logic/trigger here
    }
};
```

### Ending an Event

```csharp
// End an event using its GUID
EventScheduler.EndEvent(eventGuid);
```

## Event Types

### Preset Events
- Occur at specific days and times
- Can be scheduled for specific days of the week or daily
- Requires exact hour and minute configuration

### Interval Events
- Occur randomly based on a configured interval
- Must pass the minimum voting requirements
- Cannot trigger if another instance is currently running

## Voting System

Events can require a minimum number of players to vote before activating:

- If minPlayers is 0, votes must exceed half of the online player count
- If minPlayers is set, at least minPlayers votes are required
- Players can vote using !vote command
- Voting period is configurable by server owners using the EventScheduler.cfg

## Important Notes

1. Always use EndEvent() to properly terminate events
2. Events won't trigger if:
   - Maximum concurrent events are running
   - A vote is in progress
   - The same event is already running
3. The scheduler runs on a 60-second check interval
