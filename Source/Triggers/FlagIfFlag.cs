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
    //[Tracked]
    [CustomEntity("FlaglinesAndSuch/FlagIfFlag")]
    class FlagIfFlag : Trigger //Some code aquired from Everest's github page! Make sure to check that out
    {

        String ifFlag;
        String setFlag;
        bool ifState;
        bool setState;
        bool resetIfflag;
        bool onlyOnce;
        bool triggered;

        public FlagIfFlag(EntityData data, Vector2 offset) : base(data, offset) {
            ifFlag = data.Attr("if_flag");
            setFlag = data.Attr("set_flag");
            ifState = data.Bool("if_state");
            setState = data.Bool("set_state");
            resetIfflag = data.Bool("reset_if_flag");
            onlyOnce = data.Bool("only_once");
        }

        public override void OnEnter(Player player) {

            if (triggered)
            {
                return;
            }
            if ((Scene as Level).Session.GetFlag(ifFlag) != ifState)
            {
                return;
            }
            
            (Scene as Level).Session.SetFlag(setFlag, setState);

            if (resetIfflag)
            {
                (Scene as Level).Session.SetFlag(ifFlag, !ifState);
            }
            if (onlyOnce)
            {
                triggered = true;
            }
        }
    }
}
