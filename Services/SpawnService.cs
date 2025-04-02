using ProjectM;
using Stunlock.Core;
using System;
using Unity.Entities;
using Unity.Mathematics;
using VAMP.Patches;

namespace VAMP.Services;

/// <summary>
/// Provides methods for spawning units in the game world.
/// </summary>
public static class SpawnService
{
    /// <summary>
    /// Spawns a unit at the specified position and executes a callback action after a duration.
    /// </summary>
    /// <param name="unit">The PrefabGUID of the unit to spawn.</param>
    /// <param name="position">The position in 3D space where the unit will be spawned.</param>
    /// <param name="duration">The duration in seconds before the callback is executed.</param>
    /// <param name="postActions">The callback action to execute after the duration. Takes the spawned Entity as a parameter.</param>
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