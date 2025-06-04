
# Server Wipe System Documentation

The Server Wipe system in VAMP provides functionality to handle server wipes and mod data persistence between server sessions.

## Overview

The wipe system consists of:
- A dedicated `_WipeData` folder for mod persistence
- Automatic wipe detection on server startup
- Events to notify mods when wipes occur
- Configuration options for server admins

## For Server Admins

### _WipeData Folder

The system creates a `_WipeData` folder in your BepInEx config directory. This folder:
- Stores mod persistence data between server sessions
- Contains a README.txt explaining its purpose
- Is automatically cleaned during server wipes (if enabled)

### Configuration

In your VAMP.cfg file:
- `AutoWipeDetection`: Controls whether VAMP automatically deletes mod data during wipes
- `StartDateFile`: Path to the file containing the server start date

### Performing a Wipe

1. Stop your server
2. Delete your save data as normal
3. Start the server
4. VAMP will automatically:
   - Detect the fresh server state
   - Clear mod persistence data (if AutoWipeDetection is enabled)
   - Notify mods of the wipe

## For Modders

### Wipe Detection

VAMP detects wipes by checking:
1. The server start date file
2. If the date matches today
3. If there are no cached users

### Wipe Events

Subscribe to the wipe event to handle server wipes:

```csharp
Events.OnServerWiped += (autoWiped) => {
    if (autoWiped) {
        // VAMP automatically cleared _WipeData
        // Initialize fresh mod state
    } else {
        // Manual wipe handling required
        // Check/clear your mod's persistence files
    }
};
```

### Using _WipeData

Store persistence files in the `_WipeData` directory:

```csharp
string modDataPath = Path.Combine(FilePaths.WipeData, "mymod-data.json");

// Save data
File.WriteAllText(modDataPath, JsonSerializer.Serialize(data));

// Load data
if (File.Exists(modDataPath)) {
    data = JsonSerializer.Deserialize<MyData>(File.ReadAllText(modDataPath));
}
```

## Best Practices

1. Always handle both automatic and manual wipe scenarios
2. Use the `_WipeData` folder for any data that should persist between server sessions
3. Subscribe to `OnServerWiped` before `OnCoreLoaded` to ensure proper initialization
4. Include version information in your persistence files
5. Handle missing or corrupt persistence files gracefully
