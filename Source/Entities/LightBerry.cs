using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace FlaglinesAndSuch
{
    [RegisterStrawberry(true, false)]
    [CustomEntity("FlaglinesAndSuch/LightBerry")]
    public class LightBerry : Entity, IStrawberry
    {
        public static ParticleType P_GhostGlow = Strawberry.P_GhostGlow;

        public static ParticleType P_LightGlow = new ParticleType(Strawberry.P_Glow)
        {
            Color = Calc.HexToColor("d6d6ce"),
            Color2 = Calc.HexToColor("ededeb")
        };

        public static ParticleType P_DarkGlow = new ParticleType(Strawberry.P_Glow)
        {
            Color = Calc.HexToColor("33332d"),
            Color2 = Calc.HexToColor("80807e")
        };

        private readonly bool isDark;
        private readonly bool isGhostBerry;
        private readonly bool isMoon;
        private readonly Vector2 start;

        private BloomPoint bloom;
        private bool collected;
        private float collectTimer;
        public Follower Follower;
        public NineBlockGame game;
        public EntityID ID;
        private VertexLight light;
        private Tween lightTween;
        public bool ReturnHomeWhenLost = true;
        private Sprite sprite;
        private bool waitingOnGame;
        private Wiggler wiggler;
        private float wobble;

        public LightBerry(EntityData data, Vector2 offset, EntityID gid)
        {
            ID = gid;
            Position = start = data.Position + offset;
            isDark = data.Bool("isDark");
            isMoon = data.Bool("moon");
            isGhostBerry = SaveData.Instance.CheckStrawberry(ID);
            Depth = -100;
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(Follower =
                new Follower(ID, null, OnLoseLeader));
            Follower.FollowDelay = 0.3f;
            game = new NineBlockGame(isDark, isGhostBerry, this);
        }

        private string gotGameFlag => "finished_game_of_" + ID;


        public void OnCollect()
        {
            if (collected)
                return;
            var collectIndex = 0;
            collected = true;
            if (Follower.Leader != null)
            {
                var entity = Follower.Leader.Entity as Player;
                collectIndex = entity.StrawberryCollectIndex;
                ++entity.StrawberryCollectIndex;
                entity.StrawberryCollectResetTimer = 2.5f;
                Follower.Leader.LoseFollower(Follower);
            }

            SaveData.Instance.AddStrawberry(ID, false);
            var session = (Scene as Level).Session;
            session.DoNotLoad.Add(ID);
            session.Strawberries.Add(ID);
            session.UpdateLevelStartDashes();
            Add(new Coroutine(CollectRoutine(collectIndex)));
            //var OnStrawberryCollectFieldInfo = typeof(Everest.Discord).GetMethod("OnStrawberryCollect",
            //    BindingFlags.Static | BindingFlags.NonPublic);
            //OnStrawberryCollectFieldInfo?.Invoke(null, null);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (isDark)
            {
                if (isGhostBerry)
                    sprite = Class1.spriteBank.Create("ghostdarkberry");
                else
                    sprite = Class1.spriteBank.Create("darkberry");
            }
            else
            {
                if (isGhostBerry)
                    sprite = Class1.spriteBank.Create("ghostlightberry");
                else
                    sprite = Class1.spriteBank.Create("lightberry");
            }

            Add(sprite);
            sprite.OnFrameChange = OnAnimate;
            Add(wiggler = Wiggler.Create(0.4f, 4f,
                v =>
                    sprite.Scale = Vector2.One * (float)(1.0 + (double)v * 0.349999994039536)));
            Add(Wiggler.Create(0.5f, 4f,
                v => sprite.Rotation = (float)((double)v * 30.0 * (Math.PI / 180.0))));
            Add(bloom = new BloomPoint(0.5f, 12f));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(lightTween = light.CreatePulseTween());
            if (!(scene as Level).Session.GetFlag(gotGameFlag))
            {
                scene.Add(game);
                Visible = false;
                Collidable = false;
                waitingOnGame = true;
                bloom.Visible = light.Visible = false;
            }

            if ((scene as Level).Session.BloomBaseAdd <= 0.100000001490116)
                return;
            bloom.Alpha *= 0.5f;
        }

        public override void Update()
        {
            if (waitingOnGame)
                return;
            if (!collected)
            {
                wobble += Engine.DeltaTime * 4f;
                sprite.Y = bloom.Y = light.Y = (float)Math.Sin(wobble) * 2f;
                var followIndex = Follower.FollowIndex;
                if (Follower.Leader != null && Follower.DelayTimer <= 0.0 &&
                    StrawberryRegistry.IsFirstStrawberry(this))
                {
                    var entity = Follower.Leader.Entity as Player;
                    var flag = false;
                    if (entity != null && entity.Scene != null && !entity.StrawberriesBlocked)
                        if (entity.OnSafeGround && entity.StateMachine.State != 13)
                            flag = true;

                    if (flag)
                    {
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > 0.150000005960464)
                            OnCollect();
                    }
                    else
                    {
                        collectTimer = Math.Min(collectTimer, 0.0f);
                    }
                }
                else
                {
                    if (followIndex > 0)
                        collectTimer = -0.15f;
                }
            }

            base.Update();
            if (Follower.Leader == null || !Scene.OnInterval(0.08f))
                return;
            SceneAs<Level>().ParticlesFG.Emit(isGhostBerry ? P_GhostGlow : isDark ? P_DarkGlow : P_LightGlow,
                Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
        }

        private void OnAnimate(string id)
        {
            if (sprite.CurrentAnimationFrame != 35)
                return;
            lightTween.Start();
            if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>()))
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
            }
            else
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
            }
        }

        public void OnPlayer(Player player)
        {
            if (Follower.Leader != null || collected || waitingOnGame)
                return;
            ReturnHomeWhenLost = true;
            Audio.Play(
                isGhostBerry ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch",
                Position);
            player.Leader.GainFollower(Follower);
            wiggler.Start();
            Depth = -1000000;
        }

        private IEnumerator CollectRoutine(int collectIndex)
        {
            Tag = (int)Tags.TransitionUpdate;
            Depth = -2000010;
            if (isMoon)
                Audio.Play("event:/game/general/strawberry_get", Position, "colour", 3, "count",
                collectIndex);
            else
            {
                Audio.Play("event:/game/general/strawberry_get", Position, "colour", isGhostBerry ? 1 : 0, "count", collectIndex);
            }
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            sprite.Play("collect");
            while (sprite.Animating)
                yield return null;
            if (isMoon)
                Scene.Add(new LightBerryPoints(Position, isGhostBerry, collectIndex, true));
            else
            {
                Scene.Add(new StrawberryPoints(Position, isGhostBerry, collectIndex, false));
            }
            RemoveSelf();
        }

        private void OnLoseLeader()
        {
            if (collected || !ReturnHomeWhenLost)
                return;
            Alarm.Set(this, 0.15f, () =>
            {
                var vector = (start - Position).SafeNormalize();
                var val = Vector2.Distance(Position, start);
                var num = Calc.ClampedMap(val, 16f, 120f, 16f, 96f);
                var curve = new SimpleCurve(Position, start,
                    start + vector * 16f + vector.Perpendicular() * num * Calc.Random.Choose(1, -1));
                var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(val / 100f, 0.4f),
                    true);
                tween.OnUpdate = f => Position = curve.GetPoint(f.Eased);
                tween.OnComplete = f => Depth = 0;
                Add(tween);
            });
        }

        public void FinishedGame()
        {
            waitingOnGame = false;
            Visible = true;
            Collidable = true;
            bloom.Visible = light.Visible = true;
            sprite.SetAnimationFrame(0);
            wobble = 0f;
            Audio.Play("event:/game/general/strawberry_laugh", Position);
            (Scene as Level).Session.SetFlag(gotGameFlag);
        }
    }












    public class NineBlockGame : Entity
    {
        private readonly DashListener dashListener;
        private readonly List<int> index;
        private readonly List<NineBlockGamePieces> pieces;
        private readonly Sprite sprite;
        private int dstpos;
        public int fivepos;
        private bool isFinished;
        public LightBerry LightBerry;
        private Vector2 MovingSpeed;
        private Vector2 target1;
        private Vector2 target2;

        public NineBlockGame(bool isDark, bool isGhost, LightBerry berry)
        {
            Depth = 11;
            LightBerry = berry;
            Position = berry.Position + new Vector2(-2f, -3f);
            pieces = new List<NineBlockGamePieces>();
            index = new List<int>();
            isFinished = false;
            int[] numbers;
            var seed = isDark ? SaveData.Instance.TotalDeaths : SaveData.Instance.TotalJumps;
            while (true)
            {
                var rand = new Random(seed);
                numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                var x = 0;
                while (x != 9)
                {
                    var y = rand.Next(0, 9 - x);
                    index.Add(numbers[y]);
                    while (y != 8)
                    {
                        numbers[y] = numbers[y + 1];
                        y++;
                    }

                    x++;
                }

                fivepos = index.FindIndex(s => s == 5);

                if (CheckDoable() && !CheckFinished())
                    break;
                seed++;
                index.Clear();
            }

            dstpos = fivepos;
            var path = isGhost ? "ghost" : "";
            path += isDark ? "dark" : "light";
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(-4f, -5f), index[0]));
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(0f, -5f), index[1]));
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(4f, -5f), index[2]));
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(-4f, 0f), index[3]));
            pieces.Add(new NineBlockGamePieces(path, Position, index[4]));
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(4f, 0f), index[5]));
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(-4f, 5f), index[6]));
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(0f, 5f), index[7]));
            pieces.Add(new NineBlockGamePieces(path, Position + new Vector2(4f, 5f), index[8]));
            dashListener = new DashListener();
            dashListener.OnDash = OnPlayerDash;
            Add(dashListener);
            sprite = Class1.spriteBank.Create("lightberryboard");
            sprite.Position = new Vector2(2f, 3f);
            sprite.Play("idle");
            Add(sprite);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (var piece in pieces) scene.Add(piece);
        }

        public override void Update()
        {
            base.Update();
            if (dstpos == fivepos)
                return;
            pieces[fivepos].Position.X += MovingSpeed.X;
            pieces[fivepos].Position.Y += MovingSpeed.Y;
            pieces[dstpos].Position.X -= MovingSpeed.X;
            pieces[dstpos].Position.Y -= MovingSpeed.Y;
            if (pieces[fivepos].Position != target2)
                return;

            var temp = pieces[fivepos];
            pieces[fivepos] = pieces[dstpos];
            pieces[dstpos] = temp;
            index[fivepos] = index[dstpos];
            index[dstpos] = 5;
            fivepos = dstpos;

            isFinished = CheckFinished();
            if (isFinished)
            {
                LightBerry.FinishedGame();
                foreach (var piece in pieces) piece.RemoveSelf();
                Add(new Coroutine(Fade()));
            }
        }

        private IEnumerator Fade()
        {
            sprite.Play("fade");
            while (sprite.Animating)
                yield return null;
            RemoveSelf();
        }

        private void OnPlayerDash(Vector2 dir)
        {
            if (isFinished)
                return;
            if (Math.Abs(dir.X) > 0.1f && Math.Abs(dir.Y) > 0.1f)
                return;
            if (!(Scene as Level).IsInCamera(Position, 0))
                return;
            if (dir.X < -0.1f)
            {
                if (fivepos % 3 != 2)
                    dstpos = fivepos + 1;
            }
            else if (dir.X > 0.1f)
            {
                if (fivepos % 3 != 0)
                    dstpos = fivepos - 1;
            }
            else if (dir.Y > 0.1f)
            {
                if (fivepos > 2)
                    dstpos = fivepos - 3;
            }
            else if (dir.Y < -0.1f)
            {
                if (fivepos < 6)
                    dstpos = fivepos + 3;
            }

            if (dstpos == fivepos)
                return;
            target1 = pieces[fivepos].Position * Vector2.One;
            target2 = pieces[dstpos].Position * Vector2.One;
            MovingSpeed = (target2 - target1).Sign() * Vector2.One;
        }

        private bool CheckDoable()
        {
            var y = new List<int>();
            foreach (var a in index) y.Add(a);

            var pos = fivepos;
            var q = pos / 3;
            y[pos] = y[q * 3 + 1];
            y[q * 3 + 1] = y[4];
            y[4] = 5;
            var sum = 0;
            for (var i = 0; i < 9; ++i)
            {
                if (i == 4)
                    continue;
                for (var j = 0; j < i; ++j)
                {
                    if (j == 4) continue;
                    if (y[j] > y[i])
                        ++sum;
                }
            }

            return sum % 2 == 0;
        }

        public bool CheckFinished()
        {
            for (var i = 0; i < 9; ++i)
                if (i + 1 != index[i])
                    return false;

            return true;
        }

        private class NineBlockGamePieces : Entity
        {
            private readonly bool isFive;
            private readonly Sprite sprite;


            public NineBlockGamePieces(string spr, Vector2 p, int k)
            {
                Position = p;
                Depth = 10;
                sprite = new Sprite(GFX.Game, "objects/FlaglinesAndSuch/lightberrynineblock/");
                sprite.Add("idle", spr, 0.0f, k);
                sprite.Play("idle");
                Add(sprite);
                isFive = k == 5;
                if (isFive)
                    Visible = false;
            }
        }
    }








    [Tracked]
    public class LightBerryPoints : Entity
    {
        private readonly BloomPoint bloom;
        private readonly bool darkberry;
        private readonly bool ghostberry;
        private readonly VertexLight light;
        private readonly Sprite sprite;
        private DisplacementRenderer.Burst burst;
        private int index;

        public LightBerryPoints(Vector2 position, bool isGhost, int index, bool isDark)
            : base(position)
        {
            Add(sprite = Class1.spriteBank.Create("lightberryfade"));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(bloom = new BloomPoint(1f, 12f));
            Depth = -2000100;
            Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate | (int)Tags.FrozenUpdate;
            ghostberry = isGhost;
            darkberry = isDark;
            this.index = index;
        }

        public override void Added(Scene scene)
        {
            index = Math.Min(5, index);
            if (index >= 5)
                Achievements.Register(Achievement.ONEUP);
            sprite.Play("yeah");
            sprite.OnFinish = a => RemoveSelf();
            base.Added(scene);
            foreach (var entity in Scene.Tracker.GetEntities<StrawberryPoints>())
                if (entity != this && Vector2.DistanceSquared(entity.Position, Position) <= 256.0)
                    entity.RemoveSelf();

            burst = (scene as Level).Displacement.AddBurst(Position, 0.3f, 16f, 24f, 0.3f);
        }

        public override void Update()
        {
            var scene = Scene as Level;
            if (scene.Frozen)
            {
                if (burst == null)
                    return;
                burst.AlphaFrom = burst.AlphaTo = 0.0f;
                burst.Percent = burst.Duration;
            }
            else
            {
                base.Update();
                var camera = scene.Camera;
                Y -= 8f * Engine.DeltaTime;
                X = Calc.Clamp(X, camera.Left + 8f, camera.Right - 8f);
                Y = Calc.Clamp(Y, camera.Top + 8f, camera.Bottom - 8f);
                light.Alpha = Calc.Approach(light.Alpha, 0.0f, Engine.DeltaTime * 4f);
                bloom.Alpha = light.Alpha;

                var type = LightBerry.P_LightGlow;
                if (ghostberry)
                    type = LightBerry.P_GhostGlow;
                else if (darkberry)
                    type = LightBerry.P_DarkGlow;
                if (Scene.OnInterval(0.05f))
                {
                    if (sprite.Color == type.Color2)
                        sprite.Color = type.Color;
                    else
                        sprite.Color = type.Color2;
                }

                if (Scene.OnInterval(0.06f) && sprite.CurrentAnimationFrame > 11)
                    scene.ParticlesFG.Emit(type, 1, Position + Vector2.UnitY * -2f, new Vector2(8f, 4f));
            }
        }
    }
}