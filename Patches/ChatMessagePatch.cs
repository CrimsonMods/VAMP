using HarmonyLib;
using ProjectM.Network;
using ProjectM;
using Unity.Entities;
using Unity.Collections;
using VAMP.Services;
using VAMP.Utilities;
using System.Linq;

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

                if (messageText == "!vote")
                {
                    if (EventScheduler.IsVoting())
                    {
                        EventScheduler.AddVote(userData.CharacterName.Value);
                        continue;
                    }
                    else
                    {
                        ChatUtil.SystemSendUser(userData, $"There is currently no event vote in progress.");
                    }
                }

                if (chatEventData.MessageType == ChatMessageType.System) continue;
                if (!userData.IsAdmin) continue;

                if(messageText.StartsWith("!!hex"))
                {
                    var parts = messageText.Split(' ');
                    if (parts.Length > 1)
                    {
                        var hexString = parts[1];
                        ChatUtil.SystemSendUser(userData, $"The dog jumped <color=#{hexString}>over the moon</color>");
                    }
                }

                if (messageText == "!!spawnDebug")
                    Plugin.SpawnDebug = !Plugin.SpawnDebug;

                if (messageText == "!!chatDebug")
                {
                    DevUtil.ChatDebugMode = !DevUtil.ChatDebugMode;
                    ChatUtil.SystemSendUser(userData, $"Chat Debug Mode: {DevUtil.ChatDebugMode}");

                    if (DevUtil.ChatDebugMode == false)
                    {
                        DevUtil.ChatDebugKeys = null;
                    }
                }

                if (messageText.StartsWith("!!chatDebugKey"))
                {
                    var parts = messageText.Split(' ');
                    if (parts.Length > 1)
                    {
                        DevUtil.ChatDebugKeys = parts.Skip(1).ToArray();
                        ChatUtil.SystemSendUser(userData, $"Chat Debug Keys: {string.Join(", ", DevUtil.ChatDebugKeys)}");
                    }
                }
            }
        }

        return true;
    }
}
