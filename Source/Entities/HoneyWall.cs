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

/* Thank you to marshall for letting me base the code for this entity off his attached ice walls */
namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/HoneyWall")]
    [Tracked]
    public class HoneyWall : Entity {
        private readonly StaticMover staticMover = new StaticMover();
        private Vector2 imageOffset;
        private readonly List<Sprite> tiles;
        public Facings Facing;

        //String Spritepath;

        public HoneyWall(Vector2 position, float height, bool left)
            : base(position) {
            Tag = Tags.TransitionUpdate;
            Depth = 1999;
            staticMover.OnAttach = p => Depth = p.Depth - 1;
            staticMover.OnShake = amount => imageOffset += amount;
            staticMover.OnEnable = () => Visible = Collidable = true;
            staticMover.OnDisable = () => Visible = Collidable = true;

            if (left) {
                Collider = new Hitbox(2f, height);
                staticMover.SolidChecker = s => CollideCheck(s, Position - Vector2.UnitX);
                Facing = Facings.Left;
            } else {
                Collider = new Hitbox(2f, height, 6f);
                staticMover.SolidChecker = s => CollideCheck(s, Position + Vector2.UnitX);
                Facing = Facings.Right;
            }
         

            Add(staticMover);

            tiles = BuildSprite(left);
        }

        public HoneyWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, data.Bool("left")) {
        }


        public override void Render() {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }

        private List<Sprite> BuildSprite(bool left) {
            List<Sprite> list = new List<Sprite>();
            for (int i = 0; (float) i < Height; i += 8) {
                string id = i == 0 ? "FlaglinesAndSuch_grabWall_Top" : !(i + 16 > Height) ? "FlaglinesAndSuch_grabWall_Mid" : "FlaglinesAndSuch_grabWall_Bottom";
                Sprite sprite = GFX.SpriteBank.Create(id);//change to the flaglines spritebank!
                if (!left) {
                    sprite.FlipX = true;
                    sprite.Position = new Vector2(5f, i);
                } else {
                    sprite.Position = new Vector2(-1f, i);
                }
                list.Add(sprite);
                Add(sprite);
            }

            return list;
        }

        /*public override void Awake(Scene scene) {
            base.Awake(scene);
            On.Celeste.Player.ClimbUpdate += (orig, self) => {
                if (self.CollideCheck(this, self.Position + Vector2.UnitX * (float) self.Facing)) {
                    self.RefillStamina();
                }

                return orig(self);
            };
        }*/

        public override void Update() {
            if (!SceneAs<Level>().Transitioning) {
                base.Update();
            }
        }
    }
}
