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
	[CustomEntity("FlaglinesAndSuch/WickerBlock")]
    [Tracked]
    class WickerBlock : Solid
	{

		private MTexture[,] nineSlice;
		//public WickerSolid Solid;
		
		public bool NoGravity;//man I haven't even TRIED putting this in yet
		public bool Reinforced;//causes way too many issues for what it's worth
		//todo for no gravity not reinforced:
		//actors getting stuck in the blocks when pushed in
		//madeline being teleported to the side of the block when pushed in or walking on a chain of blocks
		//going offscreen


		public WickerBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
		{
			NoGravity = true;
			Reinforced = data.Bool("reinforced");

			base.Depth = -9000;
			base.Collider = new Hitbox(data.Width, data.Height, 0, 0);
			Add(new LightOcclude());

			MTexture mTexture = GFX.Game["objects/FlaglinesAndSuch/WickerBlock/block"];
			nineSlice = new MTexture[3, 3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
				}
			}

		}

		public override void Render()
		{

			float num = base.Collider.Width / 8f - 1f;
			float num2 = base.Collider.Height / 8f - 1f;
			for (int i = 0; (float)i <= num; i++)
			{
				for (int j = 0; (float)j <= num2; j++)
				{
					int num3 = ((float)i < num) ? Math.Min(i, 1) : 2;
					int num4 = ((float)j < num2) ? Math.Min(j, 1) : 2;
					nineSlice[num3, num4].Draw(Position /*+ base.Shake*/ + new Vector2(i * 8, j * 8));
				}
			}
			base.Render();
		}
		public override void Added(Scene scene)
		{
			base.Added(scene);
			//scene.Add(this.Solid = new WickerSolid(Position, (int)Width, (int)Height, this));
		}



        /*protected override void OnSquish(CollisionData data)
		{
			if (!Reinforced && !TrySquishWiggle(data))
			{
				Audio.Play("event:/game/general/wall_break_wood", Position);
				for (int i = 0; (float)i < base.Width / 8f; i++)
				{
					for (int j = 0; (float)j < base.Height / 8f; j++)
					{
						base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '9', true)/*.BlastFrom(from)* /);
					}
				}

				Solid.RemoveSelf();
				RemoveSelf();
			}
		}

		public override bool IsRiding(JumpThru jumpThru)
		{
			if (IgnoreJumpThrus || NoGravity)
			{
				return false;
			}
			return CollideCheckOutside(jumpThru, Position + Vector2.UnitY);
		}

		public override bool IsRiding(Solid solid)
		{
			if (NoGravity) {
				return false;
			}
			return CollideCheck(solid, Position + Vector2.UnitY);
		}*/





        /*
		public class WickerSolid : Solid
		{
			public WickerBlock Parent;

			public bool sandwichX;//set if the block is touching a solid on both horizontal sides: CollideCheck<Solid>(Position - Vector2.UnitX) && CollideCheck<Solid>(Position + Vector2.UnitX)
			public bool sandwichY;//CollideCheck<Solid>(Position - Vector2.UnitY) && CollideCheck<Solid>(Position + Vector2.UnitY)

			public WickerSolid(Vector2 position, int width, int height, WickerBlock parent) : base(position, width, height, true)
			{
				Parent = parent;
				Collider.Position = new Vector2(/*-Width / 2f, -Height* /0,0);
				this.SurfaceSoundIndex = 5;
			}

			public override void Update()//PROBABLY NOT FINAL
			{
				base.Update();
				if (Parent.Reinforced)
				{
					sandwichX = CollideCheck<Solid>(Position - Vector2.UnitX) && CollideCheck<Solid>(Position + Vector2.UnitX);
					sandwichY = CollideCheck<Solid>(Position - Vector2.UnitY) && CollideCheck<Solid>(Position + Vector2.UnitY);
				}
			}

			public override void MoveHExact(int move)
			{
				Parent.AllowPushing = false;
				base.MoveHExact(move);
				Parent.AllowPushing = true;
			}

			public override void MoveVExact(int move)
			{
				Parent.AllowPushing = false;
				base.MoveVExact(move);
				Parent.AllowPushing = true;
			}

		}*/




        //Wicker block hooks

        public static void Load()
        {
            //On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
            //On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
            //On.Celeste.Actor.OnGround_int += Actor_OnGround_int;
            //On.Celeste.Platform.MoveHExactCollideSolids += Platform_MoveHExactCollideSolids;
            //On.Celeste.Platform.MoveVExactCollideSolids += Platform_MoveVExactCollideSolids;
            //On.Celeste.Solid.HasPlayerRider += Solid_HasPlayerRider; BROKEN!!!!!!!!!!!!!!!!!!!!!!

        }

        public static void UnLoad()
        {

            //On.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
            //On.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
            //On.Celeste.Actor.OnGround_int -= Actor_OnGround_int;
            //On.Celeste.Platform.MoveHExactCollideSolids -= Platform_MoveHExactCollideSolids;
            //On.Celeste.Platform.MoveVExactCollideSolids -= Platform_MoveVExactCollideSolids;
            //On.Celeste.Solid.HasPlayerRider -= Solid_HasPlayerRider

        }

        //this one broke stuff so it's commented out
        /*private static bool Solid_HasPlayerRider(On.Celeste.Solid.orig_HasPlayerRider orig, Solid self)
        {
            var rider = self.GetPlayerRider();
            if (rider == null)
            {
                return false;
            }
            var solidList = Engine.Scene.Tracker.GetEntities<Solid>();
            Vector2 pointL, pointR;
            pointL = new Vector2(rider.Left, rider.Bottom + 1);
            pointR = new Vector2(rider.Right, rider.Bottom + 1);
            var candidateList = new List<Entity>();
            foreach (Entity item in solidList)
            {
                if (Collide.CheckPoint(item, pointL) || Collide.CheckPoint(item, pointR))
                {
                    candidateList.Add(item);
                }
            }
            if (candidateList.Count == 1 && candidateList[0] == self)
            {
                return true;
            }
            return Collide.CheckPoint(self, pointL + Vector2.UnitX * (rider.Width / 2));
        }*/

        //these were causing weird errors I didn't want to fix at the moment... rip
        /*private bool Actor_OnGround_int(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int downCheck)
        {
            if (self is WickerBlock wicker)
            {
                var wasCollidable = wicker.Solid.Collidable;
                wicker.Solid.Collidable = false;
                var result = orig(self, downCheck);
                wicker.Solid.Collidable = wasCollidable;
                return result;
            }
            return orig(self, downCheck);
        }

        private bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher)
        {
            if (self is WickerBlock wicker)
            {
                var wasCollidable = wicker.Solid.Collidable;
                wicker.Solid.Collidable = false;

                var lastY = self.Position.Y;
                var result = orig(self, moveV, onCollide, pusher);
                var deltaY = self.Position.Y - lastY;


                wicker.Solid.Collidable = true;
                wicker.Solid.MoveV(deltaY);
                wicker.Solid.Collidable = wasCollidable;

                return result;
            }
            return orig(self, moveV, onCollide, pusher);
        }
        private static bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
        {
            if (!(self is WickerBlock)) { 
                
            }
            /*
            if (self is WickerBlock wicker)
            {
                var wasCollidable = wicker.Solid.Collidable;
                wicker.Solid.Collidable = false;

                var lastX = self.Position.X;
                var result = orig(self, moveH, onCollide, pusher);
                var deltaX = self.Position.X - lastX;

                wicker.Solid.Collidable = true;
                wicker.Solid.MoveH(deltaX);
                wicker.Solid.Collidable = wasCollidable;


                return result;
            }
            return orig(self, moveH, onCollide, pusher);
        }
        */

        private static bool Platform_MoveVExactCollideSolids(On.Celeste.Platform.orig_MoveVExactCollideSolids orig, Celeste.Platform self, int moveV, bool thruDashBlocks, Action<Vector2, Vector2, Celeste.Platform> onCollide)
        {

            int moveVReal = moveV;//moveV gets altered by the following method

            //copy the existing code to move the block, but instead we only care about moving wicker blocks if they come along
            //we DON'T want to actually move the block here; this is just a preliminary step
            float y = self.Y;
            int num = Math.Sign(moveV);
            int num2 = 0;
            Platform platform = null;

            while (moveV != 0)
            {
                foreach (WickerBlock entity in self.Scene.Tracker.GetEntities<WickerBlock>())//do we collide with a wicker block
                {
                    if (self.CollideCheck(entity, self.Position + Vector2.UnitY * num))
                    {
                        //move the found block so that it looks like the current block pushed it
                        entity.MoveVExactCollideSolids(moveV, false, null); //i think
                    }
                }
                platform = self.CollideFirst<Solid>(self.Position + Vector2.UnitY * num);//the following stuff is just to make sure if the block doesn't move TOO far
                if (platform != null && !(platform is WickerBlock))
                {
                    break;
                }
                if (moveV > 0)
                {
                    platform = self.CollideFirstOutside<JumpThru>(self.Position + Vector2.UnitY * num);
                    if (platform != null && !(platform is WickerBlock))
                    {
                        break;
                    }
                }
                num2 += num;
                moveV -= num;
                self.Y += num; //I anticipate several knock-on effects here

            }
            self.Y = y;
            //Console.WriteLine("pancakes for dinner: ending moveV hook");
            return orig(self, moveVReal, thruDashBlocks, onCollide);
        }

        /*
        private bool Platform_MoveHExactCollideSolids(On.Celeste.Platform.orig_MoveHExactCollideSolids orig, Celeste.Platform self, int moveH, bool thruDashBlocks, Action<Vector2, Vector2, Celeste.Platform> onCollide)
        {

            var wasCollidable = new Dictionary<WickerBlock.WickerSolid, bool>();
            foreach (WickerBlock.WickerSolid wicker in self.Scene.Tracker.GetEntities<WickerBlock.WickerSolid>())
            {
                if (wicker != self && !wicker.sandwichX)
                {
                    wasCollidable.Add(wicker, wicker.Collidable);
                    wicker.Collidable = false;
                }
            }

            bool temp = orig(self, moveH, thruDashBlocks, onCollide);

            foreach (WickerBlock.WickerSolid wicker in self.Scene.Tracker.GetEntities<WickerBlock.WickerSolid>())
            {
                if (wicker != self && !wicker.sandwichX)
                {
                    wicker.Collidable = wasCollidable[wicker];
                }
            }

            return temp;
        }*/



    }
}







/*
 *

 
 
 
 private static void Platform_MoveVExactCollideSolids(On.Celeste.Solid.orig_MoveVExact orig, Celeste.Solid self, int moveV)
        {
            //NOTE TO SELF: THIS STILL PRRRROBABLY ISNT A FANTASTIC IDEA






            int moveVReal = moveV;//moveV gets altered by the following method

            //copy the existing code to move the block, but instead we only care about moving wicker blocks if they come along
            //we DON'T want to actually move the block here; this is just a preliminary step
            float y = self.Y;
            int num = Math.Sign(moveV);
            int num2 = 0;
            Platform platform = null;

            //Console.WriteLine("pancakes for dinner: beginning moveV hook");
            while (moveV != 0)
            {
                foreach (WickerBlock entity in self.Scene.Tracker.GetEntities<WickerBlock>())//do we collide with a wicker block
                {
                    if (self.CollideCheck(entity, self.Position + Vector2.UnitY * num))
                    {
                        //move the found block so that it looks like the current block pushed it
                        entity.MoveVExactCollideSolids(moveV, false, null); //i think
                        //Console.WriteLine("pancakes for dinner: found a wicker block to move, moving this much: "+ moveV);
                    }
                }
                platform = self.CollideFirst<Solid>(self.Position + Vector2.UnitY * num);//the following stuff is just to make sure if the block doesn't move TOO far
                if (platform != null && !(platform is WickerBlock))
                {
                    //Console.WriteLine("pancakes for breakfast: smacked a platform, abort at: " + moveV);
                    break;
                }
                if (moveV > 0)
                {
                    platform = self.CollideFirstOutside<JumpThru>(self.Position + Vector2.UnitY * num);
                    if (platform != null && !(platform is WickerBlock))
                    {
                        //Console.WriteLine("pancakes for lunch: smacked a platform, abort at: " + moveV);
                        break;
                    }
                }
                num2 += num;
                moveV -= num;
                self.Y += num; //I anticipate several knock-on effects here

            }
            self.Y = y;
 */