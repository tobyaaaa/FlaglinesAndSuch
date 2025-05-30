using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMod.InlineRT.MonoModRule;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/Sawblade")]
    [Tracked(false)]
    class Sawblade : Entity
    {

        private Sprite sprite;
        public Vector2 Start;
        public Vector2 End;
        public float Percent; //WOW what a badly named variable

        public bool Moving = true; //TODO: never assigned
        public float PauseTimer;
        bool Up = true;

        public float MoveTime;
        public float pauseTime;
        public bool useSineEasing;
        public bool UseWrapping;

        private SoundSource idlesfx;

        private MTexture bgLineTexture;
        private bool drawTrack;


        public bool silent = false;
        public bool no_nail_flaglines = false;
        public bool no_nail_kelper = false;

        public Sawblade(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            switch (data.Attr("size"))
            {
                case "tiny":
                    base.Collider = new Circle(8f);
                    Add(sprite = Class1.spriteBank.Create("Sawblade"));
                    sprite.Play("tiny", restart: true);
                    break;
                default:
                    base.Collider = new Circle(10f);
                    Add(sprite = Class1.spriteBank.Create("Sawblade"));
                    break;
                case "medium":
                    base.Collider = new Circle(14);
                    Add(sprite = Class1.spriteBank.Create("Sawblade"));
                    break;
                case "big":
                    base.Collider = new Circle(24f);
                    Add(sprite = Class1.spriteBank.Create("Sawblade"));
                    sprite.Play("big", restart: true);
                    break;
                    //case "huge":
                    //base.Collider = new Circle(28f);
                    //Add(sprite = FlaglinesAndSuchModule.spriteBank.Create("Sawblade"));
                    //sprite.Play("huge", restart: true);
                    //break;
            }

            if (data.Nodes.Length == 1)
            {
                Start = data.Position + offset;
                End = data.Nodes[0] + offset;
            }
            else
            {
                Start = End = data.Position + offset;
            }

            MoveTime = data.Float("move_time");
            pauseTime = data.Float("pause_time");
            Percent = data.Float("start_offset") % 1;
            useSineEasing = data.Bool("easing");
            UseWrapping = data.Bool("wrap");


            if (data.Has("time_offset")) {
                //blade moves first, then pauses
                float timeOffset = data.Float("time_offset") % ((MoveTime + pauseTime) * 2);
                if (timeOffset > MoveTime + pauseTime) { //start the blade at the other node
                    Vector2 temp = Start;
                    Start = End;
                    End = temp;
                    timeOffset -= (MoveTime + pauseTime);
                }
                if (timeOffset > MoveTime) { //we're at the other node
                    Percent = 1;
                    Position = End;
                    PauseTimer = ((MoveTime + pauseTime) - timeOffset);
                }
                else {
                    Percent = timeOffset / MoveTime;
                }
            }

            Add(new PlayerCollider(OnPlayer));
            Add(idlesfx = new SoundSource());

            drawTrack = data.Bool("draw_track");
            string inputLinePath = data.Attr("track_sprite");
            if (inputLinePath == "" || inputLinePath == null) { inputLinePath = "default"; }
            bgLineTexture = GFX.Game["objects/FlaglinesAndSuch/Sawblade/line/" + inputLinePath];


            silent = data.Bool("silent");
            if (data.Has("no_nail_kelper"))
            {
                no_nail_kelper = data.Bool("no_nail_kelper");
            }
            else
            {
                no_nail_kelper = true;
            }
            if (data.Has("no_nail_flaglines"))
            {
                no_nail_flaglines = data.Bool("no_nail_flaglines");
            }
            else
            {
                no_nail_flaglines = false;
            }

            if (!no_nail_kelper)
            {
                Component kelperCollider = KelperImports.CreateNailCollider(kelper_nail_hit);
                if (kelperCollider != null) { Add(kelperCollider); }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!silent) {
                idlesfx.Play("event:/tobyaaa_HKnail/sawblade");
            }
        }


        public override void Update()
        {
            base.Update();
            if (!Moving)
            {
                return;
            }
            if (PauseTimer > 0f)
            {
                PauseTimer -= Engine.DeltaTime;
                if (PauseTimer <= 0f)
                {
                }
                return;
            }
            Percent = Calc.Approach(Percent, Up ? 1 : 0, Engine.DeltaTime / MoveTime);
            UpdatePosition();
            if ((Up && Percent == 1f) || (!Up && Percent == 0f))
            {
                if (!UseWrapping)
                {
                    Up = !Up;
                    PauseTimer = pauseTime;
                }
                else {
                    Up = true;
                    Percent = 0f;
                    PauseTimer = pauseTime;
                }
            }
        }

        public void UpdatePosition()
        {
            Position = useSineEasing ? Vector2.Lerp(Start, End, Ease.SineInOut(Percent)) : Vector2.Lerp(Start, End, Percent);
        }

        private void OnPlayer(Player hitplayer)
        {
            Vector2 vector = (hitplayer.Center - base.Center).SafeNormalize();
            hitplayer.Die(vector);
        }

        public override void Render()
        {
            if(drawTrack && Start != End)
            {
                Vector2 i = Start;
                Vector2 dir = new Vector2(1, 0).Rotate((float) Math.Atan2(End.Y - Start.Y, End.X - Start.X));
                double dist = Math.Pow(End.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2);
                while (dist - 16 > Math.Pow(Start.Y - i.Y, 2) + Math.Pow(Start.X - i.X, 2)) {
                    bgLineTexture.GetSubtexture(0, 32, 16, 16).Draw(i, new Vector2(8, 8), Color.White, 1, (float)((Math.PI / 2) + Math.Atan2(End.Y - Start.Y, End.X - Start.X)));
                    i += dir * 16;
                }
                if (dist > 20) {
                    Vector2 halfway = new Vector2((End.X - Start.X) / 2 + Start.X, (End.Y - Start.Y) / 2 + Start.Y);
                    bgLineTexture.GetSubtexture(0, 48, 16, 16).Draw(halfway, new Vector2(8, 8), Color.White, 1, (float)((Math.PI / 2) + Math.Atan2(End.Y - Start.Y, End.X - Start.X)));
                }
                bgLineTexture.GetSubtexture(0, 0, 16, 24).Draw(Start, new Vector2(8,8), Color.White, 1, (float) (-(Math.PI / 2) + Math.Atan2(End.Y - Start.Y, End.X - Start.X)) );
                bgLineTexture.GetSubtexture(0, 0, 16, 24).Draw(End, new Vector2(8, 8), Color.White, 1, (float)((Math.PI / 2) + Math.Atan2(End.Y - Start.Y, End.X - Start.X)));
            }
            base.Render();
        }

        public void kelper_nail_hit(Player player, Vector2 nailDir)
        {
            KelperImports.ConsumeNailSwing();
            KelperImports.ApplyNailRebound(1);
            KelperImports.PlayNailTinkSound();
        }
    }
}
