using System;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    public class TestTeleporter : GameTeleporter
    {
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            SayTo(player, "Hello! I'm a custom teleporter!");
            return true;
        }
    }
}