using System;
using ProjectM;
using ProjectM.CastleBuilding;
using Stunlock.Core;
using Unity.Entities;
using VAMP.Data;
using VAMP.Utilities;

namespace VAMP.Structs;

public class Unit
{
    protected PrefabGUID prefabGuid;
    protected int team = 10;
    protected int level = -1;
    protected bool isImmaterial = false;
    protected float maxHealth = -1;
    protected float aggroRadius = -1;
    protected bool knockbackResistance = false;
    protected bool isRooted = false;
    protected float respawnTime = -1;
    protected bool drawsAggro = true;
    protected bool isTargetable = true;
    protected bool isInvisible = false;
    protected bool isInvulnerable = false;
    protected bool abilityCastImpair = false;
    protected bool dynamicCollision = false;
    protected float maxDistanceFromPreCombatPosition = -1;
    protected bool mapCollision = true;
    protected string gameMode = "";
    protected int spawnDelay = -1;
    protected bool softSpawn = false;
    protected bool announceSpawn = false;
    protected string name = "";

    public PrefabGUID PrefabGuid { get => prefabGuid; set => prefabGuid = value; }
    public int Team { get => team; set => team = value; }
    public int Level { get => level; set => level = value; }
    public bool IsImmaterial { get => isImmaterial; set => isImmaterial = value; }
    public bool IsInvulnerable { get => isInvulnerable; set => isInvulnerable = value; }
    public bool AbilityCastImpair { get => abilityCastImpair; set => abilityCastImpair = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float AggroRadius { get => aggroRadius; set => aggroRadius = value; }
    public bool KnockbackResistance { get => knockbackResistance; set => knockbackResistance = value; }
    public bool IsRooted { get => isRooted; set => isRooted = value; }
    public float RespawnTime { get => respawnTime; set => respawnTime = value; }
    public bool DrawsAggro { get => drawsAggro; set => drawsAggro = value; }
    public bool DynamicCollision { get => dynamicCollision; set => dynamicCollision = value; }
    public float MaxDistanceFromPreCombatPosition { get => maxDistanceFromPreCombatPosition; set => maxDistanceFromPreCombatPosition = value; }
    public bool MapCollision { get => mapCollision; set => mapCollision = value; }
    public bool IsTargetable { get => isTargetable; set => isTargetable = value; }
    public string Category { get => gameMode; set => gameMode = value; }
    public bool AnnounceSpawn { get => announceSpawn; set => announceSpawn = value; }
    public int SpawnDelay { get => spawnDelay; set => spawnDelay = value; }
    public bool SoftSpawn { get => softSpawn; set => softSpawn = value; }
    public string Name { get => name; set => name = value; }


    public Unit(PrefabGUID prefabGuid, int team = 10, int level = -1)
    {
        this.prefabGuid = prefabGuid;
        this.team = team;
        this.level = level;
    }

    public virtual void Modify(Entity e, Entity buffEntity)
    {
        Modify(e);
    }

    public virtual void Modify(Entity e)
    {

    }
}

public class HardBoss : Boss
{
    public HardBoss(PrefabGUID prefabGuid, int team = 10) : base(prefabGuid, team)
    {
        level = 120;
        maxHealth = 1500;
    }

    public override void Modify(Entity e, Entity buffEntity)
    {

        base.Modify(e, buffEntity);
    }
}

public class Boss : Unit
{
    public Boss(PrefabGUID prefabGuid, int team = 10, int level = -1) : base(prefabGuid, team, level)
    {
        isImmaterial = false;
        aggroRadius = -1;
        knockbackResistance = true;
        isRooted = false;
        drawsAggro = false;
        isTargetable = false;
        softSpawn = true;
    }

    public override void Modify(Entity e, Entity buffEntity)
    {
        base.Modify(e, buffEntity);
    }
}

public class AngramBoss : Boss
{
    public AngramBoss(int team = 10, int level = -1) : base(Prefabs.CHAR_Gloomrot_Purifier_VBlood, team, level)
    {
        name = "Angram";
        softSpawn = true;
        isRooted = true;
    }
}

public class Turret : Unit
{
    public Turret(PrefabGUID prefabGuid, int team = 10, int level = -1) : base(prefabGuid, team, level)
    {
        isImmaterial = true;
        knockbackResistance = false;
        isRooted = true;
        drawsAggro = false;
        isTargetable = false;
    }
}

public class BaseTurret : Unit
{
    public static PrefabGUID PrefabGUID = Prefabs.CHAR_Gloomrot_SentryTurret;
    public BaseTurret(PrefabGUID prefabGuid, int team = 10, int level = -1) : base(prefabGuid, team, level)
    {
        isImmaterial = false;
        knockbackResistance = true;
        isRooted = true;
        drawsAggro = true;
        isTargetable = false;
        aggroRadius = 3f;
        maxDistanceFromPreCombatPosition = 20;
    }

    public override void Modify(Entity e)
    {
        base.Modify(e);
        BuffUtil.BuffEntity(e, Prefabs.AB_Gloomrot_SentryTurret_BunkerDown_Buff, out var buffEntity, 0);
        BuffUtil.ModifyBuff(buffEntity, BuffModificationTypes.None, true);
    }
}

public class DyeableStructure : Unit
{
    protected int color = 0;
    public int Color { get => color; set => color = value; }
    public DyeableStructure(PrefabGUID prefabGuid, int color) : base(prefabGuid)
    {
        Color = color;
    }

    public override void Modify(Entity e)
    {
        if (e.Has<DyeableCastleObject>())
        {
            var dyeable = e.Read<DyeableCastleObject>();
            dyeable.ActiveColorIndex = (byte)color;
            e.Write(dyeable);
        }
    }


    public override void Modify(Entity e, Entity buffEntity)
    {
        Modify(e);
    }
}

public class HealingOrb : Unit
{
    public HealingOrb() : base(Prefabs.AB_General_HealingOrb_Object)
    {

    }

    public override void Modify(Entity e)
    {
        var lifetime = e.Read<LifeTime>();
        lifetime.EndAction = LifeTimeEndAction.None;
        lifetime.Duration = -1;
        var buffer = e.ReadBuffer<CreateGameplayEventsOnTimePassed>();
        buffer.Clear();
    }
}

public class UltimateOrb : Unit
{
    public UltimateOrb() : base(Prefabs.CHAR_Militia_ConstrainingPole)
    {
        isRooted = true;
        knockbackResistance = true;
        drawsAggro = false;
        isTargetable = false;
        name = "Ultimate Orb";
        announceSpawn = true;
    }

    public override void Modify(Entity e)
    {
        var lifetime = e.Read<LifeTime>();
        lifetime.EndAction = LifeTimeEndAction.None;
        lifetime.Duration = -1;
        if (BuffUtil.BuffEntity(e, Prefabs.Buff_BloodQuality_T01_OLD, out var buffEntity, 0))
        {
            BuffUtil.ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.MovementImpair);
        }
    }
}

public class Horse : Unit
{
    protected static readonly new PrefabGUID prefabGuid = Prefabs.CHAR_Mount_Horse;
    protected float speed = 11;
    protected float acceleration = 7;
    protected float rotation = 14;
    protected string name = "";

    public float Speed { get => speed; set => speed = value; }
    public float Acceleration { get => acceleration; set => acceleration = value; }
    public float Rotation { get => rotation; set => rotation = value; }
    public string Name { get => name; set => name = value; }

    public Horse(int team = 10) : base(prefabGuid, team)
    {

    }

    public override void Modify(Entity e, Entity buffEntity)
    {
        if (!string.IsNullOrEmpty(name))
        {
            var nameableInteractable = e.Read<NameableInteractable>();
            nameableInteractable.Name = name;
            e.Write(nameableInteractable);
        }

        var mountable = e.Read<Mountable>();
        mountable.MaxSpeed = Speed;
        mountable.Acceleration = Acceleration;
        mountable.RotationSpeed = Rotation * 10;
        e.Write(mountable);
    }
}