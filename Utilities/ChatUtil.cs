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
    /// The maximum character limit for a single chat message.
    /// </summary>
    /// <remarks>
    /// Calculated as 508 (total buffer size) - 26 (header size) - 2 (prefix size) - 20 (reserved space).
    /// </remarks>
    public static readonly int MESSAGE_LIMIT = 508 - 26 - 2 - 20;

    /// <summary>
    /// Sends a system message to a specific user.
    /// </summary>
    /// <param name="user">The user to send the message to</param>
    /// <param name="message">The message to send</param>
    public static void SystemSendUser(User user, string message)
    {
        var messages = SplitMessage(message, MESSAGE_LIMIT);
        foreach (var msg in messages)
        {
            FixedString512Bytes fixedMessage = msg;
            ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref fixedMessage);
        }
    }

    /// <summary>
    /// Sends a system message to multiple specified users.
    /// </summary>
    /// <param name="users">Array of users to send the message to</param>
    /// <param name="message">The message to send</param>
    public static void SystemSendUsers(User[] users, string message)
    {
        var messages = SplitMessage(message, MESSAGE_LIMIT);
        foreach (var user in users)
        {
            foreach (var msg in messages)
            {
                FixedString512Bytes fixedMessage = msg;
                ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref fixedMessage);
            }
        }
    }

    /// <summary>
    /// Sends a system message to all online users.
    /// </summary>
    /// <param name="message">The message to send</param>
    public static void SystemSendAll(string message)
    {
        var messages = SplitMessage(message, MESSAGE_LIMIT);
        foreach (var msg in messages)
        {
            FixedString512Bytes fixedMessage = msg;
            ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, ref fixedMessage);
        }
    }

    /// <summary>
    /// Sends a system message to all online users except the specified user.
    /// </summary>
    /// <param name="user">The user to exclude from the message</param>
    /// <param name="message">The message to send</param>
    public static void SystemSendAllExcept(User user, string message)
    {
        var messages = SplitMessage(message, MESSAGE_LIMIT);
        List<Entity> players = PlayerService.GetUsersOnline()
            .Where(x => !x.Read<User>().Equals(user))
            .ToList();

        foreach (var player in players)
        {
            if (player.Read<User>().Equals(user)) continue;

            foreach (var msg in messages)
            {
                FixedString512Bytes fixedMessage = msg;
                ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, player.Read<User>(), ref fixedMessage);
            }
        }
    }

    /// <summary>
    /// Sends a system message to all online users except the specified users.
    /// </summary>
    /// <param name="users">Array of users to exclude from the message</param>
    /// <param name="message">The message to send</param>
    public static void SystemSendAllExcept(User[] users, string message)
    {
        var messages = SplitMessage(message, MESSAGE_LIMIT);
        List<Entity> players = PlayerService.GetUsersOnline()
            .Where(x => !users.Contains(x.Read<User>()))
            .ToList();

        foreach (var player in players)
        {
            foreach (var msg in messages)
            {
                FixedString512Bytes fixedMessage = msg;
                ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, player.Read<User>(), ref fixedMessage);
            }
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
        var messages = SplitMessage(message, MESSAGE_LIMIT);
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
                    foreach (var msg in messages)
                    {
                        FixedString512Bytes fixedMessage = msg;
                        ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, player.Read<User>(), ref fixedMessage);
                    }
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
        var messages = SplitMessage(message, MESSAGE_LIMIT);
        List<Entity> players = PlayerService.GetUsersOnline()
            .Where(x => x.Read<User>().ClanEntity._Entity.Has<ClanTeam>())
            .ToList();

        List<Entity> members = players
            .Where(x => x.Read<User>().ClanEntity._Entity.Equals(user.ClanEntity._Entity))
            .ToList();

        foreach (var player in members)
        {
            if (player.Read<User>().Equals(user)) continue;

            foreach (var msg in messages)
            {
                FixedString512Bytes fixedMessage = msg;
                ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, player.Read<User>(), ref fixedMessage);
            }
        }
    }

    /// <summary>
    /// Sends a system message to all online administrators.
    /// </summary>
    /// <param name="message">The message to send to administrators</param>
    public static void SystemSendAdmins(string message)
    {
        var messages = SplitMessage(message, MESSAGE_LIMIT);
        List<Player> players = PlayerService.GetCachedUsersOnlineAsPlayer().Where(x => x.IsAdminCapable).ToList();

        foreach (var player in players)
        {
            foreach (var msg in messages)
            {
                FixedString512Bytes fixedMessage = msg;
                ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, player.User.Read<User>(), ref fixedMessage);
            }
        }
    }

    #region Helper Functions
    private static List<string> SplitMessage(string message, int maxLength)
    {
        var result = new List<string>();
        
        if (string.IsNullOrEmpty(message))
            return result;

        // If message is within limit, return as-is
        if (message.Length <= maxLength)
        {
            result.Add(message);
            return result;
        }

        var lines = message.Split('\n');
        var currentMessage = "";
        
        foreach (var line in lines)
        {
            // If adding this line would exceed the limit
            if (currentMessage.Length + line.Length + 1 > maxLength)
            {
                // If we have content in currentMessage, add it to results
                if (!string.IsNullOrEmpty(currentMessage))
                {
                    result.Add(currentMessage.TrimEnd());
                    currentMessage = "";
                }
                
                // If a single line is too long, split it on spaces
                if (line.Length > maxLength)
                {
                    var splitLine = SplitLongLine(line, maxLength);
                    result.AddRange(splitLine);
                }
                else
                {
                    currentMessage = line + "\n";
                }
            }
            else
            {
                currentMessage += line + "\n";
            }
        }
        
        // Add any remaining content
        if (!string.IsNullOrEmpty(currentMessage))
        {
            result.Add(currentMessage.TrimEnd());
        }
        
        return result;
    }
    
    private static List<string> SplitLongLine(string line, int maxLength)
    {
        var result = new List<string>();
        var words = line.Split(' ');
        var currentLine = "";
        
        foreach (var word in words)
        {
            // If adding this word would exceed the limit
            if (currentLine.Length + word.Length + 1 > maxLength)
            {
                // Add current line if it has content
                if (!string.IsNullOrEmpty(currentLine))
                {
                    result.Add(currentLine.TrimEnd());
                    currentLine = "";
                }
                
                // If a single word is too long, we have to break it (edge case)
                if (word.Length > maxLength)
                {
                    result.Add(word.Substring(0, maxLength - 3) + "...");
                    // Could continue splitting the word, but for practical purposes this should be rare
                }
                else
                {
                    currentLine = word + " ";
                }
            }
            else
            {
                currentLine += word + " ";
            }
        }
        
        // Add any remaining content
        if (!string.IsNullOrEmpty(currentLine))
        {
            result.Add(currentLine.TrimEnd());
        }
        
        return result;
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
    #endregion
}