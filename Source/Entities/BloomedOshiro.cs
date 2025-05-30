using System;
using Celeste;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace FlaglinesAndSuch
{

    [Tracked]
    [CustomEntity("FlaglinesAndSuch/BloomedOshiro")]
    public class BloomedOshiro : Entity
    {
        private readonly SoundSource chargeSfx;
        private readonly PlayerCollider leftCollider;
        private readonly TransitionListener leftListener;
        private readonly Sprite lightning;
        private readonly SoundSource prechargeSfx;
        private readonly PlayerCollider rightCollider;
        private readonly TransitionListener rightListener;
        private readonly Shaker shaker;
        private readonly bool tweening;
        private float anxietySpeed;
        private float attackSpeed;
        private float cameraXOffset;
        private Vector2 colliderTargetPosition;
        public bool doRespawnAnim;
        private bool easeBackFromLeftEdge;
        private bool easeBackFromRightEdge;
        private bool hasEnteredSfx;
        public bool isRight;
        private Level level;
        private bool lightningVisible;
        public Sprite Sprite;
        public StateMachine state;
        private float targetAnxiety;
        private float yApproachSpeed = 100f;

        public const int StWaiting = 0;
        public const int StCharge = 1;
        public const int StAttack = 2;
        public const int StChase = 3;
        public const int StHurt = 4;

        public BloomedOshiro(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(Sprite = Class1.spriteBank.Create("bloomed_oshiro"));
            isRight = data.Bool("Right");
            tweening = data.Bool("Tween");
            if (tweening)
                isRight = !isRight;
            Sprite.Play("idle");
            Add(lightning = GFX.SpriteBank.Create("oshiro_boss_lightning"));
            lightning.Visible = false;
            lightning.OnFinish = s => lightningVisible = false;
            Collider = new Circle(14f);
            Add(new SineWave(0.5f, 0.0f));
            leftCollider = new PlayerCollider(OnPlayerBounce, new Hitbox(28f, 6f, -11f, -11f));
            rightCollider = new PlayerCollider(OnPlayerBounce, new Hitbox(28f, 6f, -17f, -11f));
            Add(new PlayerCollider(OnPlayer));
            Depth = -12500;
            Visible = false;
            Add(new VertexLight(Color.White, 1f, 32, 64));
            Add(shaker = new Shaker(false));
            state = new StateMachine();
            state.SetCallbacks(StWaiting, WaitingUpdate);
            state.SetCallbacks(StChase, ChaseUpdate, ChaseCoroutine, ChaseBegin);
            state.SetCallbacks(StCharge, ChargeUpUpdate, ChargeUpCoroutine, null, ChargeUpEnd);
            state.SetCallbacks(StAttack, AttackUpdate, AttackCoroutine, AttackBegin, AttackEnd);
            state.SetCallbacks(StHurt, HurtUpdate, null, HurtBegin);
            Add(state);
            leftListener = new TransitionListener
            {
                OnOutBegin = () =>
                {
                    if (X > level.Bounds.Left + Sprite.Width / 2.0)
                        Visible = false;
                    else
                        easeBackFromRightEdge = true;
                },
                OnOut = f =>
                {
                    lightning.Update();
                    if (!easeBackFromRightEdge)
                        return;
                    X -= 128f * Engine.RawDeltaTime;
                }
            };
            rightListener = new TransitionListener
            {
                OnOutBegin = () =>
                {
                    if (X < level.Bounds.Right - Sprite.Width / 2.0)
                        Visible = false;
                    else
                        easeBackFromLeftEdge = true;
                },
                OnOut = f =>
                {
                    lightning.Update();
                    if (!easeBackFromLeftEdge)
                        return;
                    X += 128f * Engine.RawDeltaTime;
                }
            };
            SetRightValue();
            Add(prechargeSfx = new SoundSource());
            Add(chargeSfx = new SoundSource());
            Distort.AnxietyOrigin = new Vector2(1f, 0.5f);
        }

        private float TargetY
        {
            get
            {
                var entity = level.Tracker.GetEntity<Player>();
                return entity != null
                    ? MathHelper.Clamp(entity.CenterY, level.Bounds.Top + 8,
                        level.Bounds.Bottom - 8)
                    : Y;
            }
        }

        private void SetRightValue()
        {
            if (isRight)
            {
                Sprite.Scale.X = -1;
                lightning.Scale.X = -1;
                Collider.Position = colliderTargetPosition = new Vector2(-3f, 4f);
                Remove(leftCollider);
                Remove(leftListener);
                Add(rightCollider);
                Add(rightListener);
            }
            else
            {
                Sprite.Scale.X = 1;
                lightning.Scale.X = 1;
                Collider.Position = colliderTargetPosition = new Vector2(3f, 4f);
                Remove(rightCollider);
                Remove(rightListener);
                Add(leftCollider);
                Add(leftListener);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            Y = TargetY;
            if (isRight)
                cameraXOffset = 48f;
            else
                cameraXOffset = -48f;
        }

        private void OnPlayer(Player player)
        {
            if (isRight)
            {
                if (state.State == StHurt || CenterX <= player.CenterX - 4.0 &&
                    !(Sprite.CurrentAnimationID != "respawn"))
                    return;
            }
            else
            {
                if (state.State == StHurt || CenterX >= player.CenterX + 4.0 &&
                    !(Sprite.CurrentAnimationID != "respawn"))
                    return;
            }

            player.Die((player.Center - Center).SafeNormalize(Vector2.UnitX));
        }

        private void OnPlayerBounce(Player player)
        {
            if (state.State != StAttack || player.Bottom > Top + 6.0)
                return;
            Audio.Play("event:/game/general/thing_booped", Position);
            Celeste.Celeste.Freeze(0.2f);
            player.Bounce(Top + 2f);
            state.State = StHurt;
            prechargeSfx.Stop();
            chargeSfx.Stop();
        }

        public override void Update()
        {
            base.Update();
            if (isRight)
                Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, -1f, 0.6f * Engine.DeltaTime);
            else
                Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 0.6f * Engine.DeltaTime);
            Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 0.6f * Engine.DeltaTime);
            if (!doRespawnAnim)
            {
                if (isRight)
                    Visible = X < level.Bounds.Right + Width / 2.0;
                else
                    Visible = X > level.Bounds.Left - Width / 2.0;
            }

            yApproachSpeed = Calc.Approach(yApproachSpeed, 100f, 300f * Engine.DeltaTime);

            if (isRight)
            {
                if (state.State == StAttack && attackSpeed > 200.0)
                {
                    var entity = Scene.Tracker.GetEntity<Player>();
                    Engine.TimeRate =
                        entity == null || entity.Dead || (double)CenterX <= (double)entity.CenterX - 4.0
                            ? 1f
                            : MathHelper.Lerp(Calc.ClampedMap(CenterX - entity.CenterX, 30f, 80f, 0.5f), 1f,
                                Calc.ClampedMap(Math.Abs(entity.CenterY - CenterY), 32f, 48f));
                }
                else
                {
                    Engine.TimeRate = 1f;
                }
            }
            else
            {
                if (state.State == StAttack && attackSpeed > 200.0)
                {
                    var entity = Scene.Tracker.GetEntity<Player>();
                    Engine.TimeRate =
                        entity == null || entity.Dead || (double)CenterX >= (double)entity.CenterX + 4.0
                            ? 1f
                            : MathHelper.Lerp(Calc.ClampedMap(entity.CenterX - CenterX, 30f, 80f, 0.5f), 1f,
                                Calc.ClampedMap(Math.Abs(entity.CenterY - CenterY), 32f, 48f));
                }
                else
                {
                    Engine.TimeRate = 1f;
                }
            }


            Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f),
                Engine.DeltaTime * 8f);
            Distort.Anxiety = Calc.Approach(Distort.Anxiety, targetAnxiety,
                anxietySpeed * Engine.DeltaTime);
        }

        public override void Render()
        {
            if (lightningVisible)
            {
                if (isRight)
                    lightning.RenderPosition = new Vector2(level.Camera.Right + 2f, Top + 16f);
                else
                    lightning.RenderPosition = new Vector2(level.Camera.Left - 2f, Top + 16f);
                lightning.Render();
            }

            Sprite.Position = shaker.Value * 2f;
            base.Render();
        }

        private void ChaseBegin()
        {
            if (tweening)
            {
                isRight = !isRight;
                SetRightValue();
                if (isRight)
                {
                    X = level.Camera.Right + 48f;
                    cameraXOffset = 48f;
                }
                else
                {
                    X = level.Camera.Left - 48f;
                    cameraXOffset = -48f;
                }
            }

            Sprite.Play("idle");
        }

        private int ChaseUpdate()
        {
            if (isRight)
            {
                if (!hasEnteredSfx && cameraXOffset <= 16.0 && !doRespawnAnim)
                {
                    Audio.Play("event:/char/oshiro/boss_enter_screen", Position);
                    hasEnteredSfx = true;
                }

                if (doRespawnAnim && cameraXOffset <= 0.0)
                {
                    Collider.Position.X = 48f;
                    Visible = true;
                    Sprite.Play("respawn");
                    doRespawnAnim = false;
                    if (Scene.Tracker.GetEntity<Player>() != null)
                        Audio.Play("event:/char/oshiro/boss_reform", Position);

                    /*foreach (Minishiro minishiro in Scene.Tracker.GetEntities<Minishiro>())
                    {
                        if (minishiro.Orbiting == this)
                            minishiro.CanRespawn = true;
                    }*/
                }

                cameraXOffset = Calc.Approach(cameraXOffset, -20f, 80f * Engine.DeltaTime);
                X = level.Camera.Right + cameraXOffset;
                Collider.Position.X = Calc.Approach(Collider.Position.X, colliderTargetPosition.X,
                    Engine.DeltaTime * 128f);
            }
            else
            {
                if (!hasEnteredSfx && cameraXOffset >= -16.0 && !doRespawnAnim)
                {
                    Audio.Play("event:/char/oshiro/boss_enter_screen", Position);
                    hasEnteredSfx = true;
                }

                if (doRespawnAnim && cameraXOffset >= 0.0)
                {
                    Collider.Position.X = -48f;
                    Visible = true;
                    Sprite.Play("respawn");
                    doRespawnAnim = false;
                    if (Scene.Tracker.GetEntity<Player>() != null)
                        Audio.Play("event:/char/oshiro/boss_reform", Position);

                    /*foreach (Minishiro minishiro in Scene.Tracker.GetEntities<Minishiro>())
                    {
                        if (minishiro.Orbiting == this)
                            minishiro.CanRespawn = true;
                    }*/
                }

                cameraXOffset = Calc.Approach(cameraXOffset, 20f, 80f * Engine.DeltaTime);
                X = level.Camera.Left + cameraXOffset;
                Collider.Position.X = Calc.Approach(Collider.Position.X, colliderTargetPosition.X,
                    Engine.DeltaTime * 128f);
            }


            Collidable = Visible;
            if (level.Tracker.GetEntity<Player>() != null && Sprite.CurrentAnimationID != "respawn")
                CenterY = Calc.Approach(CenterY, TargetY, yApproachSpeed * Engine.DeltaTime);
            return StChase;
        }

        private IEnumerator ChaseCoroutine()
        {
            yield return 1f;

            /*var minishiros = Scene.Tracker.GetEntities<Minishiro>()
                .Where((e) => (e as Minishiro).Orbiting == this)
                .OrderBy((e) => (e as Minishiro).OrbitIndex).ToList();

            while (minishiros.Count > 0)
            {
                var minishiro = minishiros[0] as Minishiro;
                minishiro.StateMachine.State = (int)Minishiro.State.Charge;
                yield return minishiro.BossDelay;
                minishiros.RemoveAt(0);
            }*/

            prechargeSfx.Play("event:/char/oshiro/boss_precharge");
            Sprite.Play("charge");
            yield return 0.7f;
            if (Scene.Tracker.GetEntity<Player>() != null)
            {
                Alarm.Set(this, 0.216f,
                    () => chargeSfx.Play("event:/char/oshiro/boss_charge"));
                state.State = StCharge;
            }
            else
            {
                Sprite.Play("idle");
            }
        }

        private int ChargeUpUpdate()
        {
            if (level.OnInterval(0.05f))
                Sprite.Position = Calc.Random.ShakeVector();
            cameraXOffset = Calc.Approach(cameraXOffset, 0.0f, 40f * Engine.DeltaTime);
            if (isRight)
                X = level.Camera.Right + cameraXOffset;
            else
                X = level.Camera.Left + cameraXOffset;

            var entity = level.Tracker.GetEntity<Player>();
            if (entity != null)
                CenterY = Calc.Approach(CenterY,
                    MathHelper.Clamp(entity.CenterY, level.Bounds.Top + 8,
                        level.Bounds.Bottom - 8), 30f * Engine.DeltaTime);
            return StCharge;
        }

        private void ChargeUpEnd()
        {
            Sprite.Position = Vector2.Zero;
        }

        private IEnumerator ChargeUpCoroutine()
        {
            Celeste.Celeste.Freeze(0.05f);
            Distort.Anxiety = 0.3f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            lightningVisible = true;
            lightning.Play("once", true);
            yield return 0.3f;
            var player = Scene.Tracker.GetEntity<Player>();
            state.State = player == null ? StChase : StAttack;
        }

        private void AttackBegin()
        {
            attackSpeed = 0.0f;
            targetAnxiety = 0.3f;
            anxietySpeed = 4f;
            level.DirectionalShake(Vector2.UnitX);
        }

        private void AttackEnd()
        {
            targetAnxiety = 0.0f;
            anxietySpeed = 0.5f;
        }

        private int AttackUpdate()
        {
            if (isRight)
                X -= attackSpeed * Engine.DeltaTime;
            else
                X += attackSpeed * Engine.DeltaTime;
            attackSpeed = Calc.Approach(attackSpeed, 500f, 2000f * Engine.DeltaTime);
            if (isRight)
            {
                if (X <= level.Camera.Left - 48.0)
                {
                    X = level.Camera.Right + 48f;
                    cameraXOffset = 48f;
                    doRespawnAnim = true;
                    Visible = false;
                    return StChase;
                }
            }
            else
            {
                if (X >= level.Camera.Right + 48.0)
                {
                    X = level.Camera.Left - 48f;
                    cameraXOffset = -48f;
                    doRespawnAnim = true;
                    Visible = false;
                    return StChase;
                }
            }


            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            if (Scene.OnInterval(0.05f))
                TrailManager.Add(this, Color.Red * 0.6f, 0.5f, false);
            return StAttack;
        }

        private IEnumerator AttackCoroutine()
        {
            yield return 0.1f;
            targetAnxiety = 0.0f;
            anxietySpeed = 0.5f;
        }

        private int WaitingUpdate()
        {
            var entity = Scene.Tracker.GetEntity<Player>();
            if (isRight)
            {
                if (entity != null && entity.Speed != Vector2.Zero &&
                    entity.X < (double)(level.Bounds.Right - 48))
                {
                    if (level.Tracker.GetEntity<BloomingFlower>() == null)
                    {
                        Scene.Add(new BloomingFlower());
                    }

                    return StChase;
                }
            }
            else
            {
                if (entity != null && entity.Speed != Vector2.Zero &&
                    entity.X > (double)(level.Bounds.Left + 48))
                {
                    if (level.Tracker.GetEntity<BloomingFlower>() == null)
                    {
                        Scene.Add(new BloomingFlower());
                    }

                    return StChase;
                }
            }

            return StWaiting;
        }

        private void HurtBegin()
        {
            Sprite.Play("hurt", true);
        }

        private int HurtUpdate()
        {
            if (isRight)
            {
                X -= 100f * Engine.DeltaTime;
                Y += 200f * Engine.DeltaTime;
                if (Top <= (double)(level.Bounds.Bottom + 20))
                    return StHurt;
                X = level.Camera.Right + 48f;
                cameraXOffset = 48f;
            }
            else
            {
                X += 100f * Engine.DeltaTime;
                Y += 200f * Engine.DeltaTime;
                if (Top <= (double)(level.Bounds.Bottom + 20))
                    return StHurt;
                X = level.Camera.Left - 48f;
                cameraXOffset = -48f;
            }

            doRespawnAnim = true;
            Visible = false;
            return StChase;
        }

        public bool IsLookedAt()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
                return false;
            Facings playerFacing = GetPlayerLookingAt();
            if (playerFacing == Facings.Right && isRight && X - 14f > player.Right)
                return true;
            if (playerFacing == Facings.Left && !isRight && X + 14f < player.Left)
                return true;
            return false;
        }

        public static Facings GetPlayerLookingAt()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                return Facings.Right;
            }
            Facings playerFacing = player.Facing;
            if (player.Sprite.CurrentAnimationID == "climbLookBack" ||
                player.Sprite.CurrentAnimationID == "climbLookBackStart")
            {
                return (Facings)(-(int)playerFacing);
            }
            return playerFacing;
        }
    }











    [Tracked]
    public class BloomingFlower : Entity
    {
        private readonly float downspeed;
        private Player player;
        private readonly Sprite sprite;
        private readonly float upspeed;
        private float currentframe;

        public BloomingFlower()
        {
            Depth = -12501;
            upspeed = 0.2f;
            downspeed = 0.1f;
            currentframe = 0;
            sprite = Class1.spriteBank.Create("blooming_flower");
            sprite.Position = new Vector2(0f, -8f);
            sprite.Play("blooming");
            Add(sprite);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                RemoveSelf();
                return;
            }
            X = player.X;
            Y = player.Top;
        }

        public override void Update()
        {
            base.Update();
            if (player.Dead)
            {
                RemoveSelf();
                return;
            }

            X = player.X;
            Y = player.Top;
            bool sawOshiro = false;
            bool oshiroHurt = true;
            foreach (BloomedOshiro oshiro in Scene.Tracker.GetEntities<BloomedOshiro>())
            {
                if (oshiro.state.State != BloomedOshiro.StHurt && !oshiro.doRespawnAnim && oshiro.Visible)
                {
                    oshiroHurt = false;
                }

                if (oshiro.IsLookedAt())
                    sawOshiro = true;
            }
            if (oshiroHurt)
                currentframe -= 2 * downspeed;
            else if (sawOshiro)
                currentframe += upspeed;
            else
                currentframe -= downspeed;
            if (currentframe < 0)
                currentframe = 0;
            else if (currentframe > 21)
                player.Die(-player.Height * Vector2.UnitY, true);
            sprite.SetAnimationFrame((int)Math.Ceiling(currentframe));
        }
    }
}