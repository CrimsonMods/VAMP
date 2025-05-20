
# JSON Utility Documentation

The JsonUtil is a utility class in VAMP that provides methods and converters for JSON serialization and deserialization operations. It includes custom converters for special data types and default serialization options.

## Key Features

- Default JSON serializer options with pretty printing
- Custom converter for long/short name tuples
- Custom converter for PrefabGUID objects
- Custom converters for time-related types (TimeOnly, DayOfWeek)
- Comment header support for JSON files
- Field inclusion enabled by default
- Type-safe JSON conversion

## Usage

### Using Default Serializer Options
```csharp
// Serialize an object with pretty printing
var myObject = new MyClass { Name = "Test" };
string json = JsonSerializer.Serialize(myObject, JsonUtil.PrettyJsonOptions);

// Deserialize with default options
MyClass deserialized = JsonSerializer.Deserialize<MyClass>(json, JsonUtil.PrettyJsonOptions);
```

### Adding Comments to JSON
```csharp
var myObject = new MyClass { Name = "Test" };
string commentHeader = "This is a configuration file\nLast updated: 2024";
string json = JsonUtil.SerializeWithComments(myObject, commentHeader);
```

### Working with Long/Short Name Tuples
```csharp
// Create a tuple with long and short names
var names = ("Long Name", "Short");

// Serialize the tuple
string json = JsonSerializer.Serialize(names, new JsonSerializerOptions
{
    Converters = { new JsonUtil.LongShortNamesConverter() }
});

// Deserialize back to tuple
var deserializedNames = JsonSerializer.Deserialize<(string Long, string Short)>(
    json,
    new JsonSerializerOptions { Converters = { new JsonUtil.LongShortNamesConverter() }}
);
```

### Handling PrefabGUIDs
```csharp
// Create a PrefabGUID
var prefabGuid = new PrefabGUID(12345);

// Serialize PrefabGUID
string json = JsonSerializer.Serialize(prefabGuid, new JsonSerializerOptions
{
    Converters = { new JsonUtil.PrefabGUIDConverter() }
});

// Deserialize back to PrefabGUID
var deserializedGuid = JsonSerializer.Deserialize<PrefabGUID>(
    json,
    new JsonSerializerOptions { Converters = { new JsonUtil.PrefabGUIDConverter() }}
);
```

### Working with Time Types
```csharp
// TimeOnly with hours, minutes, and seconds
var time = new TimeOnly(13, 30, 45);
string json = JsonSerializer.Serialize(time, new JsonSerializerOptions
{
    Converters = { new JsonUtil.TimeOnlyConverter() }
});

// TimeOnly with just hours and minutes
var timeHM = new TimeOnly(13, 30);
string jsonHM = JsonSerializer.Serialize(timeHM, new JsonSerializerOptions
{
    Converters = { new JsonUtil.TimeOnlyHourMinuteConverter() }
});

// DayOfWeek with null support
DayOfWeek? day = DayOfWeek.Monday;
string jsonDay = JsonSerializer.Serialize(day, new JsonSerializerOptions
{
    Converters = { new JsonUtil.DayOfWeekConverter() }
});
```

## Important Notes

1. PrettyJsonOptions includes indented formatting by default
2. Field values are included in serialization
3. Comments are automatically skipped during parsing
4. Trailing commas are allowed in JSON
5. LongShortNamesConverter supports both named and tuple item formats
6. PrefabGUIDConverter accepts both number and string inputs
7. TimeOnly converters support different time formats
8. DayOfWeekConverter represents null as "Daily"
9. Custom converters must be explicitly added to JsonSerializerOptions

## Integration with Other Systems

The JsonUtil works seamlessly with other VAMP systems:

- [File Reload](https://vrising.wiki/docs/file-reload.html): Automatically reload JSON files when they change, no server restarting required. 
- Configuration System: For saving and loading settings
- Data Storage: For persistent data serialization
- Network Communication: For data transfer
- Mod Support: For custom data handling

By using JsonUtil for all JSON operations, you ensure consistent serialization behavior and proper handling of special types throughout your mods.