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
    [CustomEntity("FlaglinesAndSuch/tminusHazardFlagController")]
    class tminusHazardFlagController : Entity
    {
        //pretty much all of this code is lifted from maddie480's helping hand.
        //genuinely cannot understate how much she's done for the community

        public tminusHazardFlagController(EntityData data, Vector2 offset) : base()
        {
            // we aren't supposed to access the level this early on, but that's a good way to be sure to set the flag
            // before Added or Awake is called on any entity in the level.
            Level level = Engine.Scene as Level;
            if (level == null)
            {
                level = (Engine.Scene as LevelLoader)?.Level;
            }

            //if (!data.Bool("onlyOnRespawn", defaultValue: false) || isRespawning)
            //{
            string setFlag = data.Attr("setFlag");
            string[] eventFlags = data.Attr("eventFlags").Split(',');
            string[] conditionFlagsAll = data.Attr("conditionFlags").Split(';');

            bool set = true;
            for (int i = 0; i < eventFlags.Length; i++) 
            {
                if (string.IsNullOrEmpty(eventFlags[i]) || level.Session.GetFlag(eventFlags[i]))
                {
                    foreach (string flag in conditionFlagsAll[i].Split(','))
                    {
                        //note: the definition of "set" used to be here. I've moved it.
                        if (string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag))
                        {
                            set = false; //ricky's lightning is off when the flag is true
                                        //so if we want the lightning to SHOW UP on condition, we set it to FALSE
                        }
                        //note: setting the flag used to be here. I've moved it.
                    }
                }

                level?.Session.SetFlag(setFlag, set);
            }
            //}
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // this entity only has work to do on spawn, it can disappear after that.
            RemoveSelf();
        }
    }
}
