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
using VAMP.Structs;
using VAMP.Systems;

namespace VAMP;

public static class Core
{
    public static World Server { get; } = GetServerWorld() ?? throw new Exception("There is no Server world (yet)...");

    public static EntityManager EntityManager => Server.EntityManager;
    public static SystemService SystemService { get; } = new(Server);
    public static ServerScriptMapper ServerScriptMapper => SystemService.ServerScriptMapper;
    public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();
    public static Entity TileModelSpatialLookupSystem { get; private set; }

    public static PlayerService PlayerService { get; private set; }
    public static CastleHeartService CastleHeartService { get; private set; }
    public static CastleTerritoryService CastleTerritoryService { get; private set; }
    public static ClanService ClanService { get; private set; }
    public static ModSystem ModSystem { get; private set; }
    internal static RecordLevelSystem RecordLevelSystem { get; private set; }

    public static bool hasInitialized = false;

    static MonoBehaviour monoBehaviour;

    internal static Database Database { get; private set; }

    public static void Initialize()
    {
        if (hasInitialized) return;

        PlayerService = new PlayerService();
        CastleHeartService = new CastleHeartService();
        CastleTerritoryService = new CastleTerritoryService();
        ClanService = new ClanService();
        ModSystem = new ModSystem();

        TileModelSpatialLookupSystem = EntityUtil.GetEntitiesByComponentTypes<TileModelSpatialLookupSystem.Singleton>(EntityQueryOptions.IncludeSystems)[0];

        hasInitialized = true;

        ServerWipe.WipeDataDetection();

        Database = new Database();

        RecordLevelSystem = new RecordLevelSystem();

        Plugin.OnCoreLoaded?.Invoke();
        Events.OnCoreLoaded?.Invoke();

        StartCoroutine(EventScheduler.ScheduleLoop());
    }

    static World GetServerWorld()
    {
        return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
    }

    /// <summary>
    /// Starts a coroutine using a MonoBehaviour instance.
    /// </summary>
    /// <param name="routine">The IEnumerator routine to start as a coroutine.</param>
    /// <returns>A Coroutine instance that can be used to stop the routine later.</returns>
    /// <remarks>
    /// If no MonoBehaviour instance exists, creates a new GameObject with an IgnorePhysicsDebugSystem component.
    /// The GameObject is marked to not be destroyed when loading new scenes.
    /// </remarks>
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

    /// <summary>
    /// Stops a running coroutine.
    /// </summary>
    /// <param name="coroutine">The Coroutine instance to stop.</param>
    /// <remarks>
    /// If no MonoBehaviour instance exists, the method returns without doing anything.
    /// </remarks>
    public static void StopCoroutine(Coroutine coroutine)
    {
        if (monoBehaviour == null)
        {
            return;
        }

        monoBehaviour.StopCoroutine(coroutine);
    }
}