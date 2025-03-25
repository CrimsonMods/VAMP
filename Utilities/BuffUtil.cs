using System.Linq;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;
using VAMP.Services;

namespace VAMP.Utilities;

public static class BuffUtil
{
    static ServerGameManager ServerGameManager => Core.ServerGameManager;
    static SystemService SystemService => Core.SystemService;
    static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;

    #region Functions
    /// <summary>
    /// Modifies a buff entity with specified modification types.
    /// </summary>
    /// <param name="buffEntity">The buff entity to modify.</param>
    /// <param name="buffModificationTypes">The types of modifications to apply.</param>
    /// <param name="overwrite">If true, clears existing modifications before applying new ones.</param>
    public static void ModifyBuff(Entity buffEntity, BuffModificationTypes buffModificationTypes, bool overwrite = false)
    {
        buffEntity.Add<BuffModificationFlagData>();
        var buffModificationFlagData = buffEntity.Read<BuffModificationFlagData>();
        if (overwrite)
        {
            buffModificationFlagData.ModificationTypes = (long)BuffModificationTypes.None;
        }
        buffModificationFlagData.ModificationTypes |= (long)buffModificationTypes;
        buffEntity.Write(buffModificationFlagData);
    }

    /// <summary>
    /// Applies a buff to an entity with optional duration and persistence settings.
    /// </summary>
    /// <param name="entity">The entity to apply the buff to.</param>
    /// <param name="buff">The PrefabGUID of the buff to apply.</param>
    /// <param name="buffEntity">Output parameter that receives the created buff entity.</param>
    /// <param name="duration">Duration of the buff in seconds. 0 for infinite duration.</param>
    /// <param name="attemptToPersistThroughDeath">If true, the buff will attempt to persist through death.</param>
    /// <returns>True if the buff was successfully applied, false otherwise.</returns>
    public static bool BuffEntity(Entity entity, PrefabGUID buff, out Entity buffEntity, float duration = 0, bool attemptToPersistThroughDeath = false)
    {
        var buffEvent = new ApplyBuffDebugEvent()
        {
            BuffPrefabGUID = buff
        };
        var fromCharacter = new FromCharacter()
        {
            User = PlayerService.GetUsersOnline().ToArray()[0],
            Character = entity
        };

        if (!HasBuff(entity, buff))
        {
            DebugEventsSystem.ApplyBuff(fromCharacter, buffEvent);
        }

        if (ServerGameManager.TryGetBuff(entity, buff, out buffEntity))
        {
            if (attemptToPersistThroughDeath || duration == 0)
            {
                buffEntity.Add<Buff_Persists_Through_Death>();
                buffEntity.Remove<Buff_Destroy_On_Owner_Death>();

                if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
                {
                    var buffer = buffEntity.ReadBuffer<RemoveBuffOnGameplayEventEntry>();
                    buffer.Clear();
                }
            }
            else if (!attemptToPersistThroughDeath)
            {
                buffEntity.Remove<Buff_Persists_Through_Death>();
                buffEntity.Add<Buff_Destroy_On_Owner_Death>();
            }

            if (duration == 0)
            {
                if (buffEntity.Has<LifeTime>())
                {
                    var lifetime = buffEntity.Read<LifeTime>();
                    buffEntity.Remove<Age>();
                    lifetime.Duration = 0;
                    lifetime.EndAction = LifeTimeEndAction.None;
                    buffEntity.Write(lifetime);
                }
            }
            else if (duration > 0)
            {
                if (buffEntity.Has<Age>())
                {
                    var age = buffEntity.Read<Age>();
                    age.Value = 0;
                    buffEntity.Write(age);
                }

                if (!buffEntity.Has<LifeTime>())
                {
                    buffEntity.Add<LifeTime>();
                }
                buffEntity.Write(new LifeTime
                {
                    EndAction = LifeTimeEndAction.Destroy,
                    Duration = duration
                });
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Applies a cosmetic buff to an entity and removes gameplay-related components.
    /// </summary>
    /// <param name="entity">The entity to apply the cosmetic to.</param>
    /// <param name="visual">The PrefabGUID of the cosmetic buff to apply.</param>
    public static void ApplyCosmetic(Entity entity, PrefabGUID visual)
    {
        ApplyBuffDebugEvent applyBuffDebugEvent = new()
        {
            BuffPrefabGUID = visual,
        };

        FromCharacter fromCharacter = new()
        {
            Character = entity,
            User = entity
        };

        DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);
        if (ServerGameManager.TryGetBuff(entity, applyBuffDebugEvent.BuffPrefabGUID.ToIdentifier(), out Entity buff))
        {
            HandleBuffComponent(buff);
            HandleLifetime(buff);

            if (!buff.Has<Buff_Persists_Through_Death>())
                buff.Add<Buff_Persists_Through_Death>();

            RemoveComponent<CreateGameplayEventsOnSpawn>(buff);
            RemoveComponent<GameplayEventListeners>(buff);
            RemoveComponent<RemoveBuffOnGameplayEvent>(buff);
            RemoveComponent<RemoveBuffOnGameplayEventEntry>(buff);
            RemoveComponent<DealDamageOnGameplayEvent>(buff);
            RemoveComponent<HealOnGameplayEvent>(buff);
            RemoveComponent<BloodBuffScript_ChanceToResetCooldown>(buff);
            RemoveComponent<ModifyMovementSpeedBuff>(buff);
            RemoveComponent<ApplyBuffOnGameplayEvent>(buff);
            RemoveComponent<DestroyOnGameplayEvent>(buff);
            RemoveComponent<WeakenBuff>(buff);
            RemoveComponent<ReplaceAbilityOnSlotBuff>(buff);
            RemoveComponent<AmplifyBuff>(buff);
            RemoveComponent<GameplayEventIdMapping>(buff);
        }
    }

    /// <summary>
    /// Removes a specific buff from an entity.
    /// </summary>
    /// <param name="entity">The entity to remove the buff from.</param>
    /// <param name="guid">The PrefabGUID of the buff to remove.</param>
    public static void RemoveBuff(Entity entity, PrefabGUID guid)
    {
        if (BuffUtility.TryGetBuff(Core.EntityManager, entity, guid, out var buff))
        {
            DestroyUtility.Destroy(Core.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);
        }
    }
    #endregion

    #region Public Helpers
    /// <summary>
    /// Checks if an entity has a specific buff.
    /// </summary>
    /// <param name="entity">The entity to check for the buff.</param>
    /// <param name="buff">The PrefabGUID of the buff to check for.</param>
    /// <returns>True if the entity has the specified buff, false otherwise.</returns>
    public static bool HasBuff(Entity entity, PrefabGUID buff)
    {
        return BuffUtility.HasBuff(Core.EntityManager, entity, buff);
    }
    #endregion

    #region Private Helpers
    private static void RemoveComponent<T>(Entity buff) where T : struct
    {
        if (buff.Has<T>())
        {
            buff.Remove<T>();
        }
    }

    private static void HandleBuffComponent(Entity buff)
    {
        if (buff.Has<Buff>())
        {
            BuffCategory component = buff.Read<BuffCategory>();
            component.Groups = BuffCategoryFlag.None;
            buff.Write(component);
        }
    }

    private static void HandleLifetime(Entity buff)
    {
        if (buff.Has<LifeTime>())
        {
            LifeTime lifetime = buff.Read<LifeTime>();
            lifetime.Duration = -1;
            lifetime.EndAction = LifeTimeEndAction.None;
            buff.Write(lifetime);
        }
    }
    #endregion
}