using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.ServerRules
{
    public class DarkSpireJumpPoint : IJumpPointHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool IsAllowedToJump(ZonePoint targetPoint, GamePlayer player)
        {
            DarkSpireInstance instance = GetDarkSpireInstance(player);
            
            if (instance == null)
            {
                player.Out.SendMessage("Failed to create DarkSpire instance!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            targetPoint.TargetRegion = instance.ID;
            
            return true;
        }

        private DarkSpireInstance GetDarkSpireInstance(GamePlayer player)
        {
            // Create new instance using Region 69 as template
            DarkSpireInstance instance = (DarkSpireInstance)WorldMgr.CreateInstance(69, typeof(DarkSpireInstance));
            
            log.InfoFormat("Created DarkSpire instance {0} for player {1}", instance.ID, player.Name);
            
            return instance;
        }
    }
}