
# File Reload Documentation

The FileReload system provides automatic file change detection and reloading capabilities in VAMP. It allows mods to track specific files and automatically trigger reload callbacks when those files are modified.

## Key Features

- Automatic file change detection
- Callback system for file modifications
- Support for both fields and properties
- Configurable reload intervals

## Usage

### Basic File Tracking

```csharp
public class MyMod
{
    // Basic usage - will look for "ReloadConfigPath" method
    [FileReload]
    public static string ConfigPath = "config/mymod.json";

    // Called automatically when file changes
    private static void ReloadConfigPath()
    {
        // Reload your config here
        Console.WriteLine("Config file changed!");
    }
}
```

### Custom Callback Method

```csharp
public class MyMod
{
    // Specify a custom callback method
    [FileReload("OnSettingsChanged")]
    public static string SettingsPath { get; } = "config/settings.json";

    private static void OnSettingsChanged()
    {
        // Handle file changes
        Console.WriteLine("Settings file changed!");
    }
}
```

### Manual Registration

```csharp
public class MyMod
{
    private static readonly string LogPath = "logs/mymod.log";

    void Start()
    {
        // Manually register a file to watch
        FileWatcherSystem.RegisterFileForTracking(
            LogPath,
            typeof(MyMod),
            "OnLogChanged"
        );
    }

    private static void OnLogChanged()
    {
        Console.WriteLine("Log file was modified!");
    }
}
```

## Important Notes

1. Files are checked for changes every 60 seconds by default
2. Only static string fields and properties can use the FileReload attribute
3. Callback methods must be:
   - Static
   - Return void
   - Take no parameters
4. Non-existent files will be logged as warnings but won't crash the system
5. The system automatically filters out system and Unity assemblies

## Best Practices

1. Use absolute paths or paths relative to the game directory
2. Keep callback methods lightweight to avoid performance issues
3. Handle potential file access exceptions in your callback methods
4. Use meaningful callback method names that reflect their purpose
5. Consider using properties for more control over file path access

## Example Implementation

```csharp
public class ConfigManager
{
    [FileReload("ReloadServerConfig")]
    public static string ServerConfigPath { get; } = "config/server.json";

    [FileReload("ReloadClientConfig")]
    public static string ClientConfigPath { get; } = "config/client.json";

    private static ServerConfig _serverConfig;
    private static ClientConfig _clientConfig;

    private static void ReloadServerConfig()
    {
        try
        {
            string json = File.ReadAllText(ServerConfigPath);
            _serverConfig = JsonSerializer.Deserialize<ServerConfig>(json);
            Console.WriteLine("Server config reloaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to reload server config: {ex.Message}");
        }
    }

    private static void ReloadClientConfig()
    {
        try
        {
            string json = File.ReadAllText(ClientConfigPath);
            _clientConfig = JsonSerializer.Deserialize<ClientConfig>(json);
            Console.WriteLine("Client config reloaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to reload client config: {ex.Message}");
        }
    }
}
```
