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
	[CustomEntity("FlaglinesAndSuch/CustomFlagline")]
	public class CustomFlagline : Entity
	{
		public Color[] colors;
		//Calc.HexToColor("d85f2f"),
		//Calc.HexToColor("d82f63"),
		//Calc.HexToColor("2fd8a2"),
		//Calc.HexToColor("d8d62f")

		public Color lineColor;// = Color.Lerp(Color.Gray, Color.DarkBlue, 0.25f);

		public Color pinColor;// = Color.Gray;

		public int minHeight;
		public int maxHeight;
		public int minLength;
		public int maxLength;
		public int minSpacing;
		public int maxSpacing;
		public float flagDroopAmount;

		int entitydepth;

		public CustomFlagline(EntityData data, Vector2 offset)
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

			minHeight = data.Int("MinFlagHeight");
			maxHeight = data.Int("MaxFlagHeight");
			minLength = data.Int("MinFlagLength");
			maxLength = data.Int("MaxFlagLength");
			minSpacing = data.Int("MinSpacing");
			maxSpacing = data.Int("MaxSpacing");
			flagDroopAmount = data.Float("FlagDroopAmount");
			entitydepth = data.Int("Depth", 8999);//8999

			minHeight = Math.Min(minHeight, maxHeight);//assorted crash prevention
			minLength = Math.Min(minLength, maxLength);
			minSpacing = Math.Min(minSpacing, maxSpacing);

			if (minSpacing <= 0 && Math.Abs(minSpacing) >= Math.Abs(minLength))
			{
				minSpacing = minLength + 1;
			}
			if (minLength <= 0 && Math.Abs(minLength) >= Math.Abs(minSpacing))
			{
				minLength = minSpacing + 1;
			}

			if (maxSpacing == 0)
			{
				maxSpacing = 1;
			}

			base.Depth = entitydepth;
			Position = data.Position + offset;
			Flagline flagline = new Flagline(data.Nodes[0] + offset, lineColor, pinColor, colors, minHeight, maxHeight, minLength, maxLength, minSpacing, maxSpacing);
			Add(flagline);
			flagline.ClothDroopAmount = flagDroopAmount;

		}
	}

}
