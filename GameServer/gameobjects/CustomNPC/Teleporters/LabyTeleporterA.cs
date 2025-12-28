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
    /// Labyrinth Entrance Teleporter
    /// </summary>
    /// <author>CMeyerJohnson</author>
    public class LabyATeleporter : GameTeleporter
    {
        /// <summary>
        /// Add model and packageID to the teleporter.
        /// </summary>
        /// <returns></returns>
        public override bool AddToWorld()
        {
            PackageID = "Yggdrasil";
            // randomly selects Minotaur Model for Teleporter
            ushort[] minotaurModels = { 1407, 1419, 1395 };
            Random random = new Random();
            Model = minotaurModels[random.Next(minotaurModels.Length)];
            
            return base.AddToWorld();
        }
        
        protected override string Type
        {
            get { return "Labyrinth"; }
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
                "You stand before the great Emissary of the Minotaur, hornless one., \n {0} {1} {2}",
                "Three paths lead to the maze's heart, each guarded by my brethren.",
                "Will you enter through the [Albion Entrance], the [Midgard Entrance], or dare the [Hibernia Entrance]?",
                "Choose wisely—the Labyrinth remembers those who wander lost, and stone walls tell no lies.");

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
            switch (destination.TeleportID.ToLower())
            {
                case "albion entrance":
                    SayTo(player, "Through Albion's stone you pass... all paths lead to the center.");
                    break;
                case "midgard entrance":
                    SayTo(player, "Through Midgard's stone you pass... all paths lead to the center.");
                    break;
                case "hibernia entrance":
                    SayTo(player, "Through Hibernia's stone you pass... all paths lead to the center.");
                    break;
            }
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