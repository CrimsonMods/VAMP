using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using VAMP.Data;
using VAMP.Models;
using VAMP.Services;
using VAMP.Structs;
using VAMP.Utilities;

namespace VAMP.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnectedPatch
{
    private static List<ulong> informedAdminOfUpdates = new List<ulong>();

    public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
    {
        if (Core.PlayerService == null) Core.Initialize();
        try
        {
            var entityManager = __instance.EntityManager;
            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;
            var userData = entityManager.GetComponentData<User>(userEntity);

            if (!userData.CharacterName.IsEmpty)
            {
                var playerName = userData.CharacterName.ToString();
                PlayerService.UpdatePlayerCache(userEntity, playerName, playerName);

                if (!informedAdminOfUpdates.Contains(userData.PlatformId) && PlayerService.TryFindBySteam(userData.PlatformId, out Player player))
                {
                    var mods = ModSystem.GetLoadedModsInfo();
                    List<ModInfo> modList = new List<ModInfo>();
                    foreach (var mod in mods)
                    {
                        if (Version.TryParse(mod.Version, out Version modVersion) && Version.TryParse(mod.ThunderstoreVersion, out Version thunderstoreVersion) && modVersion < thunderstoreVersion)
                        {
                            modList.Add(mod);
                        }
                    }

                    if (modList.Count > 0)
                    {
                        var modString = string.Join("\n", modList.Select(mod => $"• <color=#def>{mod.Name}</color> (<color=#def>{mod.Version}</color> -> <color=#def>{mod.ThunderstoreVersion}</color>)"));
                        ChatUtil.SystemSendUser(userData, $"Updates Available:\n{modString}");
                        informedAdminOfUpdates.Add(userData.PlatformId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance.Log.LogError(ex.Message);
        }
    }
}

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class OnUserDisconnectedPatch
{
    private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
    {
        if (Core.PlayerService == null) Core.Initialize();
        try
        {
            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];
            var userData = __instance.EntityManager.GetComponentData<User>(serverClient.UserEntity);
            bool isNewVampire = userData.CharacterName.IsEmpty;

            if (!isNewVampire)
            {
                var playerName = userData.CharacterName.ToString();
                Services.PlayerService.UpdatePlayerCache(serverClient.UserEntity, playerName, playerName, true);
            }
        }
        catch { };
    }
}

[HarmonyPatch(typeof(Destroy_TravelBuffSystem), nameof(Destroy_TravelBuffSystem.OnUpdate))]
public class Destroy_TravelBuffSystem_Patch
{
    private static void Postfix(Destroy_TravelBuffSystem __instance)
    {
        if (Core.PlayerService == null) Core.Initialize();
        var entities = __instance.__query_615927226_0.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);

            if (GUID.Equals(Prefabs.AB_Interact_TombCoffinSpawn_Travel))
            {
                var owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!__instance.EntityManager.HasComponent<PlayerCharacter>(owner)) return;

                var userEntity = __instance.EntityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                var userData = __instance.EntityManager.GetComponentData<User>(userEntity);

                var playerName = userData.CharacterName.ToString();
                Services.PlayerService.UpdatePlayerCache(userEntity, playerName, playerName);
            }
        }
    }
}
