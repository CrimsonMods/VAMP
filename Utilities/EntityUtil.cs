using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Shared;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;

namespace VAMP.Utilities;
//#pragma warning disable CS8500
public static class EntityUtil
{
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

        var query = Core.EntityManager.CreateEntityQuery(queryDesc);

        var entities = query.ToEntityArray(Allocator.Temp);
        return entities;
    }

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

        var query = Core.EntityManager.CreateEntityQuery(queryDesc);

        var entities = query.ToEntityArray(Allocator.Temp);
        return entities;
    }
}
//#pragma warning restore CS8500
