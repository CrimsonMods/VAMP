
# Mod System Documentation

The Mod System in VAMP provides functionality to manage and track installed mods, their versions, and dependencies.

## Overview

The mod system consists of:
- Automatic mod detection and caching
- Thunderstore version checking
- Dependency tracking
- Mod status monitoring

## For Server Admins

### Loaded Mods

VAMP automatically detects and logs all installed mods on server startup. You can find:
- A list of installed mods in your server logs
- Each mod's version number
- Active/inactive status of mods
- Dependency relationships between mods

### Version Checking

The system automatically:
- Checks Thunderstore for latest mod versions
- Logs version information for installed mods
- Helps identify outdated mods

## For Modders

### Mod Detection

VAMP detects mods through:
1. BepInEx IL2CPP Chainloader
2. Assembly metadata
3. BepInPlugin attributes

### Accessing Mod Information

Query installed mods programmatically:

```csharp
// Check if a specific mod is loaded
bool isLoaded = ModSystem.IsModLoaded("MyMod.GUID");

// Get information about a specific mod
if (ModSystem.TryGetMod("MyMod.GUID", out PluginInfo modInfo)) {
    // Use modInfo.Metadata for version, name, etc
}

// Get list of all loaded mods
var loadedMods = ModSystem.GetLoadedModsInfo();
```

### Mod Metadata

In order for your mod to support automatic version checking, it must include the following assembly attributes:

```csharp
// The Authors parameter in your .csproj file must match your Thunderstore uploader name:
<Authors>YourThunderstoreUsername</Authors>
```

## Best Practices

1. Always provide unique GUIDs for your mods
2. Include proper assembly metadata for better mod tracking
3. Declare dependencies using BepInDependency attributes
4. Use semantic versioning for your mod versions
5. Keep your Thunderstore package name consistent with your mod author
