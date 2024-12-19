using ProjectM;
using System;
using System.IO;
using Unity.Entities;

namespace VAMP.Utilities;

public static class DevUtil
{
    /// <summary>
    /// Dumps all component information of an entity to a specified file for debugging purposes.
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
        filePath = "FANGDump/" + filePath;
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
