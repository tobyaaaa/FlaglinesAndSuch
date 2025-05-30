using System;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/ShootingEye")]
    public class ShootingEye : Entity
    {
        private MTexture eyeTexture;
        private MTexture pupilTexture;
        private Sprite eyelid;
        private Vector2 pupilPosition;
        private Vector2 pupilTarget;
        private float blinkTimer;
        private bool isBG;
        private bool shooting;
        private bool locked;
        private readonly DashListener dashListener;
        private SoundSource laserSfx;

        public ShootingEye(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            dashListener = new DashListener();
            dashListener.OnDash = OnPlayerDash;
            Add(dashListener);
            Add(laserSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            isBG = !scene.CollideCheck<Solid>(Position);
            if (isBG)
            {
                eyeTexture = GFX.Game["scenery/temple/eye/bg_eye"];
                pupilTexture = GFX.Game["scenery/temple/eye/bg_pupil"];
                Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/bg_lid"));
                Depth = 8990;
            }
            else
            {
                eyeTexture = GFX.Game["scenery/temple/eye/fg_eye"];
                pupilTexture = GFX.Game["scenery/temple/eye/fg_pupil"];
                Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/fg_lid"));
                Depth = -10001;
            }

            eyelid.AddLoop("open", "", 0.0f, new int[1]);
            eyelid.Add("blink", "", 0.08f, "open", 0, 1, 1, 2, 3, 0);
            eyelid.Play("open");
            eyelid.CenterOrigin();
            SetBlinkTimer();
        }

        private void SetBlinkTimer()
        {
            blinkTimer = Calc.Random.Range(1f, 15f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            pupilTarget = (entity.Center - Position).SafeNormalize();
            pupilPosition = pupilTarget * 3f;
        }

        public override void Update()
        {
            if (!locked)
                pupilPosition = Calc.Approach(pupilPosition, pupilTarget * 3f, Engine.DeltaTime * 16f);
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                pupilTarget = (entity.Center - Position).SafeNormalize();
                if (Scene.OnInterval(0.25f) && Calc.Random.Chance(0.01f) && !shooting)
                    eyelid.Play("blink");
            }

            blinkTimer -= Engine.DeltaTime;
            if (blinkTimer <= 0.0 && !shooting)
            {
                SetBlinkTimer();
                eyelid.Play("blink");
            }

            base.Update();
        }

        public override void Render()
        {
            eyeTexture.DrawCentered(Position);
            pupilTexture.DrawCentered(Position + pupilPosition, Color.Red);
            base.Render();
        }

        private void OnPlayerDash(Vector2 dir)
        {
            if (shooting)
                return;
            if (!(Scene as Level).IsInCamera(Position, 0))
                return;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
                return;
            Facings playerFacing = GetPlayerLookingAt();
            int sign = Math.Sign(X - player.X);
            if (sign * (int)playerFacing == 1)
                Add(new Coroutine(Beam()));
        }

        private IEnumerator Beam()
        {
            shooting = true;
            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            eyelid.Play("open");
            yield return 0.1f;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
                Scene.Add(Engine.Pooler.Create<EyeBeam>().Init(this, player));
            yield return 0.9f;
            locked = true;
            yield return 0.5f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
            shooting = false;
            locked = false;
        }

        public Vector2 BeamOrigin => pupilPosition * 1.2f + Position;


        [Pooled]
        [Tracked]
        private class EyeBeam : Entity
        {
            private VertexPositionColor[] fade = new VertexPositionColor[24];
            public const float ChargeTime = 1.4f;
            public const float FollowTime = 0.9f;
            public const float ActiveTime = 0.12f;
            private const float AngleStartOffset = 100f;
            private const float RotationSpeed = 200f;
            private const float CollideCheckSep = 2f;
            private const float BeamLength = 2000f;
            private const float BeamStartDist = 12f;
            private const int BeamsDrawn = 15;
            private const float SideDarknessAlpha = 0.35f;
            private ShootingEye eye;
            private Player player;
            private Sprite beamSprite;
            private Sprite beamStartSprite;
            private float chargeTimer;
            private float followTimer;
            private float activeTimer;
            private float angle;
            private float beamAlpha;
            private float sideFadeAlpha;

            public EyeBeam()
            {
                Add(beamSprite = GFX.SpriteBank.Create("badeline_beam"));
                beamSprite.OnLastFrame = anim =>
                {
                    if (!(anim == "shoot"))
                        return;
                    Destroy();
                };
                Add(beamStartSprite = GFX.SpriteBank.Create("badeline_beam_start"));
                beamSprite.Visible = false;
                Depth = -1000000;
            }

            public EyeBeam Init(ShootingEye eye, Player target)
            {
                this.eye = eye;
                chargeTimer = ChargeTime;
                followTimer = FollowTime;
                activeTimer = ActiveTime;
                beamSprite.Play("charge");
                sideFadeAlpha = 0.0f;
                beamAlpha = 0.0f;
                int num = target.Y > eye.Y ? -1 : 1;
                if (target.X >= eye.X)
                    num *= -1;
                angle = Calc.Angle(eye.BeamOrigin, target.Center);
                Vector2 to =
                    Calc.ClosestPointOnLine(eye.BeamOrigin, eye.BeamOrigin + Calc.AngleToVector(angle, BeamLength),
                        target.Center)
                    + (target.Center - eye.BeamOrigin).Perpendicular().SafeNormalize(AngleStartOffset) * num;
                angle = Calc.Angle(eye.BeamOrigin, to);
                return this;
            }

            public override void Update()
            {
                base.Update();
                player = Scene.Tracker.GetEntity<Player>();
                beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);
                if (chargeTimer > 0.0)
                {
                    sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                    if (player == null || player.Dead)
                        return;
                    followTimer -= Engine.DeltaTime;
                    chargeTimer -= Engine.DeltaTime;
                    if (followTimer > 0.0 && player.Center != eye.BeamOrigin)
                        angle = Calc.Angle(eye.BeamOrigin,
                            Calc.Approach(
                                Calc.ClosestPointOnLine(eye.BeamOrigin,
                                    eye.BeamOrigin + Calc.AngleToVector(angle, BeamLength), player.Center),
                                player.Center, RotationSpeed * Engine.DeltaTime));
                    else if (beamSprite.CurrentAnimationID == "charge")
                        beamSprite.Play("lock");
                    if (chargeTimer <= 0.0)
                    {
                        SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        DissipateParticles();
                    }
                }
                else
                {
                    if (activeTimer <= 0.0)
                        return;
                    sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0.0f, Engine.DeltaTime * 8f);
                    if (beamSprite.CurrentAnimationID != "shoot")
                    {
                        beamSprite.Play("shoot");
                        beamStartSprite.Play("shoot", true);
                    }

                    activeTimer -= Engine.DeltaTime;
                    if (activeTimer > 0.0)
                        PlayerCollideCheck();
                }
            }

            private void DissipateParticles()
            {
                Level level = SceneAs<Level>();
                Vector2 closestTo = level.Camera.Position + new Vector2(160f, 90f);
                Vector2 lineA = eye.BeamOrigin + Calc.AngleToVector(angle, BeamStartDist);
                Vector2 lineB = eye.BeamOrigin + Calc.AngleToVector(angle, BeamLength);
                Vector2 vector = (lineB - lineA).Perpendicular().SafeNormalize();
                Vector2 vector2_1 = (lineB - lineA).SafeNormalize();
                Vector2 min = -vector * 1f;
                Vector2 max = vector * 1f;
                float direction1 = vector.Angle();
                float direction2 = (-vector).Angle();
                float num = Vector2.Distance(closestTo, lineA) - BeamStartDist;
                Vector2 vector2_2 = Calc.ClosestPointOnLine(lineA, lineB, closestTo);
                for (int index1 = 0; index1 < 200; index1 += 12)
                {
                    for (int index2 = -1; index2 <= 1; index2 += 2)
                    {
                        level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                            vector2_2 + vector2_1 * index1 + vector * 2f * index2 +
                            Calc.Random.Range(min, max), direction1);
                        level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                            vector2_2 + vector2_1 * index1 - vector * 2f * index2 +
                            Calc.Random.Range(min, max), direction2);
                        if (index1 != 0 && index1 < num)
                        {
                            level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                                vector2_2 - vector2_1 * index1 + vector * 2f * index2 +
                                Calc.Random.Range(min, max), direction1);
                            level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate,
                                vector2_2 - vector2_1 * index1 - vector * 2f * index2 +
                                Calc.Random.Range(min, max), direction2);
                        }
                    }
                }
            }

            private void PlayerCollideCheck()
            {
                Vector2 from = eye.BeamOrigin + Calc.AngleToVector(angle, BeamStartDist);
                Vector2 to = eye.BeamOrigin + Calc.AngleToVector(angle, BeamLength);
                Vector2 vector2 = (to - from).Perpendicular().SafeNormalize(CollideCheckSep);
                Player player =
                    (Scene.CollideFirst<Player>(from + vector2, to + vector2) ??
                     Scene.CollideFirst<Player>(from - vector2, to - vector2)) ??
                    Scene.CollideFirst<Player>(from, to);
                player?.Die((player.Center - eye.BeamOrigin).SafeNormalize());
                foreach (FallingBlock block in Scene.CollideAll<FallingBlock>(from, to))
                {
                    if (!block.HasStartedFalling)
                    {
                        block.StartShaking();
                        block.Triggered = true;
                        block.FallDelay = 0.4f;
                    }
                }
            }

            public override void Render()
            {
                Vector2 beamOrigin = eye.BeamOrigin;
                Vector2 vector1 = Calc.AngleToVector(angle, beamSprite.Width);
                beamSprite.Rotation = angle;
                beamSprite.Color = Color.White * beamAlpha;
                beamStartSprite.Rotation = angle;
                beamStartSprite.Color = Color.White * beamAlpha;
                if (beamSprite.CurrentAnimationID == "shoot")
                    beamOrigin += Calc.AngleToVector(angle, 8f);
                for (int index = 0; index < BeamsDrawn; ++index)
                {
                    beamSprite.RenderPosition = beamOrigin;
                    beamSprite.Render();
                    beamOrigin += vector1;
                }

                if (beamSprite.CurrentAnimationID == "shoot")
                {
                    beamStartSprite.RenderPosition = eye.BeamOrigin;
                    beamStartSprite.Render();
                }

                GameplayRenderer.End();
                Vector2 vector2 = vector1.SafeNormalize();
                Vector2 vector2_1 = vector2.Perpendicular();
                Color color = Color.Black * this.sideFadeAlpha * SideDarknessAlpha;
                Color transparent = Color.Transparent;
                Vector2 vector2_2 = vector2 * 4000f;
                Vector2 vector2_3 = vector2_1 * 120f;
                int v = 0;
                this.Quad(ref v, beamOrigin, -vector2_2 + vector2_3 * 2f, vector2_2 + vector2_3 * 2f,
                    vector2_2 + vector2_3,
                    -vector2_2 + vector2_3, color, color);
                this.Quad(ref v, beamOrigin, -vector2_2 + vector2_3, vector2_2 + vector2_3, vector2_2, -vector2_2,
                    color,
                    transparent);
                this.Quad(ref v, beamOrigin, -vector2_2, vector2_2, vector2_2 - vector2_3, -vector2_2 - vector2_3,
                    transparent, color);
                this.Quad(ref v, beamOrigin, -vector2_2 - vector2_3, vector2_2 - vector2_3, vector2_2 - vector2_3 * 2f,
                    -vector2_2 - vector2_3 * 2f, color, color);
                GFX.DrawVertices<VertexPositionColor>((this.Scene as Level).Camera.Matrix, this.fade, this.fade.Length,
                    (Effect)null, (BlendState)null);
                GameplayRenderer.Begin();
            }

            private void Quad(
                ref int v,
                Vector2 offset,
                Vector2 a,
                Vector2 b,
                Vector2 c,
                Vector2 d,
                Color ab,
                Color cd)
            {
                fade[v].Position.X = offset.X + a.X;
                fade[v].Position.Y = offset.Y + a.Y;
                fade[v++].Color = ab;
                fade[v].Position.X = offset.X + b.X;
                fade[v].Position.Y = offset.Y + b.Y;
                fade[v++].Color = ab;
                fade[v].Position.X = offset.X + c.X;
                fade[v].Position.Y = offset.Y + c.Y;
                fade[v++].Color = cd;
                fade[v].Position.X = offset.X + a.X;
                fade[v].Position.Y = offset.Y + a.Y;
                fade[v++].Color = ab;
                fade[v].Position.X = offset.X + c.X;
                fade[v].Position.Y = offset.Y + c.Y;
                fade[v++].Color = cd;
                fade[v].Position.X = offset.X + d.X;
                fade[v].Position.Y = offset.Y + d.Y;
                fade[v++].Color = cd;
            }

            public void Destroy()
            {
                RemoveSelf();
            }
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
}