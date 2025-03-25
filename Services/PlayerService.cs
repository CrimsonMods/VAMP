using ProjectM.Network;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using VAMP.Structs;
using VAMP.Utilities;

namespace VAMP.Services;

public class PlayerService
{
    private static readonly Dictionary<string, PlayerData> namePlayerCache = new();
    private static readonly Dictionary<ulong, PlayerData> steamPlayerCache = new();
    private static readonly Dictionary<NetworkId, PlayerData> idPlayerCache = new();

    internal PlayerService()
    {
        namePlayerCache.Clear();
        steamPlayerCache.Clear();

        var userEntities = EntityUtil.GetEntitiesByComponentType<User>(includeDisabled: true);
        foreach (var entity in userEntities)
        {
            var userData = entity.Read<User>();
            var playerData = new PlayerData(userData.CharacterName, userData.PlatformId, userData.IsConnected, entity, userData.LocalCharacter._Entity);

            namePlayerCache.TryAdd(userData.CharacterName.ToString(), playerData);
            steamPlayerCache.TryAdd(userData.PlatformId, playerData);
        }

        var onlinePlayers = namePlayerCache.Values.Where(p => p.IsOnline).Select(p => $"\t{p.CharacterName}");

        Plugin.LogInstance.LogWarning($"Player Cache Created with {namePlayerCache.Count} entries total, listing {onlinePlayers.Count()} online:");
        Plugin.LogInstance.LogWarning(string.Join("\n", onlinePlayers));
    }

    internal static void UpdatePlayerCache(Entity userEntity, string oldName, string newName, bool forceOffline = false)
    {
        var userData = userEntity.Read<User>();
        namePlayerCache.Remove(oldName);

        if (forceOffline) userData.IsConnected = false;
        var playerData = new PlayerData(newName, userData.PlatformId, userData.IsConnected, userEntity, userData.LocalCharacter._Entity);

        namePlayerCache[newName] = playerData;
        steamPlayerCache[userData.PlatformId] = playerData;
        idPlayerCache[userEntity.Read<NetworkId>()] = playerData;
    }

    /// <summary>
    /// Gets all online users from the game's entity system.
    /// </summary>
    /// <returns>An enumerable collection of online user entities.</returns>
    public static IEnumerable<Entity> GetUsersOnline()
    {
        NativeArray<Entity> _userEntities = Core.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp);
        foreach (var entity in _userEntities)
        {
            if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
                yield return entity;
        }
    }

    /// <summary>
    /// Gets all online users from the cached player data.
    /// </summary>
    /// <returns>An enumerable collection of online user entities from cache.</returns>
    public static IEnumerable<Entity> GetCachedUsersOnline()
    {
        foreach (var pd in namePlayerCache.Values.ToArray())
        {
            var entity = pd.UserEntity;
            if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
                yield return entity;
        }
    }

    /// <summary>
    /// Attempts to find a player by their Steam ID.
    /// </summary>
    /// <param name="steamId">The Steam ID to search for.</param>
    /// <param name="playerData">When this method returns, contains the player data associated with the specified Steam ID, if found; otherwise, the default value.</param>
    /// <returns>true if a player with the specified Steam ID was found; otherwise, false.</returns>
    public static bool TryFindBySteam(ulong steamId, out PlayerData playerData)
    {
        return steamPlayerCache.TryGetValue(steamId, out playerData);
    }

    /// <summary>
    /// Attempts to find a player by their character name.
    /// </summary>
    /// <param name="name">The character name to search for.</param>
    /// <param name="playerData">When this method returns, contains the player data associated with the specified name, if found; otherwise, the default value.</param>
    /// <returns>true if a player with the specified name was found; otherwise, false.</returns>
    public static bool TryFindByName(string name, out PlayerData playerData)
    {
        return namePlayerCache.TryGetValue(name, out playerData);
    }

    /// <summary>
    /// Attempts to find a player by their character name using a FixedString64Bytes.
    /// </summary>
    /// <param name="name">The character name as FixedString64Bytes to search for.</param>
    /// <param name="playerData">When this method returns, contains the player data associated with the specified name, if found; otherwise, the default value.</param>
    /// <returns>true if a player with the specified name was found; otherwise, false.</returns>
    public static bool TryFindByName(FixedString64Bytes name, out PlayerData playerData)
    {
        return namePlayerCache.TryGetValue(name.ToString(), out playerData);
    }

    /// <summary>
    /// Attempts to find a user entity by their network ID.
    /// </summary>
    /// <param name="networkId">The network ID to search for.</param>
    /// <param name="userEntity">When this method returns, contains the user entity associated with the specified network ID, if found; otherwise, Entity.Null.</param>
    /// <returns>true if a user entity with the specified network ID was found; otherwise, false.</returns>
    public static bool TryFindByNetworkId(NetworkId networkId, out Entity userEntity)
    {
        if (idPlayerCache.TryGetValue(networkId, out var playerData))
        {
            userEntity = playerData.UserEntity;
            return true;
        }
        userEntity = Entity.Null;
        return false;
    }
}