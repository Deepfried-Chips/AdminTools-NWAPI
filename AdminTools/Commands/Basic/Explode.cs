﻿using CommandSystem;
using PluginAPI.Core;
using System;
using System.Linq;

namespace AdminTools.Commands.Basic
{

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class Explode : ParentCommand
    {
        public Explode() => LoadGeneratedCommands();

        public override string Command => "expl";

        public override string[] Aliases { get; } =
        {
            "boom"
        };

        public override string Description => "Explodes a specified user or everyone instantly";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender) sender).CheckPermission(PlayerPermissions.ForceclassToSpectator))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count >= 1)
                return arguments.At(0) switch
                {
                    "*" or "all" => All(arguments, out response),
                    _ => HandleDefault(arguments, out response)
                };
            response = "Usage: expl ((player id / name) or (all / *))";
            return false;

        }
        private static bool All(ArraySegment<string> arguments, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: expl (all / *)";
                return false;
            }

            foreach (Player p in Player.GetPlayers().Where(Extensions.IsAlive))
            {
                p.Kill("Exploded by admin.");
                Handlers.CreateThrowable(ItemType.GrenadeHE).SpawnActive(p.Position, .5f, p);
            }
            response = "Everyone exploded, Hubert cannot believe you have done this";
            return true;
        }
        private static bool HandleDefault(ArraySegment<string> arguments, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: expl (player id / name)";
                return false;
            }

            Player p = Extensions.GetPlayer(arguments.At(0));
            if (p == null)
            {
                response = $"Invalid target to explode: {arguments.At(0)}";
                return false;
            }

            if (!p.IsAlive)
            {
                response = $"Player \"{p.Nickname}\" is not a valid class to explode";
                return false;
            }

            p.Kill("Exploded by admin.");
            Handlers.CreateThrowable(ItemType.GrenadeHE).SpawnActive(p.Position, .1f, p);
            response = $"Player \"{p.Nickname}\" game ended (exploded)";
            return true;
        }
    }
}