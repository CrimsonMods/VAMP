using ProjectM;
using ProjectM.Network;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAMP.Services;

namespace VAMP.Utilities;

public static class ChatUtil
{
    public static void SystemSendLocal(User user, float3 translation, string message)
    {
        float maxDistance = 40f;
        float2 currentPoint = new float2(translation.x, translation.z);

        var onlinePlayers = PlayerService.GetUsersOnline();
        foreach (var player in onlinePlayers)
        {
            if (player.Read<User>().Equals(user)) continue;

            if (player.TryGetComponent(out LocalToWorld localToWorld))
            {
                var playerPos = new float2(localToWorld.Position.x, localToWorld.Position.z);
                var distance = math.distance(playerPos, currentPoint);
                if (distance <= maxDistance)
                {
                    ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, player.Read<User>(), message);
                }
            }
        }
    }

    public static void SystemSendTeam(User user, string message)
    {
        List<Entity> players = PlayerService.GetUsersOnline()
            .Where(x => x.Read<User>().ClanEntity._Entity.Has<ClanTeam>())
            .ToList();

        List<Entity> members = players
            .Where(x => x.Read<User>().ClanEntity._Entity.Equals(user.ClanEntity._Entity))
            .ToList();

        foreach (var player in members)
        {
            if (player.Read<User>().Equals(user)) continue;

            ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, player.Read<User>(), message);
        }
    }

    public static string Color(string text, string hex)
    {
        return $"<color=#{hex.Replace(" ", "").Replace("#", "")}>{text}</color>";
    }
}
