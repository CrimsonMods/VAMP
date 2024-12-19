using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VAMP.Utilities;

namespace VAMP.Structs;
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

public class Player
{
    public string Name { get; set; }
    public ulong SteamID { get; set; }
    public bool IsOnline { get; set; }
    public bool IsAdmin { get; set; }
    public Entity User { get; set; }
    public Entity Character { get; set; }
    public Player(Entity userEntity = default)
    {
        User = userEntity;
        var user = User.Read<User>();
        Character = user.LocalCharacter._Entity;
        Name = user.CharacterName.ToString();
        IsOnline = user.IsConnected;
        IsAdmin = user.IsAdmin;
        SteamID = user.PlatformId;
    }
}
