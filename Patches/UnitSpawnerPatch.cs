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
    internal static void Prefix(UnitSpawnerReactSystem __instance)
    {
        try
        {
            var _entities = __instance._Query.ToEntityArray(Allocator.Temp);

            foreach (var entity in _entities)
            {
                if (!entity.Has<LifeTime>())
                {
                    continue;
                }

                DevUtil.ChatDebug($"Entity {entity.Index} has LifeTime", "spawn");

                var lifetimeComp = entity.Read<LifeTime>();
                var durationKey = (long)Mathf.Round(lifetimeComp.Duration);
                DevUtil.ChatDebug($"Entity {entity.Index} has durationKey {durationKey}", "spawn");

                if (PostActions.TryGetValue(durationKey, out var unitData))
                {
                    DevUtil.ChatDebug($"Entity {entity.Index} has unitData", "spawn");
                    var (actualDuration, actions) = unitData;
                    PostActions.Remove(durationKey);

                    var endAction = actualDuration <= 0 ? LifeTimeEndAction.None : LifeTimeEndAction.Destroy;

                    var newLifeTime = new LifeTime()
                    {
                        Duration = actualDuration,
                        EndAction = endAction
                    };

                    entity.Write(newLifeTime);
                    entity.Add<CanFly>();

                    actions(entity);
                }
            }
        }
        catch (Exception e)
        {
            Plugin.LogInstance.LogError($"Error in UnitSpawnerPatch: {e}");
        }
    }
}