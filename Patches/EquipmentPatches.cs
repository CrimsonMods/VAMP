using System.Collections.Generic;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using VAMP.Data;
using VAMP.Systems;
using VAMP.Utilities;

namespace VAMP.Patches;

[HarmonyPatch]
internal static class EquipmentPatches
{
    [HarmonyPatch(typeof(WeaponLevelSystem_Spawn), nameof(WeaponLevelSystem_Spawn.OnUpdate))]
    [HarmonyPostfix]
    static void WeaponLevelPatch(WeaponLevelSystem_Spawn __instance)
    {
        if (!Core.hasInitialized) return;

        NativeArray<Entity> entities = __instance.__query_1111682356_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity entity in entities)
            {
                if (!entity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists()) continue;
                else if (entityOwner.Owner.TryGetComponent(out PlayerCharacter playerCharacter))
                {
                    RecordLevelSystem.SetRecord(playerCharacter.UserEntity.Read<User>().PlatformId);
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }

    [HarmonyPatch(typeof(ArmorLevelSystem_Spawn), nameof(ArmorLevelSystem_Spawn.OnUpdate))]
    [HarmonyPostfix]
    static void ArmorLevelPatch(ArmorLevelSystem_Spawn __instance)
    {
        if (!Core.hasInitialized) return;

        NativeArray<Entity> entities = __instance.__query_663986227_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity entity in entities)
            {
                if (!entity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists()) continue;
                else if (entityOwner.Owner.TryGetComponent(out PlayerCharacter playerCharacter))
                {
                    RecordLevelSystem.SetRecord(playerCharacter.UserEntity.Read<User>().PlatformId);
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }

    private static readonly HashSet<PrefabGUID> MagicSourceGuids = new HashSet<PrefabGUID>
    {
        Prefabs.Item_EquipBuff_MagicSource_BloodKey_T01,
        Prefabs.Item_EquipBuff_MagicSource_General,
        Prefabs.Item_EquipBuff_MagicSource_Soulshard_Dracula,
        Prefabs.Item_EquipBuff_MagicSource_Soulshard_Manticore,
        Prefabs.Item_EquipBuff_MagicSource_Soulshard_Morgana,
        Prefabs.Item_EquipBuff_MagicSource_Soulshard_Solarus,
        Prefabs.Item_EquipBuff_MagicSource_Soulshard_TheMonster,
        Prefabs.Item_EquipBuff_MagicSource_T06_Blood,
        Prefabs.Item_EquipBuff_MagicSource_T06_Chaos,
        Prefabs.Item_EquipBuff_MagicSource_T06_Frost,
        Prefabs.Item_EquipBuff_MagicSource_T06_Illusion,
        Prefabs.Item_EquipBuff_MagicSource_T06_Storm,
        Prefabs.Item_EquipBuff_MagicSource_T06_Unholy,
        Prefabs.Item_EquipBuff_MagicSource_T08_Blood,
        Prefabs.Item_EquipBuff_MagicSource_T08_Chaos,
        Prefabs.Item_EquipBuff_MagicSource_T08_Frost,
        Prefabs.Item_EquipBuff_MagicSource_T08_Illusion,
        Prefabs.Item_EquipBuff_MagicSource_T08_Storm,
        Prefabs.Item_EquipBuff_MagicSource_T08_Unholy,
        Prefabs.Item_EquipBuff_Shared_General
    };

    [HarmonyPatch(typeof(BuffDebugSystem), nameof(BuffDebugSystem.OnUpdate))]
    [HarmonyPostfix]
    private static void BuffDebugPatch(BuffDebugSystem __instance)
    {
        if (!Core.hasInitialized) return;
        NativeArray<Entity> entities = __instance.__query_401358787_0.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            var guid = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);

            if (MagicSourceGuids.Contains(guid))
            {
                if (!Core.EntityManager.TryGetComponentData<EntityOwner>(entity, out var entityOwner) ||
                        !Core.EntityManager.TryGetComponentData<PlayerCharacter>(entityOwner.Owner, out var playerCharacter) ||
                        !Core.EntityManager.TryGetComponentData<User>(playerCharacter.UserEntity, out var user)) return;

                RecordLevelSystem.SetRecord(user.PlatformId);
            }
        }
    }
}