using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VAMP.Utilities;

namespace VAMP.Services;

public class CastleHeartService
{
    public static EntityQuery CastleHeartQuery;

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