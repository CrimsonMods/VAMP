using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VAMP.Utilities;

namespace VAMP.Services;

public class ClanService
{
    public static EntityQuery ClansQuery;

    public ClanService()
    {
        var componentTypes = new ComponentType[] { ComponentType.ReadOnly<ClanTeam>() };
        ClansQuery = Core.EntityManager.CreateEntityQuery(componentTypes);
    }

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

    public static Entity GetByName(string clanName)
    {
        var clans = GetAll().Where(x => x.ReadBuffer<SyncToUserBuffer>().Length > 0).ToList();
        Entity clanEntity = clans.FirstOrDefault(entity => entity.Read<ClanTeam>().Name.Value.ToLower() == clanName.ToLower());
        return clanEntity;
    }

    public static Entity GetByGUID(Il2CppSystem.Guid guid)
    {
        Entity clanEntity = GetAll().FirstOrDefault(entity => entity.Read<ClanTeam>().ClanGuid == guid);
        return clanEntity != Entity.Null ? clanEntity : Entity.Null;
    }

    public static Entity GetByNetworkId(NetworkId networkId)
    {
        Entity clanEntity = GetAll().Where(x => x.Has<NetworkId>()).FirstOrDefault(entity => entity.Read<NetworkId>() == networkId);
        return clanEntity != Entity.Null ? clanEntity : Entity.Null;
    }
}