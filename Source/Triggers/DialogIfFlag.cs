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
    [CustomEntity("FlaglinesAndSuch/DialogIfFlag")]
    class DialogIfFlag : Trigger //Some code aquired from Everest's github page! Make sure to check that out
    {


        private string dialogEntry;
        private bool triggered;
        private EntityID id;
        private bool onlyOnce;
        private bool endLevel;
        private int deathCount;

        String flag;
        bool flagState;
        bool resetFlag;

        public DialogIfFlag(EntityData data, Vector2 offset, EntityID entId)
            : base(data, offset)
        {
            dialogEntry = data.Attr("dialogID");
            onlyOnce = data.Bool("onlyOnce", true);
            endLevel = data.Bool("endLevel", false);
            deathCount = data.Int("deathCount", -1);
            triggered = false;
            id = entId;

            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
        }

        public override void OnEnter(Player player)
        {
            if (triggered || (Scene as Level).Session.GetFlag("DoNotLoad" + id) ||
                (deathCount >= 0 && SceneAs<Level>().Session.DeathsInCurrentLevel != deathCount))
            {
                return;
            }
            if ((Scene as Level).Session.GetFlag(flag) != flagState) {
                return;
            }

                triggered = true;

            Scene.Add(new DialogCutscene(dialogEntry, player, endLevel));

            if (resetFlag)
            {
                (Scene as Level).Session.SetFlag(flag, !flagState);
            }

            if (onlyOnce)
                (Scene as Level).Session.SetFlag("DoNotLoad" + id, true); // Sets flag to not load
        }
    }
}
