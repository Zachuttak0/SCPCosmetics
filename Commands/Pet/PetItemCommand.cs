﻿namespace SCPCosmetics.Commands.Pet
{
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.Permissions.Extensions;
    using MEC;
    using PlayerRoles;
    using System;

    public class PetItemCommand : ICommand
    {
        public string Command => "item";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Give your pet an item!";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "This command can only be ran by a player!";
                return true;
            }

            // Pet items config check.
            if (!Plugin.Instance.Config.PetsCanHoldItems)
            {
                response = "Pets cannot hold items on this server!";
                return true;
            }

            if (!Plugin.Instance.Config.EnablePets)
            {
                response = "Pets are disabled on this server.";
                return false;
            }

            if (!sender.CheckPermission("scpcosmetics.pet"))
            {
                response = "You do not have access to the Pet command!";
                return false;
            }

            // Arguments check.
            if (arguments.Count == 0)
            {
                response = "Usage: .pet item <itemtype>";
            }

            if (player.Role.Team == Team.Dead)
            {
                response = "Please wait until you spawn in as a normal class.";
                return false;
            }

            if (Plugin.Instance.CheckPetRateLimited(player.Id))
            {
                response = "You are ratelimited.";
                return false;
            }

            Plugin.Instance.PetRateLimitPlayer(player.Id, 3d);

            if (!Plugin.Instance.PetDictionary.TryGetValue($"pet-{player.UserId}", out Npc petNpc))
            {
                response = "You don't currently have a pet spawned in!";
                return true;
            }

            if (Pets.allowedPetItems.TryGetValue(arguments.At(0), out ItemType HeldItem))
            {
                if (HeldItem == ItemType.None)
                {
                    petNpc.ClearInventory();
                }
                else
                {
                    Timing.CallDelayed(0.5f, () =>
                    {
                        petNpc.ClearInventory();
                        petNpc.CurrentItem = Item.Create(HeldItem, petNpc);
                    });
                }

                response = $"Set pet's held item to type '{arguments.At(0)}'";
                return true;
            }
            else
            {
                response = "Couldn't find an allowed item with this name. Maybe the item was disabled by server staff.";
                return false;
            }
        }
    }
}
