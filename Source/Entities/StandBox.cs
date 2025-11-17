using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.RuntimeDetour;
using System.Reflection;


namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/StandBox")]
    public class StandBox : Actor
    {
        public Vector2 Speed;
        public Holdable Hold;
        private Sprite sprite;
        private Level Level;
        private Collision onCollideH;
        private Collision onCollideV;
        private float noGravityTimer;
        private Vector2 prevLiftSpeed;
        private BoxJumpThru stand;
        public BoxJumpThru Stand => stand;

        public StandBox(EntityData e, Vector2 offset)
            : base(e.Position + offset)
        {
            Depth = 100;
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(sprite = Class1.spriteBank.Create("stand_box"));
            sprite.Play("idle");
            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(20f, 23f, -10f, -17f);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.OnHitSpring = HitSpring;
            Hold.SpeedGetter = () => Speed;
            Hold.OnCarry = OnCarry;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            Add(new MirrorReflection());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            stand = new BoxJumpThru(Position, 16, this);
            Scene.Add(stand);
        }

        public override void Update()
        {
            base.Update();
            Depth = 100;
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else
            {
                if (OnGround(1))
                {
                    Speed.X = Calc.Approach(Speed.X,
                        OnGround(Position + Vector2.UnitX * 3f, 1)
                            ? (OnGround(Position - Vector2.UnitX * 3f, 1) ? 0.0f : -20f)
                            : 20f, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0.0f);
                        if (Speed.X != 0.0 && Speed.Y == 0.0)
                            Speed.Y = -60f;
                        if (Speed.Y < 0.0)
                            noGravityTimer = 0.15f;
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0.0 && Speed.Y < 0.0)
                            Speed.Y = 0.0f;
                    }

                    //Position.Y = (float)Math.Ceiling(Position.Y); //attempt to prevent box from landing at a subpixel pos.
                    //works a little, but not perfectly. boxes can still end up w/ subpixel pos. (try both...?)

                }
                else if (Hold.ShouldHaveGravity)
                {
                    float num1 = 800f;
                    if (Math.Abs(Speed.Y) <= 30.0)
                        num1 *= 0.5f;
                    float num2 = 350f;
                    if (Speed.Y < 0.0)
                        num2 *= 0.5f;
                    Speed.X = Calc.Approach(Speed.X, 0.0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0.0)
                        noGravityTimer -= Engine.DeltaTime;
                    else
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num1 * Engine.DeltaTime);
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                if (Left < Level.Bounds.Left)
                {
                    Left = Level.Bounds.Left;
                    stand.SetPosition(Position);
                    OnCollideH(new CollisionData
                    {
                        Direction = -Vector2.UnitX
                    });
                }
                else if (Right > Level.Bounds.Right)
                {
                    Right = Level.Bounds.Right;
                    stand.SetPosition(Position);
                    OnCollideH(new CollisionData
                    {
                        Direction = Vector2.UnitX
                    });
                }
                if (Top < Level.Bounds.Top)
                {
                    Top = Level.Bounds.Top;
                    stand.SetPosition(Position);
                    OnCollideV(new CollisionData
                    {
                        Direction = -Vector2.UnitY
                    });
                }
                else if (Top > Level.Bounds.Bottom + 16)
                {
                    Destroy();
                    return;
                }
            }

            Hold.CheckAgainstColliders();
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0.0)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }

                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0.0)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }

                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0.0)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }

            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            //else if (data.Hit is PairedDashSwitch)
            //{
            //    (data.Hit as PairedDashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            //}

            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            //else if (data.Hit is PairedDashSwitch)
            //{
            //    (data.Hit as PairedDashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            //}

            if (Speed.Y > 0.0)
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity",
                        0.0f);
            }

            Speed.Y = 0.0f;
        }



        public override bool IsRiding(Solid solid)
        {
            return Speed.Y == 0.0 && base.IsRiding(solid);
        }

        protected override void OnSquish(CollisionData data)
        {
            if (TrySquishWiggle(data))
                return;
            Destroy();
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            stand.Collidable = false;
            AddTag((int)Tags.Persistent);
            stand.AddTag((int)Tags.Persistent);
        }

        private void OnRelease(Vector2 force)
        {
            //try resetting subpixels here
            Position = Position.Round();
            //
            stand.Collidable = true;
            RemoveTag((int)Tags.Persistent);
            stand.RemoveTag((int)Tags.Persistent);
            stand.SetPosition(Position);
            if (force.X != 0.0 && force.Y == 0.0)
                force.Y = -0.4f;
            Speed = force * 200f;
            if (!(Speed != Vector2.Zero))
                return;
            noGravityTimer = 0.1f;
        }

        private void Destroy()
        {
            stand.RemoveStaticMovers();
            stand.RemoveSelf();
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            stand = null;
        }

        private void OnCarry(Vector2 target)
        {
            Position = target;
            stand.SetPosition(target);
        }

        public override bool IsRiding(JumpThru jumpThru)
        {
            bool flag = stand.Collidable;
            stand.Collidable = false;
            bool result = base.IsRiding(jumpThru);
            stand.Collidable = flag;
            return result;
        }

        private static Hook hookOnTrySquishWiggle;

        public static void Load()
        {
            On.Celeste.Actor.MoveHExact += BoxMoveHExact;
            On.Celeste.Actor.MoveVExact += BoxMoveVExact;

            // the number of parameters of TrySquishWiggle changes between starting with 1.3.3.10.
            // both methods exist as they are added by Everest, so we should pick the right one to hook based on Celeste version.
            if (Celeste.Celeste.Instance.Version >= new Version(1, 3, 3, 10))
            {
                // 1.3.3.10 and up: 3 parameters
                hookOnTrySquishWiggle = new Hook(typeof(Player).GetMethod("TrySquishWiggle", BindingFlags.NonPublic | BindingFlags.Instance,
                    null, CallingConventions.Any, new Type[] { typeof(CollisionData), typeof(int), typeof(int) }, null),
                    typeof(StandBox).GetMethod("BoxWiggle_1_3_3", BindingFlags.NonPublic | BindingFlags.Static));
            }
            else
            {
                // before 1.3.3.10: 1 parameter
                hookOnTrySquishWiggle = new Hook(typeof(Player).GetMethod("TrySquishWiggle", BindingFlags.NonPublic | BindingFlags.Instance,
                    null, CallingConventions.Any, new Type[] { typeof(CollisionData) }, null),
                    typeof(StandBox).GetMethod("BoxWiggle_1_3_1", BindingFlags.NonPublic | BindingFlags.Static));
            }

            On.Celeste.Actor.OnGround_int += BoxOnGround;

            On.Celeste.Spring.OnCollide += BoxOnSpring;
            On.Celeste.Spikes.IsRiding_JumpThru += BoxRidingSpike;
            On.Celeste.Spikes.OnCollide += BoxOnSpike;
            //On.Celeste.TriggerSpikes.IsRiding_JumpThru += BoxRidingTriggerSpike;
            //On.Celeste.TriggerSpikes.OnCollide += BoxOnTriggerSpike;
        }

        public static void UnLoad()
        {

            On.Celeste.Actor.MoveHExact -= BoxMoveHExact;
            On.Celeste.Actor.MoveVExact -= BoxMoveVExact;

            hookOnTrySquishWiggle?.Dispose();
            hookOnTrySquishWiggle = null;

            On.Celeste.Actor.OnGround_int -= BoxOnGround;

            On.Celeste.Spring.OnCollide -= BoxOnSpring;
            On.Celeste.Spikes.IsRiding_JumpThru -= BoxRidingSpike;
            On.Celeste.Spikes.OnCollide -= BoxOnSpike;
            //On.Celeste.TriggerSpikes.IsRiding_JumpThru -= BoxRidingTriggerSpike;
            //On.Celeste.TriggerSpikes.OnCollide -= BoxOnTriggerSpike;
        }

        private static bool BoxMoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH,
            Collision onCollide, Solid pusher)
        {
            if (self is StandBox)
            {
                StandBox box = self as StandBox;
                bool flag = box.Stand.Collidable;
                box.Stand.Collidable = false;
                float origX = box.Position.X;
                bool result = orig(self, moveH, onCollide, pusher);
                float actualH = box.Position.X - origX;
                box.Stand.Collidable = flag;
                box.Stand.MoveH(actualH, box.Speed.X);
                return result;
            }
            return orig(self, moveH, onCollide, pusher);
        }

        private static bool BoxMoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV,
            Collision onCollide, Solid pusher)
        {
            if (self is StandBox)
            {
                StandBox box = self as StandBox;
                bool flag = box.Stand.Collidable;
                box.Stand.Collidable = false;
                float origY = box.Position.Y;
                bool result = orig(self, moveV, onCollide, pusher);
                float actualV = box.Position.Y - origY;
                box.Stand.Collidable = flag;
                box.Stand.MoveV(actualV, box.Speed.Y);
                return result;
            }

            return orig(self, moveV, onCollide, pusher);
        }

        // TrySquishWiggle got more parameters in 1.3.3.10, so we have to handle both signatures.

        private delegate bool orig_TrySquishWiggle_1_3_1(Actor self, CollisionData data);
        private delegate bool orig_TrySquishWiggle_1_3_3(Actor self, CollisionData data, int wiggleX, int wiggleY);

        private static bool BoxWiggle_1_3_1(orig_TrySquishWiggle_1_3_1 orig, Actor self, CollisionData data)
        {
            return BoxWiggle(() => orig(self, data), self);
        }

        private static bool BoxWiggle_1_3_3(orig_TrySquishWiggle_1_3_3 orig, Actor self, CollisionData data, int wiggleX, int wiggleY)
        {
            return BoxWiggle(() => orig(self, data, wiggleX, wiggleY), self);
        }


        private static bool BoxWiggle(Func<bool> orig, Actor self)
        {
            if (self is StandBox)
            {
                StandBox box = self as StandBox;
                bool flag = box.Stand.Collidable;
                box.Stand.Collidable = false;
                bool result = orig();
                box.Stand.Collidable = flag;
                if (result)
                {
                    box.Stand.SetPosition(box.Position);
                }
                return result;
            }

            return orig();
        }

        private static bool BoxOnGround(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int off)
        {
            if (self is StandBox)
            {
                StandBox box = self as StandBox;
                bool flag = box.Stand.Collidable;
                box.Stand.Collidable = false;
                bool result = orig(self, off);
                box.Stand.Collidable = flag;

                return result;
            }

            return orig(self, off);
        }

        private static void BoxOnSpring(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player player)
        {
            if (player?.Holding != null)
            {
                Entity entity = player.Holding.Entity;
                if (entity is StandBox)
                {
                    StandBox box = entity as StandBox;
                    if (box.Stand.ContainEntity(self))
                        return;
                }
            }

            orig(self, player);
        }

        private static bool BoxRidingSpike(On.Celeste.Spikes.orig_IsRiding_JumpThru orig, Spikes self,
            JumpThru jumpThru)
        {
            if (jumpThru is BoxJumpThru)
            {
                switch (self.Direction)
                {
                    case Spikes.Directions.Up:
                        return orig(self, jumpThru);
                    case Spikes.Directions.Down:
                        return self.CollideCheck(jumpThru, self.Position - Vector2.UnitY);
                    case Spikes.Directions.Left:
                        return self.CollideCheck(jumpThru, self.Position + Vector2.UnitX);
                    case Spikes.Directions.Right:
                        return self.CollideCheck(jumpThru, self.Position - Vector2.UnitX);
                }
            }

            return orig(self, jumpThru);
        }

        private static void BoxOnSpike(On.Celeste.Spikes.orig_OnCollide orig, Spikes self, Player player)
        {
            if (player?.Holding != null)
            {
                Entity entity = player.Holding.Entity;
                if (entity is StandBox)
                {
                    StandBox box = entity as StandBox;
                    if (box.Stand.ContainEntity(self))
                        return;
                }
            }

            orig(self, player);
        }

        /*
        private static bool BoxRidingTriggerSpike(On.Celeste.TriggerSpikes.orig_IsRiding_JumpThru orig, TriggerSpikes self,
            JumpThru jumpThru)
        {
            Console.WriteLine("Wft entered");
            if (jumpThru is BoxJumpThru)
            {
                FieldInfo dirInfo =
                    typeof(TriggerSpikes).GetField("direction", BindingFlags.Instance | BindingFlags.NonPublic);
                if(dirInfo == null)
                    Console.WriteLine("Wft");
                TriggerSpikes.Directions dir = (TriggerSpikes.Directions)dirInfo?.GetValue(self);
                switch (dir)
                {
                    case TriggerSpikes.Directions.Up:
                        return orig(self, jumpThru);
                    case TriggerSpikes.Directions.Down:
                        return self.CollideCheck(jumpThru, self.Position - Vector2.UnitY);
                    case TriggerSpikes.Directions.Left:
                        return self.CollideCheck(jumpThru, self.Position + Vector2.UnitX);
                    case TriggerSpikes.Directions.Right:
                        return self.CollideCheck(jumpThru, self.Position - Vector2.UnitX);
                }
            }

            return orig(self, jumpThru);
        }

        private static void BoxOnTriggerSpike(On.Celeste.TriggerSpikes.orig_OnCollide orig, TriggerSpikes self, Player player)
        {
            Console.WriteLine("Wft Becha");
            if (player?.Holding != null)
            {
                Entity entity = player.Holding.Entity;
                if (entity is StandBox)
                {
                    StandBox box = entity as StandBox;
                    if (box.Stand.ContainEntity(self))
                        return;
                }
            }

            orig(self, player);
        }
        */

        public class BoxJumpThru : JumpThru
        {
            private static readonly Vector2 offset = new Vector2(-8f, -16f);
            private StandBox box;
            public float DEBUG_IDENTIFIER;
            public StandBox Box => box;
            public BoxJumpThru(Vector2 position, int width, StandBox parent) : base(position + offset, width, false)
            {
                box = parent;
                DEBUG_IDENTIFIER = position.X;
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                foreach (StaticMover mover in staticMovers)
                {
                    mover.Entity.Depth = 101;
                }
            }

            public void SetPosition(Vector2 target) //only called while held I think
            {
                Vector2 origPosition = Position;
                Position = target + offset;
                MoveStaticMovers(Position - origPosition);

            }

            public override void MoveHExact(int move)
            {
                box.Collidable = false;
                base.MoveHExact(move);
                box.Collidable = true;
            }

            public override void MoveVExact(int move)
            {
                box.Collidable = false;
                base.MoveVExact(move);
                box.Collidable = true;


                Console.WriteLine("standbox " + DEBUG_IDENTIFIER + " is at " + Position.X + ", " + Position.Y + "and" + (Position == box.Position));
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                box = null;
            }

            public void RemoveStaticMovers()
            {
                foreach (StaticMover mover in staticMovers)
                {
                    mover.Destroy();
                }
            }

            public bool ContainEntity(Entity entity)
            {
                foreach (StaticMover mover in staticMovers)
                {
                    if (mover.Entity == entity)
                        return true;
                }

                return false;
            }
        }
    }
}
