using ProjectM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VAMP.Structs;
using VAMP.Utilities;
using Event = VAMP.Structs.Event;

namespace VAMP.Services;

/// <summary>
/// Manages scheduling and execution of game events.
/// </summary>
public static class EventScheduler
{
    /// <summary>
    /// Event handler that is triggered when an event starts.
    /// </summary>
    public static Action<Event> OnEventStarted;

    /// <summary>
    /// List of events that have already occurred.
    /// </summary>
    public static List<Event> Past;

    /// <summary>
    /// List of events currently running.
    /// </summary>
    public static List<Event> Current;

    /// <summary>
    /// List of all possible events that can be triggered.
    /// </summary>
    public static List<Event> Possible;

    /// <summary>
    /// List of events that can occur at intervals.
    /// </summary>
    private static List<Event> Intervals;

    /// <summary>
    /// List of players who have voted for the current event.
    /// </summary>
    private static List<string> _voters;

    /// <summary>
    /// Indicates whether a vote is currently in progress.
    /// </summary>
    private static bool _voting = false;

    /// <summary>
    /// The timestamp of the last interval trigger.
    /// </summary>
    private static DateTime _lastIntervalTrigger = DateTime.MinValue;

    /// <summary>
    /// Main scheduling loop that checks for and triggers events based on various conditions.
    /// </summary>
    /// <returns>An IEnumerator for Unity's coroutine system.</returns>
    public static IEnumerator ScheduleLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);

            if (Current !=null && Current.Count >= VSettings.Concurrent.Value) continue;
            if (_voting) continue;

            DateTime date = DateTime.UtcNow;

            bool loopBreak = false;
            if (Possible != null && Possible.Count > 0)
                foreach (Event e in Possible)
                {
                    if (Current.Any(x => x.id == e.id)) continue;
                    if (loopBreak) break;
                    if (e.dateTimes != null)
                    {
                        if (e.eventTrigger == EventTrigger.Preset)
                        {
                            if (e.dateTimes.Any(x => x.DayOfWeek == date.DayOfWeek) || e.dateTimes.First().DayOfWeek == null)
                            {
                                ScheduleEntry day = e.dateTimes.FirstOrDefault(x => x.DayOfWeek == date.DayOfWeek);

                                if (day == null)
                                    day = e.dateTimes.First();

                                if (day.Hour == date.Hour && day.Minute == date.Minute)
                                {
                                    StartVote(e);
                                    ChatUtil.SystemSendAll($"A new event vote has started for {e.name}! Vote for it using !vote");
                                    loopBreak = true;
                                    break;
                                }
                            }
                        }
                    }
                }

            if (loopBreak) continue;

            TimeSpan timeSinceLastTrigger = date - _lastIntervalTrigger;
            if ((_lastIntervalTrigger == DateTime.MinValue && date.Minute == 0) || timeSinceLastTrigger.TotalMinutes >= VSettings.Interval.Value)
            {
                if (Intervals != null && Intervals.Count > 0 && RandomInterval(out Event e))
                {
                    StartVote(e);
                    ChatUtil.SystemSendAll($"A new event vote has started for {e.name}! Vote for it using !vote");
                    _lastIntervalTrigger = date;
                }
            }
        }
    }

    /// <summary>
    /// Registers a new event in the scheduler.
    /// </summary>
    /// <param name="e">The event to register.</param>
    public static void Register(Event e)
    {
        if (Possible == null) Possible = new List<Event>();
        if (Past == null) Past = new List<Event>();
        if (Current == null) Current = new List<Event>();

        Possible.Add(e);

        if (e.eventTrigger == EventTrigger.Interval)
        {
            if (Intervals == null) Intervals = new List<Event>();
            Intervals.Add(e);
        }
    }

    /// <summary>
    /// Initiates a vote for a specific event.
    /// </summary>
    /// <param name="e">The event to start voting for.</param>
    public static void StartVote(Event e)
    {
        _voters = new List<string>();
        Core.StartCoroutine(HoldVote(e));
    }

    /// <summary>
    /// Adds a player's vote for the current event.
    /// </summary>
    /// <param name="characterName">The name of the character voting.</param>
    public static void AddVote(string characterName)
    {
        if (_voters == null) _voters = new List<string>();
        if (!_voters.Contains(characterName))
        {
            _voters.Add(characterName);
        }
    }

    /// <summary>
    /// Checks if there is currently an active vote.
    /// </summary>
    /// <returns>True if a vote is in progress, false otherwise.</returns>
    public static bool IsVoting()
    {
        return _voting;
    }

    /// <summary>
    /// Ends an event and removes it from the current events list. 
    ///  You MUST use this method to end an event, otherwise it will not be removed from the list and will not retrigger.
    /// </summary>
    /// <param name="guid">The unique identifier of the event to end.</param>
    /// <returns>True if the event was successfully ended, false otherwise.</returns>
    public static bool EndEvent(Guid guid)
    {
        if (Current.Any(x => x.id == guid))
        {
            return Current.Remove(Current.First(x => x.id == guid));
        }

        return false;
    }

    /// <summary>
    /// Manages the voting process for an event.
    /// </summary>
    /// <param name="e">The event being voted on.</param>
    /// <returns>An IEnumerator for Unity's coroutine system.</returns>
    static IEnumerator HoldVote(Event e)
    {
        int votingMins = 0;
        _voting = true;
        while (votingMins <= VSettings.AcceptPeriod.Value)
        {
            yield return new WaitForSeconds(60);

            if (HasPassed(e))
            {
                OnEventStarted?.Invoke(e);
                Current.Add(e);
                Past.Add(e);
                votingMins = 999;
            }
        }
        _voting = false;

        if (!HasPassed(e))
        {
            ChatUtil.SystemSendAll($"The vote for {e.name} has failed.");
        }
    }

    /// <summary>
    /// Selects a random interval event that isn't currently running.
    /// </summary>
    /// <param name="e">The selected event if successful.</param>
    /// <returns>True if an event was selected, false if no events are available.</returns>
    private static bool RandomInterval(out Event e)
    {
        e = null;

        var availableEvents = Intervals.Where(x => !Current.Any(c => c.id == x.id)).ToList();

        if (availableEvents.Count == 0)
        {
            return false;
        }

        e = availableEvents[UnityEngine.Random.Range(0, availableEvents.Count)];
        return true;
    }

    /// <summary>
    /// Determines if an event vote has passed based on the minimum player requirements.
    /// </summary>
    /// <param name="e">The event being voted on.</param>
    /// <returns>True if the vote passed, false otherwise.</returns>
    private static bool HasPassed(Event e)
    {
        if (e.minPlayers != 0)
        {
            return _voters.Count > (PlayerService.GetUsersOnline().Count() / 2);
        }

        return _voters.Count > e.minPlayers;
    }
}