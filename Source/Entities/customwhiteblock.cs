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
	[CustomEntity("FlaglinesAndSuch/CustomWhiteBlock")]
	class CustomWhiteBlock : JumpThru
	{
		private MTexture[,] nineSlice;
		public bool DontRemoveOnHeart = false;
		public int width = 48;
		public int height = 24;
		public Color tint = new Color(Color.White, 255);

		public float duckDuration = 3f;

		private float playerDuckTimer;

		private bool enabled = true;

		private bool activated;

		//private Image sprite;

		private Entity bgSolidTiles;

		//public int sound;
		//public float holdTime;

		public CustomWhiteBlock(EntityData data, Vector2 offset)
			: base(data.Position + offset, data.Width == 0 ? 48 : data.Width, true)
		{
			//Add(sprite = new Image(GFX.Game["objects/whiteblock"]));
			MTexture mTexture = GFX.Game["objects/FlaglinesAndSuch/CustomWhiteBlock/block"];
			nineSlice = new MTexture[3, 3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
				}
			}

			base.Depth = -9000;
			SurfaceSoundIndex = /*27*/data.Int("sound_index");
			duckDuration = data.Float("hold_time");
			if (data.Width > 0) {
			width = data.Width;
			height = data.Height;
			}
			DontRemoveOnHeart = data.Bool("permasolid_bgtiles");
		}

		public override void Render()
		{

			float num = width / 8f - 1f;
			float num2 = height / 8f - 1f;
			for (int i = 0; (float)i <= num; i++)
			{
				for (int j = 0; (float)j <= num2; j++)
				{
					int num3 = ((float)i < num) ? Math.Min(i, 1) : 2;
					int num4 = ((float)j < num2) ? Math.Min(j, 1) : 2;
					nineSlice[num3, num4].Draw(Position /*+ base.Shake*/ + new Vector2(i * 8, j * 8), Vector2.Zero, tint);
				}
			}
			base.Render();
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if ((scene as Level).Session.HeartGem)
			{
				Disable();
			}
		}

		private void Disable()
		{
			enabled = false;
			tint *= 0.25f;
			Collidable = false;
		}

		private void Activate(Player player)
		{
			Audio.Play("event:/game/04_cliffside/whiteblock_fallthru", base.Center);
			activated = true;
			Collidable = false;
			if (!DontRemoveOnHeart)
			{
				player.Depth = 10001;
			}
			Level level = base.Scene as Level;
			Rectangle rectangle = new Rectangle(level.Bounds.Left / 8, level.Bounds.Y / 8, level.Bounds.Width / 8, level.Bounds.Height / 8);
			Rectangle tileBounds = level.Session.MapData.TileBounds;
			bool[,] array = new bool[rectangle.Width, rectangle.Height];
			for (int i = 0; i < rectangle.Width; i++)
			{
				for (int j = 0; j < rectangle.Height; j++)
				{
					array[i, j] = (level.BgData[i + rectangle.Left - tileBounds.Left, j + rectangle.Top - tileBounds.Top] != '0');
				}
			}
			bgSolidTiles = new Solid(new Vector2(level.Bounds.Left, level.Bounds.Top), 1f, 1f, safe: true);
			bgSolidTiles.Collider = new Grid(8f, 8f, array);
			base.Scene.Add(bgSolidTiles);
		}

		public override void Update()
		{
			base.Update();
			if (!enabled)
			{
				return;
			}
			if (!activated)
			{
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (HasPlayerRider() && entity != null && entity.Ducking)
				{
					playerDuckTimer += Engine.DeltaTime;
					if (playerDuckTimer >= duckDuration)
					{
						Activate(entity);
					}
				}
				else
				{
					playerDuckTimer = 0f;
				}
				if ((base.Scene as Level).Session.HeartGem)
				{
					Disable();
				}
			}
			else if (base.Scene.Tracker.GetEntity<HeartGem>() == null && !DontRemoveOnHeart)
			{
				Player entity2 = base.Scene.Tracker.GetEntity<Player>();
				if (entity2 != null)
				{
					Disable();
					entity2.Depth = 0;
					base.Scene.Remove(bgSolidTiles);
				}
			}
			else if (DontRemoveOnHeart) {
				Disable();
			}
		}
	}
}
