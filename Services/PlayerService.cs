using ProjectM.Network;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using VAMP.Structs;
using VAMP.Utilities;

namespace VAMP.Services;

internal class PlayerService
{
    readonly Dictionary<FixedString64Bytes, PlayerData> namePlayerCache = new();
    readonly Dictionary<ulong, PlayerData> steamPlayerCache = new();
    readonly Dictionary<NetworkId, PlayerData> idPlayerCache = new();
    internal bool TryFindSteam(ulong steamId, out PlayerData playerData)
    {
        return steamPlayerCache.TryGetValue(steamId, out playerData);
    }

    internal bool TryFindName(FixedString64Bytes name, out PlayerData playerData)
    {
        return namePlayerCache.TryGetValue(name, out playerData);
    }

    internal PlayerService()
    {
        namePlayerCache.Clear();
        steamPlayerCache.Clear();

        var userEntities = EntityUtil.GetEntitiesByComponentType<User>(includeDisabled: true);
        foreach (var entity in userEntities)
        {
            var userData = Core.EntityManager.GetComponentData<User>(entity);
            var playerData = new PlayerData(userData.CharacterName, userData.PlatformId, userData.IsConnected, entity, userData.LocalCharacter._Entity);

            namePlayerCache.TryAdd(userData.CharacterName.ToString().ToLower(), playerData);
            steamPlayerCache.TryAdd(userData.PlatformId, playerData);
        }

        var onlinePlayers = namePlayerCache.Values.Where(p => p.IsOnline).Select(p => $"\t{p.CharacterName}");
    }

    internal void UpdatePlayerCache(Entity userEntity, string oldName, string newName, bool forceOffline = false)
    {
        var userData = Core.EntityManager.GetComponentData<User>(userEntity);
        namePlayerCache.Remove(oldName.ToLower());

        if (forceOffline) userData.IsConnected = false;
        var playerData = new PlayerData(newName, userData.PlatformId, userData.IsConnected, userEntity, userData.LocalCharacter._Entity);

        namePlayerCache[newName.ToLower()] = playerData;
        steamPlayerCache[userData.PlatformId] = playerData;
        idPlayerCache[userEntity.Read<NetworkId>()] = playerData;
    }

    public bool TryFindUserFromNetworkId(NetworkId networkId, out Entity userEntity)
    {
        if (idPlayerCache.TryGetValue(networkId, out var playerData))
        {
            userEntity = playerData.UserEntity;
            return true;
        }
        userEntity = Entity.Null;
        return false;
    }

    public static IEnumerable<Entity> GetUsersOnline()
    {

        NativeArray<Entity> _userEntities = Core.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp);
        foreach (var entity in _userEntities)
        {
            if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
                yield return entity;
        }
    }

    public IEnumerable<Entity> GetCachedUsersOnline()
    {
        foreach (var pd in namePlayerCache.Values.ToArray())
        {
            var entity = pd.UserEntity;
            if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
                yield return entity;
        }
    }
}