using System.Collections.Generic;

namespace VAMP.Structs;

public class ModInfo
{
    public string Name { get; set; } = string.Empty;
    public string GUID { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    private string _author = string.Empty;
    public string Author 
    { 
        get => GetAuthor();
        set => _author = value;
    }
    public string Description { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string LoadSource { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public string ThunderstoreVersion { get; set; } = string.Empty;

    private string GetAuthor()
    {
        // Apparently me and odjit are bad at setting our author name on mods.
        if (_author == Name)
        {
            if (Name.StartsWith("Crimson"))
            {
                return "skytech6";
            }
            else if (Name.StartsWith("Kindred"))
            {
                return "odjit";
            }
        }

        return _author;
    }
}