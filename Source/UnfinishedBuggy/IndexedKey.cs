using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/IndexedKey")]
    [Tracked(false)]

    public class IndexedKey : Entity
    {
        public static ParticleType P_Shimmer;
        public static ParticleType P_Insert;
        public static ParticleType P_Collect;

        public EntityID ID;
        public bool IsUsed;
        public bool StartedUsing;
        private Follower follower;
        private Sprite sprite;
        private Wiggler wiggler;
        private VertexLight light;
        private ParticleEmitter shimmerParticles;
        private float wobble;
        private bool wobbleActive;
        private Tween tween;
        private Alarm alarm;
        private Vector2[] nodes;
        public bool Turning;

        public int keyIndex;
        public string spritepath;
        public string overrideSpritebank;

        public struct persistencyVars {
            public EntityID id;
            public int keyIndex;
            public String spritepath;
            public String overrideSpritebank;
        }
        public persistencyVars makePersistentVars(IndexedKey ik)
        {
            persistencyVars pvars = new persistencyVars();
            pvars.id = ik.ID;
            pvars.keyIndex = ik.keyIndex;
            pvars.spritepath = ik.spritepath;
            pvars.overrideSpritebank = ik.overrideSpritebank;
            return pvars;
        }


        public IndexedKey(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {

            ID = id;
            base.Collider = new Hitbox(12f, 12f, -6f, -6f);
            this.nodes = data.Nodes;
            Add(follower = new Follower(id));
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());

            spritepath = data.Attr("spritepath");
            overrideSpritebank = data.Attr("OverrideSpritepath");

            if (overrideSpritebank != "")
            {
                Add(sprite = GFX.SpriteBank.Create(overrideSpritebank));
            }
            else
            {
                Add(sprite = GFX.SpriteBank.Create("Key"));
            }


            Add(new TransitionListener
            {
                OnOut = delegate
                {
                    StartedUsing = false;
                    if (!IsUsed)
                    {
                        if (tween != null)
                        {
                            tween.RemoveSelf();
                            tween = null;
                        }
                        if (alarm != null)
                        {
                            alarm.RemoveSelf();
                            alarm = null;
                        }
                        Turning = false;
                        Visible = true;
                        sprite.Visible = true;
                        sprite.Rate = 1f;
                        sprite.Scale = Vector2.One;
                        sprite.Play("idle");
                        sprite.Rotation = 0f;
                        wiggler.Stop();
                        follower.MoveTowardsLeader = true;
                    }
                }

            });
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(light = new VertexLight(Color.White, 1f, 32, 48));
        }

        private void OnPlayer(Player player)
        {
            SceneAs<Level>().Particles.Emit(P_Collect, 10, Position, Vector2.One * 3f);
            Audio.Play("event:/game/general/key_get", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            player.Leader.GainFollower(follower);
            Collidable = false;
            Session session = SceneAs<Level>().Session;
            session.DoNotLoad.Add(ID);
            //session.Keys.Add(ID);
            Class1.Session.IndexedKeys.Add(makePersistentVars(this));
            //session.UpdateLevelStartDashes();
            wiggler.Start();
            base.Depth = -1000000;
            if (nodes != null && nodes.Length >= 2)
            {
                //Add(new Coroutine(NodeRoutine(player)));
            }
        }


    }
}
