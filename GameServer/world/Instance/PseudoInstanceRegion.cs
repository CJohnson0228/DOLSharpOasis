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
using System.Reflection;
using DOL.GS;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// A region class for pseudo-instance copies that returns the base region's 
    /// skin ID so the client displays the correct zone visuals.
    /// 
    /// The skin ID is derived from the region ID by taking regionID / 10.
    /// For example: Region 3970 -> Skin 397, Region 3971 -> Skin 397, etc.
    /// 
    /// To use: Set the Regions.ClassType field to 'DOL.GS.PseudoInstanceRegion'
    /// </summary>
    public class PseudoInstanceRegion : Region
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private ushort m_skinID;

        /// <summary>
        /// Constructor required for Region.Create() reflection
        /// </summary>
        /// <param name="time">The time manager for this region</param>
        /// <param name="data">The region data</param>
        public PseudoInstanceRegion(GameTimer.TimeManager time, RegionData data)
            : base(time, data)
        {
            // Derive the skin ID from the region ID
            // Region 3970, 3971, 3972 -> Skin 397
            // This assumes copy regions are baseID * 10 + copyNumber
            m_skinID = (ushort)(data.Id / 10);
            
            log.Info("[PseudoInstanceRegion] Created region " + data.Id + " with skin " + m_skinID);
        }

        /// <summary>
        /// Returns the skin ID (the visual appearance) for this region.
        /// This tells the client which zone data files to load.
        /// </summary>
        public override ushort Skin
        {
            get { return m_skinID; }
        }

        /// <summary>
        /// Override description to indicate this is a copy
        /// </summary>
        public override string Description
        {
            get { return base.Description; }
        }
    }
}