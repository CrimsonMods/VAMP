using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using UnityEngine;
using VAMP.Structs;
using Event = VAMP.Structs.Event;

namespace VAMP.Services;

public static class EventScheduler
{
    public static Action<Event> OnEventStarted;
    public static List<Event> Past;
    public static List<Event> Current;
    public static List<Event> Possible;
    private static List<Event> Intervals;
    private static List<string> _voters;
    private static bool _voting = false;

    public static IEnumerator ScheduleLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(60);

            DateTime date = DateTime.UtcNow;

            if(date.Minute == 0 || date.Minute == 30)
            {
                Event e = Intervals[UnityEngine.Random.Range(0, Possible.Count)];
                StartVote(e);
                ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, $"A new event vote has started for {e.name}! Vote for it using !vote");
            }
        }
    }

    public static void Register(Event e)
    {
        if(Possible == null) Possible = new List<Event>();
        if(Past == null) Past = new List<Event>();
        if(Current == null) Current = new List<Event>();

        Possible.Add(e);

        if(e.eventTrigger == EventTrigger.Interval)
        {
            if(Intervals == null) Intervals = new List<Event>();
            Intervals.Add(e);
        }
    }

    static IEnumerator HoldVote(Event e)
    {
        int votingMins = 0;
        _voting = true;
        while(votingMins <= 3)
        {
            yield return new WaitForSeconds(60);

            if(HasPassed())
            {
                OnEventStarted?.Invoke(e);
                Current.Add(e);
                Past.Add(e);
                votingMins = 999;
            }
        }
        _voting = false;

        if(!HasPassed())
        {
            ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, $"The vote for {e.name} has failed.");
        }
    }

    public static void StartVote(Event e)
    {
        _voters = new List<string>();
        Core.StartCoroutine(HoldVote(e));
    }

    public static void AddVote(string characterName)
    {
        if(_voters == null) _voters = new List<string>();
        if(!_voters.Contains(characterName))
        {
            _voters.Add(characterName);
        }
    }

    private static bool HasPassed()
    {
        return _voters.Count > (PlayerService.GetUsersOnline().Count() / 2);
    }

    public static bool IsVoting()
    {
        return _voting;
    }
}