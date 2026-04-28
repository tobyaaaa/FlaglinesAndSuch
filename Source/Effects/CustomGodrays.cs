using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{
	public class CustomGodrays : Backdrop
	{
		private struct Ray
		{
			public float X;

			public float Y;

			public float Percent;

			public float Duration;

			public float Width;

			public float Length;

			public void Reset(int minwidth, int maxwidth, int minlength, int maxlength, float durationbase, float durationadd, bool extbound)
			{
				Percent = 0f;
                Duration = Math.Abs(durationbase + Calc.Random.NextFloat() * durationadd);
				Width = Calc.Random.Next(minwidth, maxwidth);
				Length = Calc.Random.Next(minlength, maxlength);


                X = Calc.Random.NextFloat(384f + (extbound ? Width * 2 : 0));
                Y = Calc.Random.NextFloat(244f + (extbound ? Length * 2 : 0));
            }
		}
		public int minWidth;
		public int maxWidth;
		public int minLength;
		public int maxLength;
		public float durationBase;
		public float durationAdd;
		//public float rayColorAlpha;
		public Color rayColor; // = Calc.HexToColor("f52b63") * 0.5f;
		public Color rayColorFade;
		public float scrollX;
		public float scrollY;
		public float speedX;
		public float speedY;
		public float angleX = -1.67079639f;
		public float angleY = 1f;

		public int RayCount = 6;

		bool extendedBounds;

		private VertexPositionColor[] vertices;

		private int vertexCount;



		private Ray[] rays;

		private float fade;

		public CustomGodrays(int minwidth, int maxwidth, int minlength, int maxlength, float durationbase, float durationadd, float raycoloralpha, String raycolor, String raycolorfade, float scrollx, float scrolly, float speedx, float speedy, int raycount, float anglex, float angley, bool extendbounds)
		{
			maxWidth = maxwidth;
			minWidth = Math.Min(minwidth, maxwidth);
			maxLength = maxlength;
			minLength = Math.Min(minlength, maxlength);

			durationBase = durationbase;
			durationAdd = durationadd;
			rayColor = Calc.HexToColor(raycolor) * raycoloralpha;
			rayColorFade = (raycolorfade=="") ?  rayColor : Calc.HexToColor(raycolorfade) * raycoloralpha;
			scrollX = scrollx;
			scrollY = scrolly;
			speedX = speedx;
			speedY = speedy;
			RayCount = raycount;
			angleX = anglex;
			angleY = angley;

			rays = new Ray[RayCount];
			vertices = new VertexPositionColor[6*RayCount];

            extendedBounds = extendbounds;
            UseSpritebatch = false;
			for (int i = 0; i < rays.Length; i++)
			{
                rays[i].Reset(minWidth, maxWidth, minLength, maxLength, durationBase, durationAdd, extendedBounds);
				rays[i].Percent = Calc.Random.NextFloat();
			}
        }

		public override void Update(Scene scene)
		{
			Level level = scene as Level;
			bool flag = IsVisible(level);
			fade = Calc.Approach(fade, flag ? 1 : 0, Engine.DeltaTime);
			Visible = (fade > 0f);
			if (!Visible)
			{
				return;
			}
			Player player = level.Tracker.GetEntity<Player>();
			Vector2 value = Calc.AngleToVector(angleX, angleY);
			Vector2 value2 = new Vector2(0f - value.Y, value.X);
			int num = 0;
			for (int i = 0; i < rays.Length; i++)
            {
                float width = rays[i].Width;
                float length = rays[i].Length;

                float Xmodrange = 384f + (extendedBounds ? width * 2 : 0);
                float Ymodrange = 244f + (extendedBounds ? length * 2 : 0);
                float Xadd = extendedBounds ? -width : -32;
                float Yadd = extendedBounds ? -length : -32;


                if (rays[i].Percent >= 1f)
				{
					rays[i].Reset(minWidth, maxWidth, minLength, maxLength, durationBase, durationAdd, extendedBounds);
				}
				rays[i].Percent += Engine.DeltaTime / rays[i].Duration;
				rays[i].Y += speedY * Engine.DeltaTime;
				rays[i].X += speedX * Engine.DeltaTime;
				float percent = rays[i].Percent;


                float num2 = Xadd + Mod(rays[i].X - level.Camera.X * scrollX, Xmodrange);
				float num3 = Yadd + Mod(rays[i].Y - level.Camera.Y * scrollY, Ymodrange);

				Vector2 value3 = Calc.Floor(new Vector2(num2, num3));
				Color color = Color.Lerp(rayColor, rayColorFade, percent) * Ease.CubeInOut(Calc.Clamp(((percent < 0.5f) ? percent : (1f - percent)) * 2f, 0f, 1f)) * fade;
				if (player != null)
				{
					float num4 = (value3 + level.Camera.Position - player.Position).Length();
					if (num4 < 64f)
					{
						color *= 0.25f + 0.75f * (num4 / 64f);
					}
				}
				VertexPositionColor vertexPositionColor = new VertexPositionColor(new Vector3(value3 + value2 * width + value * length, 0f), color);
				VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(value3 - value2 * width, 0f), color);
				VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(value3 + value2 * width, 0f), color);
				VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(value3 - value2 * width - value * length, 0f), color);
				vertices[num++] = vertexPositionColor;
				vertices[num++] = vertexPositionColor2;
				vertices[num++] = vertexPositionColor3;
				vertices[num++] = vertexPositionColor2;
				vertices[num++] = vertexPositionColor3;
				vertices[num++] = vertexPositionColor4;
			}
			vertexCount = num;
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}

		public override void Render(Scene scene)
		{
			if (vertexCount > 0 && fade > 0f)
			{
				GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
			}
		}
	}

}
