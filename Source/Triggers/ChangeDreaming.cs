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

    [CustomEntity("FlaglinesAndSuch/ChangeDreaming")]
    class ChangeDreaming : Trigger
    {
        String flag;
        bool flagState;
        bool resetFlag;

        bool toSetDreaming;

        public ChangeDreaming(EntityData data, Vector2 offset) : base(data, offset)
        {
            toSetDreaming = data.Bool("newDreamingValue");

            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
        }

        public override void OnEnter(Player player)
        {
            if (flag != "" && (Scene as Level).Session.GetFlag(flag) != flagState)
            {
                return;
            }

            Level level = SceneAs<Level>();
            level.Session.Dreaming = toSetDreaming;

            if (resetFlag)
            {
                (Scene as Level).Session.SetFlag(flag, !flagState);
            }
        }
    }
}
