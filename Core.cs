using System;
using System.Linq;
using VAMP.Services;
using Unity.Entities;
using ProjectM.Physics;
using UnityEngine;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace VAMP;

public static class Core
{
    public static World Server { get; } = GetServerWorld() ?? throw new Exception("There is no Server world (yet)...");
    public static EntityManager EntityManager => Server.EntityManager;

    public static PlayerService PlayerService { get; private set; }
    public static CastleHeartService CastleHeartService { get; private set; }
    
    public static bool hasInitialized = false;
    public static Action OnCoreLoaded;

    static MonoBehaviour monoBehaviour;

    public static void Initialize()
    {
        if (hasInitialized) return;

        PlayerService = new PlayerService();
        CastleHeartService = new CastleHeartService();
        
        hasInitialized = true;
        OnCoreLoaded?.Invoke();
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