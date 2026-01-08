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
    public class AlbCatacombsObelisk : GameTeleporter
    {
        /// <summary>
        /// Add model and packageID to the teleporter.
        /// </summary>
        /// <returns></returns>
        public override bool AddToWorld()
        {
            PackageID = "Yggdrasil";
            Model = 1878;
            Realm = eRealm.Albion;
            return base.AddToWorld();
        }
        
        protected override string Type
        {
            get { return "Alb Catacombs"; }
        }
        
        /// <summary>
        /// Player right-clicked the teleporter.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            String intro = String.Format(
                "Adventurer, the catacombs are a dangerous place, but I may be able to ease the burden of travel. {0} {1} {2} {3} {4} {5} {6} {7} {8}",
                "Locations I can teleport your mortal body too are as follows.\n", 
                "\n", 
                "Will you venture into the burning depths of the [Glashtin Forge],\n",
                "walk among the massive mushrooms in the [Underground Forest],\n",
                "scour the [Deadlands of Annwn] for lost valuables,\n",
                "delve into the haunted depths of the [Lower Crypt],\n", 
                "join the fight against the corrupted hordes on the [Frontlines],\n", 
                "find your fortunes in the network of tunnels beneath Albion in the [Abandoned Mines].\n \n",
                "or return to the relative safety of the [Inconnu Crypt].");

            SayTo(player, intro);
            return true;
        }

        /// <summary>
        /// Player has Picked a destination.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected override void OnDestinationPicked(GamePlayer player, Teleport destination)
        {
            SayTo(player, "Safe Travels Mortal");
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