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

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/ShyGhost")]
    public class ShyGhost : Actor
    {
        private bool reversed;
        private float movingSpeed;
        private StateMachine stateMachine;
        private const int StSleep = 0;
        private const int StShy = 1;
        private const int StAttack = 2;
        private const int StHurt = 3;
        private Sprite sprite;
        private Vector2 speed;
        public ShyGhost(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -10001;
            AllowPushing = false;
            reversed = data.Bool("reversed");
            movingSpeed = data.Float("speed", 60);
            stateMachine = new StateMachine(4);
            stateMachine.SetCallbacks(StSleep, SleepUpdate, null, SleepBegin);
            stateMachine.SetCallbacks(StShy, ShyUpdate, null, ShyBegin);
            stateMachine.SetCallbacks(StAttack, AttackUpdate, null, AttackBegin);
            stateMachine.SetCallbacks(StHurt, null, null, HurtBegin);
            Add(stateMachine);
            Collider = new Circle(6f);
            Add(sprite = Class1.spriteBank.Create("shy_ghost"));
            Add(new PlayerCollider(OnPlayer));
            Add(new PlayerCollider(OnBounce, new Hitbox(16f, 6f, -8f, -3f)));
            bool right = data.Bool("right");
            if (right)
                sprite.Scale.X = -1;
        }

        public override void Update()
        {
            base.Update();
            NaiveMove(speed * Engine.DeltaTime);
        }

        private void SleepBegin()
        {
            speed = Vector2.Zero;
            sprite.Play("sleep");
        }
        private int SleepUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.Speed != Vector2.Zero)
            {
                if (IsLookedAt())
                    return reversed ? StAttack : StShy;
                return reversed ? StShy : StAttack;
            }
            return StSleep;
        }

        private void ShyBegin()
        {
            speed = Vector2.Zero;
            sprite.Play("shy", false, true);
        }

        private int ShyUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (IsLookedAt())
                    return reversed ? StAttack : StShy;
                return reversed ? StShy : StAttack;
            }
            return StShy;
        }

        private void AttackBegin()
        {
            sprite.Play("chase", false, true);
        }

        private int AttackUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && !player.Dead)
            {
                sprite.Scale.X = X > player.X ? 1 : -1;
                Facings playerFacing = GetPlayerLookingAt();
                Vector2 dir;
                if (reversed)
                    dir = player.Position + new Vector2((int)playerFacing * player.Width, -4) - Position;
                else
                    dir = player.Position - new Vector2((int)playerFacing * player.Width, 4) - Position;
                speed = dir.SafeNormalize() * movingSpeed;
                if (IsLookedAt())
                    return reversed ? StAttack : StShy;
                return reversed ? StShy : StAttack;
            }
            speed = Vector2.Zero;
            return StAttack;
        }

        private void HurtBegin()
        {
            speed = Vector2.Zero;
            sprite.Play("die");
            sprite.OnLastFrame = anim =>
            {
                RemoveSelf();
            };
        }

        private void OnPlayer(Player player)
        {
            if (stateMachine.State == StHurt)
                return;
            if (stateMachine.State != StAttack && player.Bottom <= Y + 4.0)
                return;
            player.Die((player.Center - Center).SafeNormalize());
        }

        private void OnBounce(Player player)
        {
            if (stateMachine.State == StHurt || stateMachine.State == StAttack)
                return;
            if (player.Bottom > Y + 4.0 || player.Speed.Y < 0.0)
                return;
            Audio.Play("event:/game/general/thing_booped", Position);
            Collidable = false;
            player.Bounce((int)(Y - 2.0));
            stateMachine.State = StHurt;
        }

        private bool IsLookedAt()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
                return false;
            int playerLooking = (int)GetPlayerLookingAt();
            int sign = Math.Sign(X - player.X - playerLooking * (player.Width - 16f) / 2f);
            return sign * playerLooking == 1;
        }

        public override bool IsRiding(JumpThru jumpThru)
        {
            return false;
        }

        public override bool IsRiding(Solid solid)
        {
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
}
