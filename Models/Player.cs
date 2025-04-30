
using System;
using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using ProjectM.Terrain;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAMP.Data;
using VAMP.Utilities;

namespace VAMP.Models;
public class Player
{
	private Entity _user;
	private Entity _character;
	private ulong _steamID;

	public Entity User
	{
		get 
		{ 
			return _user;
		} 
		set { SetUser(value); }
	}

	public Entity Character
	{
		get
		{
			return _character;
		}
		set { SetCharacter(value); }
	}

	public ulong SteamID
	{
		get => _steamID == default && _user != default ? _user.Read<User>().PlatformId : _steamID;
		set => _steamID = value;
	}

	public Entity Clan => GetClan();

	public string Name => GetName();
	public string FullName => GetFullName();
	public int Level => GetLevel();
	public int Height => GetHeight();
	public bool IsAdmin => GetIsAdmin();
	public bool IsAdminCapable => GetIsAdminCapable();
	public bool IsOnline => GetIsOnline();
	public bool IsAlive => GetIsAlive();
	public Entity Inventory => GetInventory();
	public Equipment Equipment => GetEquipment();
	public List<Entity> EquipmentEntities => GetEquipmentEntities();
	public Entity ControlledEntity => GetControlledEntity();

	public float3 Position => GetPosition();
	public float3 AimPosition => GetAimPosition();
	public int2 TilePosition => GetTilePosition();
	public Team Team => GetTeam();

	public WorldRegionType WorldZone => GetWorldZone();
	public string WorldZoneString => GetWorldZoneString();

	private void SetUser(Entity user)
	{
		if (_user != user)
		{
			if (!user.Exists())
			{
				throw new Exception("Invalid User");
			}

			_user = user;

			_steamID = _user.Read<User>().PlatformId;
			if (!_character.Exists())
			{
				_character = _user.Read<User>().LocalCharacter._Entity;
			}
		}
	}

	private void SetCharacter(Entity character)
	{
		if (character.Exists())
		{
			_character = character;
			if (!_user.Exists())
			{
				if (_character.Read<PlayerCharacter>().UserEntity.Exists())
				{
					_user = _character.Read<PlayerCharacter>().UserEntity;
					_steamID = _user.Read<User>().PlatformId;
				}
				else
				{
					throw new Exception("Tried to load a player without a valid user");
				}
			}
		}
	}

	private string GetName()
	{
		var userData = User.Read<User>();
		var name = userData.CharacterName.ToString();
		if (name == "")
		{
			name = $"[No Character - {userData.PlatformId}]";
		}
		return name;
	}

	public string GetFullName()
	{
		var playerCharacter = Character.Read<PlayerCharacter>();
		if (!playerCharacter.SmartClanName.IsEmpty)
		{
			return $"{playerCharacter.SmartClanName} {Name}";
		}
		else
		{
			return Name;
		}
	}

	private int GetHeight()
	{
		return Character.Read<TilePosition>().HeightLevel;
	}
	private int GetLevel()
	{
		return (int)Character.Read<Equipment>().GetFullLevel();
	}

	private Entity GetClan()
	{
		return User.Read<User>().ClanEntity._Entity;
	}

	private Team GetTeam()
	{
		return User.Read<Team>();
	}

	private bool GetIsAdmin()
	{
		return User.Read<User>().IsAdmin;
	}

	private bool GetIsAdminCapable()
	{
		var AdminAuthSystem = Core.Server.GetExistingSystemManaged<AdminAuthSystem>();
		return AdminAuthSystem._LocalAdminList.Contains(SteamID);
	}

	private bool GetIsOnline()
	{
		if (User.Exists())
		{
			return User.Read<User>().IsConnected;
		}
		else
		{
			return false;
		}
	}
	private float3 GetAimPosition()
	{
		return User.Read<EntityInput>().AimPosition;
	}

	private float3 GetPosition()
	{
		return Character.Read<Translation>().Value;
	}

	private int2 GetTilePosition()
	{
		if (Character.Has<TilePosition>())
		{
			return Character.Read<TilePosition>().Tile;
		}
		else
		{
			return new int2(0, 0);
		}
	}


	private bool GetIsAlive()
	{
		return !Character.Read<Health>().IsDead && !BuffUtil.HasBuff(this.Character, Prefabs.Buff_General_Vampire_Wounded_Buff);
	}

	private Entity GetInventory()
	{
		return Character.ReadBuffer<InventoryInstanceElement>()[0].ExternalInventoryEntity._Entity;
	}
	
	private Equipment GetEquipment()
	{
		return Character.Read<Equipment>();
	}

	private List<Entity> GetEquipmentEntities()
	{
		var equipmentEntities = new NativeList<Entity>(Allocator.Temp);
		var equipment = Equipment;
		equipment.GetAllEquipmentEntities(equipmentEntities, true);
		var results = new List<Entity>();
		foreach (var equipmentEntity in equipmentEntities)
		{
			results.Add(equipmentEntity);
		}
		if (equipment.BagSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.BagSlot.SlotEntity._Entity);
		}
		if (equipment.ChestCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.ChestCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.CloakCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.CloakCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.FootgearCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.FootgearCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.GlovesCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.GlovesCosmeticSlot.SlotEntity._Entity);
		}
		if (equipment.LegsCosmeticSlot.SlotEntity._Entity.Exists())
		{
			results.Add(equipment.LegsCosmeticSlot.SlotEntity._Entity);
		}
		return results;
	}
	
	public WorldRegionType GetWorldZone()
	{
		return User.Read<CurrentWorldRegion>().CurrentRegion;
	}

	public string GetWorldZoneString()
	{
		var region = User.Read<CurrentWorldRegion>().CurrentRegion;
		if (WorldRegions.WorldRegionToString.TryGetValue(region, out var zoneName))
		{
			return zoneName;
		}
		return "";
	}

	public Entity GetControlledEntity()
	{
		return User.Read<Controller>().Controlled._Entity;
	}

	public FromCharacter ToFromCharacter()
	{
		return new FromCharacter
		{
			Character = this.Character,
			User = this.User
		};
	}

	public bool IsAlliedWith(Player player)
	{
		return Team.IsAllies(Character.Read<Team>(), player.Character.Read<Team>());
	}

	public bool HasControlledEntity()
	{
		if (ControlledEntity == Character)
		{
			return true;
		}
		else
		{
			bool isDead;
			if (ControlledEntity.Has<Health>())
			{
				isDead = ControlledEntity.Read<Health>().IsDead;
			}
			else
			{
				isDead = true;
			}

			if (isDead)
			{
				return false;
			}

			return ControlledEntity.Exists() && ControlledEntity.Has<PrefabGUID>();
		}
	}

	public override int GetHashCode()
	{
		return SteamID.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		Player other = (Player)obj;
		return SteamID == other.SteamID;
	}

	public override string ToString()
	{
		return Name;
	}
}