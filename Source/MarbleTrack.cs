using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Celeste.Mod.FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/MarbleTrack")]
    class MarbleTrack : Entity
    {
        
        List<Vector2> Nodes;
        List<float> Slopes;
        List<Vector2> Normals;

        public MarbleTrack(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Nodes = new List<Vector2>();
            Nodes.Add(data.Position + offset);
            foreach (Vector2 node in data.Nodes)
            {
                Nodes.Add(node + offset);
            }
            //Console.WriteLine("Flaglines and Such track" + data.Nodes.Length);
            //Console.WriteLine("Flaglines and Such track" + Nodes.Count);
            RegisterSlopes();
        }

        public override void Update()
        {
            base.Update();
            foreach (Marble marble in base.Scene.Tracker.GetEntities<Marble>()) {
                for (int i = 0; i < Nodes.Count - 1; i++)
                {
                    //if touching, call OnTrack
                    if (marble.CollideLine(Nodes[i], Nodes[i+1] )) {
                        marble.OnTrack(this, i);
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            for (int i = 0; i < Nodes.Count - 1; i++) {
                Draw.Line(Nodes[i], Nodes[i+1], Color.Gold);
                Draw.Line(Nodes[i], Nodes[i] + (Normals[i] * 8), Color.Green);

            }
        }

        void RegisterSlopes()
        {
            Slopes = new List<float>();
            Normals = new List<Vector2>();
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                if (Nodes[i].X == Nodes[i + 1].X)
                {
                    //vertical track.
                    Slopes.Add(100f);
                    //theoretically I could do math here based on the other segments to figure out what the "correct" normal is
                    //but that can happen later
                    Normals.Add(new Vector2(1, 0));
                }
                else {
                    float rise = Nodes[i + 1].Y - Nodes[i].Y;
                    float run = Nodes[i + 1].X - Nodes[i].X;
                    Slopes.Add( (Nodes[i + 1].Y - Nodes[i].Y) / (Nodes[i + 1].X - Nodes[i].X) );

                    //ternary statement here ensures the normal is always facing upwards
                    Normals.Add((run > 0) ? new Vector2(rise, -run).SafeNormalize() : new Vector2(-rise, run).SafeNormalize() );
                }
            }
        }


        /// <summary>
        /// Get track segment index this position is "above". Doesn't work w/ purely vert. tracks, of course.
        /// Also won't really collide acute angles
        /// </summary>
        /// <param name="pos">position to check</param>
        /// <returns>integer index of segment, -1 if not above track at all</returns>
        public int aboveWhichSegment(Vector2 pos) {
            int ret = -1;
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                if (pos.X > Nodes[i].X && pos.X < Nodes[i+1].X) {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        public bool isColliding(Entity other, int segment) {
            return other.CollideLine(Nodes[segment], Nodes[segment + 1]);
        }

        public Vector2 getNormal(int segment) { 
            return Normals[segment];
        }

    }
}
