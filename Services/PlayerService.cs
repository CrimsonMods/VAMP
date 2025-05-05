using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using VAMP.Models;
using VAMP.Structs;
using VAMP.Utilities;

namespace VAMP.Services;

public class PlayerService
{
    private static readonly Dictionary<string, PlayerData> old_namePlayerCache = new();
    private static readonly Dictionary<ulong, PlayerData> old_steamPlayerCache = new();
    private static readonly Dictionary<NetworkId, PlayerData> old_idPlayerCache = new();

    private static readonly Dictionary<string, Player> namePlayerCache = new();
    private static readonly Dictionary<ulong, Player> steamPlayerCache = new();
    private static readonly Dictionary<NetworkId, Player> idPlayerCache = new();

    internal PlayerService()
    {
        namePlayerCache.Clear();
        steamPlayerCache.Clear();

        var userEntities = EntityUtil.GetEntitiesByComponentType<User>(includeDisabled: true);
        foreach (var entity in userEntities)
        {
            var userData = entity.Read<User>();
            var playerData = new PlayerData(userData.CharacterName, userData.PlatformId, userData.IsConnected, entity, userData.LocalCharacter._Entity);

            old_namePlayerCache.TryAdd(userData.CharacterName.ToString(), playerData);
            old_steamPlayerCache.TryAdd(userData.PlatformId, playerData);
        }

        var onlinePlayers = old_namePlayerCache.Values.Where(p => p.IsOnline).Select(p => $"\t{p.CharacterName}");

        Plugin.LogInstance.LogWarning($"Player Cache Created with {old_namePlayerCache.Count} entries total.");
    }

    internal static void UpdatePlayerCache(Entity userEntity, string oldName, string newName, bool forceOffline = false)
    {
        var userData = userEntity.Read<User>();
        old_namePlayerCache.Remove(oldName);
        namePlayerCache.Remove(oldName);

        if (forceOffline) userData.IsConnected = false;
        var playerData = new PlayerData(newName, userData.PlatformId, userData.IsConnected, userEntity, userData.LocalCharacter._Entity);
        var player = PlayerFromUser(userData);

        old_namePlayerCache[newName] = playerData;
        old_steamPlayerCache[userData.PlatformId] = playerData;
        old_idPlayerCache[userEntity.Read<NetworkId>()] = playerData;

        namePlayerCache[newName] = player;
        steamPlayerCache[userData.PlatformId] = player;
        idPlayerCache[userEntity.Read<NetworkId>()] = player;
    }

    /// <summary>
    /// Gets all online users from the game's entity system.
    /// </summary>
    /// <returns>An enumerable collection of online user entities.</returns>
    public static IEnumerable<Entity> GetUsersOnline()
    {
        var componentTypes = new ComponentType[] { ComponentType.ReadOnly<User>() };
        NativeArray<Entity> _userEntities = Core.EntityManager.CreateEntityQuery(componentTypes).ToEntityArray(Allocator.Temp);

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
            var entity = pd.User;
            if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
                yield return entity;
        }
    }

    /// <summary>
    /// Gets all online players from the cached player data.
    /// </summary>
    /// <returns>An enumerable collection of online players from cache.</returns>
    public static IEnumerable<Player> GetCachedUsersOnlineAsPlayer()
    {
        foreach (var pd in namePlayerCache.Values.ToArray())
        {
            if (pd.Character.Exists() && pd.IsOnline)
                yield return pd;
        }
    }

    /// <summary>
    /// Attempts to find a player by their Steam ID.
    /// </summary>
    /// <param name="steamId">The Steam ID to search for.</param>
    /// <param name="playerData">When this method returns, contains the player data associated with the specified Steam ID, if found; otherwise, the default value.</param>
    /// <returns>true if a player with the specified Steam ID was found; otherwise, false.</returns>
    [Obsolete("This method is deprecated and will be removed in a future version. Use TryFindBySteam(ulong, out Player) instead.")]
    public static bool TryFindBySteam(ulong steamId, out PlayerData playerData)
    {
        return old_steamPlayerCache.TryGetValue(steamId, out playerData);
    }

    /// <summary>
    /// Attempts to find a player by their Steam ID.
    /// </summary>
    /// <param name="steamId">The Steam ID to search for.</param>
    /// <param name="player">When this method returns, contains the player associated with the specified Steam ID, if found; otherwise, null.</param>
    /// <returns>true if a player with the specified Steam ID was found; otherwise, false.</returns>
    public static bool TryFindBySteam(ulong steamId, out Player player)
    {
        return steamPlayerCache.TryGetValue(steamId, out player);
    }

    /// <summary>
    /// Attempts to find a player by their character name.
    /// </summary>
    /// <param name="name">The character name to search for.</param>
    /// <param name="playerData">When this method returns, contains the player data associated with the specified name, if found; otherwise, the default value.</param>
    /// <returns>true if a player with the specified name was found; otherwise, false.</returns>
    [Obsolete("This method is deprecated and will be removed in a future version. Use TryFindByName(string, out Player) instead.")]
    public static bool TryFindByName(string name, out PlayerData playerData)
    {
        return old_namePlayerCache.TryGetValue(name, out playerData);
    }

    /// <summary>
    /// Attempts to find a player by their character name.
    /// </summary>
    /// <param name="name">The character name to search for.</param>
    /// <param name="player">When this method returns, contains the player associated with the specified name, if found; otherwise, null.</param>
    /// <returns>true if a player with the specified name was found; otherwise, false.</returns>
    public static bool TryFindByName(string name, out Player player)
    {
        return namePlayerCache.TryGetValue(name, out player);
    }

    /// <summary>
    /// Attempts to find a player by their character name using a FixedString64Bytes.
    /// </summary>
    /// <param name="name">The character name as FixedString64Bytes to search for.</param>
    /// <param name="playerData">When this method returns, contains the player data associated with the specified name, if found; otherwise, the default value.</param>
    /// <returns>true if a player with the specified name was found; otherwise, false.</returns>
    [Obsolete("This method is deprecated and will be removed in a future version. Use TryFindByName(string, out Player) instead.")]
    public static bool TryFindByName(FixedString64Bytes name, out PlayerData playerData)
    {
        return old_namePlayerCache.TryGetValue(name.ToString(), out playerData);
    }

    /// <summary>
    /// Attempts to find a player by their character name using a FixedString64Bytes.
    /// </summary>
    /// <param name="name">The character name as FixedString64Bytes to search for.</param>
    /// <param name="player">When this method returns, contains the player associated with the specified name, if found; otherwise, null.</param>
    /// <returns>true if a player with the specified name was found; otherwise, false.</returns>
    public static bool TryFindByName(FixedString64Bytes name, out Player player)
    {
        return namePlayerCache.TryGetValue(name.ToString(), out player);
    }

    /// <summary>
    /// Attempts to find a user entity by their network ID.
    /// </summary>
    /// <param name="networkId">The network ID to search for.</param>
    /// <param name="userEntity">When this method returns, contains the user entity associated with the specified network ID, if found; otherwise, Entity.Null.</param>
    /// <returns>true if a user entity with the specified network ID was found; otherwise, false.</returns>
    [Obsolete("This method is deprecated and will be removed in a future version. Use TryFindByNetworkId(networkId, out Player) instead.")]
    public static bool TryFindByNetworkId(NetworkId networkId, out Entity userEntity)
    {
        if (old_idPlayerCache.TryGetValue(networkId, out var playerData))
        {
            userEntity = playerData.UserEntity;
            return true;
        }
        userEntity = Entity.Null;
        return false;
    }

    /// <summary>
    /// Attempts to find a player by their network ID.
    /// </summary>
    /// <param name="networkId">The network ID to search for.</param>
    /// <param name="player">When this method returns, contains the player associated with the specified network ID, if found; otherwise, null.</param>
    /// <returns>true if a player with the specified network ID was found; otherwise, false.</returns>
    public static bool TryFindByNetworkId(NetworkId networkId, out Player player)
    {
        if (idPlayerCache.TryGetValue(networkId, out var playerData))
        {
            player = playerData;
            return true;
        }

        player = null;
        return false;
    }

    /// <summary>
    /// Creates a new Player instance from the provided PlayerData.
    /// </summary>
    /// <param name="data">The PlayerData containing the player information.</param>
    /// <returns>A new Player instance initialized with the character entity from the PlayerData.</returns>
    public static Player PlayerFromData(PlayerData data)
    {
        return PlayerFromChar(data.CharEntity);
    }

    /// <summary>
    /// Creates a new Player instance from the provided character entity.
    /// </summary>
    /// <param name="entity">The character entity to create the player from.</param>
    /// <returns>A new Player instance initialized with the specified character entity.</returns>
    public static Player PlayerFromChar(Entity entity)
    {
        Player player;
        player = new Player
        {
            Character = entity
        };
        return player;
    }

    /// <summary>
    /// Creates a new Player instance from the provided User.
    /// </summary>
    /// <param name="user">The User to create the player from.</param>
    /// <returns>A new Player instance initialized with the user's local character entity.</returns>
    public static Player PlayerFromUser(User user)
    {
        return PlayerFromChar(user.LocalCharacter._Entity);
    }
}