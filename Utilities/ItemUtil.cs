using Il2CppSystem;
using ProjectM;
using Stunlock.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace VAMP.Utilities;

/// <summary>
/// Utility class for handling item-related operations in the game.
/// </summary>
public static class ItemUtil
{
    /// <summary>
    /// Clears the inventory of a specified entity.
    /// </summary>
    /// <param name="entity">The entity whose inventory should be cleared.</param>
    /// <param name="all">If true, clears all inventory items. Default is false.</param>
    public static void ClearEntityInventory(Entity entity, bool all = false)
    {
        var buffer = entity.ReadBuffer<InventoryInstanceElement>();
        foreach (var item in buffer)
        {
            InventoryUtilitiesServer.ClearInventory(Core.EntityManager, item.ExternalInventoryEntity._Entity);
        }
    }

    /// <summary>
    /// Adds an item to an entity's inventory with optional equipping and dropping functionality.
    /// </summary>
    /// <param name="recipient">The entity receiving the item.</param>
    /// <param name="guid">The prefab GUID of the item to add.</param>
    /// <param name="amount">The quantity of items to add.</param>
    /// <param name="entity">Output parameter that receives the newly created item entity.</param>
    /// <param name="equip">If true, attempts to equip the item after adding. Default is true.</param>
    /// <param name="slot">The inventory slot to place the item in. Default is 0.</param>
    /// <param name="drop">If true, drops excess items that don't fit in inventory. Default is true.</param>
    /// <returns>True if the item was successfully added to the inventory, false otherwise.</returns>
    public static bool AddItemToInventory(Entity recipient, PrefabGUID guid, int amount, out Entity entity, bool equip = true, int slot = 0, bool drop = true)
    {
        var inventoryResponse = Core.ServerGameManager.TryAddInventoryItem(recipient, guid, amount, new Nullable_Unboxed<int>(slot), false);
        if (inventoryResponse.RemainingAmount > 0 && drop)
        {
            if (recipient.Has<Translation>())
            {
                CreateDroppedItem(recipient.Read<Translation>().Value, guid, inventoryResponse.RemainingAmount);
            }
        }
        if (inventoryResponse.Success)
        {
            entity = inventoryResponse.NewEntity;
            if (equip)
            {
                // TODO: Add equip item logic
                //EquipItem(recipient, entity);
            }
            return true;
        }
        else
        {
            entity = new Entity();
            return false;
        }
    }

    /// <summary>
    /// Creates a dropped item in the game world at a specified position.
    /// </summary>
    /// <param name="pos">The position where the item should be dropped.</param>
    /// <param name="prefabGUID">The prefab GUID of the item to drop.</param>
    /// <param name="amount">The quantity of items to drop.</param>
    public static void CreateDroppedItem(float3 pos, PrefabGUID prefabGUID, int amount)
    {
        if (Core.SystemService.GameDataSystem.ItemHashLookupMap.TryGetValue(prefabGUID, out var itemData))
        {
            while (amount > 0)
            {
                try
                {
                    var amountToDrop = System.Math.Min(amount, itemData.MaxAmount);
                    Core.ServerScriptMapper._ServerGameManager.CreateDroppedItemEntity(pos, prefabGUID, amountToDrop);
                    amount -= amountToDrop;
                }
                catch (System.Exception e)
                {
                    Plugin.LogInstance.LogWarning($"Could not create dropped item: {pos} {prefabGUID.LookupName()} {amount}");
                    var action = () => CreateDroppedItem(pos, prefabGUID, amount);
                    Reattempt(action);
                    return;
                }
            }
        }
    }

    public static string CleanItemName(string input)
    {
        // Check for Nether Shards first
        var netherShardName = GetNetherShardName(input);
        if (netherShardName != null) return netherShardName;

        // Check for Gems
        var gemName = GetGemName(input);
        if (gemName != null) return gemName;

        // Check for known prefixes
        var transformation = PrefabTransformations.FirstOrDefault(x => input.Contains(x.Key));
        if (transformation.Key != null) return transformation.Value(input);

        // Default fallback
        return input.Split('_').Skip(1).Aggregate((a, b) => $"{a} {b}");
    }

    private static readonly Dictionary<string, System.Func<string, string>> PrefabTransformations = new()
    {
        ["Item_Weapon_"] = s => s.Replace("Item_Weapon_", "")
                                .Replace("_T\\d{2}", "")
                                .Replace("_", " ")
                                .Split(' ')
                                .Reverse()
                                .Aggregate((a, b) => $"{a} {b}"),

        ["Item_Ingredient_Mineral_"] = s => s.Replace("Item_Ingredient_Mineral_", "")
                                           .Replace("_", " ")
                                           .Split(' ')
                                           .Aggregate((a, b) => $"{a} {b}"),

        ["Item_Ingredient_Coin_"] = s => s.Replace("Item_Ingredient_Coin_", "")
                                                   .Replace("_", " ") + " Coin"
                                                   .Replace("Royal", "Goldsun"),

        ["Item_Ingredient_"] = s => s.Replace("Item_Ingredient_", "")
                                    .Replace("_", " ")
                                    .Split(' ')
                                    .Aggregate((a, b) => $"{a} {b}"),

        ["Item_MagicSource_SoulShard"] = s => $"Soul Shard of {s.Replace("Item_MagicSource_SoulShard_", "").Replace("Manticore", "the Winged Horror").Replace("Monster", "the Monster")}",

        ["Item_Cloak_"] = s => s.Replace("Item_Cloak_", "")
                               .Replace("_T\\d{2}_", " ")
                               .Replace("_", " ")
                               .Split(' ')
                               .Aggregate((a, b) => $"{a} {b}") + " Cloak",

        ["Item_Headgear_"] = s => s.Replace("Item_Headgear_", "")
                                            .Replace("_", " ")
                                            .Split(' ')
                                            .Aggregate((a, b) => $"{a} {b}"),

        ["Item_Building_Sapling_"] = s => s.Replace("Item_Building_Sapling_", "")
                                                        .Replace("_Seed", "")
                                                        .Replace("_", " ") + " Sapling",

        ["Item_Consumable_Eat_"] = s => string.Concat(s.Replace("Item_Consumable_Eat_", "")
                                                        .Select(c => char.IsUpper(c) ? " " + c : c.ToString()))
                                                        .TrimStart()
                                                        .Replace("_", " ")
                                                        .Split(' ')
                                                        .Aggregate((a, b) => $"{a} {b}"),

    };

    private static string GetNetherShardName(string input) => input switch
    {
        var s when s.Contains("NetherShard_T01") => "Stygian Shard",
        var s when s.Contains("NetherShard_T02") => "Greater Stygian Shard",
        var s when s.Contains("NetherShard_T03") => "Primal Stygian Shard",
        _ => null
    };

    private static string GetGemName(string input) => input switch
    {
        var s when s.Contains("_T01") => "Crude " + s.Split('_')[3],
        var s when s.Contains("_T02") => "Regular " + s.Split('_')[3],
        var s when s.Contains("_T03") => "Flawless " + s.Split('_')[3],
        var s when s.Contains("_T04") => "Perfect " + s.Split('_')[3],
        _ => null
    };

    private static async void Reattempt(Action action)
    {
        await Task.Delay(300);
        action.Invoke();
    }
}