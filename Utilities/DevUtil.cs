using ProjectM;
using System;
using System.IO;
using System.Linq;
using Unity.Entities;

namespace VAMP.Utilities;

public static class DevUtil
{
    public static bool ChatDebugMode = false;
    public static string[] ChatDebugKeys;

    /// <summary>
    /// Sends a debug message to all clients through the chat system if debug mode is enabled.
    /// </summary>
    /// <param name="message">The message to be sent to all clients</param>
    /// <param name="key">The key to use to request specific debug messages</param>
    /// <remarks>
    /// This method will only send messages if ChatDebugMode is set to true by using !!chatDebug command.
    /// If the message is null or empty, no message will be sent.
    /// Highly recommended to use a key specific to your mod or sections of your mod.
    /// </remarks>
    /// <example>
    /// DevUtil.ChatDebug($"Player position updated: x={x}, y={y}");
    /// DevUtil.ChatDebug($"Player position updated: x={x}, y={y}", "positionInfo");
    /// </example>
    public static void ChatDebug(string message, string key = null)
    { 
        if(!ChatDebugMode) return;
        if (string.IsNullOrEmpty(message)) return;
        if(!string.IsNullOrEmpty(key) && !ChatDebugKeys.Contains(key)) return;

        ChatUtil.SystemSendAll(message);
    }

    /// <summary>
    /// /// Dumps all component information of an entity to a specified file for debugging purposes.
    /// </summary>
    /// <param name="entity">The entity to dump information from</param>
    /// <param name="filePath">The file path where the dump will be written</param>
    /// <remarks>
    /// The dump includes a list of all components attached to the entity and their detailed information.
    /// The output is appended to the specified file with clear section separators.
    /// </remarks>
    /// <example>
    /// someEntity.Dump("C:/debug/entity_dump.txt");
    /// </example>
    public static void Dump(this Entity entity, string filePath)
    {
        string directory = "VAMPDump";
        Directory.CreateDirectory(directory);  // Create directory if it doesn't exist
        
        filePath = Path.Combine(directory, filePath);
        File.AppendAllText(filePath, $"--------------------------------------------------" + Environment.NewLine);
        File.AppendAllText(filePath, $"Dumping components of {entity}:" + Environment.NewLine);
        foreach (var componentType in Core.Server.EntityManager.GetComponentTypes(entity))
        { File.AppendAllText(filePath, $"{componentType}" + Environment.NewLine); }
        File.AppendAllText(filePath, $"--------------------------------------------------" + Environment.NewLine);
        File.AppendAllText(filePath, DumpEntity(entity));
    }

    private static string DumpEntity(Entity entity, bool fullDump = true)
    {
        var sb = new Il2CppSystem.Text.StringBuilder();
        EntityDebuggingUtility.DumpEntity(Core.Server, entity, fullDump, sb);
        return sb.ToString();
    }
}
