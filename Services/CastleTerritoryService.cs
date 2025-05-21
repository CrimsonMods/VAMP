using System.Collections.Generic;
using ProjectM.CastleBuilding;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VAMP.Utilities;

namespace VAMP.Services;

/// <summary>
/// Service for managing castle territory-related (plot) operations and queries.
/// </summary>
public class CastleTerritoryService
{
    const float BLOCK_SIZE = 10;
    static Dictionary<int2, int> blockCoordToTerritoryIndex = [];
    static Dictionary<int2, Entity> blockCoordToTerritory = [];

    /// <summary>
    /// Initializes a new instance of the CastleTerritoryService class.
    /// Loads and maps all castle territories and their blocks.
    /// </summary>
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

    /// <summary>
    /// Attempts to get the castle territory entity for a given entity based on its position.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <param name="territoryEntity">The output territory entity if found.</param>
    /// <returns>True if a territory was found for the entity, false otherwise.</returns>
    public static bool TryGetCastleTerritory(Entity entity, out Entity territoryEntity)
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

    /// <summary>
    /// Gets the territory index for a given position.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    /// <returns>The territory index if found, -1 otherwise.</returns>
    public static int GetTerritoryIndex(float3 pos)
    {
        var blockCoord = ConvertPosToBlockCoord(pos);
        if (blockCoordToTerritoryIndex.TryGetValue(blockCoord, out var index))
            return index;
        return -1;
    }

    /// <summary>
    /// Gets the castle heart entity for a given territory index.
    /// </summary>
    /// <param name="territoryIndex">The territory index to search for.</param>
    /// <returns>The castle heart entity if found, Entity.Null otherwise.</returns>
    public static Entity GetHeartForTerritory(int territoryIndex)
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

    /// <summary>
    /// Converts a world position to a grid position.
    /// </summary>
    /// <param name="pos">The world position to convert.</param>
    /// <returns>The grid position.</returns>
    public static float3 ConvertPosToGrid(float3 pos)
    {
        return new float3(Mathf.FloorToInt(pos.x * 2) + 6400, pos.y, Mathf.FloorToInt(pos.z * 2) + 6400);
    }

    /// <summary>
    /// Converts a world position to a block coordinate.
    /// </summary>
    /// <param name="pos">The world position to convert.</param>
    /// <returns>The block coordinate as an int2.</returns>
    public static int2 ConvertPosToBlockCoord(float3 pos)
    {
        var gridPos = ConvertPosToGrid(pos);
        return new int2((int)math.floor(gridPos.x / BLOCK_SIZE), (int)math.floor(gridPos.z / BLOCK_SIZE));
    }
}