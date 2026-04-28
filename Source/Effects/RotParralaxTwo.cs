using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.FlaglinesAndSuch
{

    // This is an extended version of Rotating Parallax, from Prismatic Helper, ported here with permission from Luna
    public class RotParralaxTwo : Backdrop
    {

        public readonly MTexture Texture;
        public readonly float Alpha, RotationSpeed, Scalef;
        public readonly bool FadeIn;

        protected float rotation, fade = 1;

        protected Vector2 Origin;
        protected Vector2 Scale;



        private VirtualRenderTarget buffer;

        public RotParralaxTwo(BinaryPacker.Element e)
        {
            Texture = TryGet(e.Attr("atlas", "game"), e.Attr("texture"));
            Alpha = e.AttrFloat("alpha", 1);
            RotationSpeed = e.AttrFloat("rotationSpeed", 1);
            FadeIn = e.AttrBool("fadeIn");

            Position.X = e.AttrFloat("x");
            Position.Y = e.AttrFloat("y");
            //Position = e.AttrPos();
            Scroll.X = e.AttrFloat("scrollX");
            Scroll.Y = e.AttrFloat("scrollY");
            //Scroll = e.AttrVec("scroll");
            Speed.X = e.AttrFloat("speedX");
            Speed.Y = e.AttrFloat("speedY");
            //Speed = e.AttrVec("speed");

            Origin.X = e.AttrFloat("originX");
            Origin.Y = e.AttrFloat("originY");
            //Origin = e.AttrVec("origin");
            rotation = e.AttrFloat("initialRotation");

            if (e.AttrFloat("scale") != 0) {
                Scalef = e.AttrFloat("scale", 1);
                Scale = Vector2.One;
            }
            else
            {
                Scale.X = e.AttrFloat("scaleX");
                Scale.Y = e.AttrFloat("scaleY");
                Scalef = 1;

                Scroll.X /= Scale.X;
                Scroll.Y /= Scale.Y;
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            rotation += Engine.DeltaTime * RotationSpeed;

            Position += Speed * Engine.DeltaTime;
            Position += WindMultiplier * ((Level)scene).Wind * Engine.DeltaTime;
            var target = Visible ? 1 : 0;
            fade = FadeIn ? Calc.Approach(fade, target, Engine.DeltaTime) : target;
        }

        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            buffer = EnsureValidBuffer();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
            Renderer.StartSpritebatch(BlendState.AlphaBlend); //TODO make a param.
            drawThing(scene);
            Renderer.EndSpritebatch();

        }

        public override void Render(Scene scene)
        {
            base.Render(scene);
            /*
            // based on Parallax::Render
            Vector2 cameraPos = ((Level)scene).Camera.Position.Floor();
            Vector2 pos = (Position - cameraPos * Scroll).Floor();
            float opacity = fade * Alpha * FadeAlphaMultiplier;
            if (FadeX != null)
                opacity *= FadeX.Value(cameraPos.X + 160f);
            if (FadeY != null)
                opacity *= FadeY.Value(cameraPos.Y + 90f);
            Microsoft.Xna.Framework.Color color = Color;
            if (opacity < 1.0)
                color *= opacity;
            if (color.A <= 1)
                return;

            SpriteEffects flip = SpriteEffects.None;
            if (FlipX)
                flip |= SpriteEffects.FlipHorizontally;
            if (FlipY)
                flip |= SpriteEffects.FlipVertically;

            //Texture.Draw(new Vector2(pos.X, pos.Y), Texture.Center, color, Scale, rotation, flip);
            Texture.Draw(new Vector2(pos.X, pos.Y), Origin, color, Scale, rotation, flip);*/
            Draw.SpriteBatch.Draw(
          buffer,
          position: Vector2.Zero,
          sourceRectangle: null,
          Microsoft.Xna.Framework.Color.White,
          rotation: 0,
          origin: Vector2.Zero,//notably changing this messes w/ back compat
          Scale,
          SpriteEffects.None,
          layerDepth: 0f);
          //TODO: currently, the scaling method scales the draw window TOWARDS THE TOP CORNER OF THE SCREEN. It should
          //probably scale towards the anchor point instead. It also
          //means the virtual texture must be 1/Scale bigger than the screen size, to avoid the scaling cutting off the image.
          // ALSO see the other todo
        }

        private static MTexture TryGet(string atlas, string name)
        {
            return atlas switch
            {
                "game" when GFX.Game.Has(name) => GFX.Game[name],
                "gui" when GFX.Gui.Has(name) => GFX.Gui[name],
                "portraits" when GFX.Portraits.Has(name) => GFX.Portraits[name], // for completion
                _ => GFX.Misc[name]
            };
        }



        void drawThing(Scene scene) {
            Vector2 cameraPos = ((Level)scene).Camera.Position.Floor();
            Vector2 pos = (Position - cameraPos * Scroll).Floor();
            float opacity = fade * Alpha * FadeAlphaMultiplier;
            if (FadeX != null)
                opacity *= FadeX.Value(cameraPos.X + 160f);
            if (FadeY != null)
                opacity *= FadeY.Value(cameraPos.Y + 90f);
            Microsoft.Xna.Framework.Color color = Color;
            if (opacity < 1.0)
                color *= opacity;
            if (color.A <= 1)
                return;

            SpriteEffects flip = SpriteEffects.None;
            if (FlipX)
                flip |= SpriteEffects.FlipHorizontally;
            if (FlipY)
                flip |= SpriteEffects.FlipVertically;

            //Texture.Draw(new Vector2(pos.X, pos.Y), Texture.Center, color, Scale, rotation, flip);
            Texture.Draw(new Vector2(pos.X, pos.Y), Origin, color, Scalef, rotation, flip);

        }

        //stolen from sunsetquasar who stole it from ja
        private VirtualRenderTarget EnsureValidBuffer()
        {
            var gpBuffer = GameplayBuffers.Gameplay;

            int targetWidth = gpBuffer?.Width ?? 320;
            int targetHeight = gpBuffer?.Height ?? 180;

            //if this effect is being scaled down, it needs to be drawn on a bigger canvas; so 
            //that once the scale takes effect the *result* fills the screen.
            
            if (Scale.X < 1)
            {
                targetWidth = (int)(targetWidth / Scale.X);
            }
            if (Scale.Y < 1)
            {
                targetHeight = (int)(targetHeight / Scale.Y);
            }


            if (gpBuffer is null || gpBuffer.Width == 320 || gpBuffer.Width == 321)
            {
                buffer ??= VirtualContent.CreateRenderTarget("FlaglinesandSuch/RotParralaxTwo", targetWidth, targetHeight);
                return buffer;
            }

            // We need a bigger buffer due to zoomout.
            // We'll keep the 320x180 buffer around, in case some other cloudscape wants to render with ZoomBehavior=StaySame
            if (buffer is null || buffer.IsDisposed || buffer.Width != gpBuffer.Width)
            {
                buffer?.Dispose();
                buffer = VirtualContent.CreateRenderTarget("FlaglinesandSuch/RotParralaxTwo", targetWidth, targetHeight);
            }

            return buffer;
        }


    }
}
