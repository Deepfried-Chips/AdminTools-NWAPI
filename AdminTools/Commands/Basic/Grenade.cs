﻿using CommandSystem;
using PlayerRoles;
using PluginAPI.Core;
using System;
using System.Linq;

namespace AdminTools.Commands.Basic
{

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class Grenade : ParentCommand
    {
        public Grenade() => LoadGeneratedCommands();

        public override string Command => "grenade";

        public override string[] Aliases { get; } =
        {
            "gn"
        };

        public override string Description => "Spawns a HE grenade/flashbang/SCP-018 on a user or users";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!ValidateArguments(arguments, sender, out response, out ItemType type, out float fuseTime))
                return false;

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    if (type == ItemType.SCP018)
                        Cassie.Message("pitch_1.5 xmas_bouncyballs", true, false);

                    foreach (Player p in Player.GetPlayers().Where(player => player.Role != RoleTypeId.Spectator))
                        Handlers.CreateThrowable(type).SpawnActive(p.Position, fuseTime);

                    response = "Grenade has been sent to everyone";
                    return true;
                default:
                {
                    Player p = int.TryParse(arguments.At(0), out int id) ? Player.GetPlayers().FirstOrDefault(x => x.PlayerId == id) : Player.GetByName(arguments.At(0));
                    if (p is null)
                    {
                        response = $"Player {arguments.At(0)} not found.";
                        return false;
                    }

                    Handlers.CreateThrowable(type).SpawnActive(p.Position, fuseTime);
                    response = $"Grenade has been sent to {p.Nickname}";
                    return true;
                }
            }
        }
        private static bool ValidateArguments(ArraySegment<string> arguments, ICommandSender sender, out string response, out ItemType type, out float fuseTime)
        {
            type = ItemType.None;
            fuseTime = -1f;
            if (!((CommandSender) sender).CheckPermission(PlayerPermissions.GivingItems))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: grenade ((player id / name) or (all / *)) (GrenadeType) [fuse time]";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out type) || type is not (ItemType.SCP018 or ItemType.GrenadeHE or ItemType.GrenadeFlash))
            {
                response = $"Invalid value for grenade type: {arguments.At(1)}, expected SCP018 or GrenadeHE or GrenadeFlash";
                return false;
            }

            if (arguments.Count > 2 && !float.TryParse(arguments.At(2), out fuseTime))
            {
                response = $"Invalid fuse time for grenade: {arguments.At(2)}! Set to negative to use default fuse time";
                return false;
            }
            response = "";
            return true;
        }
    }
}