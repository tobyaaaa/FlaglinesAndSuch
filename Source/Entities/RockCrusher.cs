using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Iced.Intel;
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

    [CustomEntity("FlaglinesAndSuch/RockCrusher")]
    class RockCrusher : Entity {

        private Sprite sprite;
        private Vector2 shake;
        private SoundSource shakingSfx;
        private MTexture rockTexture;

        public RockCrusher(EntityData data, Vector2 offset) : base(data.Position + offset) {

            base.Collider = new Circle(10f);
            Add(new PlayerCollider(OnPlayer));
            Add(shakingSfx = new SoundSource());
            rockTexture = GFX.Game["objects/FlaglinesAndSuch/RockCrusher/rock"];
        }
        public override void Render()
        {
            rockTexture.DrawCentered(Position);
            //Draw.Circle(Position + shake, 12f, Color.Orange, 10);
            base.Render();
        }
        private void OnPlayer(Player hitplayer)
        {
            Vector2 vector = (hitplayer.Center - base.Center).SafeNormalize();
            hitplayer.Die(vector);
        }




        private IEnumerator Sequence()
        {
            Player entity;
            do
            {
                yield return null;
                entity = Scene.Tracker.GetEntity<Player>();
                //TODO SLIGHT SHAKING AND PARTICLES
            }
            while (entity == null ||
            !(entity.Right >= X - 8f) || //not player's in left bound
            !(entity.Left <= X + 8f) || //not player's in right bound
            !(entity.Y <= Y + 160f) || //not player 20tiles below
            !(entity.Y >= Y)); //not player above


            //shake in place
            shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");
            float time2 = 0.4f;
            Shaker shaker = new Shaker(time2, removeOnFinish: true, delegate (Vector2 v)
            {
                shake = v;
            });
            Add(shaker);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            //countdown for above
            while (time2 > 0f)
            {
                yield return null;
                time2 -= Engine.DeltaTime;
            }
            //finish shaking and fall
            //TODO PARTICLES
            shakingSfx.Param("release", 1f);
            do
            {
                yield return null;
            }
            while (!MoveDownCollide(6));
            //hit ground
            Audio.Play("event:/game/00_prologue/fallblock_first_impact", Position);
            //TODO PARTICLES
            SceneAs<Level>().Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);


            Audio.Play("event:/game/general/wall_break_stone", Position);
            //TODO all goes in same direction
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position, '6', true).BlastFrom(-Vector2.UnitY));
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitX * 8), '6', true).BlastFrom(-Vector2.UnitY));
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitX * 16), '6', true).BlastFrom(-Vector2.UnitY));
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitX * -8), '6', true).BlastFrom(-Vector2.UnitY));
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitX * -16), '6', true).BlastFrom(-Vector2.UnitY));

            RemoveSelf();
        }


        private bool MoveDownCollide(int dist) //TODO also collide room transitions, but play a different sound
        {
            Platform platform = null;

            while (dist != 0)
            {
                Position.Y += 1;

                foreach (DashBlock dblock in base.Scene.Tracker.GetEntities<DashBlock>())
                {
                    if (CollideCheck(dblock, Position + Vector2.UnitY))
                    {
                        dblock.Break(base.Center, Vector2.UnitY, true, true);
                        SceneAs<Level>().Shake(0.2f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        //break this entity
                        return true;
                    }
                }

                platform = CollideFirst<Solid>(Position + Vector2.UnitY);
                if (platform != null)
                {
                    return true;
                }
                if (dist > 0)
                {
                    platform = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY);
                    if (platform != null)
                    {
                        return true;
                    }
                }

                dist--;
            }

            return false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            //if (SceneAs<Level>().Session.GetLevelFlag("1") || SceneAs<Level>().Session.GetLevelFlag("0b"))
            //{
            //Position = end; //already triggered
            //}
            //else
            //{
            Add(new Coroutine(Sequence()));
            //}
        }
    }
}



