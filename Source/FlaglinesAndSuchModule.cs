using System;
using System.Linq;
using Microsoft.Xna.Framework;


using Monocle;
using MonoMod.Utils;

using Celeste;
using Celeste.Mod;


namespace FlaglinesAndSuch {
    public class Class1 : EverestModule {
        public static Class1 Instance { get; private set; }

        public override Type SettingsType => typeof(FlaglinesAndSuchModuleSettings);
        public static FlaglinesAndSuchModuleSettings Settings => (FlaglinesAndSuchModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(FlaglinesAndSuchModuleSession);
        public static FlaglinesAndSuchModuleSession Session => (FlaglinesAndSuchModuleSession) Instance._Session;

        public override Type SaveDataType => typeof(FlaglinesAndSuchModuleSaveData);
        public static FlaglinesAndSuchModuleSaveData SaveData => (FlaglinesAndSuchModuleSaveData) Instance._SaveData;

        public Class1() {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(Class1), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(FlaglinesAndSuchModule), LogLevel.Info);
#endif
        }


        public static SpriteBank spriteBank;

        public override void Load()
        {
            HKnail.Load();
            RocketBarrel.Load();
            StandBox.Load();
            Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
            DustNoShrinkController.Load();


            //wicker blocks
            //disabled until further notice, as I'm not working on these
            //On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
            //On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
            //On.Celeste.Actor.OnGround_int += Actor_OnGround_int;
            //On.Celeste.Platform.MoveHExactCollideSolids += Platform_MoveHExactCollideSolids;
            //On.Celeste.Platform.MoveVExactCollideSolids += Platform_MoveVExactCollideSolids;
            //On.Celeste.Solid.HasPlayerRider += Solid_HasPlayerRider; BROKEN!!!!!!!!!!!!!!!!!!!!!!
            WickerBlock.Load();

            //honey wall
            On.Celeste.Player.ClimbUpdate += Player_ClimbUpdate;
            On.Celeste.Player.ClimbJump += Player_ClimbJump;

            //facing block
            On.Celeste.TheoCrystal.ctor_Vector2 += TheoCrystal_ctor_Vector2;
            On.Celeste.TheoCrystal.Update += TheoCrystal_Update;

            //Mini Touch Switch
            Everest.Events.Player.OnDie += MiniTouchSwitch.PlayerDiesCaller;
            Everest.Events.Level.OnLoadLevel += MiniTouchSwitch.LoadLevelMethod;
            Everest.Events.Level.OnTransitionTo += MiniTouchSwitch.RoomChangeMethod;
            On.Celeste.Player.DashBegin += MTSDashBegin;
        }

        public override void Unload() {
            HKnail.UnLoad();
            RocketBarrel.UnLoad();
            StandBox.UnLoad();
            DustNoShrinkController.UnLoad();
            WickerBlock.UnLoad();

            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;

            On.Celeste.Player.ClimbUpdate -= Player_ClimbUpdate;
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;

            On.Celeste.TheoCrystal.ctor_Vector2 -= TheoCrystal_ctor_Vector2;
            On.Celeste.TheoCrystal.Update -= TheoCrystal_Update;
            Everest.Events.Player.OnDie -= MiniTouchSwitch.PlayerDiesCaller;
            Everest.Events.Level.OnLoadLevel -= MiniTouchSwitch.LoadLevelMethod;
            Everest.Events.Level.OnTransitionTo -= MiniTouchSwitch.RoomChangeMethod;
            On.Celeste.Player.DashBegin -= MTSDashBegin;
        }

        public override void LoadContent(bool firstLoad)
        {

            //kelper nail compatibility
            //here to attempt fix dependency circle
            KelperImports.Load();


            spriteBank = new SpriteBank(GFX.Game, "Graphics/FlaglinesAndSuch/CustomSprites.xml");
        }




        //facing block hooks

        private static void TheoCrystal_ctor_Vector2(On.Celeste.TheoCrystal.orig_ctor_Vector2 orig, TheoCrystal self, Vector2 position)
        {
            orig(self, position);
            new DynData<TheoCrystal>(self).Set("facing", Facings.Left);
        }

        private static void TheoCrystal_Update(On.Celeste.TheoCrystal.orig_Update orig, TheoCrystal self)
        {
            orig(self);
            if (self.Scene.Tracker.GetEntities<FacingBlock>().Any(e => (e as FacingBlock).ActorType == FacingBlock.FacingActorTypes.Theo))
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
        }




        //Honey wall hooks
        private void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            if (!ClimbBlocker.Check(self.Scene, self, self.Position + Vector2.UnitX * (float)self.Facing))
            {
                foreach (HoneyWall Hwall in self.Scene.Tracker.GetEntities<HoneyWall>())
                {
                    if (Hwall.Facing == self.Facing && self.CollideCheck(Hwall))
                    {
                        self.RefillStamina();
                    }
                }
            }
            orig(self);
        }

        private int Player_ClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self)
        {
            if (ClimbBlocker.Check(self.Scene, self, self.Position + Vector2.UnitX * (float)self.Facing))
                return orig(self);

            foreach (HoneyWall Hwall in self.Scene.Tracker.GetEntities<HoneyWall>())
            {
                if (Hwall.Facing == self.Facing && self.CollideCheck(Hwall))
                {
                    self.RefillStamina();
                }
            }
            return orig(self);
        }


        private void MTSDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {

            for (int i = 0; i < self.Leader.Followers.Count; i++)
            {
                MiniTouchSwitch mts = self.Leader.Followers[i].Entity as MiniTouchSwitch;
                if (mts != null)
                {
                    mts.mtsUsed();
                    break;
                }
            }
            orig(self);
        }



        //styleground effect hooks
        private Backdrop Level_OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
        {

            Backdrop result;
            switch (child.Name)
            {
                case "FlaglinesAndSuch/customDreamStars":
                    result = new customDreamStars(child.AttrInt("count", 50), child.AttrFloat("minSpeed", 24), child.AttrFloat("maxSpeed", 48), child.AttrFloat("minSize", 2), child.AttrFloat("maxSize", 8), child.Attr("Color", "008080"), child.AttrInt("AngleX", -2), child.AttrInt("AngleY", -7), child.AttrFloat("Scroll"), child.Attr("shape"));
                    return result;
                case "FlaglinesAndSuch/customGodrays":
                    result = new CustomGodrays(child.AttrInt("min_width"), child.AttrInt("max_width"), child.AttrInt("min_length"), child.AttrInt("max_length"), child.AttrFloat("Duration_base"), child.AttrFloat("Duration_variance"), child.AttrFloat("ray_color_alpha"), child.Attr("ray_color", "f52b63"), child.Attr("fade_to_color", ""), child.AttrFloat("scroll_x"), child.AttrFloat("scroll_y"), child.AttrFloat("speed_x"), child.AttrFloat("speed_y"), child.AttrInt("ray_count"), child.AttrFloat("angle_x"), child.AttrFloat("angle_y"));
                    return result;
                case "FlaglinesAndSuch/customBossField":
                    result = new CustomBossField(child.Attr("bg_color"), child.AttrFloat("alpha"), child.AttrInt("count"), child.Attr("particle_colors"), child.AttrFloat("speed_min"), child.AttrFloat("speed_max"), child.AttrInt("pos_x_min"), child.AttrInt("pos_x_max"), child.AttrInt("pos_y_min"), child.AttrInt("pos_y_max"), child.AttrFloat("dir_x"), child.AttrFloat("dir_y"), child.AttrFloat("dir_x_alt"), child.AttrFloat("dir_y_alt"), child.AttrFloat("scroll_x"), child.AttrFloat("scroll_y"), child.AttrFloat("stretch_multiplier"));
                    return result;
                case "FlaglinesAndSuch/SummitBoostBackground":
                    result = new SummitBoostBackground(child.AttrBool("draw_background"), child.AttrBool("draw_bars"), child.AttrBool("draw_streaks"), child.AttrBool("draw_clouds"),/*child.AttrFloat("Angle"),*/0f, child.Attr("background_color"), child.Attr("bar_color"), child.AttrInt("streak_count"), child.AttrFloat("streak_speed_min"), child.AttrFloat("streak_speed_max"), child.Attr("streak_colors"), child.AttrFloat("streak_alpha"), child.AttrInt("cloud_count"), child.AttrFloat("cloud_speed_min"), child.AttrFloat("cloud_speed_max"), child.Attr("cloud_color"), child.AttrFloat("cloud_alpha"));
                    return result;
                case "FlaglinesAndSuch/VaporwaveGrid":
                    result = new VaporwaveGrid(child.Attr("color"), child.AttrInt("Horizontal_lines"), child.AttrInt("Vertical_lines"), child.AttrFloat("Top_Scroll"), child.AttrFloat("Bottom_Scroll"), child.AttrInt("Top_Height"), child.AttrInt("view_X"), child.AttrInt("view_Y"), child.AttrBool("Flip_Y"), child.AttrFloat("Speed_y"));
                    return result;
                case "FlaglinesAndSuch/SineParallaxStyleground":
                    if (child.HasAttr("Alpha"))
                    {
                        result = new SineParallaxStyleground(child.Attr("Texture"), child.AttrFloat("posX"), child.AttrFloat("posY"), child.AttrFloat("speedX"), child.AttrFloat("speedY"), child.AttrFloat("scrollX"), child.AttrFloat("scrollY"), child.AttrBool("loopX"), child.AttrBool("loopY"), child.AttrFloat("amplitude"), child.AttrFloat("frequency"), child.AttrFloat("offset"), child.AttrBool("SineVertically"), child.AttrFloat("Alpha"), child.Attr("BlendMode"), child.Attr("Color"), child.AttrBool("InstantIn"), child.AttrBool("InstantOut"), child.AttrBool("FlipX"), child.AttrBool("FlipY"));
                    }
                    else
                    {
                        result = new SineParallaxStyleground(child.Attr("Texture"), child.AttrFloat("posX"), child.AttrFloat("posY"), child.AttrFloat("speedX"), child.AttrFloat("speedY"), child.AttrFloat("scrollX"), child.AttrFloat("scrollY"), child.AttrBool("loopX"), child.AttrBool("loopY"), child.AttrFloat("amplitude"), child.AttrFloat("frequency"), child.AttrFloat("offset"), child.AttrBool("SineVertically"));
                    }
                    return result;

                case "FlaglinesAndSuch/GeometricBlackhole":
                    result = new GeometricBlackhole(child.AttrInt("shape_count"), child.AttrFloat("speed"), child.Attr("Texture"), child.Attr("InnerColor"), child.Attr("OuterColor"), child.Attr("OuterColor2"), child.AttrFloat("swirlMin"), child.AttrFloat("swirlMax"), child.AttrFloat("swirlMinNew"), child.AttrFloat("swirlMaxNew"), child.AttrFloat("rotationDifference"), child.AttrBool("reverse"));
                    return result;
                case "FlaglinesAndSuch/PolygonEffect":
                    if (!child.HasAttr("colors"))
                    {   //should be able to remove this later once my debug stuff doesn't matter
                        result = new polygonEffect(child.Attr("positions"), child.Attr("color"), child.AttrBool("VisibleSide"), 0, 0);
                    }
                    else
                    {
                        result = new polygonEffect(child.Attr("positions"), child.Attr("colors"), child.AttrBool("VisibleSide"), child.AttrFloat("OffsetX"), child.AttrFloat("OffsetY"));
                    }
                    return result;
                case "FlaglinesAndSuch/PolygonEffect2":
                    result = new polygonEffect2(child.Attr("ObjectFilename"), child.Attr("graphicsFilename"), child.AttrFloat("posX"), child.AttrFloat("posY"), child.AttrFloat("posZ"), child.AttrFloat("scale"));
                    return result;
                case "FlaglinesAndSuch/PolygonEffectCube":
                    result = new polygonEffect(child.AttrFloat("XPos"), child.AttrFloat("YPos"), child.AttrFloat("ZPos"), child.AttrFloat("Width"), child.AttrFloat("Height"), child.AttrFloat("Depth"), child.Attr("Colors"), child.AttrFloat("OffsetX"), child.AttrFloat("OffsetY"));
                    return result;

            }
            return null;
        }


    }
}
