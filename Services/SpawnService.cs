using ProjectM;
using Stunlock.Core;
using System;
using Unity.Entities;
using Unity.Mathematics;
using VAMP.Patches;

namespace VAMP.Services;

public static class SpawnService
{
    public static void SpawnUnitWithCallback(PrefabGUID unit, float3 position, float duration, Action<Entity> postActions)
    {
        Entity empty_entity = new Entity();
        var f3pos = new float3(position.x, position.y, position.z);
        var usus = Core.Server.GetExistingSystemManaged<UnitSpawnerUpdateSystem>();
        var durationKey = NextKey();
        usus.SpawnUnit(empty_entity, unit, f3pos, 1, 1, 1, durationKey);
        UnitSpawnerPatch.PostActions.Add(durationKey, (duration, postActions));
    }

    internal static long NextKey()
    {
        System.Random r = new();
        long key;
        int breaker = 5;
        do
        {
            key = r.NextInt64(10000) * 3;
            breaker--;
            if (breaker < 0)
            {
                throw new Exception($"Failed to generate a unique key for UnitSpawnerService");
            }
        } while (UnitSpawnerPatch.PostActions.ContainsKey(key));
        return key;
    }
}