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
using DOL.Database.Attributes;

namespace DOL.Database
{
    /// <summary>
    /// Database object for pseudo-instance dungeon configurations.
    /// Stores the mapping between base dungeon regions and their static copies.
    /// </summary>
    [DataTable(TableName = "PseudoInstanceConfig")]
    public class DBPseudoInstanceConfig : DataObject
    {
        private string m_dungeonName;
        private ushort m_baseRegionID;
        private string m_copyRegionIDs;
        private int m_resetDelayMs;
        private ushort m_entryZonePointID;
        private bool m_enabled;

        /// <summary>
        /// Constructor
        /// </summary>
        public DBPseudoInstanceConfig()
        {
            m_dungeonName = string.Empty;
            m_baseRegionID = 0;
            m_copyRegionIDs = string.Empty;
            m_resetDelayMs = 60000;
            m_entryZonePointID = 0;
            m_enabled = true;
        }

        /// <summary>
        /// Display name for the dungeon (for logging/admin purposes)
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string DungeonName
        {
            get { return m_dungeonName; }
            set
            {
                Dirty = true;
                m_dungeonName = value;
            }
        }

        /// <summary>
        /// The original/base region ID for this dungeon (e.g., 397)
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public ushort BaseRegionID
        {
            get { return m_baseRegionID; }
            set
            {
                Dirty = true;
                m_baseRegionID = value;
            }
        }

        /// <summary>
        /// Comma-separated list of copy region IDs (e.g., "3970,3971,3972")
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string CopyRegionIDs
        {
            get { return m_copyRegionIDs; }
            set
            {
                Dirty = true;
                m_copyRegionIDs = value;
            }
        }

        /// <summary>
        /// Delay in milliseconds before resetting a copy after it becomes empty.
        /// Default is 60000 (1 minute).
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int ResetDelayMs
        {
            get { return m_resetDelayMs; }
            set
            {
                Dirty = true;
                m_resetDelayMs = value;
            }
        }

        /// <summary>
        /// The ZonePoint ID that serves as the entrance to this dungeon.
        /// This ZonePoint's ClassType should be set to PseudoInstanceJumpPoint.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public ushort EntryZonePointID
        {
            get { return m_entryZonePointID; }
            set
            {
                Dirty = true;
                m_entryZonePointID = value;
            }
        }

        /// <summary>
        /// Whether this pseudo-instance configuration is active.
        /// Set to false to disable without deleting the config.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public bool Enabled
        {
            get { return m_enabled; }
            set
            {
                Dirty = true;
                m_enabled = value;
            }
        }
    }
}