using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System.Collections.Generic;

namespace DOL.GS.ServerRules
{
    public class DarkSpireJumpPoint : IJumpPointHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        // Cache instances per group/player
        private static Dictionary<string, DarkSpireInstance> activeInstances = new Dictionary<string, DarkSpireInstance>();

        public bool IsAllowedToJump(ZonePoint targetPoint, GamePlayer player)
        {
            DarkSpireInstance instance = GetDarkSpireInstance(player);
            
            if (instance == null)
            {
                player.Out.SendMessage("Failed to create DarkSpire instance!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            // Modify the target to go to the instance
            targetPoint.TargetRegion = instance.ID;
            
            log.InfoFormat("Player {0} will zone to DarkSpire instance {1}", player.Name, instance.ID);
            
            return true;
        }

        private DarkSpireInstance GetDarkSpireInstance(GamePlayer player)
        {
            string key = player.Group != null ? "group_" + player.Group.GetHashCode() : "player_" + player.InternalID;
    
            // Check if instance already exists and is valid
            if (activeInstances.ContainsKey(key))
            {
                DarkSpireInstance existing = activeInstances[key];
                if (existing != null && WorldMgr.GetRegion(existing.ID) != null)
                {
                    log.InfoFormat("Reusing existing DarkSpire instance {0} for {1}", existing.ID, key);
                    return existing;
                }
                else
                {
                    activeInstances.Remove(key);
                }
            }
    
            // Create new instance
            DarkSpireInstance instance = (DarkSpireInstance)WorldMgr.CreateInstance(69, typeof(DarkSpireInstance));
    
            if (instance != null)
            {
                instance.DestroyWhenEmpty = false;
        
                // CRITICAL: Start the instance region manager
                instance.Start();
        
                activeInstances[key] = instance;
                log.InfoFormat("Created new DarkSpire instance {0} for {1}", instance.ID, key);
            }
    
            return instance;
        }
    }
}