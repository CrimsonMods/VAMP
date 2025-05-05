using ProjectM;
using ProjectM.Network;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAMP.Models;
using VAMP.Services;

namespace VAMP.Utilities;

/// <summary>
/// Utility class for handling chat-related functionality.
/// </summary>
public static class ChatUtil
{
    /// <summary>
    /// Sends a system message to a specific user.
    /// </summary>
    /// <param name="user">The user to send the message to</param>
    /// <param name="message">The message to send</param>
    public static void SystemSendUser(User user, string message)
    {
        FixedString512Bytes fixedMessage = message;
        ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref fixedMessage);
    }

    /// <summary>
    /// Sends a system message to all online users.
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="translation">The position from which the message originates</param>
    public static void SystemSendAll(string message)
    {
        FixedString512Bytes fixedMessage = message;
        ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, ref fixedMessage);
    }

    /// <summary>
    /// Sends a system message to all online users except the specified user.
    /// </summary>
    /// <param name="user">The user to exclude from the message</param>
    /// <param name="message">The message to send</param>
    public static void SystemSendAllExcept(User user, string message)
    {
        List<Entity> players = PlayerService.GetUsersOnline()
            .Where(x => !x.Read<User>().Equals(user))
            .ToList();

        foreach (var player in players)
        {
            if (player.Read<User>().Equals(user)) continue;

            SystemSendUser(player.Read<User>(), message);
        }
    }

    /// <summary>
    /// Sends a system message to all online users except the specified users.
    /// </summary>
    /// <param name="users">Array of users to exclude from the message</param>
    /// <param name="message">The message to send</param>
    public static void SystemSendAllExcept(User[] users, string message)
    {
        List<Entity> players = PlayerService.GetUsersOnline()
            .Where(x => !users.Contains(x.Read<User>()))
            .ToList();

        foreach (var player in players)
        {
            SystemSendUser(player.Read<User>(), message);
        }
    }
    
    /// <summary>
    /// Sends a system message to nearby players within a specified radius.
    /// </summary>
    /// <param name="user">The user sending the message</param>
    /// <param name="translation">The position from which to calculate the radius</param>
    /// <param name="message">The message to send</param>
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
                    SystemSendUser(player.Read<User>(), message);
                }
            }
        }
    }

    /// <summary>
    /// Sends a system message to all members of the user's clan team.
    /// </summary>
    /// <param name="user">The user sending the message</param>
    /// <param name="message">The message to send</param>
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

            SystemSendUser(player.Read<User>(), message);
        }
    }

    /// <summary>
    /// Sends a system message to all online administrators.
    /// </summary>
    /// <param name="message">The message to send to administrators</param>
    public static void SystemSendAdmins(string message)
    {
        List<Player> players = PlayerService.GetCachedUsersOnlineAsPlayer().Where(x => x.IsAdminCapable).ToList();

        foreach (var player in players)
        {
            SystemSendUser(player.User.Read<User>(), message);
        }
    }

    /// <summary>
    /// Wraps text in a color tag with the specified hex color code.
    /// </summary>
    /// <param name="text">The text to color</param>
    /// <param name="hex">The hex color code (with or without # prefix)</param>
    /// <returns>Text wrapped in a Unity rich text color tag</returns>
    public static string Color(string text, string hex)
    {
        return $"<color=#{hex.Replace(" ", "").Replace("#", "")}>{text}</color>";
    }
}