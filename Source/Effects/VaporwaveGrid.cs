using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;


//GameplayBuffers.Gameplay.Width
//GameplayBuffers.Gameplay.Height

namespace FlaglinesAndSuch
{
    class VaporwaveGrid : Backdrop
    {
        //I am aware this is a massive pile of spaghetti
        //catstare
        //if you wanna fix it and send it to me to implement go right ahead

        private Vector2 lastCamera = Vector2.Zero;

        int Height;//how high up the screen the top lines are

        float botDistV;//dist between vert lines on bot
        float botDistH;//dist between horz lines on top
        //float topDistH;//dist between horz lines on bot

        int linecountH;//# of horz lines
        int linecountV;//# of vert lines

        //float speedH;//left/right speed
        float speedV = 0.05f;//up/down speed
        float heightMod;//0 to 1 value, changed on update using speedV

        float scrollTop = -0.2f;
        float scrollBot = -0.7f;

        public Color mainColor;

        bool FLIP = false;//seems to still have some issues-namely, moving H. lines move the wrong way, and there seem to be less of them onscreen
        int ViewX;
        int ViewY;

        private float[] TopXpts;

        public VaporwaveGrid(String color, int Hlines, int Vlines, float scrollT, float scrollB, int height, int xview, int yview, bool flipit, float speedy) {

            mainColor = Calc.HexToColor(color);
            FLIP = flipit;
            Height = FLIP ? height : 180 - height;
            speedV = speedy;

            linecountH = Hlines;
            linecountV = Vlines;

            botDistH = Height / linecountH;
            botDistV = 320f / linecountV;

            TopXpts = new float[linecountV];
            for (int i = 0; i < TopXpts.Length; i++) {
                TopXpts[i] = height - ((float)(i) * botDistV); //note the height variable here is not the Height variable assigned earlier; this is independant of Flip
            }

            scrollTop = -scrollT;
            scrollBot = -scrollB;


            ViewX = FLIP ? -xview : xview;
            ViewY = yview;



        }


        public override void Update(Scene scene)
        {
            base.Update(scene);
            Vector2 position = (scene as Level).Camera.Position;
            Vector2 value = position - lastCamera;
            for (int i = 0; i < TopXpts.Length; i++)
            {
                TopXpts[i] += (value.X * scrollTop);
            }
            lastCamera = position;

            heightMod = (heightMod + speedV) % 1;  // uses speedV
        }


        public override void Render(Scene scene)
        {
            //if (FLIP)
            //{
             //   Height = 180 - Height;
            //}

            Draw.Line(0, Height, 320, Height, mainColor);//make sure there's always a line at the horizon
            int[] HlineHeights = getProjectedHeights();
            for (int i = 0; i < linecountH; i += 1) {// horizontal line rendering

                if (FLIP) {
                    Draw.Line(0, 180 + Height - HlineHeights[i], 320, 180 + Height - HlineHeights[i], mainColor);
                }
                else {
                    Draw.Line(0, HlineHeights[i], 320, HlineHeights[i], mainColor);
                }
                //float x = i + heightMod;//moving lines

                //float Fx = botDistH * (x * x) + Height;//function: Y = -(hDist)X^2 + height ; x axis is bottom of screen, y intercept is top of effect
                //Fx = -botDistH * (float)(Math.Log(x)) + Height;
                //Fx = botDistH * (float)(Math.Pow(x, 0.5)) + Height;
            }

            for (int i = 0; i < linecountV; i++)//vertical lines
            {
                Draw.Line(findParallaxPos(TopXpts[i]), FLIP ? 0 : 180, mod(TopXpts[i], 320), FLIP ? Height : Height + 1, mainColor);
            }
            //if (FLIP)
            //{
            //    Height = 180 - Height;//the epitome of spaghetti. I wonder if I could place this in awake or the constructor
            //}
        }


        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        float findParallaxPos(float X0) {
            return ((mod(X0, 320) - 160) * scrollBot / scrollTop) + 160;
        }


        //Vexatos's Horz. line placement algorithm
        private int[] getProjectedHeights() {
            Vector2 viewpoint = new Vector2(ViewX, ViewY);

            float maxDistH = (-(FLIP ? Height : 180 - Height) * viewpoint.X) / (viewpoint.Y - (FLIP ? Height : 180 - Height));
            //float maxDistH = (-(180 - Height) * viewpoint.X) / (viewpoint.Y - ( 180 - Height));
            float spacingH = maxDistH / linecountH;

            float HLineOffset = (heightMod * spacingH) % spacingH;

            int[] heights = new int[linecountH];

            for (int i = 0; i < linecountH; i++) {
                float realposX = HLineOffset + (i) * spacingH;
                double angle = Math.Asin(viewpoint.Y / Math.Sqrt(Math.Pow(realposX - viewpoint.X, 2) + Math.Pow(viewpoint.Y, 2)));
                heights[i] = FLIP ? 180 + Height + (int)(Math.Tan(angle) * realposX) : 180 - (int) (Math.Tan(angle) * realposX);

            }
            return heights;
        }
    }
}
