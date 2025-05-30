using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;

namespace FlaglinesAndSuch
{

        [CustomEntity("FlaglinesAndSuch/DustNoShrinkController")]
        public class DustNoShrinkController : Entity
        {
            public DustNoShrinkController(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Class1.Session.DustNoShrink = data.Bool("enable", true);
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                RemoveSelf();

            }


            public static void Load()
            {
                On.Celeste.DustGraphic.ctor += DustGraphic_ctor;
            }

            public static void UnLoad()
            {
                On.Celeste.DustGraphic.ctor -= DustGraphic_ctor;
            }

            private static void DustGraphic_ctor(On.Celeste.DustGraphic.orig_ctor orig, DustGraphic self, bool ignoreSolids, bool autoControlEyes, bool autoExpandDust)
            {
                orig(self, ignoreSolids, autoControlEyes, autoExpandDust);
                if (Class1.Session.DustNoShrink)
                    new DynData<DustGraphic>(self).Set("ignoreSolids", true);
            }
        }

}
