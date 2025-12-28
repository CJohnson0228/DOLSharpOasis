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
    /// Trials of Atlantis Teleporter
    /// </summary>
    /// <author>CMeyerJohnson</author>
    public class TOATeleporter : GameTeleporter
    {
        /// <summary>
        /// Add model and packageID to the teleporter.
        /// </summary>
        /// <returns></returns>
        public override bool AddToWorld()
        {
            PackageID = "Yggdrasil";
            Model = 1195;
            return base.AddToWorld();
        }
        
        protected override string Type
        {
            get { return "Trials of Atlantis"; }
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
                "Greetings Mortal, I have been sent by the Gods to facilitate your movement around the realms of Atlantis.\n {0} {1} {2} {3}",
                "Will you begin your journey in the waters of [Oceanus-Mesothalasia] where the seas bounties are both bountiful and dangerous?",
                "Or have you had enough of the water and wish the traverse the ocean of sand in the Deserts of [Stygia]?",
                "Maybe you wish to brave [Volcanus] where lava is not the only thing that flows freely?",
                "finally, the sky is but a bridge to be crossed from the majestic forests of [Aerus]");

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
                case "oceanus-mesothalasia":
                {
                    String reply = String.Format(
                        "I have many watery places I may send you. {0} {1} {2} {3} {4} {5} {6}",
                        "travel to the safety of the [Haven of Oceanus], or to several islands including", 
                        "[Hermes Island],", 
                        "the [Island of Mesos],", 
                        "the [Island of Mesomesos],",
                        "the [Island of Xanthus],",
                        "the [Egg of Youth Temple]",
                        "or near the entrance of [Sobekite Eternal].");
                    SayTo(player, reply);
                    return;
                }
                case "stygia":
                {
                    String reply = String.Format(
                        "Prefer to stay dry I see. I may send you {0} {1} {2} {3} {4}", 
                        "to the safety of the [Haven of Stygia], or to many places around the desert including", 
                        "the [Fortress of Storms],", 
                        "complete trials in [Necropolis],", 
                        "travel to outside [Fort Setian],", 
                        "or gaze at the wonders of the [Great Pyramid].");
                    SayTo(player, reply);
                    return;
                }
                case "volcanus":
                {
                    String reply = String.Format(
                        "Desiring to be burned to your very core? I may send you to {0} {1} {2}",
                        "the safety of the [Haven of Volcanus], or the many HOT locations in the volcanic wastes.",
                        "I can send you to the [Chimera Arena],",
                        "or you may travel to [Deep Volcanus]"
                        );
                    SayTo(player, reply);
                    return;
                }
                case "aerus":
                {
                    String reply = String.Format(
                        "Ahh to walk through the enchanting forest. May never see you again, but {0} {1} {2} {3} {4}", 
                        "I can send you to the relative safety of the [Haven of Aerus],", 
                        "or you may brave the [Gorgon Settlement],", 
                        "the [Centaur Village], or challenge the [Sons of Creon],", 
                        "or you may stand before the [Temple of Talos].", 
                        "or reach into the sky and enter the [City of Aerus]");
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
            switch (destination.TeleportID.ToLower())
            {
                 case "haven of stygia":
                    SayTo(player, "The Desert City welcomes you!");
                    break;
                 case "haven of oceanus":
                     SayTo(player, "The City by the Ocean welcomes you!");
                     break;
                 case "haven of volcanus":
                     SayTo(player, "The City of Fire welcomes you!");
                     break;
                 case "haven of aerus":
                     SayTo(player, "The Haven of the Realms of Air await you!");
                     break;
                 case "sobekite eternal":
                    SayTo(player, "May you marvel at the great rock temple...");
                    break;
                case "chimera arena":
                    SayTo(player, "The say dog is man's best friend... you may disagree very soon...");
                    break;
                case "great pyramid":
                    SayTo(player, "Behold the Great Pyramid awaits");
                    break;
                case "island of xanthus":
                    SayTo(player, "beware the harpies... they don't only steal children...");
                    break;
                case "deep volcanus":
                    SayTo(player, "Not afraid of fire? Even steel burns when hot enough...");
                    break;
                case "centaur village":
                    SayTo(player, "Be careful around the horse-people, they kick when agitated...");
                    break;
                case "fort setian":
                    SayTo(player, "Dogs and cats have always fought... which side will you choose...");
                    break;
                case "necropolis":
                    SayTo(player, "Part Temple... part Hell... all sacred and all dangerous");
                    break;
                case "island of mesomesos":
                    SayTo(player, "To the middle of the great ocean you go... and where you stop...");
                    break;
                case "egg of youth temple":
                    SayTo(player, "a myth once said that youth was held in a fountain... but birth always comes from an egg...");
                    break;
                case "temple of talos":
                    SayTo(player, "Talos was a great champion... can you best the best? Or fall like the rest....");
                    break;
                case "island of mesos":
                    SayTo(player, "dust devils and crocodiles... not my cup of milk... but maybe you wont get kilt...");
                    break;
                case "hermes island":
                    SayTo(player, "if you listen closely Hermes still speaks for the gods... and they never say anything good...");
                    break;
                case "gorgon settlement":
                    SayTo(player, "Hiss and bone, slide and slither... does a stoned man moan or will you just whither");
                    break;
                case "sons of creon village":
                    SayTo(player, "Thunder of hooves, archer's keen eye... stand still and you're target practice, run and you die.");
                    break;
                case "city of aerus":
                    SayTo(player, "Marble that walks, granite that breaths... the city in the sky, that nobody sees...");
                    break;
                case "fortress of storms":
                    SayTo(player, "desert and dogs, and something amiss... fire from the sky, and it's you it will kiss...");
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