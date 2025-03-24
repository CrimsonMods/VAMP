using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using System;

namespace VAMP.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnectedPatch
{
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
                Services.PlayerService.UpdatePlayerCache(userEntity, playerName, playerName);
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
