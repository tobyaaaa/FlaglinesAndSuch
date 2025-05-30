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
    [CustomEntity("FlaglinesAndSuch/RocketBarrel")]
    class RocketBarrel : Actor
    {
        public static void Load()
        {
            //rocket barrel hooks
            On.Celeste.Player.Update += RBPlayerUpdate;           //hijacks the player hitbox and moves it to the barrel; with the exception of holables (somehow)
            On.Celeste.Player.Die += RBPlayerDie;                 //makes the player not die when touching spikes, but rather reduce the barrel's health. Keep in mind infloops
            //On.Celeste.Player.Bounce += RBPlayerBounce;           //makes ice balls, ect. work like I want. (might be the same?)
            //On.Celeste.Player.SuperBounce += RBPlayerSuperBounce; //makes springs behave (might be the same?)
            //On.Celeste.Player.SideBounce += RBPlayerSideBounce;   //makes side-springs change the barrel's direction
        }

        public static void UnLoad()
        {
            On.Celeste.Player.Update -= RBPlayerUpdate;
            On.Celeste.Player.Die -= RBPlayerDie;
            //On.Celeste.Player.Bounce -= RBPlayerBounce;
            //On.Celeste.Player.SuperBounce -= RBPlayerSuperBounce;
            //On.Celeste.Player.SideBounce -= RBPlayerSideBounce;
        }


        private Sprite sprite;
        private Level Level;

        public static ParticleType P_RBarrelParticle;//??

        private Collision OnCollideVert;
        private Collision OnCollideHorz;
        Shaker shaker;
        enum barrel_states { Idle, Active, noFuel, crash }
        barrel_states barrel_state = barrel_states.Idle;

        int health = 2;
        private float vert_velocity = 0;
        private float vert_accel = -0.05f;
        float stun_timer = 0f;

        static Player barrel_rider; //bilbo baggins
        bool barrel_rider_on_this = false;
        static RocketBarrel CurrentBarrel = null;
        static Collider playercolliderstorage;

        //unchanging speed variables; not const because I might give them to the mapper
        float SpeedH = 1.5f;
        float SpeedMaxUp = -2.4f;
        float SpeedMaxDn = 2.8f;
        float MaxStunTimer = 0.8f;
        float WindDivider = 400f; //how much less of an effect wind should have. Don't set to one. DEFINITELY don't set to 0.

        public RocketBarrel(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Add(sprite = Class1.spriteBank.Create("RBarrel"));
            Add(shaker = new Shaker(on: false));
            Add(new PlayerCollider(OnPlayer));
            sprite.Play("idle");
            base.Collider = new Hitbox(16f, 14f, -8f, -6f);
            OnCollideVert = CollisionV;
            OnCollideHorz = CollisionH;

            base.Depth = -9000;

            P_RBarrelParticle = new ParticleType(Booster.P_BurstRed);//??
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            CurrentBarrel = null; //should be moved so the player can "carry" a barrel in a room transition
            barrel_rider = null;
        }

        public override void Update()
        {
            base.Update();
            if (barrel_state == barrel_states.Idle)
            {
                IdleUpdate();
            }
            if (barrel_state == barrel_states.Active)
            {
                ActiveUpdate();
            }
        }


        private void IdleUpdate() {
        
        }

        private void ActiveUpdate() {
            //input (barrel)
            if (Input.Jump.Check)
            {
                vert_accel = -0.07f;
            }
            else
            {
                vert_accel = 0.07f;
            }
            vert_velocity += vert_accel;
            //clamp velocity
            if (vert_velocity < SpeedMaxUp) { vert_velocity -= vert_velocity * 0.1f; }
            if (vert_velocity > SpeedMaxDn) { vert_velocity -= vert_velocity * 0.1f; }

            //add wind
            Vector2 wind_add = (base.Scene as Level).Wind / WindDivider;
            //move
            MoveV(vert_velocity + wind_add.Y, OnCollideVert);
            MoveH(SpeedH + wind_add.X, OnCollideHorz);
            //handle vertical OOB cases
            if (base.Top < (float)(Level.Bounds.Top - 4))
            {
                base.Top = Level.Bounds.Top - 3;
            }
            else if ((base.Top > (float)Level.Bounds.Bottom) || base.Bottom > (float)Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
            {
                Damage(new Vector2(Position.X, Position.Y + 2), true, false, stun_timer <= 0);
            }

            barrel_rider.Position = Position;
            
            //decrease timer for i-frames
            if (stun_timer > 0)
            {
                stun_timer -= Engine.DeltaTime;
            }
            //rotate sprite and do particles
            sprite.Rotation = (float)(Math.Atan((double)((vert_velocity + wind_add.Y) / 2.0f))+ (Math.PI / 2)); // + (Math.Sin(Engine.DeltaTime / 2.0)) ) 
            if (Scene.OnInterval(0.02f))
            {
                (Scene as Level).ParticlesBG.Emit(P_RBarrelParticle, 2, barrel_rider.Center + new Vector2(0f, 5f), new Vector2(3f, 3f), Color.Lerp(Color.Gray, Color.Black, Calc.Random.NextFloat(0.2f)));
                //maybe have two sets of particles; dark ones drawn first, lighter ones (with a smaller range of positions) drawn later (doesn't quite work; next grey particle goes above the black)
            }
            //if (Scene.OnInterval(0.03f))
            //{
            //    (Scene as Level).ParticlesBG.Emit(P_RBarrelParticle, 2, barrel_rider.Center + new Vector2(0f, 5f), new Vector2(1.5f, 1.5f), Color.Black);
                //maybe have two sets of particles; dark ones drawn first, lighter ones (with a smaller range of positions) drawn later
            //}
        }



        private void CollisionV(CollisionData data) {
            //treat invis. barriers like 1f0
            if (data.Hit is InvisibleBarrier) {
                return; 
            }
            //handle dream blocks... somehow?
            if (data.Hit is DreamBlock)
            {
                return;
            }
            //call damage
            else if (stun_timer <= 0) {
                Damage(data.TargetPosition, true, false, true);
                if (data.Hit is DashBlock)
                {
                    (data.Hit as DashBlock).Break(Position, Position, true);
                }
            }
            else
            {
                //Damage(data.TargetPosition, false, true, false);
            }
        }

        private void CollisionH(CollisionData data)
        {
            //not much functionality yet... should push the barrel vertically to get back on the course
            if (stun_timer > 0)
            {
                if (data.TargetPosition.Y < Position.Y)
                {
                    vert_velocity = 1.8f;
                }
                else
                {
                    vert_velocity = -1.8f;
                }
                stun_timer = MaxStunTimer;
            }
        }

        void Damage(Vector2 ouchiePosition, bool dobounce, bool dofreeze, bool dodamage) {
            if (dobounce)
            {
                if (ouchiePosition.Y < Position.Y)
                {
                    vert_velocity = 1.8f;
                }
                else
                {
                    vert_velocity = -1.8f;
                }
            }
            if (dodamage) {
                health--;
                if (health == 0 && !SaveData.Instance.Assists.Invincible)
                {
                    //handle barrel crashing. Shouldn't kill player if they're on a different barrel
                    barrel_state = barrel_states.crash;
                    if (barrel_rider_on_this) {
                        barrel_rider_on_this = false;
                        CurrentBarrel = null;
                        barrel_rider.Collider = playercolliderstorage;

                        barrel_rider.Collidable = true;
                        barrel_rider.ForceCameraUpdate = false;
                        barrel_rider.StateMachine.State = 0;
                        barrel_rider.Die(-ouchiePosition);
                    }
                }
                else
                {
                    Audio.Play("event:/game/general/wall_break_wood", Position);
                }
                stun_timer = MaxStunTimer;
            }
            if (dofreeze)
            {
                if (ouchiePosition.Y < Position.Y)
                {
                    vert_velocity = 0;
                }
                else
                {
                    vert_velocity = 0;
                }
            }
        }

        private void OnPlayer(Player hitplayer)
        {
            if (barrel_state == barrel_states.Idle && barrel_rider == null) {
                barrel_rider = hitplayer;
                barrel_rider_on_this = true;
                barrel_rider.StateMachine.State = 11; //dummy state
                barrel_rider.ForceCameraUpdate = true;
                barrel_rider.Collidable = false;
                barrel_rider.Facing = Facings.Right; //change to reflect barrel's motion

                //activation cutscene here
                Add(new Coroutine(ActivationRoutine()));
            }
        }


        private IEnumerator ActivationRoutine()
        {
            int checks = 0;
            while (checks < 4)
            {
                barrel_rider.Position = Position; //move this to an idle update kind of deal?
                if (Input.Jump.Pressed)
                {
                    //shaker.ShakeFor(0.2f, false);
                    checks++;
                    (Scene as Level).ParticlesBG.Emit(P_RBarrelParticle, 2 * checks, barrel_rider.Center + new Vector2(0f, 8f), new Vector2(3f, 3f), Color.Lerp(Color.Gray, Color.Black, Calc.Random.NextFloat()) , 90f); /* (0.5f + Calc.Random.NextFloat(0.5f))*/

                    if (checks >= 4)
                    {

                        barrel_state = barrel_states.Active;
                        sprite.Play("startup");
                        vert_velocity = SpeedMaxUp * 1.5f;

                        //yield break;
                    }

                    yield return 0.05f;
                }

                yield return null;
            }
            //yield break;
        }



        private static void RBPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            if (self == barrel_rider) {
                if (CurrentBarrel == null)
                {
                    playercolliderstorage = self.Collider;
                    CurrentBarrel = (RocketBarrel)Collide.First(self, self.Scene.Entities.FindAll<RocketBarrel>());
                }
                else {
                    self.Collider = playercolliderstorage;
                }
                //replace player hitbox w/ rocket hitbox
                //do collision checks, for the specific entities I care about (somehow? How does OnPlayer work?)
                //restore player hitbox
                //check for collisions *again*?
                self.Collider = CurrentBarrel.Collider;
            }

            orig(self);
        }

        private static PlayerDeadBody RBPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true){
            if (self == barrel_rider && CurrentBarrel.health >= 0) {
                if (CurrentBarrel.stun_timer <= 0) {
                    CurrentBarrel.Damage(direction, false, false, true);
                    
                }
                return null;
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

    }
}
