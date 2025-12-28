/*
 * This component is part of my custom work for my private DOLSharp Server, named Oasis DAoC
 *
 * I am attempting to create a personal space that feels live like, but leans towards a coop
 * experience and some quality of life tweaks.
 *
 * This component is meant to be a work in progress to create new Teleporters that are more
 * professional looking in game and follow the original live like teleporters instead of
 * just creating a list of locations for the player to click.
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.Database;
using System.Collections;
using DOL.GS.Spells;
using log4net;
using System.Reflection;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    /// <summary>
    /// Coop Dungeon Teleporter
    /// </summary>
    /// <author>CMeyerJohnson</author>
    public class DungeonTeleporter : GameTeleporter
    {
        /// <summary>
        /// Add model and packageID to the teleporter.
        /// </summary>
        /// <returns></returns>
        public override bool AddToWorld()
        {
            PackageID = "Yggdrasil";
            Model = 1280;
            Realm = eRealm.Albion;
            return base.AddToWorld();
        }
        
        protected override string Type
        {
            get { return "Dungeon Teleporter"; }
        }
        
        /// <summary>
        /// Player right-clicked the teleporter.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            String intro = String.Format(
                "Greetings Fleshling, I require assistance in exploring some out of the way locals. {0} {1} {2}",
                "These underground expeditions span the realms of men, and I require information from them all.\n", 
                "\n", 
                "Will you explore the [na sidhe of Hibernia], [hollows of Midgard], or [caverns of Albion]");

            SayTo(player, intro);
            return true;
        }

        /// <summary>
        /// Player has picked a subselection.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="subSelection"></param>
        protected override void OnSubSelectionPicked(GamePlayer player, Teleport subSelection)
        {
            switch (subSelection.TeleportID.ToLower())
            {
                case "na sidhe of hibernia":
                {
                    String reply = String.Format(
                        "the areas I require assistance for in Hibernia are: \n {0}, {1}, {2}, {3}, {4}", 
                        "[Muire's Tomb] levels 8-20\n", 
                        "[Spraggon Den] levels 20-25\n", 
                        "[Koalinth Caverns] levels 22-28\n", 
                        "[Treidh Caillte] levels 30-40\n", 
                        "[Coruscating Mines] levels 35-50\n");
                    SayTo(player, reply);
                    return;
                }
                case "hollows of midgard":
                {
                    String reply = String.Format(
                        "the areas I require assistance for in Midgard are: \n {0}, {1}, {2}, {3}, {4}", 
                        "[Nisse's Lair] levels 7-20\n", 
                        "[Cursed Tomb] levels 15-22\n", 
                        "[Vendo Caverns] levels 16-20\n", 
                        "[Varulvhamn] levels 30-40\n", 
                        "[Spindelhalla] levels 30-45\n");
                    SayTo(player, reply);
                    return;
                }
                case "caverns of albion":
                {
                    String reply = String.Format(
                        "the areas I require assistance for in Albion are: \n {0}, {1}, {2}, {3}, {4}", 
                        "[Tomb of Mithra] levels 7-20\n", 
                        "[Keltoi Fogou] levels 15-30\n", 
                        "[Tepok's Mine] levels 17-36\n", 
                        "[Catacombs of Cardova] levels 25-35\n", 
                        "[Stonehenge Barrows] levels 30-50\n");
                    SayTo(player, reply);
                    return;
                }
            }
            base.OnSubSelectionPicked(player, subSelection);
        }

        /// <summary>
        /// Player has Picked a destination.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected override void OnDestinationPicked(GamePlayer player, Teleport destination)
        {
            SayTo(player, "Travel safe fleshling");
            base.OnDestinationPicked(player, destination);
        }

        /// <summary>
        /// Teleport the player to the designated coordinates.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected override void OnTeleport(GamePlayer player, Teleport destination)
        {
            OnTeleportSpell(player, destination);
        }
    }
}