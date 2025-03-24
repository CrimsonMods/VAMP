using System;
using System.Linq;
using VAMP.Services;
using Unity.Entities;
using ProjectM.Physics;
using UnityEngine;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ProjectM.Scripting;
using ProjectM.Tiles;
using VAMP.Utilities;

namespace VAMP;

public static class Core
{
    public static World Server { get; } = GetServerWorld() ?? throw new Exception("There is no Server world (yet)...");

    public static EntityManager EntityManager => Server.EntityManager;
    public static SystemService SystemService {get; } = new(Server);
    public static ServerScriptMapper ServerScriptMapper => SystemService.ServerScriptMapper;
    public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();
    public static Entity TileModelSpatialLookupSystem { get; private set; }

    public static PlayerService PlayerService { get; private set; }
    public static CastleHeartService CastleHeartService { get; private set; }
    public static CastleTerritoryService CastleTerritoryService { get; private set; }

    public static bool hasInitialized = false;
    
    static MonoBehaviour monoBehaviour;

    public static void Initialize()
    {
        if (hasInitialized) return;

        PlayerService = new PlayerService();
        CastleHeartService = new CastleHeartService();
        CastleTerritoryService = new CastleTerritoryService();

        TileModelSpatialLookupSystem = EntityUtil.GetEntitiesByComponentTypes<TileModelSpatialLookupSystem.Singleton>(EntityQueryOptions.IncludeSystems)[0];

        hasInitialized = true;

        Plugin.OnCoreLoaded?.Invoke();

        StartCoroutine(EventScheduler.ScheduleLoop());
    }

    static World GetServerWorld()
    {
        return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
    }

    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        if (monoBehaviour == null)
        {
            var go = new GameObject("FANG");
            monoBehaviour = go.AddComponent<IgnorePhysicsDebugSystem>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        return monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
    }

    public static void StopCoroutine(Coroutine coroutine)
    {
        if (monoBehaviour == null)
        {
            return;
        }

        monoBehaviour.StopCoroutine(coroutine);
    }
}