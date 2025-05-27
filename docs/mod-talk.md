
# ModTalk System Documentation

The ModTalk system provides a robust mechanism for inter-mod communication in VAMP, allowing mods to send messages, share data, and receive responses from other mods.

## Overview

The `ModTalk` class contains a delegate-based event system that enables mods to:
- Send messages to other mods
- Pass parameters between mods
- Receive responses through callbacks
- Handle messages from other mods

## Usage

### Sending Messages

To send a message to other mods:

```csharp
bool handled = ModTalk.Invoke("my-mod.action", new object[] { "param1", 42 }, result => {
    // Handle the response here
    Plugin.LogInstance.LogInfo($"Received response: {result}");
});

if (!handled) {
    Plugin.LogInstance.LogError("Message not handled by any mod.");
}
```

### Subscribing to Messages

To receive and handle messages from other mods:

```csharp
ModTalk.MessageReceived += (identifier, args, callback) => {
    if (identifier == "my-mod.action") {
        // Process the arguments
        string param1 = args[0] as string;
        int param2 = (int)args[1];
        
        // Perform your mod's logic
        var result = ProcessAction(param1, param2);
        
        // Send response through callback
        callback?.Invoke(result);
        return true; // Message was handled
    }
    return false; // Message not handled by this mod
};
```

## Best Practices

1. Use descriptive identifiers prefixed with your mod name (e.g., "mymod.action")
2. Always check for null arguments and callbacks
3. Return true only when your mod actually handles the message
4. Document your mod's message identifiers and expected parameters

## API Reference

### ModTalkDelegate

```csharp
public delegate bool ModTalkDelegate(string identifier, object[] args = null, Action<object> callback = null)
```

#### Parameters
- `identifier`: String identifier for the specific function/action to trigger
- `args`: Optional array of arguments to pass to the receiving mod
- `callback`: Optional callback to handle the result of the action
- Returns: `true` if the message was handled, `false` otherwise

### Invoke Method

```csharp
public static bool Invoke(string identifier, object[] args = null, Action<object> callback = null)
```

Helper method to safely invoke the ModTalk event with proper null checking.

#### Parameters
- `identifier`: String identifier for the specific function/action to trigger
- `args`: Optional array of arguments to pass to the receiving mod
- `callback`: Optional callback to handle the result of the action
- Returns: `true` if any mod handled the message, `false` otherwise
