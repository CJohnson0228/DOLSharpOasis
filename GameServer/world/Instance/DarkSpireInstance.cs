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
            DestroyWhenEmpty = true;
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

        public override void OnPlayerEnterInstance(GamePlayer player)
        {
            base.OnPlayerEnterInstance(player);
            player.Out.SendMessage("You have entered the DarkSpire!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
    }
}