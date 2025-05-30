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
	[CustomEntity("FlaglinesAndSuch/CustomSlider")]
	class CustomSlider : Entity
    {
		public enum Surfaces
		{
			Floor,
			Ceiling,
			LeftWall,
			RightWall
		}

		float MaxSpeed = 80f;

		private const float Accel = 400f;

		private Vector2 dir;

		private Vector2 surface;

		private bool foundSurfaceAfterCorner;

		private bool gotOutOfWall;

		private float speed;

		private bool moving;

		private float hitboxSize = 10;
		private float renderSize = 12;

		public CustomSlider(Vector2 position, bool clockwise, Surfaces surface, float moveSpeed)
			: base(position)
		{
			base.Collider = new Circle(hitboxSize);
			Add(new StaticMover());
			switch (surface)
			{
				default:
					dir = Vector2.UnitX;
					this.surface = Vector2.UnitY;
					break;
				case Surfaces.Ceiling:
					dir = -Vector2.UnitX;
					this.surface = -Vector2.UnitY;
					break;
				case Surfaces.LeftWall:
					dir = -Vector2.UnitY;
					this.surface = -Vector2.UnitX;
					break;
				case Surfaces.RightWall:
					dir = Vector2.UnitY;
					this.surface = Vector2.UnitX;
					break;
			}
			if (!clockwise)
			{
				dir *= -1f;
			}
			moving = true;
			foundSurfaceAfterCorner = (gotOutOfWall = true);
			
			MaxSpeed = moveSpeed;
			speed = moveSpeed;

			Add(new PlayerCollider(OnPlayer));

			
		}

		public CustomSlider(EntityData e, Vector2 offset)
			: this(e.Position + offset, e.Bool("clockwise", defaultValue: true), e.Enum("surface", Surfaces.Floor), e.Float("speed"))
		{
		}
		
		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			int num = 0;
			do
			{
				if (!base.Scene.CollideCheck<Solid>(Position))
				{
					Position += surface;
					num++;
					continue;
				}
				return;
			}
			while (num < 100);
			Console.WriteLine("FlaglinesAndSuch log: Slider hit the ground too hard");
			//throw new Exception("Couldn't find surface");
			RemoveSelf();
		}

		private void OnPlayer(Player Player)
		{
			Player.Die((Player.Center - base.Center).SafeNormalize(-Vector2.UnitY));
			moving = false;
		}

		public override void Update()
		{
			base.Update();
			if (!moving)
			{
				return;
			}
			speed = Calc.Approach(speed, MaxSpeed, 400f * Engine.DeltaTime);
			Position += dir * speed * Engine.DeltaTime;
			if (!OnSurfaceCheck())
			{
				if (!foundSurfaceAfterCorner)
				{
					return;
				}
				Position = Position.Round();
				int num = 0;
				while (!OnSurfaceCheck())
				{
					Position -= dir;
					num++;
					if (num >= 100)
					{
						Console.WriteLine("FlaglinesAndSuch log: Slider blew up");
						//throw new Exception("Couldn't get back onto corner!");
						RemoveSelf();
					}
				}
				foundSurfaceAfterCorner = false;
				Vector2 value = dir;
				dir = surface;
				surface = -value;
				return;
			}
			foundSurfaceAfterCorner = true;
			if (InWallCheck())
			{
				if (!gotOutOfWall)
				{
					return;
				}
				Position = Position.Round();
				int num2 = 0;
				while (InWallCheck())
				{
					Position -= dir;
					num2++;
					if (num2 >= 100)
					{
						Console.WriteLine("FlaglinesAndSuch log: Slider suffocated in a wall");
						//throw new Exception("Couldn't get out of wall!");
						RemoveSelf();
					}
				}
				Position += dir - surface;
				gotOutOfWall = false;
				Vector2 value2 = surface;
				surface = dir;
				dir = -value2;
			}
			else
			{
				gotOutOfWall = true;
			}
		}

		private bool OnSurfaceCheck()
		{
			return base.Scene.CollideCheck<Solid>(Position.Round() + surface);
		}

		private bool InWallCheck()
		{
			return base.Scene.CollideCheck<Solid>(Position.Round() - surface);
		}

		public override void Render()
		{
			Draw.Circle(Position, renderSize, Color.Red, 8);
		}
	}
}
