﻿using CommandSystem;
using NorthwoodLib.Pools;
using System;
using System.Linq;
using System.Text;
using Utils.NonAllocLINQ;

namespace AdminTools.Commands.BreakDoors
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class BreakDoors : ParentCommand, IDefaultPermissions
    {
        public BreakDoors() => LoadGeneratedCommands();

        public override string Command => "breakdoors";

        public override string[] Aliases { get; } =
        {
            "bd"
        };

        public override string Description => "Manage break door properties for users";

        public override void LoadGeneratedCommands() { }

        public PlayerPermissions Permissions => PlayerPermissions.ForceclassWithoutRestrictions;

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(this, out response))
                return false;

            if (arguments.Count >= 1)
                return arguments.At(0).ToLower() switch
                {
                    "clear" => HandleClear(out response),
                    "list" => List(out response),
                    "remove" => Remove(arguments, out response),
                    "*" => All(out response),
                    "all" => All(out response),
                    _ => HandleDefault(arguments, out response)
                };

            response = "Usage:\nbreakdoors ((player id / name) or (all / *))" +
                "\nbreakdoors clear" +
                "\nbreakdoors list" +
                "\nbreakdoors remove (player id / name)";
            return false;

        }
        private static bool HandleDefault(ArraySegment<string> arguments, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: breakdoors (player id / name)";
                return false;
            }

            AtPlayer p = Extensions.GetPlayer(arguments.At(0));
            if (p == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            p.BreakDoorsEnabled = !p.BreakDoorsEnabled;
            response = $"Break doors is now {(p.BreakDoorsEnabled ? "on" : "off")} for {p.Nickname}";
            return true;
        }
        private static bool All(out string response)
        {
            ListExtensions.ForEach(Extensions.Players, p => p.BreakDoorsEnabled = true);
            response = "Everyone on the server can instantly kill other users now";
            return true;
        }
        private static bool Remove(ArraySegment<string> arguments, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: breakdoors remove (player id / name)";
                return false;
            }

            AtPlayer p = Extensions.GetPlayer(arguments.At(1));
            if (p == null)
            {
                response = $"Player not found: {arguments.At(1)}";
                return false;
            }

            if (p.BreakDoorsEnabled)
            {
                p.BreakDoorsEnabled = false;
                response = $"Break doors turned off for {p.Nickname}";
            }
            else
                response = $"Player {p.Nickname} does not have the ability to break doors";
            return true;
        }
        private static bool List(out string response)
        {
            AtPlayer[] list = Extensions.Players.Where(p => p.BreakDoorsEnabled).ToArray();
            StringBuilder playerLister = StringBuilderPool.Shared.Rent(list.Length != 0 ? "Players with break doors on:\n" : "No players currently online have break doors on");
            if (list.Length == 0)
            {
                response = playerLister.ToString();
                return true;
            }

            playerLister.Append(list.JoinNicknames());
            response = StringBuilderPool.Shared.ToStringReturn(playerLister);
            return true;
        }
        private static bool HandleClear(out string response)
        {
            ListExtensions.ForEach(Extensions.Players, p => p.BreakDoorsEnabled = false);
            response = "Door breaking has been removed from everyone";
            return true;
        }

    }
}
