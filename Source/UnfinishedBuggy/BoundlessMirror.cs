﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace FlaglinesAndSuch
{
	[CustomEntity("FlaglinesAndSuch/BoundlessMirror")]
	class BoundlessMirror : Entity
    {
		private class Frame : Entity
		{
			private BoundlessMirror mirror;

			public Frame(BoundlessMirror mirror)
			{
				this.mirror = mirror;
				base.Depth = 8995;
			}

			public override void Render()
			{
				Position = mirror.Position;
				MTexture[,] frame = mirror.frame;
				Vector2 size = mirror.size;
				frame[0, 0].Draw(Position + new Vector2(0f, 0f));
				frame[2, 0].Draw(Position + new Vector2(size.X - 8f, 0f));
				frame[0, 2].Draw(Position + new Vector2(0f, size.Y - 8f));
				frame[2, 2].Draw(Position + new Vector2(size.X - 8f, size.Y - 8f));
				for (int i = 1; (float)i < size.X / 8f - 1f; i++)
				{
					frame[1, 0].Draw(Position + new Vector2(i * 8, 0f));
					frame[1, 2].Draw(Position + new Vector2(i * 8, size.Y - 8f));
				}
				for (int j = 1; (float)j < size.Y / 8f - 1f; j++)
				{
					frame[0, 1].Draw(Position + new Vector2(0f, j * 8));
					frame[2, 1].Draw(Position + new Vector2(size.X - 8f, j * 8));
				}
			}
		}

		private readonly Color color = Calc.HexToColor("05070e");

		private readonly Vector2 size;

		private MTexture[,] frame = new MTexture[3, 3];

		private MirrorSurface surface;

		public BoundlessMirror(EntityData e, Vector2 offset)
			: base(e.Position + offset)
		{
			size = new Vector2(e.Width, e.Height);
			base.Depth = 9500;
			base.Collider = new Hitbox(e.Width, e.Height);
			Add(surface = new MirrorSurface());
			surface.ReflectionOffset = new Vector2(e.Float("reflectX"), e.Float("reflectY"));
			surface.OnRender = delegate
			{
				Draw.Rect(base.X + 2f, base.Y + 2f, size.X - 4f, size.Y - 4f, surface.ReflectionColor);
			};
			MTexture mTexture = GFX.Game["scenery/templemirror"];
			for (int i = 0; i < mTexture.Width / 8; i++)
			{
				for (int j = 0; j < mTexture.Height / 8; j++)
				{
					frame[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
				}
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(new Frame(this));
		}

		public override void Render()
		{
			Draw.Rect(base.X + 3f, base.Y + 3f, size.X - 6f, size.Y - 6f, color);
		}
	}
}
