  # Castle Territory Service

  The Castle Territory Service provides functionality for managing and querying castle territory (plot) entities in the game. This service handles territory-related operations and spatial queries.

  ## Features

  - Query castle territories by position and entity
  - Get territory indices for world positions
  - Find castle hearts for territories
  - Convert world positions to territory grid coordinates

  ## Usage

  ```csharp
  // Get territory for an entity
  Entity entity = /* some entity */;
  if (CastleTerritoryService.TryGetCastleTerritory(entity, out Entity territory))
  {
      // Territory found, work with territory entity
  }

  // Get territory index for a position
  float3 position = /* world position */;
  int territoryIndex = CastleTerritoryService.GetTerritoryIndex(position);

  // Get castle heart for territory
  Entity heart = CastleTerritoryService.GetHeartForTerritory(territoryIndex);
  ```

  ## Components

  The service works with the following key components:

  - `CastleTerritory` - Identifies territory entities
  - `CastleTerritoryBlocks` - Contains block data for territories
  - `CastleHeart` - Links territories to castle hearts
  - `Translation` - Position component for spatial queries

  ## Methods

  ### TryGetCastleTerritory
  Attempts to get the castle territory entity for a given entity based on its position.

  ### GetTerritoryIndex
  Gets the territory index for a given world position.

  ### GetHeartForTerritory
  Gets the castle heart entity for a given territory index.

  ### ConvertPosToGrid
  Converts a world position to a grid position.

  ### ConvertPosToBlockCoord
  Converts a world position to a block coordinate.
