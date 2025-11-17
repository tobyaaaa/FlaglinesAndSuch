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
    [CustomEntity("FlaglinesAndSuch/CustomSinkingBlock")]
    class CustomSinkingBlock : Solid
    {

		private float speed;

		private Vector2 startPos;

		private float riseTimer;

		private MTexture pinTexture;
		private MTexture[,] nineSlice;

		private Shaker shaker;

		private SoundSource downSfx;

		private SoundSource upSfx;

		public string OverrideTexture;

		public float downSpeed = 30f;
		public float downSpeedDuck = 60f;
		public float downSpeedIdle = 45f;
		public float upSpeed = -50f;
		public float IdleTimer = 0.1f;
		public float angle = 0;
		public float lookUpSpeed = 30f;
		public float hasHoldableSpeed = 0f;
		public bool noPlatformLine = false;

        public float startOffset = 0;

        Vector2 endNode;

		private Scene scenew;

		float[] Accelerations = { 400f, 600f, 400f, 400f, 400f, 400f };//sorted as such: Up, Idle, Down, Crouch, LookUp, Holdable

		public CustomSinkingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
		{
			base.Depth = 1;
			base.Collider = new Hitbox(data.Width, data.Height, 0, 0);
			SurfaceSoundIndex = 15;
			Add(shaker = new Shaker(on: false));
			//Add(new LightOcclude());
			Add(downSfx = new SoundSource());
			Add(upSfx = new SoundSource());

			noPlatformLine = data.Bool("no_bg_line");

			downSpeedDuck = data.Float("Crouching_speed");
			downSpeed = data.Float("Pressed_speed");
			downSpeedIdle = data.Float("Idle_speed");
			upSpeed = data.Float("Unpressed_speed");
			IdleTimer = data.Float("Idle_time");

			lookUpSpeed = data.Float("Look_up_speed");
			hasHoldableSpeed = data.Float("HoldableSpeed");
			//maxDist = data.Float("Max_pos"); data.Nodes[0].Y
			endNode = data.Nodes[0] + offset;
			startPos = new Vector2(base.X, base.Y);
			angle = (float)Math.Atan2(Math.Abs(endNode.Y - startPos.Y), Math.Abs(endNode.X - startPos.X));
			//Console.WriteLine("FlaglinesAndSuch log: platform: " + endNode.X + " " + endNode.Y + " " + startPos.X + " " + startPos.Y);

			OverrideTexture = data.Attr("texture");

			String Accelstrs = data.Attr("Accelerations");
			if (Accelstrs != "")
			{
				string[] Accelstrs2 = Accelstrs.Replace(" ", String.Empty).Split(',');
				for (int i = 0; i < Accelstrs2.Length; i++)
				{
					Accelerations[i] = float.Parse(Accelstrs2[i]);
				}
			}

            startOffset = Math.Max(data.Float("start_offset"), 0);

            MTexture mTexture = GFX.Game["objects/FlaglinesAndSuch/CustomSinkingBlock/" + OverrideTexture];
			nineSlice = new MTexture[4, 3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
				}
			}

		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			AreaData areaData = AreaData.Get(scene);
			string woodPlatform = areaData.WoodPlatform;
			if (OverrideTexture != null)
			{
				areaData.WoodPlatform = OverrideTexture;
			}
			orig_Added(scene);
			areaData.WoodPlatform = woodPlatform;
			scenew = scene;//if I don't do this, and instead just use Scene, the game crashes whenever I do base.Scene.Tracker
            
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

			//set position based on StartOffset
			Vector2 oldPosition = ExactPosition;
            Position = startPos + new Vector2(startOffset * (float)Math.Cos(angle), startOffset * (float)Math.Sin(angle));
            if (isOverBounds() == 2)
            {
                Position = endNode;
            }
			MoveStaticMovers(ExactPosition - oldPosition);
        }


        public override void Update()
		{
			base.Update();



			Player playerRider = null;
			if (scenew.Tracker.GetEntity<Player>() != null)
			{
				playerRider = scenew.Tracker.GetEntity<Player>();
				if (!playerRider.IsRiding(this))
				{
					playerRider = null;
				}

			}
						


			int platBoundsCheck = isOverBounds();//should be 2 if over top, -2 if under bottom; -1 or 1 if at bottom or top

			if (playerRider != null)
			{
				if (riseTimer <= 0f)
				{
					if (/*base.ExactPosition.Y <= startY*/
		platBoundsCheck >= 1)
					{
						Audio.Play("event:/game/03_resort/platform_vert_start", Position);
					}
					shaker.ShakeFor(0.15f, removeOnFinish: false);
				}
				riseTimer = IdleTimer;
				if ((float)Input.MoveY.Value == -1 && HasPlayerOnTop())
				{
					speed = Calc.Approach(speed, lookUpSpeed, Accelerations[4] * Engine.DeltaTime);//needs proper accels value! Also does this even work right? It really shouldn't override ducking
				}
				else
				{
					speed = Calc.Approach(speed, playerRider.Ducking ? downSpeedDuck : downSpeed, Accelerations[2] * Engine.DeltaTime);//duck check, for gameplay purposes //400f
				}
			}
			else if (this.HasRiderButLikeWhy())
			{
				speed = Calc.Approach(speed, hasHoldableSpeed, Accelerations[5] * Engine.DeltaTime);
			}

			else if (riseTimer > 0f)
			{
				riseTimer -= Engine.DeltaTime;
				speed = Calc.Approach(speed, downSpeedIdle, Accelerations[1] * Engine.DeltaTime);//move down idle ver //600f
			}
			else
			{
				speed = Calc.Approach(speed, upSpeed, Accelerations[0] * Engine.DeltaTime);//move up? //400f
			}

			if ((speed > 0 && /*base.ExactPosition.Y >= endNode.Y*/platBoundsCheck <= -1) || (speed < 0f && /*base.ExactPosition.Y <= startY */platBoundsCheck >= 1))
			{
				speed = 0;
			}

			if (speed > 0f && /*(base.ExactPosition.Y < endNode.Y)*/platBoundsCheck > -1)
			{
				if (!downSfx.Playing)
				{
					downSfx.Play("event:/game/03_resort/platform_vert_down_loop");
				}
				downSfx.Param("ducking", (playerRider != null && playerRider.Ducking) ? 1 : 0);
				if (upSfx.Playing)
				{
					upSfx.Stop();
				}

				MoveTowardsY(endNode.Y, (speed) * Engine.DeltaTime * (float)Math.Sin(angle));//notably uses Y only
				MoveTowardsX(endNode.X, (speed) * Engine.DeltaTime * (float)Math.Cos(angle));
				platBoundsCheck = isOverBounds(); //must recalculate this value as it still thinks the platform is back where it was before movement
				if (/*base.ExactPosition.Y >= endNode.Y*/platBoundsCheck <= -1)//notably uses Y only
				{
					downSfx.Stop();
					Audio.Play("event:/game/03_resort/platform_vert_end", Position);
					shaker.ShakeFor(0.1f, removeOnFinish: false);
				}

			}
			else if (speed < 0f && /*base.ExactPosition.Y > startY*/ platBoundsCheck < 1)//notably uses Y only
			{
				if (!upSfx.Playing)
				{
					upSfx.Play("event:/game/03_resort/platform_vert_up_loop");
				}
				if (downSfx.Playing)
				{
					downSfx.Stop();
				}
				MoveTowardsY(startPos.Y, (0f - speed) * Engine.DeltaTime * (float)Math.Sin(angle));//notably uses Y only
				MoveTowardsX(startPos.X, (0f - speed) * Engine.DeltaTime * (float)Math.Cos(angle));
				platBoundsCheck = isOverBounds(); //must recalculate this value as it still thinks the platform is back where it was before movement
				if (/*base.ExactPosition.Y <= startY */platBoundsCheck >= 1)//notably uses Y only
				{
					upSfx.Stop();
					Audio.Play("event:/game/03_resort/platform_vert_end", Position);
					shaker.ShakeFor(0.1f, removeOnFinish: false);
				}
			}
			else
			{
				if (downSfx.Playing)
				{
					downSfx.Stop();
				}
			}
		}

		public int isOverBounds()
		{//2 if over top bound; 1 if on bound; 0 if inbounds; negative if on/under bottom bound
			Vector2 StartToEnd = startPos - endNode;
			Vector2 overTop = base.ExactPosition - endNode;
			Vector2 OverBot = StartToEnd - overTop;
			if (overTop.Length() == StartToEnd.Length()) { return 1; }
			if (overTop.Length() > StartToEnd.Length()) { return 2; }
			if (OverBot.Length() == StartToEnd.Length()) { return -1; }
			if (OverBot.Length() > StartToEnd.Length()) { return -2; }
			return 0;
		}

		public void orig_Added(Scene scene)//this is all visuals
		{
            /*
			textures = new MTexture[mTexture.Width / 8];
			for (int i = 0; i < textures.Length; i++)
			{
				textures[i] = mTexture.GetSubtexture(i * 8, 0, 8, 8);
			}*/
            base.Added(scene);
            MTexture mPinTexture = GFX.Game["objects/FlaglinesAndSuch/CustomSinkingBlock/" + OverrideTexture];
            MTexture[] textures = new MTexture[mPinTexture.Width / 8];
            pinTexture = mPinTexture.GetSubtexture(3 * 8, 0, 8, 8);

            Vector2 value = new Vector2(base.Width, base.Height + 4f) / 2f;

			if (!noPlatformLine)
			{
				scene.Add(new MovingPlatformLine(new Vector2(startPos.X, startPos.Y) + value, endNode + value));
			}
		}
		public override void Render()//all visual of course
		{
			Vector2 value = shaker.Value;
            /*textures[0].Draw(Position + value);
			for (int i = 8; (float)i < base.Width - 8f; i += 8)
			{
				textures[1].Draw(Position + value + new Vector2(i, 0f));
			}
			textures[3].Draw(Position + value + new Vector2(base.Width - 8f, 0f));
			textures[2].Draw(Position + value + new Vector2(base.Width / 2f - 4f, 0f));
			*/
            float num = Width / 8f - 1f;
			float num2 = Height / 8f - 1f;
			for (int i = 0; (float)i <= num; i++)
			{
				for (int j = 0; (float)j <= num2; j++)
				{
					int num3 = ((float)i < num) ? Math.Min(i, 1) : 2;
					int num4 = ((float)j < num2) ? Math.Min(j, 1) : 2;
					nineSlice[num3, num4].Draw(Position + value + new Vector2(i* 8, j* 8), Vector2.Zero);
				}
            }
            pinTexture.Draw(Position + value + new Vector2(base.Width / 2f - 4f, 0f));

            base.Render();
		}

		public bool HasRiderButLikeWhy()//because of the scenew thing
		{
			foreach (Actor entity in scenew.Tracker.GetEntities<Actor>())
			{
				if (entity.IsRiding(this))
				{
					return true;
				}
			}
			return false;
		}

	}
}
