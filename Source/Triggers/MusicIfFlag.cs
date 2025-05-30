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
    [CustomEntity("FlaglinesAndSuch/MusicIfFlag")]
    class MusicIfFlag : Trigger
    {
        String oldTrack;
        String Track;
        int Progress;
        bool ResetOnLeave;

        String flag;
        bool flagState;
        bool resetFlag;
        bool wasSuccesfullyTriggered;

        public MusicIfFlag(EntityData data, Vector2 offset) : base(data, offset)
        {
            Track = data.Attr("Track");
            Progress = data.Int("Progress");
            
            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
            ResetOnLeave = data.Bool("Reset_On_Leave");
        }

        public override void OnEnter(Player player)
        {
            if ((Scene as Level).Session.GetFlag(flag) != flagState) {
                return;
            }

            if (ResetOnLeave)
            {
                oldTrack = Audio.CurrentMusic;
            }
            Session session = SceneAs<Level>().Session;
            session.Audio.Music.Event = SFX.EventnameByHandle(Track);
            if (Progress != 0)
            {
                session.Audio.Music.Progress = Progress;
            }
            session.Audio.Apply(forceSixteenthNoteHack: false);

            if (resetFlag) {
                (Scene as Level).Session.SetFlag(flag, !flagState);
            }
            wasSuccesfullyTriggered = true;
        }

        public override void OnLeave(Player player)
        {
            if (ResetOnLeave && wasSuccesfullyTriggered)
            {
                Session session = SceneAs<Level>().Session;
                session.Audio.Music.Event = oldTrack;
                session.Audio.Apply(forceSixteenthNoteHack: false);

            }
            wasSuccesfullyTriggered = false;
        }
    }
}
