using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/GroundRefillsIfFlag")]
    class GroundRefillsIfFlag : Trigger
    {
        bool RefillsState;
        String flag;
        bool flagState;
        bool resetFlag;

        public GroundRefillsIfFlag(EntityData data, Vector2 offset, EntityID entId)
            : base(data, offset) {

            RefillsState = data.Bool("Refill_State");
            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if ((Scene as Level).Session.GetFlag(flag) != flagState)
            {
                return;
            }

            SceneAs<Level>().Session.Inventory.NoRefills = RefillsState;

            if (resetFlag)
            {
                (Scene as Level).Session.SetFlag(flag, !flagState);
            }
        }
    }
}
