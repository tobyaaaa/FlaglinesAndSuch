using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using VivHelper.Entities;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/FacingBlock")]
    [Tracked(false)]
    public class FacingBlock : Solid
    {
        private class BoxSide : Entity
        {
            private FacingBlock block;

            private Color color;

            public BoxSide(FacingBlock block, Color color)
            {
                this.block = block;
                this.color = color;
            }

            public override void Render()
            {
                //if (drawBgBase) {
                    Draw.Rect(block.X, block.Y + block.Height - 8f, block.Width, 8 + block.blockHeight, color);
                //}
            }
        }

        public enum FacingActorTypes
        {
            Player,
            Theo,
            Seeker,
            Puffer,
            Input
        }

        private List<FacingBlock> group;
        private bool groupLeader;
        private Vector2 groupOrigin;
        private int blockHeight = 2;

        private Wiggler wiggler;//catstare
        private Vector2 wigglerScaler;

        private List<Image> pressed = new List<Image>();
        private List<Image> solid = new List<Image>();
        private List<Image> all = new List<Image>();
        private BoxSide side = null;

        private Player entity;
        public FacingActorTypes ActorType;

        int lastInputFacing = 0;

        bool facingLeft = false;
        bool trapPlayer = false;
        bool doCassetteWobble;
        Color EnabledColor = Calc.HexToColor("00bb00");
        Color DisabledColor = Calc.HexToColor("004400");

        String DisabledSprite;
        String EnabledSprite;
        bool drawBgBase = true;

        //int debug;

        public FacingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            Collidable = false;//make waterfalls pass through
            SurfaceSoundIndex = data.Int("sound_index");
            facingLeft = data.Bool("left");
            trapPlayer = data.Bool("trap_player");
            doCassetteWobble = data.Bool("do_cassette_wobble");
            DisabledColor = Calc.HexToColor(data.Attr("disabled_color"));
            EnabledColor = Calc.HexToColor(data.Attr("enabled_color"));
            DisabledSprite = data.Attr("disabled_sprites");
            EnabledSprite = data.Attr("enabled_sprite");
            ActorType = data.Enum("actor_type", FacingActorTypes.Player);
            drawBgBase = data.Has("draw_bottom") ? data.Bool("draw_bottom") : true;
        }

        private bool QueryActors<T>(Func<T, bool> result) where T : Entity
            => Scene.Tracker.GetEntities<T>().Any(entity => result(entity as T));

        private bool IsActorFacing()
        {
            switch (ActorType)
            {
                case FacingActorTypes.Player:
                    return QueryActors<Player>(player => player.Facing == (facingLeft ? Facings.Left : Facings.Right));
                case FacingActorTypes.Seeker:
                    {
                        return QueryActors<Seeker>(seeker => new DynData<Seeker>(seeker).Get<int>("facing") == (facingLeft ? -1 : 1))
                            /*|| (FlushelineModule.VivHelperLoaded && CheckCustomSeeker())*/;
                    }
                case FacingActorTypes.Theo:
                    {
                        return QueryActors<TheoCrystal>(theo => new DynData<TheoCrystal>(theo).Get<Facings>("facing") == (facingLeft ? Facings.Left : Facings.Right))
                            /*|| QueryActors<FlagCrystal>(theo => theo.Facing == (facingLeft ? Facings.Left : Facings.Right))*/;
                    }
                /*case FacingActorTypes.Puffer:
                    {
                        return QueryActors<CustomPuffer>(puffer => puffer.State != CustomPuffer.States.Gone && (facingLeft ? puffer.Facing.X < 0 : puffer.Facing.X > 0))
                            || QueryActors<Puffer>(puffer => puffer.Collidable && new DynData<Puffer>(puffer).Get<Vector2>("facing") == (facingLeft ? new Vector2(-1f, 1f) : Vector2.One));
                    }*/
                case FacingActorTypes.Input:
                    {
                        if (Input.MoveX.Value > 0)
                        {
                            lastInputFacing = 1;
                            return facingLeft;
                        }
                        else if (Input.MoveX.Value < 0)
                        {
                            lastInputFacing = -1;
                            return !facingLeft;
                        }
                        else {
                            if (lastInputFacing == 0)
                            {
                                return QueryActors<Player>(player => player.Facing == (facingLeft ? Facings.Left : Facings.Right));
                            }
                            return lastInputFacing == -1 ? !facingLeft : facingLeft;
                        }
                    }
                default:
                    return false;
            }
        }

        //private bool CheckCustomSeeker()
        //{
        //    return QueryActors<CustomSeeker>(seeker => new DynData<CustomSeeker>(seeker).Get<int>("facing") == (facingLeft ? -1 : 1));
        //}

        public override void Added(Scene scene)
        {
            base.Added(scene);
            entity = base.Scene.Tracker.GetEntity<Player>();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (drawBgBase) { 
                scene.Add(side = new BoxSide(this, DisabledColor));
            }
            foreach (StaticMover staticMover in staticMovers)
            {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null)
                {
                    spikes.EnabledColor = EnabledColor;
                    spikes.DisabledColor = DisabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(EnabledColor);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null)
                {
                    spring.DisabledColor = DisabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }

            if (group == null)
            {
                groupLeader = true;
                //Console.WriteLine("pancakes for dinner" + debug);//DEBUG
                group = new List<FacingBlock>();
                group.Add(this);
                FindInGroup(this);
                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                foreach (FacingBlock item in group)
                {
                    if (item.Left < num)
                    {
                        num = item.Left;
                    }
                    if (item.Right > num2)
                    {
                        num2 = item.Right;
                    }
                    if (item.Bottom > num4)
                    {
                        num4 = item.Bottom;
                    }
                    if (item.Top < num3)
                    {
                        num3 = item.Top;
                    }
                }
                groupOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);
                wigglerScaler = new Vector2(Calc.ClampedMap(num2 - num, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(num4 - num3, 32f, 96f, 1f, 0.2f));
                Add(wiggler = Wiggler.Create(0.3f, 3f));
                foreach (FacingBlock item2 in group)
                {
                    item2.wiggler = wiggler;
                    item2.wigglerScaler = wigglerScaler;
                    item2.groupOrigin = groupOrigin;
                }
            }
            foreach (StaticMover staticMover2 in staticMovers)
            {
                (staticMover2.Entity as Spikes)?.SetOrigins(groupOrigin);
            }
            for (float num5 = base.Left; num5 < base.Right; num5 += 8f)
            {
                for (float num6 = base.Top; num6 < base.Bottom; num6 += 8f)
                {
                    bool flag = CheckForSame(num5 - 8f, num6);
                    bool flag2 = CheckForSame(num5 + 8f, num6);
                    bool flag3 = CheckForSame(num5, num6 - 8f);
                    bool flag4 = CheckForSame(num5, num6 + 8f);
                    if (flag && flag2 && flag3 && flag4)
                    {
                        if (!CheckForSame(num5 + 8f, num6 - 8f))
                        {
                            SetImage(num5, num6, 3, 0);
                        }
                        else if (!CheckForSame(num5 - 8f, num6 - 8f))
                        {
                            SetImage(num5, num6, 3, 1);
                        }
                        else if (!CheckForSame(num5 + 8f, num6 + 8f))
                        {
                            SetImage(num5, num6, 3, 2);
                        }
                        else if (!CheckForSame(num5 - 8f, num6 + 8f))
                        {
                            SetImage(num5, num6, 3, 3);
                        }
                        else
                        {
                            SetImage(num5, num6, 1, 1);
                        }
                    }
                    else if (flag && flag2 && !flag3 && flag4)
                    {
                        SetImage(num5, num6, 1, 0);
                    }
                    else if (flag && flag2 && flag3 && !flag4)
                    {
                        SetImage(num5, num6, 1, 2);
                    }
                    else if (flag && !flag2 && flag3 && flag4)
                    {
                        SetImage(num5, num6, 2, 1);
                    }
                    else if (!flag && flag2 && flag3 && flag4)
                    {
                        SetImage(num5, num6, 0, 1);
                    }
                    else if (flag && !flag2 && !flag3 && flag4)
                    {
                        SetImage(num5, num6, 2, 0);
                    }
                    else if (!flag && flag2 && !flag3 && flag4)
                    {
                        SetImage(num5, num6, 0, 0);
                    }
                    else if (flag && !flag2 && flag3 && !flag4)
                    {
                        SetImage(num5, num6, 2, 2);
                    }
                    else if (!flag && flag2 && flag3 && !flag4)
                    {
                        SetImage(num5, num6, 0, 2);
                    }
                }
            }
            UpdateVisualState();

        }

        public override void Update()
        {
            base.Update();
            if (entity == null)
            {
                entity = base.Scene.Tracker.GetEntity<Player>();
            }

            //Collidable = isFacingCorrectly;
            if (groupLeader)
            {
                bool BlockedFlag = false;
                if (IsActorFacing())
                {
                    if (!trapPlayer)
                    {
                        BlockedFlag = BlockedCheck();
                    }

                    if (BlockedFlag)
                    {
                        foreach (FacingBlock item in group)
                        {
                            item.Collidable = false;
                            item.DisableStaticMovers();
                        }
                    }
                    else
                    {
                        foreach (FacingBlock item in group)
                        {

                            if (!Collidable && doCassetteWobble)
                            {
                                wiggler.Start();
                            }
                            item.Collidable = true;
                            item.EnableStaticMovers();
                        }

                    }
                }
                else
                {
                    foreach (FacingBlock item in group)
                    {
                        item.Collidable = false;
                        item.DisableStaticMovers();
                    }
                }

            }
            UpdateVisualState();
        }

        /*public override void Render()
        {
            base.Render();
            if (IsEnabled)
            {
                Draw.Rect(base.Collider, Color.Blue);
            }
            else
            {
                Draw.Rect(base.Collider, Color.Green);
            }
        }*/

        private void FindInGroup(FacingBlock block)
        {
            foreach (FacingBlock entity in base.Scene.Tracker.GetEntities<FacingBlock>())
            {
                if (entity != this && entity != block && entity.facingLeft == facingLeft && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
                {
                    group.Add(entity);
                    FindInGroup(entity);
                    entity.group = group;
                }
            }
        }

        public bool BlockedCheck()//returns true if blocked
        {
            var wasCollidable = new Dictionary<FacingBlock, bool>();
            var wiggledActors = new List<Actor>();

            foreach (var block in group)
            {
                foreach (Actor actor in block.CollideAll<Actor>())
                {
                    if ((actor.Get<Holdable>() == null || !actor.Get<Holdable>().IsHeld) && !wiggledActors.Contains(actor))
                        wiggledActors.Add(actor);
                }
                wasCollidable[block] = block.Collidable;
                block.Collidable = true;
            }

            var toWiggle = new List<Tuple<Actor, int>>();
            var success = true;

            foreach (var actor in wiggledActors)
            {
                var wiggled = false;
                for (int i = 1; i <= 4; i++)
                {
                    if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
                    {
                        toWiggle.Add(Tuple.Create(actor, i));
                        wiggled = true;
                        break;
                    }
                }
                success = success && wiggled;
                if (!wiggled)
                    break;
            }

            foreach (var block in group)
            {
                block.Collidable = wasCollidable[block];
            }

            if (success)
            {
                foreach (var pair in toWiggle)
                {
                    pair.Item1.Position -= Vector2.UnitY * pair.Item2;
                }
                return false;
            }
            else
            {
                return true;
            }
        }



        private void SetImage(float x, float y, int tx, int ty)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(DisabledSprite);
            pressed.Add(CreateImage(x, y, tx, ty, atlasSubtextures[facingLeft ? 1 : 0]));
            solid.Add(CreateImage(x, y, tx, ty, GFX.Game[EnabledSprite]));
        }

        private Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
        {
            Vector2 value = new Vector2(x - base.X, y - base.Y);
            Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
            Vector2 vector = groupOrigin - Position;
            image.Origin = vector - value;
            image.Position = vector;
            image.Color = EnabledColor;
            Add(image);
            all.Add(image);
            return image;
        }

        private bool CheckForSame(float x, float y)
        {
            foreach (FacingBlock entity in base.Scene.Tracker.GetEntities<FacingBlock>())
            {
                if (entity.facingLeft == facingLeft && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateVisualState()
        {
            if (!Collidable)
            {
                base.Depth = 8990;
            }
            else
            {
                //Player entity = base.Scene.Tracker.GetEntity<Player>();
                //if (entity != null && entity.Top >= base.Bottom - 1f)
                //{
                //    base.Depth = 10;
                //}
                //else
                //{
                    base.Depth = -10;
                //}
            }
            foreach (StaticMover staticMover in staticMovers)
            {
                staticMover.Entity.Depth = base.Depth + 1;
            }
            if (side != null) {
                side.Depth = base.Depth + 5;
                side.Visible = (blockHeight > 0);
            }
            //occluder.Visible = Collidable;
            foreach (Image item in solid)
            {
                item.Visible = Collidable;
            }
            foreach (Image item2 in pressed)
            {
                item2.Visible = !Collidable;
            }
            if (groupLeader)
            {
                Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
                foreach (FacingBlock item3 in group)
                {
                    foreach (Image item4 in item3.all)
                    {
                        item4.Scale = scale;
                    }
                    foreach (StaticMover staticMover2 in item3.staticMovers)
                    {
                        Spikes spikes = staticMover2.Entity as Spikes;
                        if (spikes != null)
                        {
                            foreach (Component component in spikes.Components)
                            {
                                Image image = component as Image;
                                if (image != null)
                                {
                                    image.Scale = scale;
                                }
                            }
                        }
                    }
                }
            }
        }


        /*public static void Load()
        {
            On.Celeste.TheoCrystal.ctor_Vector2 += TheoCrystal_ctor_Vector2;
            On.Celeste.TheoCrystal.Update += TheoCrystal_Update;
        }

        public static void UnLoad()
        {
            On.Celeste.TheoCrystal.ctor_Vector2 -= TheoCrystal_ctor_Vector2;
            On.Celeste.TheoCrystal.Update -= TheoCrystal_Update;
        }

        private static void TheoCrystal_ctor_Vector2(On.Celeste.TheoCrystal.orig_ctor_Vector2 orig, TheoCrystal self, Vector2 position)
        {
            orig(self, position);
            new DynData<TheoCrystal>(self).Set("facing", Facings.Left);
        }

        private static void TheoCrystal_Update(On.Celeste.TheoCrystal.orig_Update orig, TheoCrystal self)
        {
            orig(self);
            if (self.Scene.Tracker.GetEntities<FacingBlock>().Any(e => (e as FacingBlock).ActorType == FacingActorTypes.Theo))
            {
                var selfData = new DynData<TheoCrystal>(self);
                var facing = selfData.Get<Facings>("facing");
                if (self.Hold.IsHeld)
                    facing = self.Hold.Holder.Facing;
                if (self.Speed.X > 0f)
                    facing = Facings.Right;
                else if (self.Speed.X < 0f)
                    facing = Facings.Left;
                selfData.Set("facing", facing);

                var sprite = selfData.Get<Sprite>("sprite");
                sprite.Scale.X = Math.Abs(sprite.Scale.X) * (int)facing;
            }
        }*/
    }
}