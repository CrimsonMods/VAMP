
# Castle Territory Service

The Castle Territory Service provides functionality for managing and querying castle territories (plots) in the game world. It handles territory ownership, block coordinates, and castle heart relationships.

## Key Features

- Territory block coordinate mapping and lookups
- Castle heart entity tracking per territory
- Position-to-territory conversion utilities
- Territory alignment management

## Usage Examples

```csharp
// Get territory for an entity's position
Entity entity = /* ... */;
if (castleTerritoryService.TryGetCastleTerritory(entity, out var territoryEntity))
{
    // Entity is within a territory
}

// Check territory index at a position
float3 position = /* ... */;
int territoryIndex = castleTerritoryService.GetTerritoryIndex(position);

// Get castle heart for a territory
Entity heart = castleTerritoryService.GetHeartForTerritory(territoryIndex);
```

## Territory Alignment

Territories can have different alignments that determine their relationship to players:

- `Friendly` - Territory controlled by the player or allies
- `Enemy` - Territory controlled by hostile forces  
- `Neutral` - Territory not aligned with any faction
- `None` - Territory with no assigned alignment

## Position Conversion

The service provides utilities for converting between world positions and territory coordinates:

```csharp
// Convert world position to grid position
float3 gridPos = CastleTerritoryService.ConvertPosToGrid(worldPos);

// Convert world position to block coordinate
int2 blockCoord = CastleTerritoryService.ConvertPosToBlockCoord(worldPos);
```

## Implementation Details

The service maintains dictionaries mapping block coordinates to territory indices and entities. It initializes by loading all castle territories and their blocks on creation.

Territory blocks use a fixed size of 10 units and positions are converted to a grid system for consistent territory mapping.
