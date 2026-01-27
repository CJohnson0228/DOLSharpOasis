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
 
 using System.Reflection;
 using DOL.Database;
 using DOL.GS.PacketHandler;
 using log4net;

 namespace DOL.GS.ServerRules
 {
     /// <summary>
     /// Handles pseudo-instance dungeon entrance jump points.
     /// redirects players to their assigned copy of the dungeon.
     /// </summary>
     public class PseudoInstanceJumpPoint : IJumpPointHandler
     {
         private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

         /// <summary>
         /// Decides whether player can jump to the target point.
         /// redirects to assigned pseudo-instance copy.
         /// </summary>
         /// <param name="targetPoint">The jump destination</param>
         /// <param name="player">The jumping player</param>
         /// <returns>True if allowed</returns>
         public bool IsAllowedToJump(ZonePoint targetPoint, GamePlayer player)
         {
             // Get the original target region (the base dungeon)
             ushort baseRegionId = targetPoint.TargetRegion;
             
             // Get or assign a copy for this player/group
             ushort assignedCopy = PseudoInstanceManager.GetOrAssignCopy(player, baseRegionId);

             if (assignedCopy == 0)
             {
                 // no copy available
                 player.Out.SendMessage("This dungeon is currently full. Please try again later.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                 log.Warn($"[PseudoInstanceJumpPoint] No copy available for player {player.Name} entering region {baseRegionId}");
                 return false;
             }

             // Redirect to the assigned copy
             // Keep the same x,y,z,heading - just change the region
             targetPoint.TargetRegion = assignedCopy;
             
             log.Info($"[PseudoInstanceJumpPoint] Player {player.Name} redirected from region {baseRegionId} to copy {assignedCopy}");
             
             // Notify player which copy they're entering (optional, can remove if too verbose)
             player.Out.SendMessage($"Entering dungeon instance...", eChatType.CT_System, eChatLoc.CL_SystemWindow);

             return true;
         }
     }
 }