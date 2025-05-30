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
    [CustomEntity("FlaglinesAndSuch/Wingmould")]
    [Tracked(false)]
    class Wingmould : Entity
    {

        public Boolean hitboxEnabled = true;
        private Sprite sprite;
        private VertexLight light;
        private BloomPoint bloom;
        private Wiggler hitWiggler;

        private Vector2 hitDir;
        private float respawnTimer;
        private SineWave sine;


        public bool no_nail_kelper = false;

        public Wingmould(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            base.Collider = new Circle(12f);


            if (data.Has("OverrideSprite") && data.Attr("OverrideSprite") != "")
            {
                Add(sprite = GFX.SpriteBank.Create(data.Attr("OverrideSprite")));
            }
            else
            {
                Add(sprite = Class1.spriteBank.Create("Wingmould"));
            }
            
            
            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Add(bloom = new BloomPoint(0.5f, 16f));
            Add(hitWiggler = Wiggler.Create(1.2f, 1f, delegate{sprite.Position = hitDir * hitWiggler.Value * 9f;}));


            Add(sine = new SineWave(0.5f, 0f).Randomize());


            if (data.Has("no_nail_kuksa"))
            {
                no_nail_kelper = data.Bool("no_nail_kuksa");
            }
            else
            {
                no_nail_kelper = true;
            }

            if (!no_nail_kelper)
            {
                Component kelperCollider = KelperImports.CreateNailCollider(kelper_nail_wrapper);
                if (kelperCollider != null) { Add(kelperCollider); }
            }
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    light.Visible = true;
                    bloom.Visible = true;
                    hitboxEnabled = true;
                    sprite.Play("reform");
                    Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);

                }
            }
            if (!hitWiggler.Active)
            {
                sprite.Position = new Vector2(sine.Value * 2f, sine.ValueOverTwo * 1f);
            }
        }
        private void OnPlayer(Player hitplayer)
        {
            if (!SaveData.Instance.Assists.Invincible && hitboxEnabled)
            {
                Vector2 vector = (hitplayer.Center - base.Center).SafeNormalize();
                hitDir = -vector;
                hitWiggler.Start();
                Audio.Play("event:/game/09_core/hotpinball_activate", Position);
                hitplayer.Die(vector);
                //SceneAs<Level>().Particles.Emit(P_FireHit, 12, base.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
            }
        }

        public void OnNail() {
            Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
            respawnTimer = 1.0f;
            sprite.Play("hit"/*, restart: true*/);
		    light.Visible = false;
		    bloom.Visible = false;
            hitboxEnabled = false;
            //SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
        }

        public bool kelper_nail_wrapper(Player player, Vector2 nailDir)
        {
            if (!hitboxEnabled) {
                return false;
            }
            KelperImports.ConsumeNailSwing();
            KelperImports.ApplyNailRebound(1);

            player.RefillDash();
            player.RefillStamina();

            OnNail();
            return true;
        }


    }
}
