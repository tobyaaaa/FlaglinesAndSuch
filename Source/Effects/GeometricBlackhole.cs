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
    class GeometricBlackhole : Backdrop
    {

        public bool reverse;

        public int shapeCount = 20;//20
        public float moveSpeed = 2;//2

        private readonly MTexture bgTexture;
        private VirtualRenderTarget buffer;

        float swirlmin;//how fast the squares in the middle sink once they sink faster
        float swirlmax;//does something similar to new max I think?
        float swirlnewmin;//the size the squares shrink to once they shrink faster
        float swirlnewmax;//how "close" the background is

        float rot_angle;

        private Color bgColorOuter = Calc.HexToColor("d60000");
        private Color bgColorOuter2 = Calc.HexToColor("d60000");

        private Color bgColorInner = Calc.HexToColor("000000");
        private Vector2 center = new Vector2(320f, 180f) / 2f;
        private float spinTime;

        public GeometricBlackhole(int num, float spd, string texture, string col0, string col1, string col2, float smin, float smax, float sminN, float smaxN, float rangle, bool rev)
        {
            bgTexture = GFX.Game[texture];
            shapeCount = num;
            moveSpeed = spd;
            bgColorInner = Calc.HexToColor(col0);
            bgColorOuter = Calc.HexToColor(col1);
            bgColorOuter2 = Calc.HexToColor(col2);

            swirlmin = smin;
            swirlmax = smax;
            swirlnewmin = sminN;
            swirlnewmax = smaxN;

            rot_angle = rangle;
            reverse = rev;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (Visible) {
                if (reverse) {
                    spinTime -= (moveSpeed) * Engine.DeltaTime;
                }
                else {
                    spinTime += (moveSpeed) * Engine.DeltaTime;
                }
            }
        }

        public override void BeforeRender(Scene scene)
        {
            if (buffer == null || buffer.IsDisposed)
            {
                buffer = VirtualContent.CreateRenderTarget("Black Hole", 320, 180);
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(bgColorInner);
            Engine.Instance.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            Draw.SpriteBatch.Begin();
            //Engine.Instance.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            for (int i = 0; i < shapeCount; i++)
            {
                float num = ((reverse ? -1 : 1) - (spinTime % 2f))       *       (1f / shapeCount)         +         (float)i / (1f * shapeCount);
                //this number ^^^^^^^^^^^^^^^^^ normally 1, messes with Reverse though
                Color color = Color.Lerp(bgColorInner, ((i % 2 == 0) ? bgColorOuter : bgColorOuter2), Ease.SineOut(num));
                float scale = Calc.ClampedMap(num, swirlmin, swirlmax, swirlnewmin, swirlnewmax);//num,0,1,0.1,4 //changing 4 makes swirls smaller
                float rotation = (float)Math.PI * rot_angle * num;
                bgTexture.DrawCentered(center, color, scale, rotation);
            }
            Draw.SpriteBatch.End();
        }

        public override void Ended(Scene scene)
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        public override void Render(Scene scene)
        {
            Engine.Instance.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            if (buffer != null && !buffer.IsDisposed)
            {
                Vector2 vector = new Vector2(buffer.Width, buffer.Height) / 2f;
                Draw.SpriteBatch.Draw((RenderTarget2D)buffer, vector, buffer.Bounds, Color.White * FadeAlphaMultiplier, 0f, vector, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
