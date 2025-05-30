using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/parallaxTileEntity")]
    class parallaxTileEntity : Solid
    {
        private char tileType;

        private float width;

        private float height;

        private bool blendIn;

		float scroll;
        //private Player playerentity;
        //private Vector2 lastposition;
        private Vector2 RealPosition;
		Vector2 PosOffset;

        public parallaxTileEntity(Vector2 position, char tiletype, float width, float height, float Scroll, int offX, int offY)
		: base(position, width, height, safe: true)
		{
			base.Depth = -12999;
			this.width = width;
			this.height = height;
			this.blendIn = false;
			tileType = tiletype;
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
			scroll = Scroll;
			RealPosition = position;
            PosOffset = new Vector2(offX, offY);
        }

		public parallaxTileEntity(EntityData data, Vector2 offset, EntityID id)
			: this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Float("scroll"), data.Int("offsetX"), data.Int("offsetY"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			//playerentity = base.Scene.Tracker.GetEntity<Player>();
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			TileGrid tileGrid;
			if (!blendIn)
			{
				tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)width / 8, (int)height / 8).TileGrid;
				Add(new LightOcclude());
			}
			else
			{
				Level level = SceneAs<Level>();
				Rectangle tileBounds = level.Session.MapData.TileBounds;
				VirtualMap<char> solidsData = level.SolidsData;
				int x = (int)(base.X / 8f) - tileBounds.Left;
				int y = (int)(base.Y / 8f) - tileBounds.Top;
				int tilesX = (int)base.Width / 8;
				int tilesY = (int)base.Height / 8;
				tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
				Add(new EffectCutout());
				base.Depth = -10501;
			}
			Add(tileGrid);
			Add(new TileInterceptor(tileGrid, highPriority: true));
			if (CollideCheck<Player>())
			{
				RemoveSelf();
			}

			//add something here to offset depending on player position

		}
		public override void Update() {
			base.Update();
            Level level = base.Scene as Level;
			//if (playerentity == null)
			//{
			//	playerentity = base.Scene.Tracker.GetEntity<Player>();
			//	lastposition = playerentity.Position;
			//}

			//Vector2 PosDiff = playerentity.Position - lastposition;



			Vector2 RenderSpot = RealPosition - ((level.Camera.Position - PosOffset) * (scroll - 1)) ;// + Offset; also do 1-scroll
            //Vector2 RenderSpot = new Vector2(RealPosition.X * scroll + (160f * (1 - scroll)), RealPosition.Y * scroll + (90f * (1 - scroll)));
            //RenderSpot = new Vector2(RenderSpot.X - level.Camera.Position.X * scroll, RenderSpot.Y - level.Camera.Position.Y * scroll);
			//position = RenderSpot;
			MoveTo(RenderSpot);

            //Position += PosDiff * scroll;
            //lastposition = playerentity.Position;
        }

	}
}
