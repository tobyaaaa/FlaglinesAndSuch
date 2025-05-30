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

			public void Reset(int minwidth, int maxwidth, int minlength, int maxlength, float durationbase, float durationadd)
			{
				Percent = 0f;
				X = Calc.Random.NextFloat(384f);
				Y = Calc.Random.NextFloat(244f);
				Duration = Math.Abs(durationbase + Calc.Random.NextFloat() * durationadd);
				Width = Calc.Random.Next(minwidth, maxwidth);
				Length = Calc.Random.Next(minlength, maxlength);
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

		private VertexPositionColor[] vertices;

		private int vertexCount;



		private Ray[] rays;

		private float fade;

		public CustomGodrays(int minwidth, int maxwidth, int minlength, int maxlength, float durationbase, float durationadd, float raycoloralpha, String raycolor, String raycolorfade, float scrollx, float scrolly, float speedx, float speedy, int raycount, float anglex, float angley)
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

			UseSpritebatch = false;
			for (int i = 0; i < rays.Length; i++)
			{
				rays[i].Reset(minWidth, maxWidth, minLength, maxLength, durationBase, durationAdd);
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
			Player entity = level.Tracker.GetEntity<Player>();
			Vector2 value = Calc.AngleToVector(angleX, angleY);
			Vector2 value2 = new Vector2(0f - value.Y, value.X);
			int num = 0;
			for (int i = 0; i < rays.Length; i++)
			{
				if (rays[i].Percent >= 1f)
				{
					rays[i].Reset(minWidth, maxWidth, minLength, maxLength, durationBase, durationAdd);
				}
				rays[i].Percent += Engine.DeltaTime / rays[i].Duration;
				rays[i].Y += speedY * Engine.DeltaTime;
				rays[i].X += speedX * Engine.DeltaTime;
				float percent = rays[i].Percent;
				float num2 = -32f + Mod(rays[i].X - level.Camera.X * scrollX, 384f);
				float num3 = -32f + Mod(rays[i].Y - level.Camera.Y * scrollY, 244f);
				float width = rays[i].Width;
				float length = rays[i].Length;
				Vector2 value3 = new Vector2((int)num2, (int)num3);
				Color color = Color.Lerp(rayColor, rayColorFade, percent) * Ease.CubeInOut(Calc.Clamp(((percent < 0.5f) ? percent : (1f - percent)) * 2f, 0f, 1f)) * fade;
				if (entity != null)
				{
					float num4 = (value3 + level.Camera.Position - entity.Position).Length();
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
