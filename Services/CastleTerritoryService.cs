using System.Collections.Generic;
using ProjectM.CastleBuilding;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VAMP.Utilities;

namespace VAMP.Services
{
    public class CastleTerritoryService
    {
        const float BLOCK_SIZE = 10;
        Dictionary<int2, int> blockCoordToTerritoryIndex = [];
        Dictionary<int2, Entity> blockCoordToTerritory = [];

        public CastleTerritoryService()
        {
            var entities = EntityUtil.GetEntitiesByComponentType<CastleTerritory>(true);
            foreach (var castleTerritory in entities)
            {
                var castleTerritoryIndex = castleTerritory.Read<CastleTerritory>().CastleTerritoryIndex;
                var ctb = Core.EntityManager.GetBuffer<CastleTerritoryBlocks>(castleTerritory);
                for (int i = 0; i < ctb.Length; i++)
                {
                    blockCoordToTerritoryIndex[ctb[i].BlockCoordinate] = castleTerritoryIndex;
                    blockCoordToTerritory[ctb[i].BlockCoordinate] = castleTerritory;
                }
            }
            entities.Dispose();
        }

        public bool TryGetCastleTerritory(Entity entity, out Entity territoryEntity)
        {
            if (entity.Has<Translation>())
            {
                var position = entity.Read<Translation>().Value;
                var blockCoord = ConvertPosToBlockCoord(position);
                if (blockCoordToTerritory.TryGetValue(blockCoord, out territoryEntity))
                {
                    return true;
                }

            }
            
            territoryEntity = default;
            return false;
        }

        public int GetTerritoryIndex(float3 pos)
        {
            var blockCoord = ConvertPosToBlockCoord(pos);
            if (blockCoordToTerritoryIndex.TryGetValue(blockCoord, out var index))
                return index;
            return -1;
        }

        public Entity GetHeartForTerritory(int territoryIndex)
        {
            if (territoryIndex == -1)
                return Entity.Null;
            var castleHearts = EntityUtil.GetEntitiesByComponentType<CastleHeart>();
            foreach (var heart in castleHearts)
            {
                var heartData = heart.Read<CastleHeart>();
                var castleTerritoryEntity = heartData.CastleTerritoryEntity;
                if (castleTerritoryEntity.Equals(Entity.Null))
                    continue;
                var heartTerritoryIndex = castleTerritoryEntity.Read<CastleTerritory>().CastleTerritoryIndex;
                if (heartTerritoryIndex == territoryIndex)
                    return heart;
            }
            castleHearts.Dispose();
            return Entity.Null;
        }

        public static float3 ConvertPosToGrid(float3 pos)
        {
            return new float3(Mathf.FloorToInt(pos.x * 2) + 6400, pos.y, Mathf.FloorToInt(pos.z * 2) + 6400);
        }

        public static int2 ConvertPosToBlockCoord(float3 pos)
        {
            var gridPos = ConvertPosToGrid(pos);
            return new int2((int)math.floor(gridPos.x / BLOCK_SIZE), (int)math.floor(gridPos.z / BLOCK_SIZE));
        }
    }
}