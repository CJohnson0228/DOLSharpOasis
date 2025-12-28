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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    /// <summary>
    /// Test Teleporter
    /// </summary>
    /// <author>CMeyerJohnson</author>
    public class TestTeleporter : GameTeleporter
    {
        /// <summary>
        /// Player right-clicked the teleporter.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            SayTo(player, "Hello! I'm a custom teleporter!");
            return true;
        }
    }
}