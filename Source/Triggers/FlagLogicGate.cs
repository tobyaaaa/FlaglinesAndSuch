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

    [CustomEntity("FlaglinesAndSuch/FlagLogicGate")]
    class FlagLogicGate : Trigger //Some code aquired from Everest's github page! Make sure to check that out
    {
        String flag1;
        String flag2;
        String setFlag;
        //bool state1;
        //bool state2;
        bool setState;
        bool OnlyOnce;
        bool triggered;

        bool[] WorkableCases;

        public FlagLogicGate(EntityData data, Vector2 offset) : base(data, offset)
        {
            flag1 = data.Attr("flag1");
            flag2 = data.Attr("flag2");
            setFlag = data.Attr("set_flag");
            setState = data.Bool("set_state");
            OnlyOnce = data.Bool("only_once");
            WorkableCases = new bool[4];//index 0 is 0,0; 1 is 0,1; 2 is 1,0; 3 is 1,1. Note the similarity to binary.
            WorkableCases[0] = data.Bool("Case_0_0");
            WorkableCases[1] = data.Bool("Case_0_1");
            WorkableCases[2] = data.Bool("Case_1_0");
            WorkableCases[3] = data.Bool("Case_1_1");
        }

        public override void OnEnter(Player player)
        {

            if (triggered)
            {
                return;
            }
            //if ((Scene as Level).Session.GetFlag(flag) != ifState)

            //convert the values flag 1 and flag 2 into a unique number by treating them as the first and second digits of a binary number, converting them to base 10
            //this line is a mess and I love it
            if (WorkableCases[(Convert.ToInt32((Scene as Level).Session.GetFlag(flag1)) * 2) + Convert.ToInt32((Scene as Level).Session.GetFlag(flag2))]) {
                (Scene as Level).Session.SetFlag(setFlag, setState);
            }

            if (OnlyOnce)
            {
                triggered = true;
            }
        }
    }
}
