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

        Hitbox player_hitbox;
        Hitbox solids_hitbox;

        string flag;
        string texture = "rock"; //todo make this name better?

        public RockCrusher(EntityData data, Vector2 offset) : base(data.Position + offset) {

            player_hitbox = new Hitbox(20, 16, -10, -8);
            solids_hitbox = new Hitbox(10, 6, -5, 1);

            base.Collider = player_hitbox;
            Add(new PlayerCollider(OnPlayer));
            Add(shakingSfx = new SoundSource());
            rockTexture = GFX.Game["objects/FlaglinesAndSuch/RockCrusher/" + texture];
        }
        public override void Render()
        {
            rockTexture.DrawCentered(Position + shake);
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
            while (shouldActivate(entity));        //not player above


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
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitX * 8), '6', true).BlastFrom(-Vector2.UnitX));
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitY * 8), '6', true).BlastFrom(-Vector2.UnitY));
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitX * -8), '6', true).BlastFrom(Vector2.UnitX));
            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + (Vector2.UnitY * -8), '6', true).BlastFrom(-Vector2.UnitY));

            RemoveSelf();
        }


        private bool MoveDownCollide(int dist) //TODO also collide room transitions, but play a different sound
        {
            base.Collider = solids_hitbox;

            Platform platform = null;

            while (dist != 0)
            {
                Position.Y += 1;
                
                //dash block special case
                foreach (DashBlock dblock in base.Scene.Tracker.GetEntities<DashBlock>())
                {
                    if (CollideCheck(dblock, Position + Vector2.UnitY))
                    {
                        dblock.Break(base.Center, Vector2.UnitY, true, true);
                        SceneAs<Level>().Shake(0.2f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

                        base.Collider = player_hitbox;
                        return true;
                    }
                }
                //generic Solid collision
                platform = CollideFirst<Solid>(Position + Vector2.UnitY);
                if (platform != null)
                {
                    base.Collider = player_hitbox;
                    return true;
                }
                SolidTiles solid = CollideFirst<SolidTiles>(Position + Vector2.UnitY);
                if (solid != null)
                {
                    base.Collider = player_hitbox;
                    return true;
                }
                if (dist > 0)
                {
                    platform = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY);
                    if (platform != null)
                    {
                        base.Collider = player_hitbox;
                        return true;
                    }
                }

                dist--;
            }

            base.Collider = player_hitbox;
            return false;
        }

        /// <summary>
        /// whether the rock should start falling.
        /// due to while loop shenanigans I can't be bothered to refactor this returns false when conditions are met
        /// </summary>
        /// <param name="player">player entity</param>
        /// <returns>RETURNS FALSE WHEN THE ROCK SHOULD FALL !!!!</returns>
        private bool shouldActivate(Entity player) {
            if (flag != "") { return !(Scene as Level).Session.GetFlag(flag); }

            //default bounds checks
            if (player == null ||
            !(player.Right >= X - 8f) || //not player's in left bound
            !(player.Left <= X + 8f) || //not player's in right bound
            !(player.Y <= Y + 160f) || //not player 20tiles below
            !(player.Y >= Y))
            { return true; }
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



