using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VAMP.Models;
using VAMP.Utilities;

namespace VAMP.Services;

public class ClanService
{
    /// <summary>
    /// Entity query for retrieving clan entities.
    /// </summary>
    public static EntityQuery ClansQuery;

    /// <summary>
    /// Initializes a new instance of the ClanService class.
    /// </summary>
    public ClanService()
    {
        var componentTypes = new ComponentType[] { ComponentType.ReadOnly<ClanTeam>() };
        ClansQuery = Core.EntityManager.CreateEntityQuery(componentTypes);
    }

    /// <summary>
    /// Gets all clan entities in the game.
    /// </summary>
    /// <returns>An enumerable collection of clan entities.</returns>
    public static IEnumerable<Entity> GetAll()
    {
        NativeArray<Entity> clanEntities = ClansQuery.ToEntityArray(Allocator.Temp);
        try
        {
            foreach (Entity entity in clanEntities)
            {
                if (entity.Exists())
                {
                    yield return entity;
                }
            }
        }
        finally
        {
            clanEntities.Dispose();
        }
    }

    /// <summary>
    /// Gets a clan entity by its name.
    /// </summary>
    /// <param name="clanName">The name of the clan to find.</param>
    /// <returns>The clan entity if found; otherwise, null.</returns>
    [Obsolete("Use GetAllWithName instead as Clan names are not unique.")]
    public static Entity GetByName(string clanName)
    {
        var clans = GetAll().Where(x => x.ReadBuffer<SyncToUserBuffer>().Length > 0).ToList();
        Entity clanEntity = clans.FirstOrDefault(entity => entity.Read<ClanTeam>().Name.Value.ToLower() == clanName.ToLower());
        return clanEntity;
    }

    /// <summary>
    /// Gets all clan entities with the specified name.
    /// </summary>
    /// <param name="clanName">The name of the clans to find.</param>
    /// <returns>An enumerable collection of clan entities with the specified name.</returns>
    public static IEnumerable<Entity> GetAllWithName(string clanName)
    {
        return GetAll().Where(x => x.Read<ClanTeam>().Name.Value.ToLower() == clanName.ToLower());
    }

    /// <summary>
    /// Gets a clan entity by its GUID.
    /// </summary>
    /// <param name="guid">The GUID of the clan to find.</param>
    /// <returns>The clan entity if found; otherwise, Entity.Null.</returns>
    public static Entity GetByGUID(Il2CppSystem.Guid guid)
    {
        Entity clanEntity = GetAll().FirstOrDefault(entity => entity.Read<ClanTeam>().ClanGuid == guid);
        return clanEntity != Entity.Null ? clanEntity : Entity.Null;
    }

    /// <summary>
    /// Gets a clan entity by its network ID.
    /// </summary>
    /// <param name="networkId">The network ID of the clan to find.</param>
    /// <returns>The clan entity if found; otherwise, Entity.Null.</returns>
    public static Entity GetByNetworkId(NetworkId networkId)
    {
        Entity clanEntity = GetAll().Where(x => x.Has<NetworkId>()).FirstOrDefault(entity => entity.Read<NetworkId>() == networkId);
        return clanEntity != Entity.Null ? clanEntity : Entity.Null;
    }

    #region Membership Utility

    /// <summary>
    /// Gets the index of the clan leader in the clan member buffer.
    /// </summary>
    /// <param name="clanBuffer">The buffer containing clan member statuses.</param>
    /// <returns>The index of the clan leader; -1 if not found.</returns>
    public static int GetClanLeaderIndex(DynamicBuffer<ClanMemberStatus> clanBuffer)
    {
        for (int i = 0; i < clanBuffer.Length; i++)
        {
            if (clanBuffer[i].ClanRole.Equals(ClanRoleEnum.Leader))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Gets the User entity of the clan leader.
    /// </summary>
    /// <param name="clanEntity">The clan entity to check.</param>
    /// <returns>The User entity of the clan leader.</returns>
    public static User GetClanLeader(Entity clanEntity)
    {
        var clanBuffer = clanEntity.ReadBuffer<ClanMemberStatus>();
        int leaderIndex = GetClanLeaderIndex(clanBuffer);

        var userBuffer = clanEntity.ReadBuffer<SyncToUserBuffer>();
        User leaderUser = userBuffer[leaderIndex].UserEntity.Read<User>();
        return leaderUser;
    }

    /// <summary>
    /// Gets the Player entity of the clan leader.
    /// </summary>
    /// <param name="clanEntity">The clan entity to check.</param>
    /// <returns>The Player entity of the clan leader.</returns>
    public static Player GetClanLeaderAsPlayer(Entity clanEntity)
    {
        return PlayerService.PlayerFromUser(GetClanLeader(clanEntity));
    }

    /// <summary>
    /// Gets all clan members as User entities.
    /// </summary>
    /// <param name="clanEntity">The clan entity to check.</param>
    /// <returns>An enumerable collection of User entities.</returns>
    public static IEnumerable<User> GetClanMembers(Entity clanEntity)
    {
        var userBuffer = clanEntity.ReadBuffer<SyncToUserBuffer>();
        foreach (var userEntity in userBuffer)
        {
            yield return userEntity.UserEntity.Read<User>();
        }
    }

    /// <summary>
    /// Gets all clan members as Player entities.
    /// </summary>
    /// <param name="clanEntity">The clan entity to check.</param>
    /// <returns>An enumerable collection of Player entities.</returns>
    public static IEnumerable<Player> GetClanMembersAsPlayers(Entity clanEntity)
    {
        foreach (var user in GetClanMembers(clanEntity))
        {
            yield return PlayerService.PlayerFromUser(user);
        }
    }

    /// <summary>
    /// Checks if a user is a clan leader.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>True if the user is a clan leader; otherwise, false.</returns>
    public static bool IsClanLeader(User user)
    {
        if (user.ClanEntity.Equals(Entity.Null)) return false;

        Player player = PlayerService.PlayerFromUser(user);
        return player.User.TryGetComponent(out ClanRole clanRole) && !clanRole.Value.Equals(ClanRoleEnum.Leader);
    }

    #endregion
}