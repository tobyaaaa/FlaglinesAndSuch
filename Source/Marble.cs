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
using static Celeste.TrackSpinner;

namespace Celeste.Mod.FlaglinesAndSuch
{

    [CustomEntity("FlaglinesAndSuch/Marble")]
    [Tracked(false)]
    class Marble : Actor
    {
        //as an Actor, I get:
        //move commands
        //try squish wiggle
        //isRiding & onGround
        //liftspeed

        //notes:
        //when a ball AT REST descends ∆y, it should end up with v = sqrt(2g ∆y)

        //TO DO:
        //3.falling off edges
        //2.backwards liftboost when falling off blocks
        //1.friction
        //2.ball-to-ball collisions per pixel
        //2. ^ + check for balls within total movement range each frame, to reduce amt of collision checks.
        //1.tracks
        //2.track collision
        //1.track slopes
        //?.marbles can be pushed into each other; I guess getting pushed by a block is different from moving, so...
        //...naturally the collision checks wouldn't happen. "stacked" marbles just have no gravity.
        //?. entity interactions, eg springs zippers touch switches ect
        //1. OOB cases at end of update
        //ball does not correctly respond to hitting a ceiling, when moving up
        //ball gains momentum when bouncing off a track
        //when on a track, and hit by another ball, ball gains momentum towards the track that must be resolved


        /// <summary>
        ///how close to a ledge the marble will start falling off
        /// </summary>
        int LEDGE_LENIENCY = 4;
        /// <summary>
        /// The threshold for whether to bounce or not when landing
        /// </summary>
        float MIN_BOUNCE_SPEED = 4;
        /// <summary>
        /// when the ball bounces its speed is multiplied by this. Should be in range [0,1].
        /// </summary>
        float BOUNCE_SPEED_LOSS = 0.6f;
        /// <summary>
        /// To avoid clipping through things, the ball's speed won't be higher than this
        /// </summary>
        //float TOPSPEED_CLAMP;
        /// <summary>
        /// How much the ball accelerates in midair.
        /// </summary>
        float GRAVITY = 5f;
        /// <summary>
        /// The threshold for whether to bounce or not when landing on a track.
        /// </summary>
        float TRACK_MIN_BOUNCE_SPEED = 4;
        /// <summary>
        /// when the ball bounces on a track its speed is multiplied by this. Should be in range [0,1].
        /// </summary>
        float TRACK_BOUNCE_SPEED_LOSS = 0.6f;
        /// <summary>
        /// A constant (2*gravity) used for the potential energy formula when on a track. The *2 is baked in.
        /// </summary>
        float TRACK_ENERGY_2g = 10f;

        Vector2 Speed;
        Vector2 prevLiftSpeed;
        //Vector2 lastPosition;

        private Collision onCollideH;
        private Collision onCollideV;

        private bool grounded;
        private bool grounded_last_frame;
        //private Sprite sprite;
        //collider ships w/ Entity
        private MarbleTrack track = null;
        private int trackSegment = -1;

        Level level;

        public Marble(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Circle(8, 0, 0);//new Hitbox(16,16,-8,-8);//
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            grounded = OnGround();
            if (grounded || CollideCheck<Marble>(Position + new Vector2(0, 1)))//I check for a marble here too so marbles don't like ooze onto each other
            {
                //check if the marble is on "stable ground", that is 1px to the left & right are both solid
                //note that this is really nieve and doesn't follow conservation of energy at all
                float edgePush = (!OnGround(Position + Vector2.UnitX * LEDGE_LENIENCY)) ? 2f :
                                (!OnGround(Position - Vector2.UnitX * LEDGE_LENIENCY) ? (-2f) : 0f);
                Speed.X += edgePush;

                //bounce or rest
                //grounded-last-frame is checked for here because I don't want the ball to bounce when inheriting downwards liftboost...
                //...despite not leaving the block. I might revert this who knows
                if (Speed.Y > MIN_BOUNCE_SPEED && !grounded_last_frame)
                {
                    Speed.Y = -Speed.Y * BOUNCE_SPEED_LOSS;
                }
                else {
                    Speed.Y = 0;
                }

                //liftspeed notes:
                //1. not applying liftspeed until onground results in stupid behavior if the ball rolls off a moving platform
                //it's possible this only matters when the edge-push is what pushes the ball off?
                //2. I haven't tested this but I bet liftboost overrides current speed instead of adding which won't look great
                //3. If possible, test for the scenario of the platform the marble was last on moving the marble into a wall, pushing the
                //marble off. If this happens, apply the liftspeed backwards.

                if (LiftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                {
                    Speed += prevLiftSpeed;
                    prevLiftSpeed = Vector2.Zero;
                }
                else {
                    prevLiftSpeed = LiftSpeed;
                }


            }
            else if (track != null) {
                //Speed = Vector2.Zero;

                //calculate new speed based on V. diff.
                //NOTICE!!! this math NEEDS to happen between the movement (which would be last frame) and ANY change to the speed this frame
                //...or store a LastSpeed variable or something?? might mess with stuff still though

                //okay so mgh + 0.5mv(old)^2 = 0.5mv(new)^2
                //m drops out, naturally
                // sqrt( 2gh + v(old)^2 ) = v(new)

                // 2gh/v(old)^2  = v(new)^2 / v(old)^2
                //note the abs. around speed.y: it's the h of mgh, where the h doesn't make sense to be negative.
                //moving up or down it works out so the only difference in the formula is whether to add or subtract one
                
                
                if (Speed.Y != 0) {
                    if ((TRACK_ENERGY_2g * Math.Abs(Speed.Y) / Speed.LengthSquared()) + (Speed.Y > 0 ? 1 : -1) < 0)
                    {
                        Console.WriteLine("flaglines and such: ball imploded");
                        Console.WriteLine(Speed);
                        RemoveSelf();
                    }


                    Console.WriteLine("flaglines and such: ball ontrack log");
                    float coe1 = TRACK_ENERGY_2g * Math.Abs(Speed.Y);
                    Console.WriteLine("coe1 = " + coe1);
                    float coe2 = coe1 / Speed.LengthSquared();
                    Console.WriteLine("coe2 = " + coe2);
                    float coe3 = coe2 + (Speed.Y > 0 ? 1 : -1);
                    Console.WriteLine("coe3 = " + coe3);
                    Speed *= coe3;
                    Console.WriteLine("flaglines and such: ball ontrack log finished");

                    //Speed *= (float) Math.Sqrt( 
                    //    (TRACK_ENERGY_2g * Math.Abs(Speed.Y) 
                    //    / Speed.LengthSquared()  )
                    //    + (Speed.Y > 0 ? 1 : -1));
                }

                trackSegment = track.aboveWhichSegment(Position);
                if (trackSegment == -1 || !track.isColliding(this, trackSegment)) {
                    track = null;
                    trackSegment = -1;
                }
                //if we get this far we assume the ball hasn't bounced or anything.
                //get raw speed (as a float) from velocity vector (which we assume has the correct direction)
                //move that far (use vel. vector for this, that's fine)
                //change speed based on current/prev heights
                //set velocity vector's size to match (maybe normalize then multiply by new speed)
                //check for segment #; compare slope & bounce if necessary; if no slope found, remove refs.
            }
            else
            {
                //nieve gravity
                Speed.Y += GRAVITY; //Calc.Approach(Speed.Y, 200f, 300 * Engine.DeltaTime);
            }


            //previousPosition = base.ExactPosition;
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            //note: would need to rewrite Collide tracking to get this to happen every movement
            float xdirunit = Speed.X;
            if (xdirunit != 0) {xdirunit /= Math.Abs(Speed.X); }//dirunit is the direction of travel, x only


            Marble marble = CollideFirst<Marble>(Position + new Vector2(xdirunit, 0));
            if (marble != null)
            {
                float dx = Position.X - marble.Position.X;
                if (dx * xdirunit < 0)
                {
                    float temp = Speed.X;
                    Speed.X = marble.Speed.X;
                    marble.Speed.X = temp;
                }
            }
            else 
            {
                marble = CollideFirst<Marble>(Position + new Vector2(0, 1));
                if (marble != null) { 
                    //what
                    //figure out the horizontal distance between this and other
                    float dx = Position.X - marble.Position.X;
                    //...whatever. I think correctly Y speed should be set to 0 and transferred to the X speed of both
                    //more precisely: the component of motion perpendicular the tanget line to the point of impact is transferred
                    //to the other ball (I think) and the remainder stays on the first?
                    Speed.X += dx;
                    Speed.Y = 0;
                    marble.Speed.X -= dx;
                }
            }


            //handle out-of-bounds cases here...

            if (base.Top > level.Bounds.Bottom)
            {
                RemoveSelf();
            }

            grounded_last_frame = grounded;
            }


        public void OnTrack(MarbleTrack track, int segment) {
            if (track == this.track) return;

            //check component of V perp. to segment slope
            //if high: bounce, ignore track
            //if moving into the track from "behind", ignore  track
            //note: ??? would balls behave differently on undersides of tracks?
            //if low: snap to track position, lose component of speed not tanget to the slope of the line contacted

            Vector2 trackNormal = track.getNormal(segment);

            //this line makes the track "one-way"
            if ((trackNormal + Speed.SafeNormalize()).LengthSquared() >= 2 ) return;
            //if (Math.Atan2(Speed.Y, Speed.X) - Math.Atan2(trackNormal.Y, trackNormal.X) >= (?))  {return;}


            Vector2 parralelDir = new Vector2(trackNormal.Y, -trackNormal.X).SafeNormalize();
            float perpSpeed = (Speed.X * trackNormal.X) + (Speed.Y * trackNormal.Y);
            if (/*Speed.Length()*/ Math.Abs(perpSpeed) > TRACK_MIN_BOUNCE_SPEED)
            {
                float parralelSpeed = (Speed.X * parralelDir.X) + (Speed.Y * parralelDir.Y);
                Speed = -Speed * TRACK_BOUNCE_SPEED_LOSS;
                Speed += parralelDir * parralelSpeed * 2;
                return;
            }

            this.track = track;
            trackSegment = segment;
            Speed -= trackNormal * perpSpeed;



            //while on a track:
            //1. must have some way to know if the marble has left the track
            //2. marble should attempt to move along the track; gaining or losing speed based on height diff.
            //3. must have some way to tell which track segment the marble is on as well as switching from one to the next
            //4. collision w/ multiple tracks at once?


            /*
             * 1. marble stores a ref. to a track object when entering one. When more tracks are registered, idk do some math???
             * 2. if a ref. exists, also store an index to the track piece. Update that index via asking the track to check after each mvment.
             * Movement can then be extrapolated pretty easily based on the track section's slope
             * 3. see above
             * 4. ???
             * okay so I see 4 possibilities.
             * A: new track overrides old track. Treated like changing track segments.
             * B: bounce off new track, like ground. The segment slope is too perp. to the motion of the ball and ball moving fast.
             * C: same as above, but slower. Ball just stops. May need to keep references to multiple slopes in this case, since...
             * ...moving objects could push the ball onto either track proper.
             * D: Parallel tracks, or a ^ shape made of two tracks, where the ball doesn't get affected at all.
             * 
             */
        }

        public override void Render()
        {
            base.Render();
            Color c = new Color(grounded ? 0 : 255, (Speed.Length() < 0.1f) ? 0 : 255, (LiftSpeed.Length() < 0.1f) ? 0 : 255);
            Color c2 = new Color((trackSegment == -1) ? 0 : 255, 0, 255);
            Draw.Circle(Position, 8f, c2, 8);
            Draw.Circle(Position, 5f, c, 8);
        }

        private void OnCollideH(CollisionData data)
        {
            Speed.X = -Speed.X;
        }
        private void OnCollideV(CollisionData data)
        {
        }
    }
}