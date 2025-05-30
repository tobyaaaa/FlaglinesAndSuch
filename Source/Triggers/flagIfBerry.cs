using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{
    //[Tracked]
    [CustomEntity("FlaglinesAndSuch/FlagIfBerry")]
class FlagIfBerry : Trigger //Some code aquired from Everest's github page! Make sure to check that out
{

    String Flag;
    bool State;
    bool onlyOnce;
    bool notJustRedBerries; //super epic hackfix that makes the trigger work with goldens and silvers as well //I think
    bool triggered;

    public FlagIfBerry(EntityData data, Vector2 offset) : base(data, offset)
    {

        Flag = data.Attr("flag");
        State = data.Bool("state");
        onlyOnce = data.Bool("only_once");
        notJustRedBerries = data.Bool("any_berry_type");
     }

    public override void OnEnter(Player player)
    {

        if (triggered)
        {
            return;
        }

            foreach (Strawberry item in Scene.Entities.FindAll<Strawberry>())
            {
                if (item.Follower.Leader != null && (notJustRedBerries || !item.Golden))
                {
                    (Scene as Level).Session.SetFlag(Flag, State);
                    break;
                }
            }

        if (onlyOnce)
        {
            triggered = true;
        }
    }
}
}
