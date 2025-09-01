using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.FlaglinesAndSuch
{

    // This is an extended version of Rotating Parallax, from Prismatic Helper, ported here with permission from Luna
    public class RotParralaxTwo : Backdrop
    {

        public readonly MTexture Texture;
        public readonly float Alpha, RotationSpeed, Scale;
        public readonly bool FadeIn;

        protected float rotation, fade = 1;

        protected Vector2 Origin;

        public RotParralaxTwo(BinaryPacker.Element e)
        {
            Texture = TryGet(e.Attr("atlas", "game"), e.Attr("texture"));
            Alpha = e.AttrFloat("alpha", 1);
            RotationSpeed = e.AttrFloat("rotationSpeed", 1);
            Scale = e.AttrFloat("scale", 1);
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

        public override void Render(Scene scene)
        {
            base.Render(scene);
            // based on Parallax::Render
            Vector2 cameraPos = ((Level)scene).Camera.Position.Floor();
            Vector2 pos = (Position - cameraPos * Scroll).Floor();
            float opacity = fade * Alpha * FadeAlphaMultiplier;
            if (FadeX != null)
                opacity *= FadeX.Value(cameraPos.X + 160f);
            if (FadeY != null)
                opacity *= FadeY.Value(cameraPos.Y + 90f);
            Color color = Color;
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
            Texture.Draw(new Vector2(pos.X, pos.Y), Origin, color, Scale, rotation, flip);
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
    }
}
