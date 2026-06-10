using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;


namespace FlaglinesAndSuch
{
    public class VaporwaveGrid : Backdrop
    {
        private Vector2 lastCamera = Vector2.Zero;

        private readonly int OriginalHeight;
        private int Height;//how high up the screen the top lines are

        private readonly int linecountH;//# of horz lines
        private readonly int linecountV;//# of vert lines

        private readonly float speedV;//up/down speed
        private float heightMod;//0 to 1 value, changed on update using speedV

        private readonly float scrollTop;
        private readonly float scrollBot;

        private readonly Color mainColor;

        private readonly bool FLIP;//seems to still have some issues-namely, moving H. lines move the wrong way, and there seem to be less of them onscreen
        private readonly int ViewX;
        private readonly int ViewY;

        private readonly float[] TopXpts;

        private static int GameplayBufferWidth => GameplayBuffers.Gameplay?.Width ?? 320;
        private static int GameplayBufferHeight => GameplayBuffers.Gameplay?.Height ?? 180;
        
        public VaporwaveGrid(string color, int Hlines, int Vlines, float scrollT, float scrollB, int height, int xview, int yview, bool flipit, float speedy) {
            
            OriginalHeight = height;
            Height = FLIP ? OriginalHeight : GameplayBufferHeight - OriginalHeight;
            
            linecountH = Hlines;
            linecountV = Vlines;
            
            speedV = speedy;
            
            scrollTop = -scrollT;
            scrollBot = -scrollB;
            
            mainColor = Calc.HexToColor(color);
            
            FLIP = flipit;
            ViewX = FLIP ? -xview : xview;
            ViewY = yview;
            
            TopXpts = new float[linecountV];
            
            float botDistV1 = (float)GameplayBufferWidth / linecountV;
            for (int i = 0; i < TopXpts.Length; i++) {
                TopXpts[i] = OriginalHeight - (i * botDistV1); //note the height variable here is not the Height variable assigned earlier; this is independant of Flip
            }
        }


        public override void Update(Scene scene)
        {
            base.Update(scene);
            
            int CameraHeight = ((Level)Engine.Scene).Camera.Viewport.Height;
            Height = FLIP ? OriginalHeight : CameraHeight - OriginalHeight;
            
            Vector2 position = ((Level)scene).Camera.Position;
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
            int CameraWidth = ((Level)Engine.Scene).Camera.Viewport.Width;
            int CameraHeight = ((Level)Engine.Scene).Camera.Viewport.Height;
            
            Draw.Line(0, Height, CameraWidth, Height, mainColor);//make sure there's always a line at the horizon
            
            int[] HlineHeights = getProjectedHeights();
            for (int i = 0; i < linecountH; i += 1) {// horizontal line rendering

                if (FLIP) {
                    Draw.Line(0, CameraHeight + Height - HlineHeights[i], CameraWidth, CameraHeight + Height - HlineHeights[i], mainColor);
                }
                else {
                    Draw.Line(0, HlineHeights[i], CameraWidth, HlineHeights[i], mainColor);
                }
            }

            for (int i = 0; i < linecountV; i++)//vertical lines
            {
                Draw.Line(findParallaxPos(TopXpts[i]), FLIP ? 0 : CameraHeight, mod(TopXpts[i], 320), FLIP ? Height : Height + 1, mainColor);
            }
        }


        private static float mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        private float findParallaxPos(float X0)
        {
            int CameraWidth = ((Level)Engine.Scene).Camera.Viewport.Width;
            
            return ((mod(X0, 320) - (CameraWidth/2f)) * scrollBot / scrollTop) + (CameraWidth/2f);
        }


        //Vexatos's Horz. line placement algorithm
        private int[] getProjectedHeights()
        {
            int CameraHeight = ((Level)Engine.Scene).Camera.Viewport.Height;
            
            Vector2 viewpoint = new Vector2(ViewX, ViewY);

            float maxDistH = (-(FLIP ? Height : CameraHeight - Height) * viewpoint.X) / (viewpoint.Y - (FLIP ? Height : CameraHeight - Height));
            
            float spacingH = maxDistH / linecountH;

            float HLineOffset = (heightMod * spacingH) % spacingH;

            int[] heights = new int[linecountH];

            for (int i = 0; i < linecountH; i++) {
                float realposX = HLineOffset + (i) * spacingH;
                double angle = Math.Asin(viewpoint.Y / Math.Sqrt(Math.Pow(realposX - viewpoint.X, 2) + Math.Pow(viewpoint.Y, 2)));
                heights[i] = FLIP ? CameraHeight + Height + (int)(Math.Tan(angle) * realposX) : CameraHeight - (int) (Math.Tan(angle) * realposX);

            }
            return heights;
        }
    }
}
