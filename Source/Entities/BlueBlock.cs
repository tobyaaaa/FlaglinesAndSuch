using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{

    [Tracked]
    [TrackedAs(typeof(NegaBlock))]
    [CustomEntity("FlaglinesAndSuch/BlueBlock")]
    class BlueBlock : NegaBlock
    {

        private struct BlobParticle //TODO: maybe have two sets of particles? one for bubbles, one for "blobs"?
        {
            public Vector2 Position;

            public int Layer;

            //public Color Color;

            public Vector2 Speed;

            public float TimeOffset;
            public float StartPosOffset;
            public float Amplitude;

            public float Alpha;

        }

        private Player entity;
        //private List<BlueBlock> group;
        //private bool groupLeader;
        //private Vector2 groupOrigin;


        Color lineColor = Calc.HexToColor("ffffff");
        Color bubbleColor = Calc.HexToColor("44b7ff");
        Color insideColor = Calc.HexToColor("2b88d9");

        Color lineColorDisabled = Calc.HexToColor("44b7ff");
        Color bubbleColorDisabled = Calc.HexToColor("348ce6");
        Color insideColorDisabled = Calc.HexToColor("2b88d9");

        private MTexture particleTexture;
        private BlobParticle[] particles;

        float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);
        float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
        float wobbleEase = 0f;

        float animTimer = 0f;
        float transitionTimer = 0f;

        int dashes_to_activate = 0;

        public BlueBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height)
        {

            base.Depth = 5000;//TODO: fix depth on toggle + staticmover depth
            SurfaceSoundIndex = 20;
            particleTexture = GFX.Game["particles/bubble"];
            dashes_to_activate = data.Int("dashes");
            switch (dashes_to_activate) {
                default:
                    Collidable = false;
                    transitionTimer = 0f;
                    break;
                case 1:
                    bubbleColor = lineColorDisabled = Calc.HexToColor("c8453c");
                    insideColorDisabled = insideColor = Calc.HexToColor("ac3232");
                    bubbleColorDisabled = Calc.HexToColor("b33a3a");

                    
                    Collidable = true;
                    transitionTimer = 1f;
                    break;
                case 2:
                    bubbleColor = lineColorDisabled = Calc.HexToColor("ff6def");
                    insideColorDisabled = insideColor = Calc.HexToColor("e94ce8");
                    bubbleColorDisabled = Calc.HexToColor("c540d9");

                    Collidable = false;
                    transitionTimer = 0f;
                    break;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            //scene.Add(side = new BoxSide(this, DisabledColor));
            foreach (StaticMover staticMover in staticMovers)
            {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null)
                {
                    //spikes.EnabledColor = EnabledColor;
                    spikes.DisabledColor = lineColorDisabled;//DisabledColor;
                    spikes.VisibleWhenDisabled = true;
                    //spikes.SetSpikeColor(EnabledColor);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null)
                {
                    //spring.DisabledColor = DisabledColor;
                    spring.VisibleWhenDisabled = true;
                    spring.DisabledColor = lineColorDisabled;
                }
            }

            //NOTE 1. Groups. see below

        }

        public void Setup()
        {
            particles = new BlobParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), /*Calc.Random.NextFloat(base.Height)*/ base.Position.Y + base.Height / 2);
                particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2) + 1;
                particles[i].Speed = new Vector2(0, -Calc.Random.NextFloat(particles[i].Layer) * 2 - 2); //TODO: make particle speed independant of height
                particles[i].TimeOffset = Calc.Random.NextFloat((float)(Math.PI * 3));
                particles[i].StartPosOffset = Calc.Random.NextFloat(base.Height / 2) - (base.Height / 4);
                particles[i].Amplitude = Calc.Random.NextFloat(base.Height / 2) - 4 - Math.Abs(particles[i].StartPosOffset );
                particles[i].Alpha = Calc.Random.Range(0.4f, 0.8f);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            entity = base.Scene.Tracker.GetEntity<Player>();
            Setup();
        }

        public override void Update()
        {
            base.Update();
            if (entity == null)
            {
                entity = base.Scene.Tracker.GetEntity<Player>();
            }

            //NOTE 2. see below. still groups I think

            bool BlockedFlag = false;
            if (IsPlayerBlue())
            {
                BlockedFlag = BlockedCheck();

                if (BlockedFlag)
                {
                    Collidable = false;
                    DisableStaticMovers();
                    base.Depth = 8990;
                }
                else
                {
                    Collidable = true;
                    EnableStaticMovers();
                    base.Depth = 5000;
                }
            }
            else
            {
                Collidable = false;
                DisableStaticMovers();
                base.Depth = 8990;
            }

            foreach (StaticMover staticMover in staticMovers)
            {
                staticMover.Entity.Depth = base.Depth + 1;
            }

            if (Collidable)
            {

                if (transitionTimer < 1)
                {
                    transitionTimer += 5f * Engine.DeltaTime; //1 when solid
                    if (transitionTimer > 1)
                    {
                        transitionTimer = 1;
                    }
                }
            }
            if (!Collidable) {
                if (transitionTimer > 0)
                {
                    transitionTimer -= 2f * Engine.DeltaTime; //1 when solid
                    if (transitionTimer < 0)
                    {
                        transitionTimer = 0;
                    }
                }

                float animSpeedMult = 1 - transitionTimer; //slows the bubbles while block is solidifying. No speed when solid
                animTimer += 1.3f * Engine.DeltaTime * animSpeedMult;
                wobbleEase += Engine.DeltaTime * 0.5f;
                if (wobbleEase > 1f)
                {
                    wobbleEase = 0f;
                    wobbleFrom = wobbleTo;
                    wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
                }
            }

        }

        private bool IsPlayerBlue() {
            if (entity.Dashes == dashes_to_activate) {
                return true;
            }
            return false;
        }

        //NOTE 3

        public bool BlockedCheck()//returns true if blocked
        {
            // var wasCollidable = new Dictionary<BlueBlock, bool>();
            bool wasCollidable;
            var wiggledActors = new List<Actor>();

            //foreach (var block in group)
            //{
                foreach (Actor actor in CollideAll<Actor>())
                {
                    if ((actor.Get<Holdable>() == null || !actor.Get<Holdable>().IsHeld) && !wiggledActors.Contains(actor))
                        wiggledActors.Add(actor);
                }
            //wasCollidable[block] = block.Collidable;
            wasCollidable = Collidable;    
            Collidable = true;
            //}

            var toWiggle = new List<Tuple<Actor, int>>();
            var success = true;

            foreach (var actor in wiggledActors)
            {
                var wiggled = false;
                for (int i = 1; i <= 4; i++)
                {
                    if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
                    {
                        toWiggle.Add(Tuple.Create(actor, i));
                        wiggled = true;
                        break;
                    }
                }
                success = success && wiggled;
                if (!wiggled)
                    break;
            }

            //foreach (var block in group)
            //{
                Collidable = wasCollidable;
            //}

            if (success)
            {
                foreach (var pair in toWiggle)
                {
                    pair.Item1.Position -= Vector2.UnitY * pair.Item2;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private Vector2 PutInside(Vector2 pos)
        {
            if (pos.X > base.Right)
            {
                pos.X -= (float)Math.Ceiling((pos.X - base.Right) / base.Width) * base.Width;
            }
            else if (pos.X < base.Left)
            {
                pos.X += (float)Math.Ceiling((base.Left - pos.X) / base.Width) * base.Width;
            }
            if (pos.Y > base.Bottom)
            {
                pos.Y -= (float)Math.Ceiling((pos.Y - base.Bottom) / base.Height) * base.Height;
            }
            else if (pos.Y < base.Top)
            {
                pos.Y += (float)Math.Ceiling((base.Top - pos.Y) / base.Height) * base.Height;
            }
            return pos;
        }

        float WackySine(float t) {
            if (t <= Math.PI && t >= 0) {
                return (float) Math.Cos(t);
            }
            if (t < Math.PI * 3/2 && t > Math.PI) {
                return -1f;
            }
            if (t <= Math.PI * 5/2 && t >= Math.PI * 3/2)
            {
                return (float)Math.Cos(t - (Math.PI / 2));
            }
            return 1f;
        }

        [MonoModLinkTo("Celeste.Solid", "System.Void Render")]
        public void base_Render() {
        }

        public override void Render()
        {
            base_Render();


            Color insColorReal = Color.Lerp(insideColorDisabled * 0.5f, insideColor, transitionTimer);
            Color lineColReal = this.Collidable ? lineColor : lineColorDisabled;
            Color bubbColReal = Color.Lerp(bubbleColorDisabled, bubbleColor, transitionTimer);

            Draw.Rect(base.Collider, insColorReal);

            //draw bubbles first so they end up behind the outline
            for (int i = 0; i < particles.Length; i++)
            {
                //int layer = particles[i].Layer;
                //particles[i].Position += particles[i].Speed * Engine.DeltaTime; // still moves when paused???
                Vector2 position2 = particles[i].Position;
                //position2 += (position * (0.3f + 0.25f * (float)layer));
                position2.Y += WackySine((particles[i].TimeOffset + animTimer) % (float)(Math.PI * 3)) * /*(base.Height / 2 - 4f)*/ particles[i].Amplitude + particles[i].StartPosOffset;
                position2 = PutInside(position2);
                MTexture mTexture = particleTexture;


                if (position2.X >= base.X + 2f && position2.Y >= base.Y + 2f && position2.X < base.Right - 2f && position2.Y < base.Bottom - 2f)
                {
                    mTexture.DrawCentered(position2, bubbleColor * particles[i].Alpha);
                }
            }

            //if (this.Collidable)
            //{
                WobbleLine(new Vector2(base.X, base.Y), new Vector2(base.X + base.Width, base.Y), 0f, lineColReal, bubbColReal);
                WobbleLine(new Vector2(base.X + base.Width, base.Y), new Vector2(base.X + base.Width, base.Y + base.Height), 0.7f, lineColReal, bubbColReal);
                WobbleLine(new Vector2(base.X + base.Width, base.Y + base.Height), new Vector2(base.X, base.Y + base.Height), 1.5f, lineColReal, bubbColReal);
                WobbleLine(new Vector2(base.X, base.Y + base.Height), new Vector2(base.X, base.Y), 2.5f, lineColReal, bubbColReal);
                Draw.Rect(new Vector2(base.X, base.Y), 2f, 2f, lineColReal);
                Draw.Rect(new Vector2(base.X + base.Width - 2f, base.Y), 2f, 2f, lineColReal);
                Draw.Rect(new Vector2(base.X, base.Y + base.Height - 2f), 2f, 2f, lineColReal);
                Draw.Rect(new Vector2(base.X + base.Width - 2f, base.Y + base.Height - 2f), 2f, 2f, lineColReal);
            //}
            /*else {

                WobbleLine(new Vector2(base.X, base.Y), new Vector2(base.X + base.Width, base.Y), 0f, lineColorDisabled, bubbleColorDisabled);
                WobbleLine(new Vector2(base.X + base.Width, base.Y), new Vector2(base.X + base.Width, base.Y + base.Height), 0.7f, lineColorDisabled, bubbleColorDisabled);
                WobbleLine(new Vector2(base.X + base.Width, base.Y + base.Height), new Vector2(base.X, base.Y + base.Height), 1.5f, lineColorDisabled, bubbleColorDisabled);
                WobbleLine(new Vector2(base.X, base.Y + base.Height), new Vector2(base.X, base.Y), 2.5f, lineColorDisabled, bubbleColorDisabled);
                Draw.Rect(new Vector2(base.X, base.Y), 2f, 2f, lineColorDisabled);
                Draw.Rect(new Vector2(base.X + base.Width - 2f, base.Y), 2f, 2f, lineColorDisabled);
                Draw.Rect(new Vector2(base.X, base.Y + base.Height - 2f), 2f, 2f, lineColorDisabled);
                Draw.Rect(new Vector2(base.X + base.Width - 2f, base.Y + base.Height - 2f), 2f, 2f, lineColorDisabled);

            }*/

        }

        private void WobbleLine(Vector2 from, Vector2 to, float offset, Color color, Color color2)
        {
            float num = (to - from).Length();
            Vector2 value = Vector2.Normalize(to - from);
            Vector2 vector = new Vector2(value.Y, 0f - value.X);
            //Color color =  lineColor;
            //Color color2 = bubbleColor;
            float scaleFactor = 0f;
            int num2 = 16;
            for (int i = 2; (float)i < num - 2f; i += num2)
            {

                float num3 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
                if ((float)(i + num2) >= num)
                {
                    num3 = 0f;
                }
                float num4 = Math.Min(num2, num - 2f - (float)i);
                Vector2 vector2 = from + value * i + vector * scaleFactor;
                Vector2 vector3 = from + value * ((float)i + num4) + vector * num3;
                Draw.Line(vector2 - vector, vector3 - vector, color2);
                Draw.Line(vector2 - vector * 2f, vector3 - vector * 2f, color2);
                Draw.Line(vector2, vector3, color);
                scaleFactor = num3;
            }
        }

        private float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        private float Lerp(float a, float b, float percent)
        {
            return a + (b - a) * percent;
        }

    }
}



//NOTE 1
//AWAKE
/*if (group == null)
            {
                groupLeader = true;
                //Console.WriteLine("pancakes for dinner" + debug);//DEBUG
                group = new List<BlueBlock>();
                group.Add(this);
                FindInGroup(this);
                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                foreach (BlueBlock item in group)
                {
                    if (item.Left < num)
                    {
                        num = item.Left;
                    }
                    if (item.Right > num2)
                    {
                        num2 = item.Right;
                    }
                    if (item.Bottom > num4)
                    {
                        num4 = item.Bottom;
                    }
                    if (item.Top < num3)
                    {
                        num3 = item.Top;
                    }
                }
                groupOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);
                   //item2.groupOrigin = groupOrigin;
            }
            foreach (StaticMover staticMover2 in staticMovers)
            {
                (staticMover2.Entity as Spikes)?.SetOrigins(groupOrigin);
            }*/


//NOTE 2
//UPDATE
/*if (groupLeader)
            {
                bool BlockedFlag = false;
                if (IsPlayerBlue())
                {
                    //if (!trapPlayer)
                    //{
                        BlockedFlag = BlockedCheck();
                    //}

                    if (BlockedFlag)
                    {
                        foreach (BlueBlock item in group)
                        {
                            item.Collidable = false;
                            item.DisableStaticMovers();
                        }
                    }
                    else
                    {
                        foreach (BlueBlock item in group)
                        {

                            //if (!Collidable && doCassetteWobble)
                            //{
                            //    wiggler.Start();
                            //}
                            item.Collidable = true;
                            item.EnableStaticMovers();
                        }

                    }
                }
                else
                {
                    foreach (BlueBlock item in group)
                    {
                        item.Collidable = false;
                        item.DisableStaticMovers();
                    }
                }

            }*/


//NOTE 3
/*private void FindInGroup(BlueBlock block)
{
    foreach (BlueBlock entity in base.Scene.Tracker.GetEntities<BlueBlock>())
    {
        if (entity != this && entity != block && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
        {
            group.Add(entity);
            FindInGroup(entity);
            entity.group = group;
        }
    }
}*/