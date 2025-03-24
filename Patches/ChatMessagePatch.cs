using HarmonyLib;
using ProjectM.Network;
using ProjectM;
using Unity.Entities;
using Unity.Collections;
using VAMP.Services;

namespace VAMP.Patches;

[HarmonyPatch]
public static class ChatMessagePatch
{
    [HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
    public static bool Prefix(ChatMessageSystem __instance)
    {
        if (__instance.__query_661171423_0 != null)
        {
            NativeArray<Entity> entities = __instance.__query_661171423_0.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var fromData = __instance.EntityManager.GetComponentData<FromCharacter>(entity);
                var userData = __instance.EntityManager.GetComponentData<User>(fromData.User);
                var chatEventData = __instance.EntityManager.GetComponentData<ChatMessageEvent>(entity);

                var messageText = chatEventData.MessageText.ToString();

                if(messageText == "!vote")
                {
                    if(EventScheduler.IsVoting())
                    {
                        EventScheduler.AddVote(userData.CharacterName.Value);
                        continue;
                    }
                    else
                    {
                        ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, userData, $"There is currently no event vote in progress.");
                    }
                }

                if (chatEventData.MessageType == ChatMessageType.System) continue;
                if (!userData.IsAdmin) continue;

                if (messageText == "!!spawnDebug")
                    Plugin.SpawnDebug = !Plugin.SpawnDebug;
            }
        }

        return true;
    }
}
