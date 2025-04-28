using ProjectM;
using Stunlock.Core;

namespace VAMP;
//#pragma warning disable CS8500
public static class Extensions
{
    /// <summary>
    /// Converts a PrefabGUID to its corresponding name string from the game's prefab collection.
    /// </summary>
    /// <param name="prefabGuid">The PrefabGUID to look up</param>
    /// <returns>A string containing the prefab name and GUID, or "GUID Not Found" if the prefab doesn't exist</returns>
    /// <remarks>
    /// Uses the PrefabCollectionSystem to retrieve human-readable names for game objects.
    /// The returned string includes both the name and the GUID for complete identification.
    /// </remarks>
    /// <example>
    /// string itemName = someGuid.LookupName(); // Returns "Sword 12345" or "GUID Not Found"
    /// </example>
    public static string LookupName(this PrefabGUID prefabGuid)
    {
        var prefabCollectionSystem = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
        return (prefabCollectionSystem._PrefabLookupMap.ContainsKey(prefabGuid)
            ? prefabCollectionSystem._PrefabLookupMap.GetName(prefabGuid) + " " + prefabGuid : "GUID Not Found").ToString();
    }
}
//#pragma warning restore CS8500