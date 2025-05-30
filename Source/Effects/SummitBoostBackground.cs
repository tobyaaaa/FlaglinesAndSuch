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
    class SummitBoostBackground : Backdrop
    {

		//general
		bool drawBG;
		bool drawBars;
		bool drawStreaks;
		bool drawClouds;
		private MTexture BarTextureBRTL;
		private MTexture BarTextureBLTR;

		float Angle;//-90
		private float AngleQTR = (float) (.5 * Math.PI);

		//public bool dark;
		Color backgroundColor;//75a0ab if not dark or 000000 if dark
		Color barColor;//streakAlphaColors[0]

		//streaks
		//int streakCount;//80
		float MinStreakSpeed = 600f;
		float MaxStreakSpeed = 2000f;

		private StreakParticle[] streakparticles;
		private List<MTexture> streaktextures;

		private Color[] StreakColors;// FFFFFF, e69ecb if not dark or 041b44, 011230 if dark
		private Color[] StreakalphaColors;

		//clouds
		//int cloudCount;//10
		float MinCloudSpeed = 400f;
		float MaxCloudSpeed = 800f;

		private Color CloudColor;//b64a86 if not dark or 082644 if dark

		private List<MTexture> cloudtextures;
		private CloudParticle[] cloudparticles;


		private struct StreakParticle
		{
			public Vector2 Position;

			public float Speed;

			public int Index;

			public int Color;
		}
		private struct CloudParticle
		{
			public Vector2 Position;

			public float Speed;

			public int Index;
		}


		public SummitBoostBackground(bool drawbg, bool drawbars, bool drawstreaks, bool drawclouds, float angledir, String bgcolor, String barcolor, int streakcount, float streakspeedmin, float streakspeedmax, String streakcolors, float streakalpha, int cloudcount, float cloudspeedmin, float cloudspeedmax, String cloudcolor, float cloudalpha)
		{
			//public stuffs
			drawBG = drawbg;
			drawBars = drawbars;
			drawStreaks = drawstreaks;
			drawClouds = drawclouds;
			backgroundColor = Calc.HexToColor(bgcolor);
			barColor = Calc.HexToColor(barcolor);
            Angle = (float)(angledir * Math.PI / 180);


			BarTextureBRTL = GFX.Game["scenery/FlaglinesAndSuch/SummitBoostBG/bar_BR_TL"];
			BarTextureBLTR = GFX.Game["scenery/FlaglinesAndSuch/SummitBoostBG/bar_BL_TR"];

			MaxStreakSpeed = streakspeedmax;
			MinStreakSpeed = Math.Min(streakspeedmin, streakspeedmax);
			
			string[] streakstrs = streakcolors.Replace(" ", String.Empty).Split(',');//this whole block is for streak colors
			StreakColors = new Color[streakstrs.Length];
			for (int i = 0; i < streakstrs.Length; i++)
			{
				StreakColors[i] = Calc.HexToColor(streakstrs[i]) * streakalpha;
			}



			MaxCloudSpeed = cloudspeedmax;
			MinCloudSpeed = Math.Min(cloudspeedmin, cloudspeedmax);
			CloudColor = (Calc.HexToColor(cloudcolor) * cloudalpha);

			//clouds
			cloudparticles = new CloudParticle[cloudcount];
			cloudtextures = GFX.Game.GetAtlasSubtextures("scenery/launch/cloud");
			for (int i = 0; i < cloudparticles.Length; i++)
			{
				cloudparticles[i] = new CloudParticle
				{
					Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(900f)),
					Speed = Calc.Random.Range(MinCloudSpeed, MaxCloudSpeed),
					Index = Calc.Random.Next(cloudtextures.Count)
				};
			}
			

			//streaks
			streakparticles = new StreakParticle[streakcount];
			streaktextures = GFX.Game.GetAtlasSubtextures("scenery/launch/slice");
			StreakalphaColors = new Color[StreakColors.Length];
			for (int i = 0; i < streakparticles.Length; i++)
			{
				float num = 160f + Calc.Random.Range(24f, 144f) * (float)Calc.Random.Choose(-1, 1);//center streaks on screen edges	

				float y = Calc.Random.NextFloat(436f);
				float speed = Calc.ClampedMap(Math.Abs(num - 160f), 0f, 160f, 0.25f) * Calc.Random.Range(MinStreakSpeed, MaxStreakSpeed);
				streakparticles[i] = new StreakParticle
				{
					Position = new Vector2(num * (float)(Math.Sin(Angle + AngleQTR)) + y * (float)(Math.Cos(Angle + AngleQTR)), num * (float)(Math.Cos(Angle + AngleQTR)) + y * (float)(Math.Sin(Angle + AngleQTR))),
					Speed = speed,
					Index = Calc.Random.Next(streaktextures.Count),
					Color = Calc.Random.Next(StreakColors.Length)
				};
			}
		}

		public override void Update(Scene scene)
		{
			//clouds
			base.Update(scene);
			//Level level = scene as Level;
			for (int i = 0; i < cloudparticles.Length; i++)
			{
				cloudparticles[i].Position.Y += (float)(Math.Sin(Angle + AngleQTR)) *  cloudparticles[i].Speed * Engine.DeltaTime;
				cloudparticles[i].Position.X += (float)(Math.Cos(Angle + AngleQTR)) *  cloudparticles[i].Speed * Engine.DeltaTime;
			}

			//streaks
			for (int i = 0; i < streakparticles.Length; i++)
			{
				streakparticles[i].Position.Y += (float)(Math.Sin(Angle + AngleQTR)) * streakparticles[i].Speed * Engine.DeltaTime;
				streakparticles[i].Position.X += (float)(Math.Cos(Angle + AngleQTR)) * streakparticles[i].Speed * Engine.DeltaTime;
			}
		}

		public override void Render(Scene scene)
		{
			//Vector2 position = (scene as Level).Camera.Position;
			//Level level = scene as Level;
			if (drawBG)
			{
				Draw.Rect(0f, 0f, 340f, 200f, backgroundColor);//background
			}
			if (drawBars)
			{
				//Draw.Rect(0f, 0f, 26f, 200f, StreakalphaColors[0]);
				//Draw.Rect(294f, 0f, 26f, 200f, StreakalphaColors[0]);
				BarTextureBRTL.Draw(new Vector2(160, 90), new Vector2(0, 0), barColor, 1f, Angle);
				BarTextureBRTL.Draw(new Vector2(160, 90), new Vector2(0, 0), barColor, 1f, (float)(Angle - Math.PI));

				BarTextureBLTR.Draw(new Vector2(160, 90), new Vector2(0, 0), barColor, 1f, (float)(Angle - AngleQTR));
				BarTextureBLTR.Draw(new Vector2(160, 90), new Vector2(0, 0), barColor, 1f, (float)(Angle - 3 * AngleQTR));//(float)(Angle - Math.PI)

			}


			//streaks
			if (drawStreaks)
			{
				for (int i = 0; i < StreakColors.Length; i++)
				{
					StreakalphaColors[i] = StreakColors[i];
				}
				for (int j = 0; j < streakparticles.Length; j++)
				{
					Vector2 position2 = streakparticles[j].Position;
					position2.X = Mod(position2.X, 320f);
					position2.Y = -128f + Mod(position2.Y, 436f);//alright, gotta make this angled. WHat the hECK do I do here
																 //so, one. I need to mod the Y position based on the X position and the angle; I need to keep the old x position then mod the y by that according to angle

					//position2 += position;
					Vector2 scale2 = default(Vector2);
					scale2.X = Calc.ClampedMap(streakparticles[j].Speed, 600f, 2000f, 1f, 0.25f);
					scale2.Y = Calc.ClampedMap(streakparticles[j].Speed, 600f, 2000f, 1f, 2f);
					scale2 *= Calc.ClampedMap(streakparticles[j].Speed, 600f, 2000f, 1f, 4f);
					streaktextures[streakparticles[j].Index].DrawCentered(color: StreakalphaColors[streakparticles[j].Color], position: position2, scale: scale2, rotation: Angle);
				}
			}

			//clouds
			if (drawClouds)
			{
				Color color = this.CloudColor;
				for (int i = 0; i < cloudparticles.Length; i++)
				{
					Vector2 position2 = cloudparticles[i].Position;
					position2.Y = -360f + Mod(position2.Y, 900f);
					//position2.X = -640f + Mod(position2.X, 1600f);
					//position2 += position;//keeping these lines in memorial to the fact that default summit bg hAS PARRALAX SCROLL -1 WHYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
					cloudtextures[cloudparticles[i].Index].DrawCentered(position2, color);
				}
			}
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
    }
}
