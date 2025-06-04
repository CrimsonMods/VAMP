using System.IO;
using BepInEx;

namespace VAMP;

public static class FilePaths
{
    /// <summary>
    /// Gets the path to the _WipeData folder in the config directory.
    /// </summary>
    /// <returns>The full path to the _WipeData folder.</returns>
    public static string WipeData => Path.Combine(Paths.ConfigPath, "_WipeData");
}