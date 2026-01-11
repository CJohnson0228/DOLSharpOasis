using System;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS
{
    public class DarkSpireInstance : BaseInstance
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DarkSpireInstance(ushort ID, GameTimer.TimeManager time, RegionData data)
            : base(ID, time, data)
        {
            // Don't destroy immediately when empty
            DestroyWhenEmpty = false;
        }

        public override void OnPlayerEnterInstance(GamePlayer player)
        {
            base.OnPlayerEnterInstance(player);
            player.Out.SendMessage("You have entered DarkSpire instance " + ID + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            log.InfoFormat("Player {0} entered DarkSpire instance {1}, PlayersInInstance now = {2}", player.Name, ID, PlayersInInstance);
        }

        public override void OnPlayerLeaveInstance(GamePlayer player)
        {
            base.OnPlayerLeaveInstance(player);
            log.InfoFormat("Player {0} left DarkSpire instance {1}, PlayersInInstance now = {2}", player.Name, ID, PlayersInInstance);
    
            if (PlayersInInstance == 0)
            {
                log.InfoFormat("DarkSpire instance {0} is now empty, will destroy in 5 minutes", ID);
                BeginDelayCloseCountdown(5);
            }
        }

        public override void LoadFromDatabase(Mob[] mobObjs, ref long mobCount, ref long merchantCount, ref long itemCount, ref long bindCount)
        {
            base.LoadFromDatabase(mobObjs, ref mobCount, ref merchantCount, ref itemCount, ref bindCount);
            
            foreach (GameNPC mob in GetMobsInsideInstance(false))
            {
                if (mob != null)
                {
                    mob.RespawnInterval = -1;
                }
            }
            
            log.InfoFormat("DarkSpire instance {0} loaded with {1} mobs (no respawn)", ID, mobCount);
        }
        
        public override bool OnInstanceDoor(GamePlayer player, ZonePoint zonePoint)
        {
            // Log what's happening
            log.InfoFormat("DarkSpire instance {0}: Player {1} using zonepoint {2} to region {3}", 
                ID, player.Name, zonePoint.Id, zonePoint.TargetRegion);
    
            // Allow the exit to proceed normally
            return true;
        }
    }
}