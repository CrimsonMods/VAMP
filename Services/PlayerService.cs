using ProjectM.Network;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using VAMP.Models;
using VAMP.Utilities;

namespace VAMP.Services;

public class PlayerService
{
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
            Player player = new Player();
            player.User = entity;

            namePlayerCache.TryAdd(player.Name, player);
            steamPlayerCache.TryAdd(player.SteamID, player);
        }

        Plugin.LogInstance.LogWarning($"Player Cache Created with {namePlayerCache.Count} entries total.");
    }

    internal static void UpdatePlayerCache(Entity userEntity, string oldName, string newName)
    {
        var userData = userEntity.Read<User>();
        namePlayerCache.Remove(oldName);

        var player = PlayerFromUser(userData);

        namePlayerCache[newName] = player;
        steamPlayerCache[userData.PlatformId] = player;
        idPlayerCache[userEntity.Read<NetworkId>()] = player;
    }

    /// <summary>
    /// Gets all cached users from the player cache.
    /// </summary>
    /// <returns>An enumerable collection of all cached user entities.</returns>
    public static IEnumerable<Entity> GetAllCachedUsers()
    {
        foreach (var pd in namePlayerCache.Values.ToArray())
        {
            yield return pd.User;
        }
    }

    /// <summary>
    /// Gets all cached users from the player cache.
    /// </summary>
    /// <returns>An enumerable collection of all cached users as Player objects.</returns>
    public static IEnumerable<Player> GetAllCachedUsersAsPlayer()
    {
        foreach (var pd in namePlayerCache.Values.ToArray())
        {
            yield return pd;
        }
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
    /// <param name="player">When this method returns, contains the player associated with the specified name, if found; otherwise, null.</param>
    /// <returns>true if a player with the specified name was found; otherwise, false.</returns>
    public static bool TryFindByName(FixedString64Bytes name, out Player player)
    {
        return namePlayerCache.TryGetValue(name.ToString(), out player);
    }

    /// <summary>
    /// Attempts to find a player by their network ID.
    /// </summary>
    /// <param name="networkId">The network ID to search for.</param>
    /// <param name="player">When this method returns, contains the player associated with the specified network ID, if found; otherwise, null.</param>
    /// <returns>true if a player with the specified network ID was found; otherwise, false.</returns>
    public static bool TryFindByNetworkId(NetworkId networkId, out Player player)
    {
        return idPlayerCache.TryGetValue(networkId, out player);
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
