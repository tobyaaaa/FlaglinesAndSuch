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
using System.Reflection;
using static Celeste.ClutterBlock;
using System.IO;
using System.Reflection.Metadata;
//using System.Numerics;

namespace FlaglinesAndSuch
{
    class polygonEffect : Backdrop
    {
		//each entry is a point/scroll "triplet" as DotPositions[i].X, .Y, .Z
		Vector3[] DotPositions;
        Vector3 WorldOffset;

		private VertexPositionColor[] vertices;
        List<VertexPositionColor> VisibleVertices = new List<VertexPositionColor>();
		int CornerCount;
		Color[] PolyColors;
		bool Front = false;

		int NumTriangles;

        public polygonEffect(string positions, String colors, bool front, float offX, float offY)
		{
            
            //split into sections, spaces removed
			//dotstrs has one entry for point "triplet"
            string[] DotStrs = positions.Replace(" ", String.Empty).Split(';');

			CornerCount = DotStrs.Length;
            NumTriangles = CornerCount / 3; //int division, assume our input is in groups of 3 //potential suspect for array size 0
            DotPositions = new Vector3[CornerCount];
			vertices = new VertexPositionColor[CornerCount];
			for (int i = 0; i < CornerCount; i++)
			{
				string[] DotStrs2 = DotStrs[i].Split(',');
				DotPositions[i] = new Vector3(float.Parse(DotStrs2[0]), float.Parse(DotStrs2[1]), float.Parse(DotStrs2[2]));
			}

            PolyColors = new Color[NumTriangles];
            string[] colStrs = colors.Replace(" ", String.Empty).Split(',');
            for (int i = 0; i < PolyColors.Length; i++)
            {
                if (i >= colStrs.Length)
                {
                    PolyColors[i] = Calc.HexToColor(colStrs[colStrs.Length - 1]);
                }
                else
                {
                    PolyColors[i] = Calc.HexToColor(colStrs[i]);
                }
            }

            Front = front;


            WorldOffset = new Vector3(offX, offY, 0);

        }

        /// <summary>
        /// If you want to make a non-rotated rectangular prism, the vertices of all 10 triangles can be defined in only 6 numbers.
        /// The rest can be filled in with an algorithm, which is what this constructor does.
        /// </summary>
        public polygonEffect(float x, float y, float z, float wid, float hig, float dep, String colors, float offX, float offY) {
            //seperate colors into 5 using my normal method
            PolyColors = new Color[10];

            string[] colStrs = colors.Replace(" ", String.Empty).Split(',');
            for (int i = 0; i < (PolyColors.Length /2); i++)//don't do this. Use 2i, 2i+1 instead
            {
                if (i >= colStrs.Length) {
                    PolyColors[i*2] = Calc.HexToColor(colStrs[colStrs.Length - 1]);
                    PolyColors[i*2+1] = Calc.HexToColor(colStrs[colStrs.Length - 1]);
                }
                else {
                    PolyColors[i*2] = Calc.HexToColor(colStrs[i]);
                    PolyColors[i*2+1] = Calc.HexToColor(colStrs[i]);
                }
            }

            WorldOffset = new Vector3(offX, offY, 0);

            //up/down, left/right, front/back
            Vector3 LUF = new Vector3(x,       y,       z);
            Vector3 LUB = new Vector3(x,       y,       z - dep);
            Vector3 LDF = new Vector3(x,       y + hig, z);
            Vector3 LDB = new Vector3(x,       y + hig, z - dep);
            Vector3 RUF = new Vector3(x + wid, y,       z);
            Vector3 RUB = new Vector3(x + wid, y,       z - dep);
            Vector3 RDF = new Vector3(x + wid, y + hig, z);
            Vector3 RDB = new Vector3(x + wid, y + hig, z - dep);

            //it's worth noting that since we have a fixed camera angle, the back face is never rendered and thus effectively nonexistent
            //want to render it anyway for a laugh? set the depth to be negative (note: may break the point orientation parity)
            CornerCount = 30;//3 pts/triangle * 2 triangles/side * 5 sides
            NumTriangles = 10;//2 triangles/side * 5 sides
            DotPositions = new Vector3[CornerCount];

            //front
            DotPositions[0] = LUF;
            DotPositions[1] = RUF;
            DotPositions[2] = RDF;
            DotPositions[3] = LUF;
            DotPositions[4] = RDF;
            DotPositions[5] = LDF;
            //top
            DotPositions[6] = LUB;
            DotPositions[7] = RUB;
            DotPositions[8] = RUF;
            DotPositions[9] = LUB;
            DotPositions[10] = RUF;
            DotPositions[11] = LUF;
            //bot
            DotPositions[12] = LDF;
            DotPositions[13] = RDF;
            DotPositions[14] = RDB;
            DotPositions[15] = LDF;
            DotPositions[16] = RDB;
            DotPositions[17] = LDB;
            //left
            DotPositions[18] = LUB;
            DotPositions[19] = LUF;
            DotPositions[20] = LDF;
            DotPositions[21] = LUB;
            DotPositions[22] = LDF;
            DotPositions[23] = LDB;
            //right
            DotPositions[24] = RUF;
            DotPositions[25] = RUB;
            DotPositions[26] = RDB;
            DotPositions[27] = RUF;
            DotPositions[28] = RDB;
            DotPositions[29] = RDF;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            //Console.WriteLine("triangle update!!!");
            Level level = scene as Level;
            //bool flag = IsVisible(level);
            //Player entity = level.Tracker.GetEntity<Player>();
            //int num = 0;

            //float num2 = -32f + Mod(rays[i].X - level.Camera.X * scrollX, 384f);
            //float num3 = -32f + Mod(rays[i].Y - level.Camera.Y * scrollY, 244f);
            //Vector2 value3 = new Vector2((int)num2, (int)num3);
            /*if (entity != null)
            {
                float num4 = (value3 + level.Camera.Position - entity.Position).Length();
                if (num4 < 64f)
                {
                    color *= 0.25f + 0.75f * (num4 / 64f);
                }
            }*/

            VisibleVertices.Clear();//clearing and repopulating entire list each frame is potentially very slow
            //for triangle...
            for (int i = 0; i < NumTriangles; i++)
            {
                bool renderThis;

                Vector3 pt1 = GetScrolledCoordinate(i * 3, level.Camera.X, level.Camera.Y);
                Vector3 pt2 = GetScrolledCoordinate((i * 3) + 1, level.Camera.X, level.Camera.Y);
                Vector3 pt3 = GetScrolledCoordinate((i * 3) + 2, level.Camera.X, level.Camera.Y);

                //this code checks whether the camera should render the triangle
                renderThis = (((pt2.X - pt1.X)*(pt3.Y - pt2.Y)) - ((pt3.X - pt2.X)*(pt2.Y - pt1.Y)) >= 0) != Front;

                //Console.WriteLine("init triangle");
                if (renderThis)
                {
                    AddCorner(i * 3, level.Camera.X, level.Camera.Y);
                    AddCorner((i * 3) + 1, level.Camera.X, level.Camera.Y);
                    AddCorner((i * 3) + 2, level.Camera.X, level.Camera.Y);
                }
                //else { Console.WriteLine("no triangle today"); }
            }

            /*
            for (int i = 0; i < CornerCount; i++) {
				//Vector3 RenderSpot = new Vector3(DotPositions[i].X, DotPositions[i].Y, 0);
				Vector3 RenderSpot = new Vector3(DotPositions[i].X * DotPositions[i].Z + (160f * (1- DotPositions[i].Z)), DotPositions[i].Y * DotPositions[i].Z + (90f * (1 - DotPositions[i].Z)), 0);
				RenderSpot = new Vector3(RenderSpot.X - (level.Camera.X) * DotPositions[i].Z , RenderSpot.Y - (level.Camera.Y) * DotPositions[i].Z, 0);
				VertexPositionColor vertexPositionColor = new VertexPositionColor(RenderSpot, PolyColor);
				vertices[i] = vertexPositionColor;
			}
			//vertexCount = num;*/

        }


        Vector3 GetScrolledCoordinate(int index, float camx, float camy) {
            Vector3 RenderSpot =  new Vector3(DotPositions[index].X* DotPositions[index].Z + (160f * (1 - DotPositions[index].Z)), DotPositions[index].Y* DotPositions[index].Z + (90f * (1 - DotPositions[index].Z)), 0);
            RenderSpot = WorldOffset + new Vector3(RenderSpot.X - (camx) * DotPositions[index].Z, RenderSpot.Y - (camy) * DotPositions[index].Z, 0);
            return RenderSpot;
            //todo: replace all other instances of this code elsewhere with a function call here
        }

        void AddCorner(int index, float camx, float camy) {
            //Vector3 RenderSpot = new Vector3(DotPositions[index].X * DotPositions[index].Z + (160f * (1 - DotPositions[index].Z)), DotPositions[index].Y * DotPositions[index].Z + (90f * (1 - DotPositions[index].Z)), 0);
            //RenderSpot = new Vector3(RenderSpot.X - (camx) * DotPositions[index].Z, RenderSpot.Y - (camy) * DotPositions[index].Z, 0);

            Vector3 RenderSpot = GetScrolledCoordinate(index, camx, camy);
            int triIndex = index / 3;

            VertexPositionColor vertexPositionColor = new VertexPositionColor(RenderSpot, PolyColors[triIndex]);
            VisibleVertices.Add(vertexPositionColor);
            //Console.WriteLine("we're so triangling");
        }


        public override void Render(Scene scene)
		{
            base.Render(scene);
            //should have added this sooner. Making an array of size 0 isn't a good idea lol
            int numVisible = VisibleVertices.Count;
            if (numVisible > 0) {
                VertexPositionColor[] verts = new VertexPositionColor[numVisible];
                //copy over list because drawvertices doesn't know what that is
                for (int i = 0; i < numVisible; i++) {
                    verts[i] = VisibleVertices[i];
                }
                //I think this automatically splits by triangle?
                GFX.DrawVertices(Matrix.Identity, verts, numVisible);//VisibleVertices.Count, not CornerCount

            }
        }


	}



    class polygonEffect2 : Backdrop
    {
        ObjModel model;
        //VertexPositionColorTexture[] vertstextures;
        //VertexPositionColor[] vertspos;
        public static VirtualTexture texture;
        public Matrix posMatrix;
        VirtualRenderTarget buffer;
        Vector3 camPos;
        public float scale;

        public polygonEffect2(string filename, string texturefilename, float x, float y, float z, float motionScale) {
            camPos = new Vector3(x, y, z);
            scale = motionScale;
            //GET THING AT PATH

            //model = ObjModel.Create(Path.Combine(Engine.ContentDirectory, filename));
            ModAsset foundfile;
            if (Everest.Content.TryGet(filename, out foundfile)) {
                model = ObjModel.CreateFromStream(foundfile.Stream, filename);
                //Console.WriteLine("flaglines says: made the object!");
            }
            //else { Console.WriteLine("flaglines and such: 3d model could not be created!"); }
            //if (model is null) { Console.WriteLine("flaglines and such: 3d model was null after creation!"); }
            //Console.WriteLine("flaglines says: ow. Is object null? " + (model is null));
            //.CreateFromStream(asset.Stream, path)


            //Atlas Mountain = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Mountain"), Atlas.AtlasDataFormat.PackerNoAtlas);
            //texture = Mountain["mountain_0"].Texture;
            texture = GFX.Game["models/" + texturefilename].Texture;

            //vertstextures = model.verts;
            //buffer = //VirtualContent.CreateRenderTarget("mountain-a", 1920, 1080, depth: true, preserve: false);

            //if (model is null) { Console.WriteLine("flaglines and such: 3d model was null at end of constructor!"); }
            //else { Console.WriteLine("flaglines and such: 3d model was FINE at end of constructor"); }
        }
        //load() 
        //

        public override void Update(Scene scene)
        {
            base.Update(scene);
            //float scale = motionScale;
            Level level = scene as Level;
            //add offset based on camera position
            Matrix base_matrix = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 4f, (float)Engine.Width / (float)Engine.Height, 0.25f, 50f);
            Matrix trans_matrix = Matrix.CreateTranslation( -( new Vector3(level.Camera.X * scale, -1 * level.Camera.Y * scale, 0) + camPos ) );// * Matrix.CreateFromQuaternion(rotation);
            
            //trans_matrix = Matrix.CreateTranslation(-(new Vector3(0, 0, 16)));

            //may be positioned strangely? double check scales between lvl cam and overworld cam
            posMatrix = trans_matrix * base_matrix;
            //GFX.FxMountain.Parameters["WorldViewProj"].SetValue(Matrix.CreateTranslation(CoreWallPosition) * matrix3);

            //Console.WriteLine("flaglines and such: this statement gets printed once per update");
            //if (model is null) { Console.WriteLine("flaglines and such: 3d model was null at time of Update!"); }
            //else { Console.WriteLine("flaglines and such: 3d model was FINE at time of Update"); }
        }
        public override void BeforeRender(Scene scene)
        {

            //base.BeforeRender(scene);
            if (buffer == null) {
                buffer = VirtualContent.CreateRenderTarget("polygonEffect2", 320, 180, depth:true, preserve:false);
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Engine.Graphics.GraphicsDevice.Textures[0] = texture.Texture_Safe;
            //may be what makes the render order work?
            Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //hopefully self explanitory
            Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //unsure
            Engine.Graphics.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullClockwiseFace, MultiSampleAntiAlias = true };


            //I'm curious if this GFX.Mountain is necessarily what to use
            //clearly setting its position is doing something
            GFX.FxMountain.Parameters["WorldViewProj"].SetValue(posMatrix);
            //GFX.FxMountain.Parameters["fog"].SetValue(customFog.TopColor.ToVector3());
            GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Single"];


            //Console.WriteLine("flaglines says: attempt render. Is object null? " + (model is null) + " ..and is FXMountain? " + (GFX.FxMountain is null));
            if (!(model is null)) {
                model.Draw(GFX.FxMountain);
            }
            else { 
                //Console.WriteLine("flaglines and such: 3d model was null at time of BeforeRender!"); 
            }
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);


            Draw.SpriteBatch.Draw((Texture2D)(RenderTarget2D)buffer, Vector2.Zero, Color.White);

        }
        //
        public override void Ended(Scene scene)
        {
            if (buffer != null)
            {
                buffer.Dispose();
            }
            buffer = null;
            base.Ended(scene);
        }

    }

}

//note most of this happens in BeforeRender in the overworld, Render just draws a spritebatch

//in ResetRenderTargets().... see also DisposeTargets()
//buffer = VirtualContent.CreateRenderTarget("mountain-a", 1920, 1080, depth: true, preserve: false);
//VirtualRenderTarget buffer = VirtualContent.CreateRenderTarget("mountain-a", 1920, 1080, depth: true, preserve: false);
//Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

//set the texture
/*
Engine.Graphics.GraphicsDevice.Textures[0] = texture.Texture_Safe;
//may be what makes the render order work?
Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
//hopefully self explanitory
Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
//unsure
Engine.Graphics.GraphicsDevice.RasterizerState = new RasterizerState {CullMode = CullMode.CullClockwiseFace, MultiSampleAntiAlias = true};


//I'm curious if this GFX.Mountain is necessarily what to use
//clearly setting its position is doing something
GFX.FxMountain.Parameters["WorldViewProj"].SetValue(posMatrix);
//GFX.FxMountain.Parameters["fog"].SetValue(customFog.TopColor.ToVector3());
GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Single"];

model.Draw(GFX.FxMountain);*/




//float num = (float)Engine.ViewWidth / (float)buffer.Width;
//Console.WriteLine("pancakes for breakfast!");
//Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
//Console.WriteLine("pancakes for brunch!");
//...
//Draw.SpriteBatch.Draw((Texture2D)(RenderTarget2D) buffer, Vector2.Zero, (Rectangle?) buffer.Bounds, Color.White* 1f, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
//Draw.SpriteBatch.Draw((Texture2D)(RenderTarget2D) blurB, Vector2.Zero, (Rectangle?) blurB.Bounds, Color.White, 0f, Vector2.Zero, num* 2f, SpriteEffects.None, 0f);
//Console.WriteLine("pancakes for lunch!"); 
//Draw.SpriteBatch.End();
//Console.WriteLine("pancakes for dinner!");