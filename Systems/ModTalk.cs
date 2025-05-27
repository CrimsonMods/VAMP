using System;

namespace VAMP.Systems;

public static class ModTalk
{
    /// <summary>
    /// Delegate for mod-to-mod communication with optional callback.
    /// </summary>
    /// <param name="identifier">String identifier for the specific function/action to trigger.</param>
    /// <param name="args">Optional array of arguments to pass to the receiving mod.</param>
    /// <param name="callback">Optional callback to handle the result of the action.</param>
    /// <returns>True if the message was handled by any mod, false otherwise.</returns>
    public delegate bool ModTalkDelegate(string identifier, object[] args = null, Action<object> callback = null);

    /// <summary>
    /// Event for mod-to-mod communication.
    /// Use this to send messages between mods with optional parameters and receive responses.
    /// </summary>
    /// <example>
    /// // Sending a message to other mods
    /// bool handled = Events.ModTalk?.Invoke("my-mod.some-action", new object[] { "param1", 42 }, result => {
    ///     Plugin.LogInstance.LogInfo($"Got response: {result}");
    /// }) ?? false;
    /// 
    /// // Handling messages from other mods
    /// ModTalk.MessageReceived += (identifier, args, callback) => {
    ///     if (identifier == "my-mod.some-action") {
    ///         // Process args
    ///         string param1 = args[0] as string;
    ///         int param2 = (int)args[1];
    ///         
    ///         // Do something with the parameters
    ///         object result = DoSomething(param1, param2);
    ///         
    ///         // Send response if callback is provided
    ///         callback?.Invoke(result);
    ///         return true; // Mark as handled
    ///     }
    ///     return false; // Not handled by this subscriber
    /// };
    /// </example>
    public static event ModTalkDelegate MessageReceived;

    /// <summary>
    /// Helper method to invoke the ModTalk event with proper null checking.
    /// </summary>
    /// <param name="identifier">String identifier for the specific function/action to trigger.</param>
    /// <param name="args">Optional array of arguments to pass to the receiving mod.</param>
    /// <param name="callback">Optional callback to handle the result of the action.</param>
    /// <returns>True if the message was handled by any mod, false otherwise.</returns>
    public static bool Invoke(string identifier, object[] args = null, Action<object> callback = null)
    {
        if (MessageReceived == null)
            return false;

        bool handled = false;
        foreach (ModTalkDelegate subscriber in MessageReceived.GetInvocationList())
        {
            if (subscriber(identifier, args, callback))
                handled = true;
        }
        return handled;
    }
}