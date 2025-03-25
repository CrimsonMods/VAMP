using Unity.Collections;
using Unity.Entities;

namespace VAMP.Structs;

/// <summary>
/// This will be deprecated in the future in favor of the Player model.  
/// </summary>
public struct PlayerData
{
    public FixedString64Bytes CharacterName { get; set; }
    public ulong SteamID { get; set; }
    public bool IsOnline { get; set; }
    public Entity UserEntity { get; set; }
    public Entity CharEntity { get; set; }

    public PlayerData(FixedString64Bytes characterName, ulong steamID, bool isOnline, Entity userEntity, Entity charEntity)
    {
        CharacterName = characterName;
        SteamID = steamID;
        IsOnline = isOnline;
        UserEntity = userEntity;
        CharEntity = charEntity;
    }
}
