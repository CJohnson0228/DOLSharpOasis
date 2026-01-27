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
using System.ComponentModel;
using System.Reflection;
using System.Timers;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// Tracks the state of a pseudo-instance copy
    /// </summary>
    public enum eCopyState
    {
        Available, // ready for use
        Occupied,  // Players inside
        Resetting  // Cleaning up, respawning mobs
    }

    /// <summary>
    /// Configuration for pseudo-instance dungeon type
    /// </summary>
    public class PseudoInstanceConfig
    {
        public ushort BaseRegionID { get; set; }     // Original region (e.g., 397)
        public ushort[] CopyRegionIDs { get; set; }  // Copy regions (e.g. 3970, 3971, 3972)
        public int ResetDelayMs { get; set; }        // Delay before resetting after empty (default 60000 = 1 min)
        public string DungeonName { get; set; }      // For logging/display

        public PseudoInstanceConfig(string name, ushort baseRegion, ushort[] copies, int resetDelay = 60000)
        {
            DungeonName = name;
            BaseRegionID = baseRegion;
            CopyRegionIDs = copies;
            ResetDelayMs = resetDelay;
        }
    }

    /// <summary>
    /// Manages pseudo-instance dungeon copies.
    /// Tracks which copies are available, assigns players/groups to copies,
    /// and handles cleanup/reset when copies become empty
    /// </summary>
    public static class PseudoInstanceManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        // Registered dungeon configurations
        private static readonly Dictionary<ushort, PseudoInstanceConfig> m_dungeonConfigs = new Dictionary<ushort, PseudoInstanceConfig>();
        
        // State tracking per copy region
        private static readonly Dictionary<ushort, eCopyState> m_copyStates = new Dictionary<ushort, eCopyState>();
        
        // Player/Group to copy assignnment (key = player InternalID or group leader InternalID)
        private static readonly Dictionary<string, ushort> m_assignments = new Dictionary<string, ushort>();
        
        // Reset timers per copy region
        private static readonly Dictionary<ushort, Timer> m_resetTimers = new Dictionary<ushort, Timer>();
        
        // Lock for thread safety
        private static readonly object m_lock = new object();

        /// <summary>
        /// Initialize the manager and register dungeon configurations
        /// </summary>
        public static bool Init()
        {
            try
            {
                // Load all configurations from database
                // need to work with claude tomorrow to create DBPseudoInstanceConfig class
                var configs = GameServer.Database.SelectAllObjects<DBPseudoInstanceConfig>();

                foreach (var dbConfig in configs)
                {
                    if (!dbConfig.Enabled) continue;

                    // Parse copy region IDs from comma-seperated string
                    string[] parts = dbConfig.CopyRegionIDs.Split(',');
                    ushort[] copyIds = new ushort[parts.Length];
                    for (int i = 0; i < parts.Length; i++)
                    {
                        copyIds[i] = Convert.ToUInt16(parts[i].Trim());
                    }

                    RegisterDungeon(new PseudoInstanceConfig(
                        dbConfig.DungeonName,
                        dbConfig.BaseRegionID,
                        copyIds,
                        dbConfig.ResetDelayMs
                    ));
                }

                log.Info("[PseudoInstanceManager] Initialized successfully.");
                    return true;
            }
            catch (Exception ex)
            {
                log.Error("[PseudoInstanceManager] Initialization failed.]", ex);
                return false;
            }
        }

        /// <summary>
        /// Register a dungeon configuration
        /// </summary>
        public static void RegisterDungeon(PseudoInstanceConfig config)
        {
            lock (m_lock)
            {
                // Register by base region ID
                m_dungeonConfigs[config.BaseRegionID] = config;
                
                // Initialize all copies as available and hook into their events
                foreach (ushort copyId in config.CopyRegionIDs)
                {
                    m_copyStates[copyId] = eCopyState.Available;
                    
                    // Hook into the region's PlayerLeave event
                    Region region = WorldMgr.GetRegion(copyId);
                    if (region != null)
                    {
                        GameEventMgr.AddHandler(region, RegionEvent.PlayerLeave, new DOLEventHandler(OnPlayerLeaveRegion));
                        log.Info($"[PseudoInstanceManager] Registered copy region {copyId} for {config.DungeonName}");
                    }
                    else
                    {
                        log.Warn($"[PseudoInstanceManager] Region {copyId} not found during registration. Will hook when region loads.");
                    }
                }
            }
        }

        /// <summary>
        /// Get an available copy for player entering a dungeon.
        /// If Player/group already has an assignment, returns that copy.
        /// Otherwise assigns an available copy.
        /// </summary>
        /// <param name="player">The player entering</param>
        /// <param name="baseRegionId">The base dungeon region ID</param>
        /// <returns>The copy region ID to use, or 0 if no copy available</returns>
        public static ushort GetOrAssignCopy(GamePlayer player, ushort baseRegionId)
        {
            lock (m_lock)
            {
                if (!m_dungeonConfigs.TryGetValue(baseRegionId, out PseudoInstanceConfig config))
                {
                    log.Warn($"[PseudoInstanceManager] No config found for base region {baseRegionId}");
                    return 0;
                }
                
                // Determine the assignment key (group leader or solo  player)
                string assignmentKey = GetAssignmentKey(player);
                
                // Check if already assigned
                if (m_assignments.TryGetValue(assignmentKey, out ushort existingCopy))
                {
                    if (m_copyStates.TryGetValue(existingCopy, out eCopyState state) && state != eCopyState.Resetting)
                    {
                        log.Info("[PseudoInstanceManager] Player " + player.Name + " using existing assignment: region " + existingCopy);
                        m_copyStates[existingCopy] = eCopyState.Occupied;
                        CancelResetTimer(existingCopy);
                        return existingCopy;
                    }
                    else
                    {
                        // Assignment is no longer valid, remove it
                        m_assignments.Remove(assignmentKey);
                    }
                }
                
                // Find an available copy
                foreach (ushort copyId in config.CopyRegionIDs)
                {
                    if (m_copyStates.TryGetValue(copyId, out eCopyState copyState) && copyState == eCopyState.Available)
                    {
                        // Assign this copy
                        m_assignments[assignmentKey] = copyId;
                        m_copyStates[copyId] = eCopyState.Occupied;
                        log.Info($"[PseudoInstanceManager] Assigned player " + player.Name + " to copy region {copyId}");
                        return copyId;
                    }
                }
                
                // No copies available
                log.Warn($"[PseudoInstanceManager] No available copies for " + config.DungeonName);
                return 0;
            }
        }

        /// <summary>
        /// Get the assignment key for a player (group leader ID if grouped, otherwise player ID)
        /// </summary>
        private static string GetAssignmentKey(GamePlayer player)
        {
            if (player.Group != null && player.Group.Leader != null)
            {
                return "group-" + player.Group.Leader.InternalID;
            }

            return "player-" + player.InternalID;
        }

        /// <summary>
        /// Called when a player leaves a region. Used to detect when players leave a copy.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnPlayerLeaveRegion(DOLEvent e, object sender, EventArgs args)
        {
            Region region = sender as Region;
            if (region == null) return;

            ushort regionId = region.ID;
            
            // Check if this region is a pseudo-instance copy
            lock (m_lock)
            {
                if (m_copyStates.ContainsKey(regionId))
                {
                    // Check if region is now empty (after this player leaves)
                    // Note: NumPLayers may not be decremented yet, so check if <= 1
                    if (region.NumPlayers <= 1)
                    {
                        log.Info($"[PseudoInstanceManager] Copy region {regionId} is now empty. Starting reset timer.");
                        StartResetTimer(regionId);
                    }
                }
            }
        }

        /// <summary>
        /// Start the rest timer for a copy region
        /// </summary>
        private static void StartResetTimer(ushort copyRegionId)
        {
            lock (m_lock)
            {
                // Cancel any existing timer
                CancelResetTimer(copyRegionId);
                
                // Find the config for this copy
                PseudoInstanceConfig config = null;
                foreach (var kvp in m_dungeonConfigs)
                {
                    foreach (ushort copyId in kvp.Value.CopyRegionIDs)
                    {
                        if (copyId == copyRegionId)
                        {
                            config = kvp.Value;
                            break;
                        }
                    }

                    if (config != null) break;
                }

                if (config == null) return;
                
                // Create timer
                Timer timer = new Timer(config.ResetDelayMs);
                timer.AutoReset = false;
                timer.Elapsed += (s, e) => ResetCopy(copyRegionId);
                timer.Start();
                
                m_resetTimers[copyRegionId] = timer;
            }
        }

        /// <summary>
        /// Cancel the resset timer for copy region
        /// </summary>
        private static void CancelResetTimer(ushort copyRegionId)
        {
            lock (m_lock)
            {
                if (m_resetTimers.TryGetValue(copyRegionId, out Timer timer))
                {
                    timer.Stop();
                    timer.Dispose();
                    m_resetTimers.Remove(copyRegionId);
                }
            }
        }

        /// <summary>
        /// Reset a copy region - delete all mobs and respawn from database
        /// </summary>
        private static void ResetCopy(ushort copyRegionId)
        {
            lock (m_lock)
            {
                // Verify region is still empty
                Region region = WorldMgr.GetRegion(copyRegionId);
                if (region != null && region.NumPlayers > 0)
                {
                    log.Info($"[PseudoInstanceManager] Copy region {copyRegionId} has players, aborting reset.");
                    m_copyStates[copyRegionId] = eCopyState.Occupied;
                    return;
                }
                
                log.Info($"[PseudoInstanceManager] Resetting copy region {copyRegionId}...");
                m_copyStates[copyRegionId] = eCopyState.Resetting;
                
                // Remove any assignments pointing to this copy
                List<string> keysToRemove = new List<string>();
                foreach (var kvp in m_assignments)
                {
                    if (kvp.Value == copyRegionId)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (string key in keysToRemove)
                {
                    m_assignments.Remove(key);
                }
            }
            
            // Perform reset outside the lock to avoid blocking
            try
            {
                Region region = WorldMgr.GetRegion(copyRegionId);
                if (region == null)
                {
                    log.Error($"[PseudoInstanceManager] Region {copyRegionId} not found for reset.");
                    return;
                }

                // Delete all NPCs in the region
                foreach (GameObject obj in region.Objects)
                {
                    if (obj is GameNPC npc && !(obj is GamePlayer))
                    {
                        npc.Delete();
                    }
                }

                // Reload mobs from database
                var mobs = GameServer.Database.SelectObjects<Mob>(DB.Column("Region").IsEqualTo(copyRegionId));
                foreach (Mob mobData in mobs)
                {
                    GameNPC npc;

                    // Use ClassType if specified
                    if (!string.IsNullOrEmpty(mobData.ClassType))
                    {
                        try
                        {
                            Type npcType = Type.GetType(mobData.ClassType) ?? ScriptMgr.GetType(mobData.ClassType);
                            if (npcType != null)
                            {
                                npc = (GameNPC)Activator.CreateInstance(npcType);
                            }
                            else
                            {
                                npc = new GameNPC();
                            }
                        }
                        catch
                        {
                            npc = new GameNPC();
                        }
                    }
                    else
                    {
                        npc = new GameNPC();
                    }

                    npc.LoadFromDatabase(mobData);
                    npc.AddToWorld();
                }

                log.Info(
                    $"[PseudoInstanceManager] Copy region {copyRegionId} reset complete. SPawned {mobs.Count} mobs.");

                lock (m_lock)
                {
                    m_copyStates[copyRegionId] = eCopyState.Available;
                }
            }
            catch (Exception ex)
            {
                log.Error($"[PseudoInstanceManager] Error resetting copy region {copyRegionId}.", ex);
                lock (m_lock)
                {
                    m_copyStates[copyRegionId] = eCopyState.Available;
                }
            }
        }

        /// <summary>
        /// Check if a region ID is a pseudo-instance copy
        /// </summary>
        public static bool isPseudoInstanceCopy(ushort regionID)
        {
            lock (m_lock)
            {
                return m_copyStates.ContainsKey(regionID);
            }
        }

        /// <summary>
        /// Get the current state of a copy region
        /// </summary>
        public static eCopyState GetCopyState(ushort copyRegionID)
        {
            lock (m_lock)
            {
                if (m_copyStates.TryGetValue((copyRegionID), out eCopyState state))
                {
                    return state;
                }

                return eCopyState.Available;
            }
        }

        /// <summary>
        /// Force release an assignment (for admin/debug use)
        /// </summary>
        public static void ReleaseAssignment(GamePlayer player)
        {
            lock (m_lock)
            {
                string key = GetAssignmentKey(player);
                if (m_assignments.TryGetValue(key, out ushort copyId))
                {
                    m_assignments.Remove(key);
                    log.Info($"[PseudoInstanceManager] Released assignment for {player.Name} from copy {copyId}");
                }
            }
        }

        /// <summary>
        /// Get status information (for admin/debug use)
        /// </summary>
        public static string GetStatusInfo()
        {
            lock (m_lock)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("=== PseudoInstanceManager Status ===");

                foreach (var kvp in m_dungeonConfigs)
                {
                    sb.AppendLine($"Dungeon: {kvp.Value.DungeonName} (Base: {kvp.Key})");
                    foreach (ushort copyId in kvp.Value.CopyRegionIDs)
                    {
                        eCopyState state = m_copyStates.ContainsKey(copyId) ? m_copyStates[copyId] : eCopyState.Available;
                        Region region = WorldMgr.GetRegion(copyId);
                        int playerCount = region?.NumPlayers ?? 0;
                        sb.AppendLine($"  Copy {copyId}: {state} ({playerCount} players)");
                    }
                }

                sb.AppendLine($"Active Assignments: {m_assignments.Count}");
                foreach (var kvp in m_assignments)
                {
                    sb.AppendLine($"  {kvp.Key} -> {kvp.Value}");
                }
                
                return sb.ToString();
            }
        }
    }
}
