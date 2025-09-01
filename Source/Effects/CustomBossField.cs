using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace FlaglinesAndSuch
{
    class CustomBossField : Backdrop
	{
		private struct Particle
		{
			public Vector2 Position;

			public Vector2 Direction;

			public float Speed;

			public Color Color;

			public float DirectionApproach;
		}

		public float Alpha = 1f;

		public int particleCount = 200;

		private Particle[] particles;

        private VertexPositionColor[] BGverts;
        private VertexPositionColor[] verts;

		public Color[] colors;
	
		//Calc.HexToColor("030c1b"),
		//Calc.HexToColor("0b031b"),
		//Calc.HexToColor("1b0319"),
		//Calc.HexToColor("0f0301")

		public Color BGcolor = Color.Black;
		public int particlePosXmin=0;
		public int particlePosXmax=384;
		public int particlePosYmin=0;
		public int particlePosYmax=244;
		public Vector2 particleDir;
		public Vector2 particleDirB;
		//public float DirApproach;
		public float ScrollX;
		public float ScrollY;
		public float squishFactor = 1;
		public CustomBossField(String bgcolor, float alpha, int particlecount, String ParticleColors, float speedmin, float speedmax, int posxmin, int posxmax, int posymin, int posymax, float dirX, float dirY, float dirXB, float dirYB, float scrollx, float scrolly, float squishmultiplier)
		{
			BGcolor = (bgcolor.Length == 6) ? Calc.HexToColor(bgcolor) : Color.Black * 0.0f;
			Alpha = alpha;


			string[] colorStrs = ParticleColors.Replace(" ", String.Empty).Split(',');
			colors = new Color[colorStrs.Length];
			for (int i = 0; i < colorStrs.Length; i++)
			{
				colors[i] = Calc.HexToColor(colorStrs[i]);
			}

			particleCount = Math.Abs(particlecount);
			particles = new Particle[particleCount];
            BGverts = new VertexPositionColor[6];
            verts = new VertexPositionColor[(particleCount) * 6]; //used to be particlecount + 1 but I've moved the background out


			particlePosXmax = posxmax;
			particlePosXmin = Math.Min(posxmin, posxmax);
			particlePosYmax = posymax;
			particlePosYmin = Math.Min(posymin, posxmax);

			particleDir = new Vector2(dirX, dirY);
			particleDirB = new Vector2(dirXB, dirYB);

			ScrollX = scrollx;
			ScrollY = scrolly;

			if (squishmultiplier != 0)
			{
				squishFactor = squishmultiplier;
			}

			UseSpritebatch = false;
			for (int i = 0; i < particleCount; i++)
			{
				particles[i].Speed = Calc.Random.Range(speedmin, speedmax);
				particles[i].Direction = particleDir;
				particles[i].DirectionApproach = Calc.Random.Range(0.25f, 4f);
				particles[i].Position.X = Calc.Random.Range(particlePosXmin, particlePosXmax);
				particles[i].Position.Y = Calc.Random.Range(particlePosYmin, particlePosYmax);
				particles[i].Color = Calc.Random.Choose(colors);
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (Visible && Alpha > 0f)
			{
				Vector2 vector = particleDir;
				Level level = scene as Level;
				if (level.Bounds.Height > level.Bounds.Width)//get level bounds
				{
					vector = particleDirB;
				}
				float target = vector.Angle();
				for (int i = 0; i < particleCount; i++)
				{
					particles[i].Position += particles[i].Direction * particles[i].Speed * Engine.DeltaTime;
					float val = particles[i].Direction.Angle();
					val = Calc.AngleApproach(val, target, particles[i].DirectionApproach * Engine.DeltaTime);
					particles[i].Direction = Calc.AngleToVector(val, 1f);
				}
			}
		}

		public override void Render(Scene scene)
		{
			//int zoomoutCompatibleWidth = GameplayBuffers.Gameplay.Width + 64; //384f
            //int zoomoutCompatibleHeight = GameplayBuffers.Gameplay.Height + 64; //244f

			float zoomoutCompatibleWidthBG = GameplayBuffers.Gameplay.Width + 10;//330
            float zoomoutCompatibleHeightBG = GameplayBuffers.Gameplay.Height + 10;//190

            Vector2 position = (scene as Level).Camera.Position;//verts[0] through [5] is responsible for the background color
			Color color = BGcolor * Alpha;
			BGverts[0].Color = color;
			BGverts[0].Position = new Vector3(-10f, -10f, 0f);
			BGverts[1].Color = color;
			BGverts[1].Position = new Vector3(zoomoutCompatibleWidthBG, -10f, 0f);//330f
            BGverts[2].Color = color;
			BGverts[2].Position = new Vector3(zoomoutCompatibleWidthBG, zoomoutCompatibleHeightBG, 0f);//330f, 190f
            BGverts[3].Color = color;
			BGverts[3].Position = new Vector3(-10f, -10f, 0f);
            BGverts[4].Color = color;
            BGverts[4].Position = new Vector3(zoomoutCompatibleWidthBG, zoomoutCompatibleHeightBG, 0f);//330f, 190f
            BGverts[5].Color = color;
			BGverts[5].Position = new Vector3(-10f, zoomoutCompatibleHeightBG, 0f);//190f


			for (int i = 0; i < particleCount; i++)
			{
				int num = (i) * 6;//used to be (i+1) * 6 but moved bg out
				float scaleFactor = Calc.ClampedMap((particles[i].Speed * squishFactor), 0f, 1200f, 1f, 64f);
				float scaleFactor2 = Calc.ClampedMap((particles[i].Speed * squishFactor), 0f, 1200f, 3f, 0.6f);
				Vector2 direction = particles[i].Direction;
				Vector2 value = direction.Perpendicular();
				Vector2 position2 = particles[i].Position;
				position2.X = -32f + Mod(position2.X - position.X * ScrollX, 384f);//384f
                position2.Y = -32f + Mod(position2.Y - position.Y * ScrollY, 244f);//244f
                Vector2 value2 = position2 - direction * scaleFactor * 0.5f - value * scaleFactor2;
				Vector2 value3 = position2 + direction * scaleFactor * 1f - value * scaleFactor2;
				Vector2 value4 = position2 + direction * scaleFactor * 0.5f + value * scaleFactor2;
				Vector2 value5 = position2 - direction * scaleFactor * 1f + value * scaleFactor2;
				Color color2 = particles[i].Color * Alpha;
				verts[num].Color = color2;
				verts[num].Position = new Vector3(value2, 0f);
				verts[num + 1].Color = color2;
				verts[num + 1].Position = new Vector3(value3, 0f);
				verts[num + 2].Color = color2;
				verts[num + 2].Position = new Vector3(value4, 0f);
				verts[num + 3].Color = color2;
				verts[num + 3].Position = new Vector3(value2, 0f);
				verts[num + 4].Color = color2;
				verts[num + 4].Position = new Vector3(value4, 0f);
				verts[num + 5].Color = color2;
				verts[num + 5].Position = new Vector3(value5, 0f);
			}
            //GFX.DrawVertices(Matrix.Identity, verts, verts.Length);
            GFX.DrawVertices(Matrix.Identity, BGverts, BGverts.Length);

            for (int i = 0; i < GameplayBuffers.Gameplay.Width + 64; i += 384) {
                for (int j = 0; j < GameplayBuffers.Gameplay.Height + 64; j += 244)
                {
					//note: drawing the entire set of vertices will obscure some details as the background gets drawn over old particles
                    GFX.DrawVertices(Matrix.CreateTranslation(new Vector3(i, j, 0)), verts, verts.Length);
                }
            }
			//GFX.DrawVertices(Matrix.CreateTranslation(new Vector3(384f, 0, 0)), verts, verts.Length);

            //if zoomed out:
            //for copies of 384 until gameplaywidth is covered:
            //for copies of 244 until gameplayheight is covered:
            //for each vert: translate it by i, j
            //then draw all the verts (except 0 through 5) again
            //(verts 0 thru 5 can be changed dynamically like in the nieve implementation)
        }

        private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
