
# Clan Service Documentation

The ClanService is a core system in VAMP that manages clan-related operations and provides efficient lookup functionality for V Rising clans. It handles clan entity queries and provides utilities for managing clan memberships.

## Key Features

- Efficient clan entity lookups using multiple methods
- Clan member management and role verification
- Support for retrieving clan leaders and members
- Entity-based clan data handling

## Usage
### Finding Clans
```csharp
// Get all clans
foreach (var clanEntity in ClanService.GetAll())
{
    // Access clan information
    var clanTeam = clanEntity.Read<ClanTeam>();
    Console.WriteLine($"Found clan: {clanTeam.Name}");
}

// Find a clan by name
Entity clanEntity = ClanService.GetByName("Vampires United");
if (clanEntity != Entity.Null)
{
    // Work with the clan entity
    var clanTeam = clanEntity.Read<ClanTeam>();
    // Perform actions with the clan
}

// Find a clan by GUID
Il2CppSystem.Guid clanGuid = /* clan GUID */;
Entity clanEntity = ClanService.GetByGUID(clanGuid);

// Find a clan by Network ID
NetworkId networkId = /* network ID */;
Entity clanEntity = ClanService.GetByNetworkId(networkId);
```

### Working with Clan Members
```csharp
// Get clan leader
Entity clanEntity = /* get clan entity */;
User leader = ClanService.GetClanLeader(clanEntity);
Player leaderPlayer = ClanService.GetClanLeaderAsPlayer(clanEntity);

// Get all clan members
foreach (var member in ClanService.GetClanMembers(clanEntity))
{
    Console.WriteLine($"Clan member: {member.CharacterName}");
}

// Get all clan members as players
foreach (var player in ClanService.GetClanMembersAsPlayers(clanEntity))
{
    if (player.Level >= 30)
    {
        // Perform actions with each clan member
    }
}

// Check if a user is a clan leader
User user = /* get user */;
bool isLeader = ClanService.IsClanLeader(user);
```

## Important Notes
- The service initializes automatically when needed
- Clan lookups by name are case-insensitive
- Entity queries are properly disposed to prevent memory leaks
- The service maintains data for both clan entities and their members
- Clan member roles are checked through the ClanRole component

## Integration with Other Systems
The ClanService works seamlessly with other VAMP systems:

- Player Service: Convert clan members between User and Player entities
- Permission System: Verify clan leadership roles
- Command System: Handle clan-based commands
- Chat System: Target specific clan members

By using the ClanService for all clan-related operations, you ensure consistent and efficient clan data handling throughout your server.
