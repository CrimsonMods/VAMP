
# JSON Utility Documentation

The JsonUtil is a utility class in VAMP that provides methods and converters for JSON serialization and deserialization operations. It includes custom converters for special data types and default serialization options.

## Key Features

- Default JSON serializer options with pretty printing
- Custom converter for long/short name tuples
- Custom converter for PrefabGUID objects
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

## Important Notes

1. PrettyJsonOptions includes indented formatting by default
2. Field values are included in serialization
3. LongShortNamesConverter supports both named and tuple item formats
4. PrefabGUIDConverter accepts both number and string inputs
5. Custom converters must be explicitly added to JsonSerializerOptions

## Integration with Other Systems

The JsonUtil works seamlessly with other VAMP systems:

- Configuration System: For saving and loading settings
- Data Storage: For persistent data serialization
- Network Communication: For data transfer
- Mod Support: For custom data handling

By using JsonUtil for all JSON operations, you ensure consistent serialization behavior and proper handling of special types throughout your mods.