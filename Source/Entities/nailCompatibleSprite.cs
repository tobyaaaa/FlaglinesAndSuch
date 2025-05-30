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
    [CustomEntity("FlaglinesAndSuch/NailHittableSprite")]
    [Tracked(false)]
    class nailCompatibleSprite : Entity
    {
        private Sprite sprite;
        private string hitSound;

        public bool Breakable;
        public bool DoMomentum;
        public bool Refill;

        public string tileType;
        public bool TopLeft;

        public bool no_nail_kelper = false;

        public nailCompatibleSprite(EntityData data, Vector2 offset) : base(data.Position + offset) {
            hitSound = data.Attr("HitSound");
            Breakable = data.Bool("Breakable");
            DoMomentum = data.Bool("DoMomentum");
            Refill = data.Bool("RefillDash");
            if (data.Has("spriteTopLeft"))
            {
                TopLeft = data.Bool("spriteTopLeft");
            }
            else {
                TopLeft = true;
            }

            tileType = data.Attr("DebrisTileID");

            int colliderWidth = data.Width;
            int colliderHeight = data.Height;

            base.Collider = new Hitbox(colliderWidth, colliderHeight);

            Add(sprite = GFX.SpriteBank.Create(data.Attr("SpritesXMLEntry")));
            if (!TopLeft)
            {
                sprite.CenterOrigin(); 
                sprite.Position += new Vector2(colliderWidth / 2, colliderHeight / 2);
            }
            sprite.Play("idle", restart: true);

            if (!no_nail_kelper)
            {
                Component kelperCollider = KelperImports.CreateNailCollider(kelper_nail_wrapper);
                if (kelperCollider != null) { Add(kelperCollider); }
            }
        }

        public void OnNail(Vector2 nailDirVec)
        {
            if (hitSound != "")
            {
                Audio.Play(hitSound, Position);
            }
            if (Breakable) {
                Collidable = false;
                sprite.Play("break");

                //TODO: base.width and base.height are meaningless here, I believe
                if (tileType != "") { 
                    for (int i = 0; (float)i < base.Width / 8f; i++)
                    {
                        for (int j = 0; (float)j < base.Height / 8f; j++)
                        {
                            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), char.Parse(tileType), false).BlastFrom(nailDirVec));
                        }
                    }
                }
            }
        }


        public bool kelper_nail_wrapper(Player player, Vector2 nailDir)
        {
            if (!Collidable)
            {
                return false;
            }
            if (DoMomentum) {
                KelperImports.ConsumeNailSwing();
                KelperImports.ApplyNailRebound(1);
            }
            if (Refill)
            {
                player.RefillDash();
                player.RefillStamina();
            }
            OnNail(nailDir);
            return true;
        }

    }
}
