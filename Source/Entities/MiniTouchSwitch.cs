using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FlaglinesAndSuch
{
	[CustomEntity("FlaglinesAndSuch/MiniTouchSwitch")]
	[Tracked(false)]
	public class MiniTouchSwitch : Entity
	{
		//static int followerIndex;
		//int followerIndexThis;

		private Follower follower;
		public EntityID ID;
		private Sprite sprite;

		public String Flag;
		public bool flagState;
		public bool toggleFlag;

		public Color color;
		public String spritepath;
		public String overrideSpritebank;

		public struct persistencyVariables{
			public EntityID id;
			public String Flag;
			public bool flagState;
			public bool toggleFlag;

			public Color color;
			public String spritepath;
			public String overrideSpritebank;
		}
		public persistencyVariables makePersistentVars(MiniTouchSwitch mts) {
			persistencyVariables pvars = new persistencyVariables();
			pvars.id = mts.ID;
			pvars.Flag = mts.Flag;
			pvars.flagState = mts.flagState;
			pvars.toggleFlag = mts.toggleFlag;
			pvars.color = mts.color;
			pvars.spritepath = mts.spritepath;
			pvars.overrideSpritebank = mts.overrideSpritebank;
			return pvars;
		}

		public Vector2? entryRespawn;
		public bool entryFlagState;
		public bool pendingPermaGrabbed;
		public bool pendingPermaUsed;

		public MiniTouchSwitch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
		{
			ID = id;
			Flag = data.Attr("Flag");
			flagState = data.Bool("flagState");
			toggleFlag = data.Bool("toggleFlag");
			color = Calc.HexToColor(data.Attr("Color"));


			spritepath = data.Attr("spritepath");
			overrideSpritebank = data.Attr("OverrideSpritepath");
			if (overrideSpritebank != "")
			{
				Add(sprite = GFX.SpriteBank.Create(overrideSpritebank));
			}
			else
			{
				Add(sprite = Class1.spriteBank.Create("MiniTouchSwitch"));
			}
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(follower = new Follower(id));
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
			sprite.Color = color;
			/*Add(new DashListener
			{
				OnDash = OnDash
			});*/
			sprite.CenterOrigin();
			sprite.Play(spritepath != "" ? spritepath : "normal");
		}
		public MiniTouchSwitch(/*Player player,*/ persistencyVariables mts)//?
		: base(/*player.Position + new Vector2(-12 * (int)player.Facing, -8f)*/ Vector2.Zero)
		{
			ID = mts.id;
			Flag = mts.Flag;
			flagState = mts.flagState;
			toggleFlag = mts.toggleFlag;
			color = mts.color;

			spritepath = mts.spritepath;
			overrideSpritebank = mts.overrideSpritebank;
			if (overrideSpritebank != "")
			{
				Add(sprite = GFX.SpriteBank.Create(overrideSpritebank));
			}
			else
			{
				Add(sprite = Class1.spriteBank.Create("MiniTouchSwitch"));
			}
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(follower = new Follower(mts.id));
			Add(new PlayerCollider(OnPlayer));
			Add(new MirrorReflection());
			sprite.Color = color;



			/*Add(new DashListener
			{
				OnDash = OnDash
			});*/
			sprite.CenterOrigin();
			sprite.Play(spritepath != "" ? spritepath : "normal");

			//player.Leader.GainFollower(follower);
			Collidable = false;
			base.Depth = -1000000;

		}

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
			Session session = SceneAs<Level>().Session;
			entryRespawn = session.RespawnPoint;


			if (!Collidable)
			{  //
				Player player = scene.Tracker.GetEntity<Player>();
				Position = player.Position + new Vector2(-12 * (int)player.Facing, -8f);
				player.Leader.GainFollower(follower);
			}
		}


        private void OnPlayer(Player player)
		{
			//SceneAs<Level>().Particles.Emit(P_Collect, 10, Position, Vector2.One * 3f);
			Audio.Play("event:/game/general/key_get", Position);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			player.Leader.GainFollower(follower);
			Collidable = false;
			Session session = SceneAs<Level>().Session;
			session.DoNotLoad.Add(ID);
			//session.Keys.Add(ID);
			//session.UpdateLevelStartDashes();
			base.Depth = -1000000;

			pendingPermaGrabbed = true;
			entryRespawn = session.RespawnPoint;
		}

        public override void Update()
        {
            base.Update();
			Session session = SceneAs<Level>().Session;
			if (entryRespawn != session.RespawnPoint)
			{
				Console.WriteLine("pancakes for breakfast - update vars from spawnpoint");
				updatePendingVariables();
				/*if (pendingPermaGrabbed) {
					Class1.Session.MiniTouchSwitches.Add(makePersistentVars(this));
				}
				if (pendingPer maUsed) {
					//follower.Leader.LoseFollower(follower);
					Class1.Session.MiniTouchSwitches.Remove(makePersistentVars(this));
					session.DoNotLoad.Remove(ID);
					//position = ???
				}
				pendingPermaGrabbed = false;
				pendingP ermaUsed = false;
				entryRespawn = session.RespawnPoint;*/
			}
		}

		void updatePendingVariables()
		{
			Console.WriteLine("pancakes for breakfast - update vars: in function");
			Console.WriteLine("(pending used is: " + pendingPermaUsed + ")");
			Session session = SceneAs<Level>().Session;
			if (pendingPermaGrabbed)
			{
                Class1.Session.MiniTouchSwitches.Add(makePersistentVars(this));
			}
			if (pendingPermaUsed)
			{
				Console.WriteLine("pancakes for breakfast - updating perma used");
                Class1.Session.MiniTouchSwitches.Remove(makePersistentVars(this));
				session.DoNotLoad.Remove(ID);
				//position = ???
			}
			pendingPermaGrabbed = false;
			Console.WriteLine("pancakes for breakfast - resetting permaused from updatependingvars");
			pendingPermaUsed = false;
			entryRespawn = session.RespawnPoint;
			entryFlagState = session.GetFlag(Flag);
		}

        /*private bool IsFirstMTS
		{
			get
			{
				for (int num = follower.FollowIndex - 1; num >= 0; num--)
				{
					MiniTouchSwitch mts = follower.Leader.Followers[num].Entity as MiniTouchSwitch;
					if (mts != null)
					{
						return false;
					}
				}
				return true;
			}
		}


		protected virtual void OnDash(Vector2 dir)
		{
			if (follower.Leader != null && IsFirstMTS) { //seems to still activate for more than one mini touch switch! Not sure how to fix
				pendingPer maUsed = true;
				(Scene as Level).Session.SetFlag(Flag, toggleFlag ? !(Scene as Level).Session.GetFlag(Flag) : flagState);
				follower.Leader.LoseFollower(follower); //order should be fine since it'd only matter during a respawn
				//do something so the player 
			}
		}*/

		public void mtsUsed() {

			Console.WriteLine("pancakes for breakfast - set pending used to true");
			pendingPermaUsed = true;
			Console.WriteLine("(pending used is now: " + pendingPermaUsed + ")");
			(Scene as Level).Session.SetFlag(Flag, toggleFlag ? !(Scene as Level).Session.GetFlag(Flag) : flagState);
			follower.Leader.LoseFollower(follower); //order should be fine since it'd only matter during a respawn

			//potential fix thing?
			Session session = SceneAs<Level>().Session;
            Class1.Session.MiniTouchSwitches.Remove(makePersistentVars(this));
			session.DoNotLoad.Remove(ID);
		}

		/*static void RemoveFirstMTS() { // as of yet unused
			foreach (MiniTouchSwitch mts in Engine.Scene.Tracker.GetEntities<MiniTouchSwitch>())
			{ //need to loop because I need one that is a follower
				if (mts.follower.Leader != null) {
					for (int num = mts.follower.FollowIndex - 1; num >= 0; num--)
					{
						MiniTouchSwitch mts2 = mts.follower.Leader.Followers[num].Entity as MiniTouchSwitch;
						if (mts2 != null)
						{

							mts2.pending PermaUsed = true;
							(Engine.Scene as Level).Session.SetFlag(mts2.Flag, mts2.toggleFlag ? !(Engine.Scene as Level).Session.GetFlag(mts2.Flag) : mts2.flagState);
							mts2.follower.Leader.LoseFollower(mts2.follower); //order should be fine since it'd only matter during a respawn

						}
					}
					return;
				}
			
			}
		}*/

		public static void PlayerDiesCaller(Player player)
        {
			
			foreach (MiniTouchSwitch e in player.Scene.Tracker.GetEntities<MiniTouchSwitch>())
			{
				e.WhenPlayerDies(player);
			}
		}
		

		void WhenPlayerDies(Player player) {
			bool wasGrabbed = false;
			if (pendingPermaGrabbed) {
				player.Leader.LoseFollower(follower);
				Collidable = true;
				Session session = SceneAs<Level>().Session;
				session.DoNotLoad.Remove(ID);
				//session.Keys.Remove(ID);
				//session.UpdateLevelStartDashes();
				base.Depth = 0;
				//Position = ; might do this later

				wasGrabbed = true;
				pendingPermaGrabbed = false;
			}
			if (pendingPermaUsed) {

				if (!wasGrabbed) {
					Session session = SceneAs<Level>().Session;
                    Class1.Session.MiniTouchSwitches.Add(makePersistentVars(this));
					session.DoNotLoad.Add(ID);
				}

				(Scene as Level).Session.SetFlag(Flag, entryFlagState);
				Console.WriteLine("pancakes for breakfast - revert pending permaused (die)");
				pendingPermaUsed = false;
			}
		}

		public static void RoomChangeMethod(Level level, LevelData next, Vector2 direction) {

			/*Player player = Engine.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				for (int i = 0; i < player.Leader.Followers.Count; i++)
				{
					MiniTouchSwitch mts = player.Leader.Followers[i].Entity as MiniTouchSwitch;
					if (mts != null)
					{
						mts.updatePendingVariables();
					}
				}
			}*/
			Player player = Engine.Scene.Tracker.GetEntity<Player>();
			for (int i = 0; i < player.Leader.Followers.Count; i++)
			//foreach (MiniTouchSwitch e in Engine.Scene.Tracker.GetEntities<MiniTouchSwitch>())
			{
				MiniTouchSwitch mts = player.Leader.Followers[i].Entity as MiniTouchSwitch;
				if (mts != null)
				{
					Console.WriteLine("pancakes for breakfast - update vars from room change");
					mts.updatePendingVariables();
				}
				//e.follower.Leader.LoseFollower(e.follower);
			}
		}

		public static void LoadLevelMethod(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
		{

			Player player = Engine.Scene.Tracker.GetEntity<Player>();
			HashSet<EntityID> idsused = new HashSet<EntityID>();
			if (player != null) {
				for (int i = 0; i < player.Leader.Followers.Count; i++)
				{
					MiniTouchSwitch mts = player.Leader.Followers[i].Entity as MiniTouchSwitch;
					if (mts != null)
					{
						idsused.Add(mts.ID);
					}
				}
			}

			foreach (persistencyVariables mts in Class1.Session.MiniTouchSwitches) {
				if (!idsused.Contains(mts.id)) {
					level.Add(new MiniTouchSwitch(mts));
					idsused.Add(mts.id);
				}
			}
		}
	}
}
