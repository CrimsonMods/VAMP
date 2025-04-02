# Spawn Service Documentation

The SpawnService provides functionality for spawning units in the game world with callback actions. It is primarily used for creating temporary units that need to perform specific actions after a set duration.

## Key Features

- Spawn units at specific coordinates
- Set duration for unit existence
- Execute callback actions after duration
- Automatic cleanup of expired units

## Usage

### Spawning a Temporary Unit

```csharp
// Spawn a unit that exists for 10 seconds then executes a callback
var position = new float3(100, 0, 100);
SpawnService.SpawnUnitWithCallback(
    unitPrefab,
    position,
    10.0f,
    (Entity spawnedEntity) => {
        // Add your callback logic here
        Console.WriteLine($"Unit {spawnedEntity} callback executed!");
    }
);
```

### Identifying Mod Spawned Units

By default, all units spawned through the SpawnService receive the `CanFly` component:

```csharp
SpawnService.SpawnUnitWithCallback(
    flyingUnitPrefab,
    position,
    duration: 0, // Permanent unit
    (Entity entity) => {
        // Unit will already have CanFly component
    }
);
```

## Important Methods

### SpawnUnitWithCallback

```csharp
public static void SpawnUnitWithCallback(
    PrefabGUID unit,
    float3 position,
    float duration,
    Action<Entity> postActions
)
```

Parameters:
- `unit`: The PrefabGUID of the unit to spawn
- `position`: The position in 3D space where the unit will be spawned
- `duration`: The duration in seconds before the callback is executed
- `postActions`: The callback action to execute after the duration

## Important Notes

1. Units with duration = 0 will be permanent (no automatic destruction)
2. All spawned units automatically receive the `CanFly` component
3. Callbacks are guaranteed to execute after the specified duration
4. The service uses a unique key system to track unit callbacks
5. Error handling is implemented to prevent key collisions

## Technical Details

The SpawnService works in conjunction with the UnitSpawnerPatch to manage unit lifecycles:

- Uses Unity's Entity Component System (ECS)
- Integrates with ProjectM's UnitSpawnerUpdateSystem
- Manages unit lifetime through LifeTime components
- Handles concurrent spawns through a dictionary-based tracking system
