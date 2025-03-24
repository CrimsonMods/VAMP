using Il2CppSystem;
using ProjectM;
using Stunlock.Core;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace VAMP.Utilities;

public static class ItemUtil
{
    public static void ClearEntityInventory(Entity entity, bool all = false)
    { 
        var buffer = entity.ReadBuffer<InventoryInstanceElement>();
        foreach (var item in buffer)
        {
            InventoryUtilitiesServer.ClearInventory(Core.EntityManager, item.ExternalInventoryEntity._Entity);
        }
    }

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
                    Plugin.LogInstance.LogInfo($"Could not create dropped item: {pos} {prefabGUID.LookupName()} {amount}");
                    var action = () => CreateDroppedItem(pos, prefabGUID, amount);
                    Reattempt(action);
                    return;
                }
            }
        }
    }

    private static async void Reattempt(Action action)
    {
        await Task.Delay(300);
        action.Invoke();
    }
}
