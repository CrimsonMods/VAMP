using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VAMP.Utilities;

namespace VAMP.Services;

/// <summary>
/// Service for managing and querying castle heart entities in the game.
/// </summary>
public class CastleHeartService
{
    /// <summary>
    /// Query for filtering castle heart entities.
    /// </summary>
    public static EntityQuery CastleHeartQuery;

    /// <summary>
    /// Initializes a new instance of the CastleHeartService class.
    /// Sets up the entity query for castle hearts with required components.
    /// VAMP Core will perform this method on startup. 
    /// </summary>
    public CastleHeartService()
    {
        CastleHeartQuery = Core.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<CastleHeart>(),
                ComponentType.ReadOnly<Team>(),
            },
        });
    }

    /// <summary>
    /// Attempts to find a castle heart entity by its network ID.
    /// </summary>
    /// <param name="castleId">The network ID of the castle to find.</param>
    /// <param name="castle">When this method returns, contains the castle entity if found; otherwise, Entity.Null.</param>
    /// <returns>True if the castle was found; otherwise, false.</returns>
    public static bool TryGetByID(NetworkId castleId, out Entity castle)
    {
        var castleEntities = CastleHeartQuery.ToEntityArray(Allocator.Temp);
        foreach (var castleEntity in castleEntities)
        {
            var networkId = castleEntity.Read<NetworkId>();
            if (networkId.Equals(castleId))
            {
                castle = castleEntity;
                castleEntities.Dispose();
                return true;
            }
        }
        castleEntities.Dispose();
        castle = Entity.Null;
        return false;
    }

    /// <summary>
    /// Attempts to find a castle heart entity owned by a specific user.
    /// </summary>
    /// <param name="user">The user whose castle heart should be found.</param>
    /// <param name="castle">When this method returns, contains the castle entity if found; otherwise, Entity.Null.</param>
    /// <returns>True if the castle was found; otherwise, false.</returns>
    public static bool TryGetByOwnerUser(User user, out Entity castle)
    {
        var castleEntities = CastleHeartQuery.ToEntityArray(Allocator.Temp);
        
        foreach(var castleEntity in castleEntities)
        {
            if(!castleEntity.Exists()) continue;
            if(castleEntity.Has<UserOwner>())
            {
                var userOwner = castleEntity.Read<UserOwner>();
                if(!userOwner.Owner._Entity.Exists()) continue;
                
                var tUser = userOwner.Owner._Entity.Read<User>();

                if(tUser.Equals(user))
                {
                    castle = castleEntity;
                    castleEntities.Dispose();
                    return true;
                }
            }
        }

        castleEntities.Dispose();
        castle = Entity.Null;
        return false;
    }
}