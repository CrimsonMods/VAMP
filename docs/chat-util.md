# Chat Utility Documentation

The ChatUtil is a utility class in VAMP that provides methods for sending system messages to players in various ways. It offers functionality for targeted messaging, team communication, and local area broadcasts.

## Key Features

- Send system messages to specific players
- Broadcast messages to all online players
- Send messages to all players except specified ones
- Local area messaging within a radius
- Team-based communication for clan members
- Admin-only messaging
- Text color formatting support

## Usage

### Sending Messages to Players
```csharp
// Send a message to a specific player
User player = /* get user */;
ChatUtil.SystemSendUser(player, "Welcome to the server!");

// Send a message to all online players
ChatUtil.SystemSendAll("Server maintenance in 5 minutes!");

// Send a message to everyone except one player
ChatUtil.SystemSendAllExcept(player, "A new challenger approaches!");

// Send a message to everyone except multiple players
User[] excludedPlayers = /* array of users */;
ChatUtil.SystemSendAllExcept(excludedPlayers, "Event starting soon!");
```

### Local Area Messages
```csharp
// Send a message to players within 40 units
User sender = /* get user */;
float3 position = /* get position */;
ChatUtil.SystemSendLocal(sender, position, "Anyone want to trade?");
```

### Team Communication
```csharp
// Send a message to all clan members
User clanMember = /* get user */;
ChatUtil.SystemSendTeam(clanMember, "Clan meeting at the castle!");
```

### Admin Communication
```csharp
// Send a message to all online administrators
ChatUtil.SystemSendAdmins("Admin alert: Suspicious activity detected");
```

### Text Formatting
```csharp
// Color text using hex codes
string coloredText = ChatUtil.Color("Important!", "FF0000"); // Red text
string goldText = ChatUtil.Color("Legendary Item", "FFD700"); // Gold text
```

## Important Notes

1. Local messages have a fixed radius of 40 units
2. Team messages only work for players in clans
3. Color formatting uses Unity's rich text system
4. System messages are server-side only
5. Messages are limited to 512 bytes

## Integration with Other Systems

The ChatUtil works seamlessly with other VAMP systems:

- Player Service: For getting online players
- Event System: For broadcasting event notifications
- Command System: For command feedback
- Team System: For clan-based communication

By using ChatUtil for all chat-related operations, you ensure consistent message handling and formatting throughout your server.