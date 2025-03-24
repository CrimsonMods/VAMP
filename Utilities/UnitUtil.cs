using System;
using System.Collections.Generic;
using System.Threading;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using VAMP.Data;
using VAMP.Models;
using VAMP.Services;
using VAMP.Structs;

namespace VAMP.Utilities;

public static class UnitUtil
{
    public class SpawnedUnit
    {
        public Unit Unit;
        public float3 SpawnPosition;
        public Player Player;
        public SpawnedUnit(Unit unit, float3 spawnPosition, Player player = null)
        {
            Unit = unit;
            SpawnPosition = spawnPosition;
            Player = player;
        }
    }

    public static Dictionary<int, SpawnedUnit> HashToUnit = new Dictionary<int, SpawnedUnit>();
    public static Dictionary<string, List<Timer>> timersByCategory = new Dictionary<string, List<Timer>>();
    public static Dictionary<Unit, Entity> UnitToEntity = new Dictionary<Unit, Entity>();

    public static bool TryGetSpawnedUnitFromEntity(Entity unitEntity, out SpawnedUnit unit)
    {
        if (unitEntity.Has<ResistanceData>() && unitEntity.Has<CanFly>())
        {
            var resistanceData = unitEntity.Read<ResistanceData>();
            var hash = (int)resistanceData.FireResistance_RedcuedIgiteChancePerRating;
            return HashToUnit.TryGetValue(hash, out unit);
        }
        unit = default;
        return false;
    }

    public static void SpawnUnitWithCallback(Unit unit, float3 position, Action<Entity> postActions, Player player = null)
    {
        SpawnedUnit spawnedUnit = new SpawnedUnit(unit, position, player);
        SpawnService.SpawnUnitWithCallback(unit.PrefabGuid, position, 0, e =>
        {
            var hash = e.GetHashCode();
            HashToUnit[hash] = spawnedUnit;
            StoreMetaDataOnUnit(unit, e, position, player);
            SetHealth(unit, e);
            if (unit.MaxDistanceFromPreCombatPosition != -1)
            {
                var aggro = e.Read<AggroConsumer>();
                aggro.MaxDistanceFromPreCombatPosition = unit.MaxDistanceFromPreCombatPosition;
                e.Write(aggro);
            }
            if (BuffUtil.BuffEntity(e, Prefabs.Buff_BloodQuality_T01_OLD, out Entity buffEntity, 0, true))
            {
                if (unit.Level != -1 && e.Has<UnitLevel>())
                {
                    buffEntity.Add<ModifyUnitLevelBuff>();
                    buffEntity.Write(new ModifyUnitLevelBuff
                    {
                        UnitLevel = unit.Level,
                        Priority = 100
                    });
                }

                if (unit.AggroRadius != -1)
                {
                    ModifyAggroRadius(unit, buffEntity); //this increases the boss range, but keeps players in combat :(
                }
                AddBuffModifications(unit, buffEntity);
                if (unit.KnockbackResistance)
                {
                    GiveKnockbackResistance(unit, e, buffEntity);
                }
                if (!unit.DrawsAggro)
                {
                    DisableAggro(buffEntity);
                }
                unit.Modify(e, buffEntity);

                if (e.Has<BloodConsumeSource>() && !e.Has<VBloodUnit>())
                {
                    var bloodConsumeSource = e.Read<BloodConsumeSource>();
                    bloodConsumeSource.CanBeConsumed = false;
                    e.Write(bloodConsumeSource);
                }

                if (unit.SoftSpawn && unit.SpawnDelay > 0)
                {
                    BuffUtil.BuffEntity(e, Prefabs.Buff_General_VampireMount_Dead, out var softSpawnBuff, unit.SpawnDelay);
                    BuffUtil.ModifyBuff(softSpawnBuff, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.TargetSpellImpaired | BuffModificationTypes.MovementImpair | BuffModificationTypes.RelocateImpair | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.BehaviourImpair);
                }
            }
            else
            {
                unit.Modify(e);
            }

            UnitToEntity[unit] = e;

            postActions(e);
        });

    }

    private static void ModifyAggroRadius(Unit unit, Entity buffEntity)
    {
        buffEntity.Add<ModifyAggroRangesBuff>();
        buffEntity.Write(new ModifyAggroRangesBuff
        {
            AggroCircleRadiusFactor = unit.AggroRadius,
            AggroConeRadiusFactor = unit.AggroRadius,
            AlertCircleRadiusFactor = unit.AggroRadius,
            AlertConeRadiusFactor = unit.AggroRadius
        });
    }

    public static float StringToFloatHash(string input)
    {
        float hash = 0;
        float charMultiplier = 31;

        foreach (char c in input)
        {
            hash = hash * charMultiplier + (int)c;
        }

        return hash;
    }

    public static float3 GetSpawnPositionOfEntity(Entity entity)
    {
        if (entity.Has<ResistanceData>())
        {
            var resistanceData = entity.Read<ResistanceData>();
            return new float3(resistanceData.HolyResistance_DamageAbsorbPerRating, resistanceData.HolyResistance_DamageReductionPerRating, resistanceData.SilverResistance_DamageReductionPerRating);
        }
        else
        {
            return float3.zero;
        }
    }


    private static void StoreMetaDataOnUnit(Unit unit, Entity e, float3 position, Player player = null)
    {
        e.Add<NameableInteractable>();
        e.Write(new NameableInteractable
        {
            Name = unit.Category
        });
        e.Add<ResistanceData>();
        var resistanceData = e.Read<ResistanceData>();
        resistanceData.FireResistance_DamageReductionPerRating = unit.Team;
        resistanceData.FireResistance_RedcuedIgiteChancePerRating = e.GetHashCode();
        resistanceData.GarlicResistance_IncreasedExposureFactorPerRating = StringToFloatHash(unit.Category);
        resistanceData.HolyResistance_DamageAbsorbPerRating = position.x;
        resistanceData.HolyResistance_DamageReductionPerRating = position.y;
        resistanceData.SilverResistance_DamageReductionPerRating = position.z;
        e.Write(resistanceData);
        if (player != null)
        {
            e.Write(player.Character.Read<TeamReference>());
            e.Write(player.Character.Read<Team>());
        }
    }

    private static void DisableAggro(Entity buffEntity)
    {
        buffEntity.Add<DisableAggroBuff>();
        buffEntity.Write(new DisableAggroBuff
        {
            Mode = DisableAggroBuffMode.OthersDontAttackTarget
        });
    }

    private static void GiveKnockbackResistance(Unit unit, Entity e, Entity buffEntity)
    {
        e.Add<BuffResistances>();
        if (unit.IsRooted)
        {
            e.Write(new BuffResistances
            {
                SettingsEntity = new ModifiableEntity(EntityUtil.EntityFromGUID(Prefabs.BuffResistance_Golem)),
                InitialSettingGuid = Prefabs.BuffResistance_Golem
            });
        }
        else
        {
            e.Write(new BuffResistances
            {
                SettingsEntity = new ModifiableEntity(EntityUtil.EntityFromGUID(Prefabs.BuffResistance_UberMobNoKnockbackOrGrab)),
                InitialSettingGuid = Prefabs.BuffResistance_UberMobNoKnockbackOrGrab
            });
        }
    }

    private static void AddBuffModifications(Unit unit, Entity buffEntity)
    {
        buffEntity.Add<BuffModificationFlagData>();
        BuffModificationTypes modificationTypes = BuffModificationTypes.None;
        if (!unit.DynamicCollision)
        {
            modificationTypes |= BuffModificationTypes.DisableDynamicCollision;
        }
        if (!unit.MapCollision)
        {
            modificationTypes |= BuffModificationTypes.DisableMapCollision;
        }
        if (unit.IsRooted)
        {
            modificationTypes |= BuffModificationTypes.MovementImpair | BuffModificationTypes.RelocateImpair;
        }
        if (unit.IsImmaterial)
        {
            modificationTypes |= BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable;
        }
        if (unit.IsInvulnerable)
        {
            modificationTypes |= BuffModificationTypes.Invulnerable;
        }
        if (!unit.IsTargetable)
        {
            modificationTypes |= BuffModificationTypes.TargetSpellImpaired;
        }
        if (unit.AbilityCastImpair)
        {
            modificationTypes |= BuffModificationTypes.AbilityCastImpair;
        }
        BuffUtil.ModifyBuff(buffEntity, modificationTypes, true);
    }

    private static void SetHealth(Unit unit, Entity e)
    {
        if (e.Has<Health>())
        {
            var health = e.Read<Health>();
            if (unit.MaxHealth != -1)
            {
                health.MaxHealth._Value = unit.MaxHealth;
                health.MaxRecoveryHealth = unit.MaxHealth;
                health.Value = unit.MaxHealth;
            }
            e.Write(health);
        }
    }

    public static bool HasCategory(Entity entity, string category)
    {
        if (entity.Has<CanFly>() && entity.Has<NameableInteractable>())
        {
            var nameableInteractable = entity.Read<NameableInteractable>();
            if (nameableInteractable.Name.ToString() == category)
            {
                return true;
            }
        }
        return false;
    }

    public static string GetCategory(Entity entity)
    {
        if (entity.Has<CanFly>() && entity.Has<NameableInteractable>())
        {
            var nameableInteractable = entity.Read<NameableInteractable>();
            return nameableInteractable.Name.ToString();
        }
        return "";
    }

    public static void AddCategory(Entity entity, string category)
    {
        entity.Add<CanFly>();
        entity.Add<NameableInteractable>();
        entity.Write(new NameableInteractable
        {
            Name = category
        });
    }
}