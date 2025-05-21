using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Shared;
using ProjectM.Tiles;
using Stunlock.Core;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAMP.Data;
using VAMP.Services;

namespace VAMP.Utilities;
//#pragma warning disable CS8500
public static class EntityUtil
{
    #region ECS Extensions
    /// <summary>
    /// Writes component data to an entity using raw memory operations.
    /// </summary>
    /// <typeparam name="T">The type of component data to write.</typeparam>
    /// <param name="entity">The target entity to write the component data to</param>
    /// <param name="componentData">The component data to write</param>
    /// <remarks>
    /// This method uses unsafe code to directly manipulate memory for efficient component data writing.
    /// The component data is marshaled to a byte array before being written to the entity.
    /// </remarks>
    /// <example>
    /// entity.Write(new MyComponentData { value = 42 });
    /// </example>
    public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
    {
        // Get the ComponentType for T
        var ct = new ComponentType(Il2CppType.Of<T>());

        // Marshal the component data to a byte array
        byte[] byteArray = StructureToByteArray(componentData);

        // Get the size of T
        int size = Marshal.SizeOf<T>();

        // Create a pointer to the byte array
        fixed (byte* p = byteArray)
        {
            // Set the component data
            Core.EntityManager.SetComponentDataRaw(entity, ct.TypeIndex, p, size);
        }
    }

    /// <summary>
    /// Reads component data from an entity using raw memory operations.
    /// </summary>
    /// <typeparam name="T">The type of component data to read. Must be a struct.</typeparam>
    /// <param name="entity">The target entity to read the component data from</param>
    /// <returns>The component data of type T read from the entity</returns>
    /// <remarks>
    /// This method uses unsafe code to directly access memory for efficient component data reading.
    /// The raw component data is marshaled from memory into the specified struct type.
    /// </remarks>
    /// <example>
    /// var data = entity.Read<MyComponentData>();
    /// </example>
    public static byte[] StructureToByteArray<T>(T structure) where T : struct
    {
        int size = Marshal.SizeOf(structure);
        byte[] byteArray = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(structure, ptr, true);
        Marshal.Copy(ptr, byteArray, 0, size);
        Marshal.FreeHGlobal(ptr);

        return byteArray;
    }

    /// <summary>
    /// Reads component data from an entity using raw memory operations.
    /// </summary>
    /// <typeparam name="T">The type of component data to read. Must be a struct.</typeparam>
    /// <param name="entity">The target entity to read the component data from</param>
    /// <returns>The component data of type T read from the entity</returns>
    /// <remarks>
    /// This method uses unsafe code to directly access memory for efficient component data reading.
    /// The raw component data is marshaled from memory into the specified struct type.
    /// </remarks>
    /// <example>
    /// var data = entity.Read<MyComponentData>();
    /// </example>
    public unsafe static T Read<T>(this Entity entity) where T : struct
    {
        // Get the ComponentType for T
        var ct = new ComponentType(Il2CppType.Of<T>());

        // Get a pointer to the raw component data
        void* rawPointer = Core.EntityManager.GetComponentDataRawRO(entity, ct.TypeIndex);

        // Marshal the raw data to a T struct
        T componentData = Marshal.PtrToStructure<T>(new IntPtr(rawPointer));

        return componentData;
    }

    /// <summary>
    /// Gets a DynamicBuffer of components from an entity.
    /// </summary>
    /// <typeparam name="T">The type of buffer elements to read. Must be a struct.</typeparam>
    /// <param name="entity">The target entity to read the buffer from</param>
    /// <returns>A DynamicBuffer containing elements of type T</returns>
    /// <remarks>
    /// DynamicBuffers are useful for storing variable-length arrays of components on an entity.
    /// </remarks>
    /// <example>
    /// var buffer = entity.ReadBuffer<BufferElement>();
    /// foreach(var element in buffer) { /* Process element */ }
    /// </example>
    public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct
    {
        return Core.Server.EntityManager.GetBuffer<T>(entity);
    }

    /// <summary>
    /// Checks if an entity has a specific component type.
    /// </summary>
    /// <typeparam name="T">The type of component to check for</typeparam>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the entity has the component, false otherwise</returns>
    /// <example>
    /// if (entity.Has<HealthComponent>())
    /// {
    ///     // Process entity with health component
    /// }
    /// </example>
    public static bool Has<T>(this Entity entity)
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        return Core.EntityManager.HasComponent(entity, ct);
    }

    /// <summary>
    /// Adds a component of type T to the specified entity.
    /// </summary>
    /// <typeparam name="T">The type of component to add</typeparam>
    /// <param name="entity">The entity to add the component to</param>
    /// <remarks>
    /// Uses the EntityManager to add a new component instance to the entity.
    /// The component will be initialized with default values.
    /// </remarks>
    /// <example>
    /// entity.Add<HealthComponent>();
    /// </example>
    public static void Add<T>(this Entity entity)
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        Core.EntityManager.AddComponent(entity, ct);
    }

    /// <summary>
    /// Removes a component of type T from the specified entity.
    /// </summary>
    /// <typeparam name="T">The type of component to remove</typeparam>
    /// <param name="entity">The entity to remove the component from</param>
    /// <remarks>
    /// Uses the EntityManager to remove an existing component from the entity.
    /// Any data stored in the component will be lost upon removal.
    /// </remarks>
    /// <example>
    /// entity.Remove<HealthComponent>();
    /// </example>
    public static void Remove<T>(this Entity entity)
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        Core.EntityManager.RemoveComponent(entity, ct);
    }

    /// <summary>
    /// Attempts to get a component of type T from the entity.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve. Must be a struct.</typeparam>
    /// <param name="entity">The entity to get the component from</param>
    /// <param name="componentData">When this method returns, contains the component data if found, or the default value if not found</param>
    /// <returns>True if the component was found and retrieved, false otherwise</returns>
    /// <remarks>
    /// This method provides a safe way to retrieve components without throwing exceptions.
    /// </remarks>
    /// <example>
    /// if (entity.TryGetComponent&lt;HealthComponent&gt;(out var health))
    /// {
    ///     // Work with health component
    /// }
    /// </example>
    public static bool TryGetComponent<T>(this Entity entity, out T componentData) where T : struct
    {
        componentData = default;
        if (entity.Has<T>())
        {
            componentData = entity.Read<T>();
            return true;
        }
        return false;
    }

    #endregion
    #region ECS Utility
    /// <summary>
    /// Destroys an entity with proper cleanup and logging.
    /// </summary>
    /// <param name="entity">The entity to destroy</param>
    /// <remarks>
    /// This method follows the recommended destruction pattern:
    /// 1. Disables the entity
    /// 2. Creates a destroy event for tracking
    /// 3. Performs the actual entity destruction
    /// </remarks>
    /// <example>
    /// someEntity.DestroyWithReason();
    /// </example>
    public static void DestroyWithReason(this Entity entity)
    {
        Core.EntityManager.AddComponent<Disabled>(entity);
        DestroyUtility.CreateDestroyEvent(Core.EntityManager, entity, DestroyReason.Default, DestroyDebugReason.ByScript);
        DestroyUtility.Destroy(Core.EntityManager, entity);
    }

    /// <summary>
    /// Kills or destroys an entity based on game rules and conditions.
    /// </summary>
    /// <param name="entity">The entity to kill or destroy</param>
    /// <remarks>
    /// This method uses the StatChangeUtility to process the entity's death or destruction.
    /// The same entity is used for source, target, and instigator parameters.
    /// </remarks>
    /// <example>
    /// someEntity.KillOrDestroy();
    /// </example>
    public static void KillOrDestroy(Entity entity)
    {
        StatChangeUtility.KillOrDestroyEntity(Core.EntityManager, entity, entity, entity, 0, StatChangeReason.Any, true);
    }

    /// <summary>
    /// Checks if an entity currently exists in the game world.
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the entity exists, false if it has been destroyed or is invalid</returns>
    /// <example>
    /// if (entity.Exists())
    /// {
    ///     // Perform operations on the existing entity
    /// }
    /// </example>
    public static bool Exists(this Entity entity)
    {
        return Core.EntityManager.Exists(entity);
    }

    /// <summary>
    /// Checks if an entity is currently disabled.
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the entity has the Disabled component, false otherwise</returns>
    /// <example>
    /// if (entity.Disabled())
    /// {
    ///     // Handle disabled entity logic
    /// }
    /// </example>
    public static bool Disabled(this Entity entity)
    {
        return entity.Has<Disabled>();
    }

    #endregion
    #region Entity Query
    /// <summary>
    /// Gets entities that have a specific component type.
    /// </summary>
    /// <typeparam name="T1">The component type to query for</typeparam>
    /// <param name="includeAll">Include all entity states in the query</param>
    /// <param name="includeDisabled">Include disabled entities in the query</param>
    /// <param name="includeSpawn">Include entities with spawn tags in the query</param>
    /// <param name="includePrefab">Include prefab entities in the query</param>
    /// <param name="includeDestroyed">Include destroyed entities in the query</param>
    /// <returns>A NativeArray of entities matching the query criteria</returns>
    public static NativeArray<Entity> GetEntitiesByComponentType<T1>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
    {
        EntityQueryOptions options = EntityQueryOptions.Default;
        if (includeAll) options |= EntityQueryOptions.IncludeAll;
        if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
        if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
        if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
        if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

        EntityQueryDesc queryDesc = new()
        {
            All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
            Options = options
        };

        var query = Core.EntityManager.CreateEntityQuery(new[] { queryDesc });

        var entities = query.ToEntityArray(Allocator.Temp);
        return entities;
    }

    /// <summary>
    /// Gets entities that have two specific component types.
    /// </summary>
    /// <typeparam name="T1">The first component type to query for</typeparam>
    /// <typeparam name="T2">The second component type to query for</typeparam>
    /// <param name="includeAll">Include all entity states in the query</param>
    /// <param name="includeDisabled">Include disabled entities in the query</param>
    /// <param name="includeSpawn">Include entities with spawn tags in the query</param>
    /// <param name="includePrefab">Include prefab entities in the query</param>
    /// <param name="includeDestroyed">Include destroyed entities in the query</param>
    /// <returns>A NativeArray of entities matching the query criteria</returns>
    public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
    {
        EntityQueryOptions options = EntityQueryOptions.Default;
        if (includeAll) options |= EntityQueryOptions.IncludeAll;
        if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
        if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
        if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
        if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

        EntityQueryDesc queryDesc = new()
        {
            All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite), new(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite) },
            Options = options
        };

        var query = Core.EntityManager.CreateEntityQuery(new[] { queryDesc });

        var entities = query.ToEntityArray(Allocator.Temp);
        return entities;
    }

    /// <summary>
    /// Gets entities that have a specific component type using custom query options.
    /// </summary>
    /// <typeparam name="T1">The component type to query for</typeparam>
    /// <param name="queryOptions">Custom entity query options to apply</param>
    /// <returns>A NativeArray of entities matching the query criteria</returns>
    public static NativeArray<Entity> GetEntitiesByComponentTypes<T1>(EntityQueryOptions queryOptions = default)
    {
        EntityQueryDesc queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
            Options = queryOptions
        };

        var query = Core.EntityManager.CreateEntityQuery(new[] { queryDesc });
        var entities = query.ToEntityArray(Allocator.Temp);
        query.Dispose();
        return entities;
    }

    /// <summary>
    /// Gets entities within a specified area and of a specific tile type.
    /// </summary>
    /// <param name="area">The bounds to search within</param>
    /// <param name="tileType">The type of tile to search for</param>
    /// <returns>A NativeArray of entities matching the spatial and tile type criteria</returns>
    public unsafe static NativeArray<Entity> GetEntitiesInArea(BoundsMinMax area, TileType tileType)
    {
        var systemState = *Core.TileModelSpatialLookupSystem.Read<SystemInstance>().state;
        var tileModelSpatialLookupSystemData = TileModelSpatialLookupSystemData.Create(ref systemState);
        var spatialLookup = tileModelSpatialLookupSystemData.GetSpatialLookupAndComplete(ref systemState);
        spatialLookup.GetEntities(ref area, tileType);
        return spatialLookup.Results.ToArray(Allocator.Temp);
    }

    /// <summary>
    /// Gets an entity from its prefab GUID.
    /// </summary>
    /// <param name="guid">The prefab GUID to look up</param>
    /// <returns>The entity associated with the GUID</returns>
    public static Entity EntityFromGUID(PrefabGUID guid)
    {
        return Core.ServerScriptMapper._PrefabCollectionSystem._PrefabLookupMap[guid];
    }

    /// <summary>
    /// Attempts to get an entity from its prefab GUID.
    /// </summary>
    /// <param name="guid">The prefab GUID to look up</param>
    /// <param name="prefabEntity">The output entity if found</param>
    /// <returns>True if the entity was found, false otherwise</returns>
    public static bool TryGetEntityFromGUID(PrefabGUID guid, out Entity prefabEntity)
    {
        return Core.ServerScriptMapper._PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(guid, out prefabEntity);
    }

    /// <summary>
    /// Gets the prefab GUID of an entity.
    /// </summary>
    /// <param name="entity">The entity to get the GUID from</param>
    /// <returns>The prefab GUID of the entity, or PrefabGUID.Empty if not found</returns>
    public static PrefabGUID GetGUID(this Entity entity)
    {
        if (entity.Exists() && entity.Has<PrefabGUID>())
        {
            return entity.Read<PrefabGUID>();
        }

        return PrefabGUID.Empty;
    }
    #endregion
    #region Entity Extras
    /// <summary>
    /// Sorts an array of entities by their distance from a given position.
    /// </summary>
    /// <param name="entities">The array of entities to sort</param>
    /// <param name="position">The reference position to measure distances from</param>
    /// <returns>The sorted array of entities</returns>
    public static NativeArray<Entity> SortEntitiesByDistance(NativeArray<Entity> entities, float3 position)
    {
        (Entity entity, float distance)[] tempArray = new (Entity, float)[entities.Length];

        for (int i = 0; i < entities.Length; i++)
        {
            float distance = float.MaxValue;
            if (entities[i].Has<LocalToWorld>())
            {
                LocalToWorld ltw = entities[i].Read<LocalToWorld>();
                distance = math.distance(position, ltw.Position);
            }

            tempArray[i] = (entities[i], distance);
        }

        Array.Sort(tempArray, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < entities.Length; i++)
        {
            entities[i] = tempArray[i].entity;
        }

        return entities;
    }

    /// <summary>
    /// Checks if an entity is within a castle territory and determines the alignment relationship.
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <param name="territory">The output territory entity if found</param>
    /// <param name="territoryAlignment">The alignment relationship (Friendly, Enemy, or Neutral)</param>
    /// <param name="requireRoom">If true, also checks if the entity is within an enclosed room</param>
    /// <returns>True if the entity is in a territory, false otherwise</returns>
    public static bool IsInBase(Entity entity, out Entity territory, out TerritoryAlignment territoryAlignment, bool requireRoom = false)
    {
        territoryAlignment = TerritoryAlignment.None;
        if (CastleTerritoryService.TryGetCastleTerritory(entity, out territory))
        {
            var heart = territory.Read<CastleTerritory>().CastleHeart;
            if (heart.Exists())
            {
                if (Team.IsAllies(heart.Read<Team>(), entity.Read<Team>()))
                {
                    territoryAlignment = TerritoryAlignment.Friendly;
                }
                else
                {
                    territoryAlignment = TerritoryAlignment.Enemy;
                }
            }
            else
            {
                territoryAlignment = TerritoryAlignment.Neutral;
            }

            if (!requireRoom)
            {
                return true;
            }
            else
            {
                if (TryGetFloorEntityBelowEntity(entity, out var floorEntity) && floorEntity.Has<CastleRoomConnection>())
                {
                    return floorEntity.Read<CastleRoomConnection>().RoomEntity._Entity.Read<CastleRoom>().IsEnclosedRoom;
                }
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to find the floor entity directly below a given entity.
    /// </summary>
    /// <param name="entity">The entity to check below</param>
    /// <param name="floorEntity">The output floor entity if found</param>
    /// <returns>True if a floor entity was found, false otherwise</returns>
    public static bool TryGetFloorEntityBelowEntity(Entity entity, out Entity floorEntity)
    {
        floorEntity = default;
        var entityTilePosition = entity.Read<TilePosition>();

        var entitiesInArea = GetEntitiesInArea(entity.Read<TileBounds>().Value, TileType.Pathfinding);
        foreach (var entityInArea in entitiesInArea)
        {
            var floorTilePosition = entityInArea.Read<TilePosition>();
            if (floorTilePosition.HeightLevel != entityTilePosition.HeightLevel)
            {
                continue;
            }
            if (entityInArea.Has<CastleFloor>() && !entityInArea.Has<CastleStairs>()) //ignoring hearts and stairs
            {
                if (entityInArea.Read<TileBounds>().Value.Contains(entityTilePosition.Tile))
                {
                    floorEntity = entityInArea;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the hovered tile model entity of a specific type for a user.
    /// </summary>
    /// <typeparam name="T">The component type to filter for</typeparam>
    /// <param name="User">The user entity performing the hover</param>
    /// <returns>The hovered entity if it matches the type, or Entity.Null if none found</returns>
    public static Entity GetHoveredTileModel<T>(Entity User)
    {
        var input = User.Read<EntityInput>();
        var position = input.AimPosition;
        var aimTilePosition = CastleTerritoryService.ConvertPosToBlockCoord(position);
        if (input.HoveredEntity.Exists() && input.HoveredEntity.Has<T>())
        {
            return input.HoveredEntity;
        }
        else
        {
            var area = new BoundsMinMax
            {
                Min = new int2((int)aimTilePosition.x - 1, (int)aimTilePosition.y - 1),
                Max = new int2((int)aimTilePosition.x + 1, (int)aimTilePosition.y + 1)
            };
            var entities = GetEntitiesInArea(area, TileType.All);
            if (entities.Length > 0)
            {
                SortEntitiesByDistance(entities, position);
                foreach (var entity in entities)
                {
                    if (entity.Has<T>())
                    {
                        return entity;
                    }
                }
            }
        }
        return Entity.Null;
    }
    #endregion
}
//#pragma warning restore CS8500
