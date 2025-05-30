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
    public class SineParallaxStyleground : Backdrop
	{//should I have extended Parallax instead of backdrop? I don't know! This code might be extremely stupid, I just want to make it work

		public Vector2 CameraOffset = Vector2.Zero;

		public BlendState BlendState = BlendState.AlphaBlend;

		public MTexture Texture;

		//public bool DoFadeIn;

		public float Alpha = 1f;
		public Color Color;
		private float fadeIn = 1f;

		public bool InstantIn = true;
		public bool InstantOut;

		float sinValue;
		float amplitude;
		float frequency;
		float xOffset;
		bool isVert;
		float theta = 0;
		Vector2 initialPos;

		//no alpha/blendstate
		public SineParallaxStyleground(String Strtexture, float posx, float posy, float speedx, float speedy, float scrollx, float scrolly, bool loopx, bool loopy, float A, float f, float x, bool vert)
		{
			Position = new Vector2(posx, posy);

			//blend mode //do I really need this tbh
			//color //do I really need this tbh
			//speed
			//scroll 
			//alpha //do I really need this tbh
			//fade in //do I really need this tbh
			//flip X,Y //do I really need this tbh
			//Inst. in, out //do I really need this tbh
			//Loop X,Y

			Speed = new Vector2(speedx, speedy);
			Scroll = new Vector2(scrollx, scrolly);
			LoopY = loopy;
			LoopX = loopx;
			amplitude = A;
			frequency = f;
			xOffset = x;
			isVert = vert;
			Alpha = 1.0f;
			BlendState = BlendState.AlphaBlend;
			Color = Color.White;

			Texture = GFX.Game[Strtexture];
			Name = Texture.AtlasPath;
			initialPos = Position;
		}

		//yes alpha/blendstate
		public SineParallaxStyleground(String Strtexture, float posx, float posy, float speedx, float speedy, float scrollx, float scrolly, bool loopx, bool loopy, float A, float f, float x, bool vert, float alpha, String blendmode, String color, bool instin, bool instout, bool flipx, bool flipy)
		{
			Position = new Vector2(posx, posy);

			//blend mode
			//color //do I really need this tbh
			//speed
			//scroll 
			//alpha 
			//fade in //do I really need this tbh
			//flip X,Y //do I really need this tbh
			//Inst. in, out //do I really need this tbh
			//Loop X,Y

			Speed = new Vector2(speedx, speedy);
			Scroll = new Vector2(scrollx, scrolly);
			LoopY = loopy;
			LoopX = loopx;
			amplitude = A;
			frequency = f;
			xOffset = x;
			isVert = vert;

			Alpha = alpha;
			BlendState = (blendmode == "Alphablend") ? BlendState.AlphaBlend : BlendState.Additive;

			Color = Calc.HexToColor(color);
			InstantIn = instin;
			InstantOut = instout;
			FlipX = flipx;
			FlipY = flipy;

			Texture = GFX.Game[Strtexture];
			Name = Texture.AtlasPath;
			initialPos = Position;
		}

		public override void Update(Scene scene)
		{
			

			sinValue = amplitude * (float)Math.Sin(frequency * theta + (Math.PI * xOffset));
			if (isVert)
			{
					Position.Y = sinValue + initialPos.Y;
			}
			else
			{
					Position.X = sinValue + initialPos.X;
			}
			
			base.Update(scene);
			Position += Speed * Engine.DeltaTime;
			Position += WindMultiplier * (scene as Level).Wind * Engine.DeltaTime;
			theta = (theta + Engine.DeltaTime) % (float)(Math.PI * 2 / frequency); //<---- main thingy here 
			/*if (DoFadeIn)
			{
				fadeIn = Calc.Approach(fadeIn, Visible ? 1 : 0, Engine.DeltaTime);
			}
			else
			{
				fadeIn = (Visible ? 1 : 0);
			}*/
		}

		public override void Render(Scene scene)
		{
			Vector2 value = ((scene as Level).Camera.Position + CameraOffset).Floor();
			Vector2 vector = (Position - value * Scroll).Floor();
			float num = fadeIn * Alpha * FadeAlphaMultiplier;
			if (FadeX != null)
			{
				num *= FadeX.Value(value.X + 160f);
			}
			if (FadeY != null)
			{
				num *= FadeY.Value(value.Y + 90f);
			}
			Color color = Color;
			if (num < 1f)
			{
				color *= num;
			}
			if (color.A <= 1)
			{
				return;
			}
			if (LoopX)
			{
				while (vector.X < 0f)
				{
					vector.X += Texture.Width;
				}
				while (vector.X > 0f)
				{
					vector.X -= Texture.Width;
				}
			}
			if (LoopY)
			{
				while (vector.Y < 0f)
				{
					vector.Y += Texture.Height;
				}
				while (vector.Y > 0f)
				{
					vector.Y -= Texture.Height;
				}
			}
			SpriteEffects flip = SpriteEffects.None;
			if (FlipX && FlipY)
			{
				flip = (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
			}
			else if (FlipX)
			{
				flip = SpriteEffects.FlipHorizontally;
			}
			else if (FlipY)
			{
				flip = SpriteEffects.FlipVertically;
			}
			for (float num2 = vector.X; num2 < 320f; num2 += (float)Texture.Width)
			{
				for (float num3 = vector.Y; num3 < 180f; num3 += (float)Texture.Height)
				{
					Texture.Draw(new Vector2(num2, num3), Vector2.Zero, color, 1f, 0f, flip);
					if (!LoopY)
					{
						break;
					}
				}
				if (!LoopX)
				{
					break;
				}
			}
		}
	}
}
