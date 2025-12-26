using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
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

	[CustomEntity("FlaglinesAndSuch/TriangleFlagline")]
	public class CustomTriangleFlagline : Entity
	{
		public Color[] colors;
		//Calc.HexToColor("d85f2f"),
		//Calc.HexToColor("d82f63"),
		//Calc.HexToColor("2fd8a2"),
		//Calc.HexToColor("d8d62f")

		public Color lineColor;// = Color.Lerp(Color.Gray, Color.DarkBlue, 0.25f);

		public Color pinColor;// = Color.Gray;


		public int minSpacing;
		public int maxSpacing;
		public float flagDroopAmount;

		int entitydepth;

		public CustomTriangleFlagline(EntityData data, Vector2 offset)
		/*: this(data.Position + offset, data.Nodes[0] + offset)*/
		{
			string[] colorStrs = data.Attr("FlagColors").Replace(" ", String.Empty).Split(',');
			//Console.WriteLine(colorStrs.Length);
			colors = new Color[colorStrs.Length];
			for (int i = 0; i < colorStrs.Length; i++)
			{
				colors[i] = Calc.HexToColor(colorStrs[i]);
			}
			lineColor = Calc.HexToColor(data.Attr("WireColor"));
			pinColor = Calc.HexToColor(data.Attr("PinColor"));

			bool randomColors = data.Bool("RandomColors");

			minSpacing = data.Int("MinSpacing");
			maxSpacing = data.Int("MaxSpacing");
			flagDroopAmount = data.Float("FlagDroopAmount");
			entitydepth = data.Int("Depth", 8999);//8999

			minSpacing = Math.Min(minSpacing, maxSpacing);

			/*if (minSpacing <= 0 && Math.Abs(minSpacing) >= Math.Abs(minLength))
			{
				minSpacing = minLength + 1;
			}
			if (minLength <= 0 && Math.Abs(minLength) >= Math.Abs(minSpacing))
			{
				minLength = minSpacing + 1;
			}*/

			if (maxSpacing == 0)
			{
				maxSpacing = 1;
			}

			base.Depth = entitydepth;
			Position = data.Position + offset;
			triangleFlagline tflagline = new triangleFlagline(data.Nodes[0] + offset, lineColor, pinColor, colors, minSpacing, maxSpacing, randomColors);
			Add(tflagline);
			tflagline.ClothDroopAmount = flagDroopAmount;

		}

		class triangleFlagline : Component
		{
			private struct Cloth
			{
				public int Color;

				public int Height;

				public int Length;

				public int Step;
			}

			private Color[] colors;

			private Color[] highlights;

			private Color lineColor;

			private Color pinColor;

			private Cloth[] clothes;

			private float waveTimer;

			public float ClothDroopAmount = 0.6f;

			public Vector2 To;

			public Vector2 From => base.Entity.Position;

			public triangleFlagline(Vector2 to, Color lineColor, Color pinColor, Color[] colors, int minSpace, int maxSpace, bool randColors)
				: base(active: true, visible: true)
			{
				To = to;
				this.colors = colors;
				this.lineColor = lineColor;
				this.pinColor = pinColor;
				waveTimer = Calc.Random.NextFloat() * ((float)Math.PI * 2f);
				highlights = new Color[colors.Length];
				for (int i = 0; i < colors.Length; i++)
				{
					highlights[i] = Color.Lerp(colors[i], Color.White, 0.1f);
				}
				clothes = new Cloth[10];
				for (int j = 0; j < clothes.Length; j++)
				{
					clothes[j] = new Cloth
					{
						Color = randColors ? Calc.Random.Next(colors.Length) : (j % colors.Length), //Calc.Random.Next(colors.Length),
						Step = Calc.Random.Next(minSpace, maxSpace),
						Height = 10,
						Length = 10
					};
				}
			}

			public override void Update()
			{
				waveTimer += Engine.DeltaTime;
				base.Update();
			}

            private bool IsVisible(SimpleCurve curve)
            {
                Cloth[] clothes = this.clothes;
                float maxHeight = 0f;
                for (int i = 0; i < clothes.Length; i++)
                {
                    Cloth cloth = clothes[i];
                    float h = cloth.Height + (cloth.Length * ClothDroopAmount * 1.4f);

                    if (h > maxHeight)
                    {
                        maxHeight = h;
                    }
                }

                return CullHelper.IsCurveVisible(curve, maxHeight + 8f);
            }

            public override void Render()
			{

                Vector2 vector = (From.X < To.X) ? From : To;
				Vector2 vector2 = (From.X < To.X) ? To : From;
				float num = (vector - vector2).Length();
				float num2 = num / 8f;
				SimpleCurve simpleCurve = new SimpleCurve(vector, vector2, (vector2 + vector) / 2f + Vector2.UnitY * (num2 + (float)Math.Sin(waveTimer) * num2 * 0.3f));


                if (!IsVisible(simpleCurve))
                    return;

                Vector2 vector3 = vector;
				Vector2 vector4 = vector;
				float num3 = 0f;
				int num4 = 0;
				bool flag = false;
				while (num3 < 1f)
				{
					Cloth cloth = clothes[num4 % clothes.Length];
					num3 += (float)(flag ? cloth.Length : cloth.Step) / num;
					vector4 = simpleCurve.GetPoint(num3); //messes with vector4, likely vector4 is "second"
					Draw.Line(vector3, vector4, lineColor);
					if (num3 < 1f && flag)
					{
						float num5 = (float)cloth.Length * ClothDroopAmount;
						SimpleCurve simpleCurve2 = new SimpleCurve(vector3, vector4, (vector3 + vector4) / 2f + new Vector2(0f, num5 + (float)Math.Sin(waveTimer * 2f + num3) * num5 * 0.4f));
						Vector2 vector5 = vector3;
						float num6 = 1f; //inserted code
						Vector2 point = simpleCurve2.GetPoint(num6 / (float)cloth.Length); //inserted code
						/*for (float num6 = 1f; num6 <= (float)cloth.Length; num6 += 1f)
						{
							Vector2 point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
							if (point.X != vector5.X)
							{
								Draw.Rect(vector5.X, vector5.Y, /*point.X - vector5.X + *1f, cloth.Height, colors[cloth.Color]);//draws vertical bars
								vector5 = point;
							}
						}*/
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector3.Y, 1f, 3, highlights[cloth.Color]); //new left
							vector5 = point;
						}
						num6 += 1f;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 1, 1f, 3, colors[cloth.Color]); //2
							Draw.Rect(vector5.X, vector5.Y + 4, 1f, 2, highlights[cloth.Color]); //2
							vector5 = point;
						}
						num6 += 1f;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 1, 1f, 6, colors[cloth.Color]); //3
							Draw.Rect(vector5.X, vector5.Y + 7, 1f, 1, highlights[cloth.Color]); //3
							vector5 = point;
						}
						num6 += 1f;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 2, 1f, 7, colors[cloth.Color]); //4
							Draw.Rect(vector5.X, vector5.Y + 9, 1f, 1, highlights[cloth.Color]); //4
							vector5 = point;
						}
						num6 += 1f;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 2, 1f, 9, colors[cloth.Color]); //5
							Draw.Rect(vector5.X, vector5.Y + 11, 1f, 1, highlights[cloth.Color]); //5
							vector5 = point;
						}
						num6++;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 2, 1f, 10, colors[cloth.Color]); //center
							Draw.Rect(vector5.X, vector5.Y + 12, 1f, 2, highlights[cloth.Color]); //center
							vector5 = point;
						}
						num6++;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 2, 1f, 9, colors[cloth.Color]); //7
							Draw.Rect(vector5.X, vector5.Y + 11, 1f, 1, highlights[cloth.Color]); //7
							vector5 = point;
						}
						num6++;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 2, 1f, 7, colors[cloth.Color]); //8
							Draw.Rect(vector5.X, vector5.Y + 9, 1f, 1, highlights[cloth.Color]); //8
							vector5 = point;
						}
						num6++;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 1, 1f, 6, colors[cloth.Color]); //9
							Draw.Rect(vector5.X, vector5.Y + 7, 1f, 1, highlights[cloth.Color]); //9
							vector5 = point;
						}
						num6++;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y + 1, 1f, 3, colors[cloth.Color]); //10
							Draw.Rect(vector5.X, vector5.Y + 4, 1f, 2, highlights[cloth.Color]); //10
							vector5 = point;
						}
						num6++;
						point = simpleCurve2.GetPoint(num6 / (float)cloth.Length);
						if (point.X != vector5.X)
						{
							Draw.Rect(vector5.X, vector5.Y, 1f, 3, highlights[cloth.Color]); //new right
							//vector5 = point;
						}
						//Draw.Rect(vector3.X, vector3.Y, 1f, cloth.Height, highlights[cloth.Color]);  // left edge
						//Draw.Rect(vector4.X, vector4.Y, 1f, cloth.Height, highlights[cloth.Color]);  //right edge
						Draw.Rect(vector3.X, vector3.Y - 1f, 1f, 3f, pinColor);  //left pin
						Draw.Rect(vector4.X, vector4.Y - 1f, 1f, 3f, pinColor);  //right pin
						num4++;
					}
					vector3 = vector4;
					flag = !flag;
				}
			}
		}
	}
}
