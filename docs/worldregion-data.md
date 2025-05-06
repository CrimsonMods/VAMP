
# WorldRegions Data Documentation

The WorldRegions class is a static utility in VAMP that provides methods for converting WorldRegionType enum values into their human-readable names. It maintains a dictionary of all world regions and offers both full and shortened name variants.

## Key Features

- Convert WorldRegionType to full region names
- Convert WorldRegionType to shortened region names
- Support for extension methods
- Customizable region names via JSON configuration
- Complete list of all world regions in the game

## Usage

### Converting WorldRegionType to Names
```csharp
// Get full name of a region
WorldRegionType region = /* get region type */;
string fullName = WorldRegions.ToString(region); // Returns "Cursed Forest"

// Get shortened name of a region
string shortName = WorldRegions.ToShortString(region); // Returns "Forest"

// Using extension methods
string fullNameExt = region.ToStringLong(); // Returns "Cursed Forest"
string shortNameExt = region.ToStringShort(); // Returns "Forest"
```

### Customizing Region Names

Server administrators can customize the region names by modifying the configuration file located at:
```
VRising_ServerRoot/BepInEx/configs/VAMP/Names_Regions.json
```

Example JSON configuration:
```json
{
  "CursedForest": {
    "Long": "Custom Forest Name",
    "Short": "Custom Name"
  }
}
```

## Important Notes

1. Unknown regions return "Unknown Location"
2. Names are case-sensitive in the configuration
3. Configuration changes require server restart
4. Both long and short names must be specified in config
5. Default names are used if config entry is missing

## Integration with Other Systems

The WorldRegions utility works with other VAMP systems:

- Server Commands: For location-based commands
- Quest System: For region-specific objectives
- Progress Tracking: For tracking player movement
- Event System: For region-based notifications

Using WorldRegions ensures consistent region naming throughout your server's systems and allows for easy customization of displayed names.
