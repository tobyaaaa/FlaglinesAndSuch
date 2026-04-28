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
    [CustomEntity("FlaglinesAndSuch/StylegroundFlashController")]
    [Tracked(false)]
    class StylegroundFlashController : Entity
    {
        String TagID;
        String RippleType;

        float fadeInTime;
        float fadeOutTime;
        Color PulseColor;
        float pulseDiff;
        float PauseBase;
        float PauseAdd;
        float RippleDelay;
        
        //List<Backdrop> TheseBackdrops;
        //List<Color> TheseColors;

        private struct thisBackdrop {
            public Backdrop drop;
            public Color dropColor;
            public bool inUse;
        }
        List<thisBackdrop> TheseBackdrops;

        public StylegroundFlashController(EntityData data, Vector2 offset)
        {
            TagID = data.Attr("Tag");
            fadeInTime = data.Float("fade_in_time");
            fadeOutTime = data.Float("fade_out_time");
            PulseColor = Calc.HexToColor(data.Attr("pulse_color"));
            pulseDiff = data.Float("pulse_difference");
            PauseBase = data.Float("pause_base");
            PauseAdd = data.Float("pause_add");
            RippleDelay = data.Float("ripple_delay");
            RippleType = data.Attr("ripple_type");
            TheseBackdrops = new List<thisBackdrop>();

        }

        public override void Awake(Scene scene)
        {
            Level level = SceneAs<Level>();
            foreach (StylegroundFlashController entity in level.Tracker.GetEntities<StylegroundFlashController>())
            {
                if (entity != this && entity.TagID == this.TagID)
                {
                    //when a new controller with the same Tag setting is loaded, it updates the old one instead of allowing two to exist
                    CopyOverSettings(entity);
                    RemoveSelf();
                }
            }
            AddTag(Tags.Global); //prevents stylegrounds from having their basic color set wrongly on respawns


            base.Awake(scene);
            List<Backdrop>  tempList = new List<Backdrop>();//this whole little block is to poulate the backdrop list
            tempList.AddRange(level.Background.GetEach<Backdrop>(TagID));
            for (int i = 0; i < tempList.Count; i++)
            {
                thisBackdrop tempBackdrop;
                tempBackdrop.drop = tempList[i];
                tempBackdrop.dropColor = tempList[i].Color;
                tempBackdrop.inUse = false;
                TheseBackdrops.Add(tempBackdrop);
            }

            this.Add(new Coroutine(mainLoop()));
        }

        private IEnumerator mainLoop()
        {

            float delay = PauseBase + Calc.Random.NextFloat(PauseAdd);
            while (true)
            {
                delay -= Engine.DeltaTime;
                if (delay <= 0)
                {

                    
                    if (RippleType != "random no ripple")
                    {
                        /*for (int index = TheseBackdrops.Count() - 1; index >= 0; index--)
                        {                  
                            this.Add(new Coroutine(ColorPulse(TheseBackdrops.ElementAt<Backdrop>(index), PulseColor)));
                            yield return RippleDelay;
                        }*/
                        this.Add(new Coroutine(RippleTrigger()));
                    }
                    else
                    {
                        TheseBackdrops.Shuffle();
                        this.Add(new Coroutine(ColorPulse(TheseBackdrops.ElementAt<thisBackdrop>(0), PulseColor)));
                    }

                    delay = PauseBase + Calc.Random.NextFloat(PauseAdd);
                }
                yield return null;
            }
        }
        private IEnumerator RippleTrigger() {
            switch (RippleType) {
                case "forward":

                    foreach (thisBackdrop item in TheseBackdrops)
                    {
                        this.Add(new Coroutine(ColorPulse(item, PulseColor)));
                        yield return RippleDelay;
                    }

                    break;
                case "backward":

                    for (int index = TheseBackdrops.Count() - 1; index >= 0; index--)
                    {
                        this.Add(new Coroutine(ColorPulse(TheseBackdrops.ElementAt<thisBackdrop>(index), PulseColor)));
                        yield return RippleDelay;

                    }

                    break;
                case "random":

                    TheseBackdrops.Shuffle();
                    foreach (thisBackdrop item in TheseBackdrops)
                    {
                        this.Add(new Coroutine(ColorPulse(item, PulseColor)));
                        yield return RippleDelay;
                    }
                    break;

                default:
                    foreach (thisBackdrop item in TheseBackdrops)
                    {
                        this.Add(new Coroutine(ColorPulse(item, PulseColor)));
                    }

                    break;
            }
            yield break;
        }

        private IEnumerator ColorPulse(thisBackdrop thisbackdrop, Color newColor /*float fadeInTime, float fadeOutTime*/)
        {
            if (thisbackdrop.inUse)
            {
                //we do not want multiple coroutines on one styleground at once
                yield return null;
            }
            thisbackdrop.inUse = true;

            Color oldColor = thisbackdrop.dropColor;
            if (newColor == oldColor)
            {
                yield break;
            }
            newColor = Color.Lerp(oldColor, newColor, pulseDiff);
            float Percent = 0f;
            //float pauseTimer = HoldTime;
            short phase = 1;//signifies whether the color is fading in, fading out, or pausing
            //
            while (true)
            {
                if (phase == 1)
                {
                    Percent += (Engine.DeltaTime / fadeInTime);
                }
                else
                {
                    Percent -= (Engine.DeltaTime / fadeOutTime);
                }
                thisbackdrop.drop.Color = Color.Lerp(oldColor, newColor, Percent);

                if (Percent >= 1.0)
                {
                    phase = -1;

                }

                if (Percent <= 0.0 && phase == -1)
                {
                    yield break;
                }

                thisbackdrop.inUse = false;
                yield return null;
            }
        }


        void CopyOverSettings(StylegroundFlashController other) {
            other.fadeInTime = this.fadeInTime;
            other.fadeOutTime = this.fadeOutTime;
            other.PulseColor = PulseColor;
            other.pulseDiff = this.pulseDiff;
            other.PauseBase = this.PauseBase;
            other.PauseAdd = this.PauseAdd;
            other.RippleDelay = this.RippleDelay;
        }



    }
    public static class EntityExtensions
    {
        private static Random rng = new Random();
        //list shuffle method taken from stack overflow
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }//*/
    }
}
