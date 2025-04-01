using System;
using System.Collections.Generic;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using VAMP.Utilities;

namespace VAMP.Patches;


[HarmonyPatch]
public static class UnitSpawnerPatch
{
    internal static Dictionary<long, (float actualDuration, Action<Entity> Actions)> PostActions = new();

    [HarmonyPatch(typeof(UnitSpawnerReactSystem), nameof(UnitSpawnerReactSystem.OnUpdate))]
    internal static void Postfix(UnitSpawnerReactSystem __instance)
    {
        try
        {
            Plugin.LogInstance.LogInfo($"UnitSpawnerPatch running with {PostActions.Count} pending actions");

            var _entities = __instance._Query.ToEntityArray(Allocator.Temp);
            Plugin.LogInstance.LogInfo($"Found {_entities.Length} entities to process");

            foreach (var entity in _entities)
            {
                if (!entity.Has<LifeTime>())
                {
                    Plugin.LogInstance.LogInfo($"Entity {entity.Index} skipped - no LifeTime component");
                    continue;
                }

                var lifetimeComp = entity.Read<LifeTime>();
                var durationKey = (long)Mathf.Round(lifetimeComp.Duration);

                Plugin.LogInstance.LogInfo($"Checking entity {entity.Index} with duration {lifetimeComp.Duration} (key: {durationKey})");

                if (PostActions.TryGetValue(durationKey, out var unitData))
                {
                    Plugin.LogInstance.LogInfo($"Found matching PostAction for key {durationKey}");
                    var (actualDuration, actions) = unitData;
                    PostActions.Remove(durationKey);

                    var endAction = actualDuration < 0 ? LifeTimeEndAction.None : LifeTimeEndAction.Destroy;

                    var newLifeTime = new LifeTime()
                    {
                        Duration = actualDuration,
                        EndAction = endAction
                    };

                    entity.Write(newLifeTime);
                    entity.Add<CanFly>();

                    Plugin.LogInstance.LogInfo($"Executing PostAction for entity {entity.Index}");
                    actions(entity);
                }
                else
                {
                    Plugin.LogInstance.LogInfo($"No matching PostAction found for key {durationKey}");
                }
            }
        }
        catch (Exception e)
        {
            Plugin.LogInstance.LogError($"Error in UnitSpawnerPatch: {e}");
        }
    }
}