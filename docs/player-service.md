# Player Service Documentation

The PlayerService is a core system in VAMP that manages player data caching and provides efficient lookup functionality for V Rising servers. It maintains multiple caches to optimize player lookups by different identifiers.

## Key Features 

- Efficient player data caching using multiple lookup methods
- Automatic cache maintenance when players join, leave, or change names
- Thread-safe player lookups
- Support for both online and offline player data retrieval

## Cache Structure
The service maintains three separate caches:

- Name-based cache: Quick lookups by character name
- Steam ID-based cache: Lookups by platform identifier
- Network ID-based cache: Lookups by in-game network identifier

## Usage
### Finding Players
```csharp
// Find a player by their character name
if (PlayerService.TryFindByName("Dracula", out var playerData))
{
    // Access player information
    var characterEntity = playerData.CharacterEntity;
    var steamId = playerData.SteamID;
    var isOnline = playerData.IsOnline;
    
    // Do something with the player
    Console.WriteLine($"Found player {playerData.CharacterName} with Steam ID {steamId}");
}

// Find a player by their Steam ID
ulong steamId = 76561198012345678;
if (PlayerService.TryFindBySteam(steamId, out var player))
{
    // Send a message to the player
    ChatUtil.SendSystemMessage(player.UserEntity, "Hello vampire!");
}
```

### Working with Online Players
```csharp
// Get all online players
foreach (var userEntity in PlayerService.GetCachedUsersOnline())
{
    var userData = userEntity.Read<User>();
    Console.WriteLine($"Player {userData.CharacterName} is online");
    
    // Perform an action for each online player
    // For example, give them a buff, item, etc.
}

// Count online players
int onlineCount = PlayerService.GetCachedUsersOnline().Count();
Console.WriteLine($"There are {onlineCount} players online");
```

### Updating Player Information
The service automatically updates when players join or leave, but you can also manually update the cache:
```csharp
// Update a player's name in the cache
Entity userEntity = /* get user entity */;
string oldName = "OldVampireName";
string newName = "NewVampireName";
PlayerService.UpdatePlayerCache(userEntity, oldName, newName);

// Mark a player as offline in the cache
PlayerService.UpdatePlayerCache(userEntity, oldName, oldName, forceOffline: true);
```

## Important Notes
- The service initializes automatically when the server starts, caching all existing player data
- Player lookups are case-sensitive
- The cache is automatically maintained, but manual updates may be necessary in specific scenarios
- For performance reasons, use the cached methods when possible instead of querying the entity system directly
- The service maintains data for both online and offline players

## Integration with Other Systems
The PlayerService works seamlessly with other VAMP systems:

- Event Scheduler: Check player counts for event triggers
- Command System: Identify command issuers by name or ID
- Permission System: Associate permissions with specific players
- Chat System: Target specific players for messages
  
By using the PlayerService for all player-related operations, you ensure consistent and efficient player data handling throughout your server.