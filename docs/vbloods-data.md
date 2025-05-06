
# VBloods Data Documentation

The VBloods class is a static utility in VAMP that provides methods for converting V Blood boss PrefabGUIDs into their human-readable names. It maintains a dictionary of all V Blood bosses and offers both full and shortened name variants.

## Key Features

- Convert PrefabGUIDs to full V Blood boss names
- Convert PrefabGUIDs to shortened V Blood boss names
- Support for integer GUID conversion
- Customizable boss names via JSON configuration
- Complete list of all V Blood bosses in the game

## Usage

### Converting PrefabGUIDs to Names
```csharp
// Get full name of a V Blood boss
PrefabGUID bossId = /* get boss GUID */;
string fullName = VBloods.ToString(bossId); // Returns "Keely the Frost Archer"

// Get shortened name of a V Blood boss
string shortName = VBloods.ToShortString(bossId); // Returns "Keely"

// Convert from integer GUID
int guidValue = /* get GUID int */;
string bossName = VBloods.ToString(guidValue);
string shortBossName = VBloods.ToShortString(guidValue);
```

### Customizing Boss Names

Server administrators can customize the V Blood boss names by modifying the configuration file located at:
```
VRising_ServerRoot/BepInEx/configs/VAMP/Names_VBloods.json
```

Example JSON configuration:
```json
{
  "CHAR_Bandit_Frostarrow_VBlood": {
    "Long": "Custom Frost Archer Name",
    "Short": "Custom Name"
  }
}
```

## Important Notes

1. Unknown PrefabGUIDs return "VBlood Unknown"
2. Names are case-sensitive in the configuration
3. Configuration changes require server restart
4. Both long and short names must be specified in config
5. Default names are used if config entry is missing

## Integration with Other Systems

The VBloods utility works with other VAMP systems:

- Server Commands: For boss-related commands
- Quest System: For V Blood hunt objectives
- Progress Tracking: For monitoring V Blood defeats
- Event System: For V Blood spawn notifications

Using VBloods ensures consistent boss naming throughout your server's systems and allows for easy customization of displayed names.
