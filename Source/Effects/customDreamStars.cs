using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{
	class customDreamStars : Backdrop
	{
		public struct Stars
		{
			public Vector2 Position;

			public float Speed;

			public float Size;
		}

		private Stars[] stars;

		public Color StarColor;

		//public float Alpha;

		private Vector2 angle;

		private Vector2 lastCamera = Vector2.Zero;

		float ParralaxScroll;

		String ParticleShape;

		public customDreamStars(int count, float minSpeed, float maxSpeed, float minSize, float maxSize, String Color, float AngleX, float AngleY, float scroll, String Shape)
		{
			stars = new Stars[count];
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i].Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f));
				stars[i].Speed = minSpeed + Calc.Random.NextFloat(maxSpeed - minSpeed);
				stars[i].Size = minSize + Calc.Random.NextFloat(maxSize - minSize);
			}
			StarColor = Calc.HexToColor(Color);
			angle = Vector2.Normalize(new Vector2(AngleX, AngleY));//new Vector2(-2f, -7f)
			ParralaxScroll = scroll;
			ParticleShape = Shape;

		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (!Visible) {
				return;
			}//feat. ja
			Vector2 position = (scene as Level).Camera.Position;
			Vector2 value = position - lastCamera;
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i].Position += angle * stars[i].Speed * Engine.DeltaTime - value * ParralaxScroll;
			}
			lastCamera = position;
		}

		public override void Render(Scene scene)
		{
			for (int i = 0; i < stars.Length; i++)
			{

				//Vector2 RenderPos = new Vector2(mod(stars[i].Position.X, 320f), mod(stars[i].Position.Y, 180f));
				Vector2 RenderPos = new Vector2(mod(stars[i].Position.X, 320f + 2 * stars[i].Size) - stars[i].Size, mod(stars[i].Position.Y, 180f + 2 * stars[i].Size) - stars[i].Size);
				switch (ParticleShape)
				{

					case "Diamond":
						Draw.Circle(RenderPos, stars[i].Size, StarColor, 1);
						break;
					case "Circle":
						Draw.Circle(RenderPos, stars[i].Size, StarColor, /*8*/(int)Math.Log((stars[i].Size * 8) + 2 ) );
						break;
					case "FilledRect":
						Draw.Rect(RenderPos, stars[i].Size, stars[i].Size, StarColor);
						break;
					default:
						Draw.HollowRect(RenderPos, stars[i].Size, stars[i].Size, StarColor);
						break;
				}
			}
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}

	}
}
