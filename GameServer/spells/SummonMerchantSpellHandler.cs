/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Geometry;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Spell handler for summoning temporary merchants
    /// 
    /// Spell.LifeDrainReturn contains the NPCTemplate ID for the merchant
    /// Spell.Duration controls how long the merchant stays (in milliseconds)
    /// </summary>
    [SpellHandler("SummonMerchant")]
    public class SummonMerchantSpellHandler : SpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected GameMerchant m_merchant = null;

        public SummonMerchantSpellHandler(GameLiving caster, Spell spell, SpellLine line) 
            : base(caster, spell, line) 
        {
        }

        /// <summary>
        /// Called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (player != m_caster)
                    player.Out.SendMessage(
                        LanguageMgr.GetTranslation(player.Client.Account.Language, 
                            "GameObject.Casting.CastsASpell", 
                            m_caster.GetName(0, true)), 
                        eChatType.CT_Spell, 
                        eChatLoc.CL_SystemWindow);
            }

            m_caster.Mana -= PowerCost(target);

            base.FinishSpellCast(target);

            if (m_merchant == null)
                return;

            if (!string.IsNullOrEmpty(Spell.Message1))
            {
                MessageToCaster(Spell.Message1, eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster($"You summon {m_merchant.Name}!", eChatType.CT_Spell);
            }
        }

        /// <summary>
        /// Get the position where the merchant should be summoned
        /// </summary>
        protected virtual Position GetSummonPosition()
        {
            // Summon 64 units in front of the caster
            return Caster.Position.TurnedAround() + Vector.Create(Caster.Orientation, length: 64);
        }

        /// <summary>
        /// Apply effect on target - summon the merchant
        /// </summary>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            // Get the NPC template from the database
            INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
            if (template == null)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
                MessageToCaster($"NPC template {Spell.LifeDrainReturn} not found!", eChatType.CT_System);
                return;
            }

            // Create the merchant from template
            m_merchant = new GameMerchant();
            m_merchant.LoadTemplate(template);
            
            // Set merchant properties
            m_merchant.Position = GetSummonPosition();
            m_merchant.CurrentSpeed = 0;
            m_merchant.Realm = Caster.Realm;
            
            // Parse level from template (it's stored as string in database)
            if (byte.TryParse(template.Level?.ToString() ?? "50", out byte level))
            {
                m_merchant.Level = Math.Min(level, (byte)50);
            }
            else
            {
                m_merchant.Level = 50;
            }
            
            // Set the merchant to be temporary (will be removed when effect expires)
            m_merchant.RespawnInterval = -1; // Don't respawn
            
            // Add merchant to the world
            m_merchant.AddToWorld();

            // Create and start the spell effect
            GameSpellEffect effect = CreateSpellEffect(target, effectiveness);
            effect.Start(m_merchant);

            // Add event handler for when merchant is removed
            GameEventMgr.AddHandler(m_merchant, GameLivingEvent.Dying, OnMerchantRemoved);
        }

        /// <summary>
        /// No spell resistance for summoning
        /// </summary>
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        /// <summary>
        /// When the effect expires, remove the merchant
        /// </summary>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GameMerchant merchant)
            {
                if (!noMessages && Caster is GamePlayer player)
                {
                    player.Out.SendMessage($"{merchant.Name} fades away.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                RemoveHandlers(merchant);
                merchant.Health = 0;
                merchant.Delete();
            }

            return 0;
        }

        /// <summary>
        /// Remove event handlers when merchant is removed
        /// </summary>
        protected virtual void RemoveHandlers(GameMerchant merchant)
        {
            GameEventMgr.RemoveHandler(merchant, GameLivingEvent.Dying, OnMerchantRemoved);
        }

        /// <summary>
        /// Handler for when merchant dies or is removed
        /// </summary>
        protected virtual void OnMerchantRemoved(DOLEvent e, object sender, EventArgs arguments)
        {
            if (sender is GameMerchant merchant)
            {
                // Cancel the spell effect if the merchant is removed prematurely
                GameSpellEffect effect = FindEffectOnTarget(merchant, this);
                if (effect != null)
                {
                    effect.Cancel(false);
                }
            }
        }

        /// <summary>
        /// Delve information for the spell
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();

                list.Add($"Function: {(string.IsNullOrEmpty(Spell.SpellType) ? "(not implemented)" : Spell.SpellType)}");
                list.Add(" ");
                list.Add(Spell.Description);
                list.Add(" ");
                
                list.Add("Target: " + Spell.Target);
                
                if (Spell.Range != 0)
                    list.Add("Range: " + Spell.Range);

                // Duration
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add("Duration: Permanent");
                else if (Spell.Duration > 60000)
                    list.Add($"Duration: {Spell.Duration / 60000}:{(Spell.Duration % 60000 / 1000):00} min");
                else if (Spell.Duration != 0)
                    list.Add($"Duration: {Spell.Duration / 1000} sec");

                if (Spell.Power != 0)
                    list.Add($"Power cost: {Spell.Power}");

                list.Add($"Casting time: {(Spell.CastTime * 0.001):0.0##} sec");

                // Recast delay
                if (Spell.RecastDelay > 60000)
                    list.Add($"Recast time: {Spell.RecastDelay / 60000}:{(Spell.RecastDelay % 60000 / 1000):00} min");
                else if (Spell.RecastDelay > 0)
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");

                if (Spell.Radius != 0)
                    list.Add("Radius: " + Spell.Radius);

                return list;
            }
        }

        /// <summary>
        /// Short description for the spell
        /// </summary>
        public override string ShortDescription 
            => $"Summons a merchant that will buy your items and sell potions for {Spell.Duration / 1000} seconds.";
    }
}
