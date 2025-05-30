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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
//using Celeste.Mod.FlaglinesAndSuch;

namespace FlaglinesAndSuch
{

    class HKnail
    {


		public static void Load() {

			//nail; currently ON
			On.Celeste.Player.Update += nailPlayerUpdate;
			//On.Celeste.Player.Render += nailPlayerRender; //only renders the yellow nail hitbox; good for debugging
			On.Celeste.FireBall.OnBounce += FireBall_OnBounce;
			//On.Celeste.PlayerSprite.ctor += NailPlayerspriteSetup;
			On.Celeste.Player.Die += NailPlayerDie;
		}

		public static void UnLoad()
		{
			On.Celeste.Player.Update -= nailPlayerUpdate;
			//On.Celeste.Player.Render -= nailPlayerRender;
			On.Celeste.FireBall.OnBounce -= FireBall_OnBounce;
			//On.Celeste.PlayerSprite.ctor -= NailPlayerspriteSetup;
			On.Celeste.Player.Die -= NailPlayerDie;
		}

		public class NailSpriteWrapper : Component
		{

			public Sprite sprite;

			private NailSpriteWrapper(Sprite sprite) : base(false, false)
			{
				this.sprite = sprite;
			}

			public static Sprite GetOrCreateNailSprite(Player player)
			{
				// it already exists, so just return it
				var wrapper = player.Get<NailSpriteWrapper>();
				if (wrapper != null)
					return wrapper.sprite;

				// it doesn't exist, so make it
				Sprite nailSprite = Class1.spriteBank.Create("PlayerNail");
				nailSprite.Visible = true;
				
				player.Add(nailSprite);
				player.Add(new NailSpriteWrapper(nailSprite));
				return nailSprite;
			}
		}

		//nail stuff
		//should this be in another class? I dunno :p


		//effectData.Get<Color>("varJumpTimer");

		private static readonly float NAILTIMERMAX = 0.08f; //used to be 0.12
		//private static readonly float NAILRECHARGETIMERMAX = 0;

		private static readonly Hitbox nailhitboxDown = new Hitbox(16f, 14f, -8f, 0f);
		private static readonly Hitbox nailhitboxUp = new Hitbox(16f, 14f, -8f, -25f);//used to be 14 wide 18 tall; 14/18/-7/-29
		private static readonly Hitbox nailhitboxRight = new Hitbox(14f, 16f, 4f, -12f);//used to be 18 wide 12 tall: 18/12/4/-11
		private static readonly Hitbox nailhitboxLeft = new Hitbox(14f, 16f, -18f, -12f);//used to be 18 wide 12 tall: 18/12/-22/-11
		enum Dirs { Up, Down, Left, Right }
		static float nailTimer;
		static float nailRechargeTimer;
		static bool nailHitSomething;
		static bool usingNailFireballVariable = false;
		static Dirs nailDir;

		static Sprite nailSprite;
		//static Sprite nailSprite;

		//static NailRenderEntity nailSprite;
		//static string NAIL_DOWN = "naildownstring";
		//static string NAIL_SIDE = "nailsidestring";
		//static string NAIL_UP = "nailupstring";


		private static void FireBall_OnBounce(On.Celeste.FireBall.orig_OnBounce orig, FireBall self, Player player)
		{
			if ((Class1.Settings.PlayerAlwaysHasNail || self.SceneAs<Level>().Session.GetFlag("flaglinesandsuch_nail_enabled")) && usingNailFireballVariable)
			{
				var thisFireballData = DynamicData.For(self);//inefficient???
				if (thisFireballData.Get<bool>("iceMode") && !thisFireballData.Get<bool>("broken"))
				{
					Audio.Play("event:/game/09_core/iceball_break", self.Position);
					//self.sprite.Play("shatter");
					thisFireballData.Get<Sprite>("sprite").Play("shatter");
					//self.broken = true;
					thisFireballData.Set("broken", true);
					self.Collidable = false;
					self.SceneAs<Level>().Particles.Emit(FireBall.P_IceBreak, 18, self.Center, Vector2.One * 6f);
				}
			}
			else
			{
				orig(self, player);
			}
		}

		private static void DoNailMomentum(Player self)
		{
            Celeste.Celeste.Freeze(0.05f);
			var playerData = DynamicData.For(self);//inefficient???
			playerData.Set("varJumpTimer", 0f);

			//self.AutoJump = true;
			//self.AutoJumpTimer = 0.1f; Test this

			switch (nailDir)//todo: There HAS to be a more efficient method here
			{
				case Dirs.Down:
					self.Speed.Y = (Math.Min(self.Speed.Y - 20f, -200f));//changes with direction; also maybe add less to current speed? Things might get a bit chaotic otherwise
																		 //self.Bounce(self.Bottom - 2f); (not quite; I don't want to refill dash/stamina. Good reference though!)

					//if player doing a down pogo, and has nonzero momentum; activate the nailhitbox for that direction; if that hits a spike(left/right) or a spinner, slow the player down instead
					//alternatively; in the spike checks: if the player is downslashing into a left/right spike, and moving towards it, call nailmomentum w/ flag. For spinners, do this if player is downslashing a spinner and nailhitbox left/right finds a spinner
					break;
				case Dirs.Up:
					//self.varJumpTimer = 0f;x
					self.Speed.Y = (Math.Max(self.Speed.Y + 20f, 200f)); //200f or yspeed + 20
					//self.Speed.X = 0f;x
					break;
				case Dirs.Right:
					//self.Speed.Y = 0f;
					self.Speed.X = (Math.Min(self.Speed.X - 80f, -120f)); //-80f or xspeed + 20
					break;
				case Dirs.Left:
					//self.Speed.Y = 0f;
					self.Speed.X = (Math.Max(self.Speed.X + 80f, 120f));
					break;
			}
			//self.Speed += self.LiftSpeed; //TODO: shouldn't apply if the block's moving away from them; also doesn't work may need to recode
		}

		/*private Action onDashDeleg;
		private void Naildash_Entity_Added(On.Monocle.Entity.orig_Added orig, Entity self, Scene scene)
		{
			if (self is Celeste.Platform) {
				PlayerCollider col = self.Components.Get<PlayerCollider>();
				onDashDeleg = col.OnDashCollide;
			}
		}*/
		//https://discord.com/channels/403698615446536203/908809001834274887/989902167764795402
		//likely not useful, I can remove this



		private static void nailPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
		{
			if ((Class1.Settings.PlayerAlwaysHasNail || self.SceneAs<Level>().Session.GetFlag("flaglinesandsuch_nail_enabled"))) {


				//Console.WriteLine("if this makes it into post yell at tobyaaa" + nailRechargeTimer);
				if (nailRechargeTimer > 0)
				{
					nailRechargeTimer -= Engine.DeltaTime;
				}

				if (Class1.Settings.NailHit.Pressed && nailTimer <= 0 && nailRechargeTimer <= 0 && self.Holding == null && (Class1.Settings.PlayerAlwaysHasNail || self.SceneAs<Level>().Session.GetFlag("flaglinesandsuch_nail_enabled")) && (self.StateMachine == 0 || self.StateMachine == 2 || self.StateMachine == 3 || self.StateMachine == 5 || self.StateMachine == 19))
				{
					nailSprite = NailSpriteWrapper.GetOrCreateNailSprite(self);


					nailHitSomething = false;
					nailTimer = NAILTIMERMAX;
					Audio.Play("event:/tobyaaa_HKnail/sword_swipe");

					if (Input.MoveY > 0 && !self.OnGround()) { nailDir = Dirs.Down; }
					else if (Input.MoveY < 0) { nailDir = Dirs.Up; }
					else { nailDir = (self.Facing == Facings.Left) ? Dirs.Left : Dirs.Right; }

					//code provided by Xaphan
					/*if (nailSprite == null) {
						nailSprite = Class1.spriteBank.Create("PlayerNail");
						nailSprite.Visible = false;

						//nailSprite.OnFinish = (ID) => {
						//	self.Sprite.Visible = true;
						//	nailSprite.Visible = false;
						//};

						self.Add(nailSprite);
					}*/
				
					if (self != null) //(don't play animation in starfly state)
					{

						nailSprite.RenderPosition = self.Position ; //+ new Vector2(-16f, -31f)
						//string backpack = SceneAs<Level>().Session.Inventory.Backpack ? "Backpack" : "NoBackpack";
						//if (Input.Aim.Value.SafeNormalize().X != 0 && !self.SceneAs<Level>().Paused)
						//{
						//	nailSprite.RenderPosition += new Vector2(Input.Aim.Value.SafeNormalize().X, 0f);
						//	nailSprite.Play("move");
						//}
						//else
						//{
						//}
						nailSprite.Visible = true;
						nailSprite.FlipX = self.Facing == Facings.Left;
						string InFeather = "feather";
						if (self.StateMachine.State != 19)
						{
							self.Sprite.Visible = false;
							InFeather = "";
						}
						switch (nailDir)
						{
							case Dirs.Down:
								nailSprite.Play("downhit" + InFeather);
								break;
							case Dirs.Up:
								nailSprite.Play("uphit" + InFeather);
								break;
							default:
								nailSprite.Play("sidehit" + InFeather);
								break;
						}

					}

				}

				if (nailSprite != null && (nailSprite.CurrentAnimationFrame >= nailSprite.CurrentAnimationTotalFrames) && nailRechargeTimer <= 0) //NOTE: can/will still mess up but should be uncommon
				{
					self.Sprite.Visible = true;
					nailSprite.Visible = false;
				}

				if (nailTimer > 0)
				{
					nailTimer -= Engine.DeltaTime;

					Collider tempCollider = self.Collider;

					switch (nailDir)
					{
						case Dirs.Down:
							self.Collider = nailhitboxDown;
							break;
						case Dirs.Up:
							self.Collider = nailhitboxUp;
							break;
						case Dirs.Right:
							self.Collider = nailhitboxRight;
							//self.Facing = Facings.Right; //this and next similar line SHOULD (haven't checked) lock the player's facing direction while the nail swipe is happening (update: doesn't work lmao)
							break;
						case Dirs.Left:
							self.Collider = nailhitboxLeft; //I wonder if these lines mess with some animation
							//self.Facing = Facings.Left;
							break;
					}

					//List<Entity> hitSolids = self.CollideAll<Solid>();

					//bumper? : no
					//puffer? : no
					//badeline boss? : no
					//rising ice: this'd be cool maybe
					//oshiro boss?

					List<Entity> hitSpinners = self.CollideAll<CrystalStaticSpinner>(); //Spinners
					List<Entity> hitSpikes = self.CollideAll<Spikes>(); //Spikes
					bool nailSlowXSpeed = false; //used to keep player from running into spike walls they want to pogo up the side of

					Seeker hitSeeker = self.CollideFirst<Seeker>();
					//Bumper hitBumper = (Bumper)Collide.First(self, self.Scene.Entities.FindAll<Bumper>());

					Wingmould hitWingmould = self.CollideFirst<Wingmould>();
					Sawblade hitSawblade = self.CollideFirst<Sawblade>();

					BladeRotateSpinner hitRotBlade = (BladeRotateSpinner)Collide.First(self, self.Scene.Entities.FindAll<BladeRotateSpinner>()); //!!!!!!!!!!!!!!!!!!!!!!! THANKS VIV !!!!!!!
					BladeTrackSpinner hitTrackBlade = (BladeTrackSpinner)Collide.First(self, self.Scene.Entities.FindAll<BladeTrackSpinner>());
					FireBall hitFireball = (FireBall)Collide.First(self, self.Scene.Entities.FindAll<FireBall>());

					DashBlock hitDashblock = self.CollideFirst<DashBlock>();
					DashSwitch hitDashSwitch = (DashSwitch)Collide.First(self, self.Scene.Entities.FindAll<DashSwitch>());
					CrushBlock hitKevin = (CrushBlock)Collide.First(self, self.Scene.Entities.FindAll<CrushBlock>());

					nailCompatibleSprite hitNailSprite = self.CollideFirst<nailCompatibleSprite>();

					self.Collider = tempCollider;

					//Bumper hitBumper = self.CollideFirst<Bumper>();
					//FireBall hitFireball = self.CollideFirst<FireBall>();
					if (hitFireball != null && !nailHitSomething)
					{
						var FireballData = DynamicData.For(hitFireball);//inefficient???
						if (FireballData.Get<bool>("iceMode"))
						{

							usingNailFireballVariable = true;
							FireballData.Invoke("OnBounce", self);
							usingNailFireballVariable = false;

							nailHitSomething = true;
							self.StateMachine.State = 0;
                            Celeste.Celeste.Freeze(0.05f);
							DoNailMomentum(self);
						}
					}


					//bool nailBonkedSolid = false;

					//SPEEN
					if (hitSpinners.Count() != 0 && !nailHitSomething)
					{
						foreach (Entity i in hitSpinners)
						{
							(i as CrystalStaticSpinner).Destroy();
						}
						nailHitSomething = true;
						self.StateMachine.State = 0;
						//Celeste.Celeste.Freeze(0.05f); moved to nailmomentum
						DoNailMomentum(self);
					}







					//WINGMOULD
					if (hitWingmould != null && !nailHitSomething)
					{
						if (hitWingmould.hitboxEnabled)
						{
							hitWingmould.OnNail();
							self.RefillDash();
							self.RefillStamina();
							nailHitSomething = true;
							self.StateMachine.State = 0;
							DoNailMomentum(self);
						}
					}

					//SAWBLADE
					if (hitSawblade != null && !nailHitSomething && !hitSawblade.no_nail_flaglines)
					{
						Audio.Play("event:/tobyaaa_HKnail/sword_tink", self.Position);
						nailHitSomething = true;
						self.StateMachine.State = 0;
						DoNailMomentum(self);

					}

					//BLADES
					if ((hitRotBlade != null || hitTrackBlade != null) && !nailHitSomething)
					{
						nailHitSomething = true;
						self.StateMachine.State = 0;
						Audio.Play("event:/tobyaaa_HKnail/sword_tink", self.Position);
						DoNailMomentum(self);
					}

					//SPIKES
					bool nailHitSpikes = false;
					if (hitSpikes.Count() != 0 && !nailHitSomething)
					{
						//nailHitSpikes = true;
						//Entity closestSpike = hitSpikes.First();
						foreach (Entity i in hitSpikes)
						{
							Spikes tempSpikes = i as Spikes;

							//Vector2 TRpoint = ((i.TopRight - self.Center).Length() < (nailhitboxDown.TopRight - self.Center).Length()) ? i.TopRight  : nailhitboxDown.TopRight;
							//Vector2 TLpoint = ((i.TopLeft - self.Center).Length() < (nailhitboxDown.TopLeft - self.Center).Length()) ? i.TopLeft : nailhitboxDown.TopLeft;
							//Vector2 BRpoint = ((i.BottomRight - self.Center).Length() < (nailhitboxDown.BottomRight - self.Center).Length()) ? i.BottomRight : nailhitboxDown.BottomRight;
							//Vector2 BLpoint = ((i.BottomLeft - self.Center).Length() < (nailhitboxDown.BottomLeft - self.Center).Length()) ? i.BottomLeft : nailhitboxDown.BottomLeft;
							//CHECK SEEKER CANSEEPLAYER

							if (nailDir.ToString() != tempSpikes.Direction.ToString())
							{
								nailHitSpikes = true;
							}
							if (nailDir == Dirs.Down && ((tempSpikes.Direction == Spikes.Directions.Left && self.Speed.X < 0.0) ||(tempSpikes.Direction == Spikes.Directions.Right && self.Speed.X > 0.0))) {
								//slow player down for pogoing up spikewalls; only if they're swinging downward AND are moving towards the spikes they're hitting (must be sideways spikes)
								nailSlowXSpeed = true;
							}
						}

						//	if ((closestSpike as Spikes).Direction == Spikes.Directions.Down) {
						//		nailHitSpikes = false;//for if the only spikes that got hit is facing down, it'll pass every check up to this point
						//	}

						//	foreach (Entity i in hitSolids)
						//	{
						//		//Console.WriteLine(i.ToString());
						//		if (closestSpike.Y > i.Y) { /*nailBonkedSolid = true;*/ }//changes with direction
						//	}
					}
					//else if (hitSpikes.Count() != 0)
					//{
					//	nailBonkedSolid = true;
					//}

					if (nailHitSpikes) //and !nailBonkedSolid
					{
						nailHitSomething = true;
						self.StateMachine.State = 0;
						Audio.Play("event:/tobyaaa_HKnail/sword_tink", self.Position);
						//Celeste.Celeste.Freeze(0.05f); moved to nailmomentum
						DoNailMomentum(self);
						if (nailSlowXSpeed) {
							self.Speed.X /= 2;
						}
						/*switch (nailDir)//todo: There HAS to be a more efficient method here
						{
							case Dirs.Down:
								self.Speed.Y = (Math.Min(self.Speed.Y - 20f, -200f));//changes with direction; also maybe add less to current speed? Things might get a bit chaotic otherwise
								//self.Speed.X = 0f;
								break;
							case Dirs.Up:
								self.Speed.Y = (Math.Max(self.Speed.Y + 20f, 200f));
								//self.Speed.X = 0f;
								break;
							case Dirs.Right:
								//self.Speed.Y = 0f;
								self.Speed.X = (Math.Min(self.Speed.X - 20f, -200f));
								break;
							case Dirs.Left:
								//self.Speed.Y = 0f;
								self.Speed.X = (Math.Max(self.Speed.X + 20f, 200f));
								break;
						}*/
					}

					//KEVINS
					if (hitKevin != null && !nailHitSomething)
					{
						var kevinData = DynamicData.For(hitKevin);//inefficient???
						Dirs nailDirsKevin;
						if (self.Position.X > hitKevin.Position.X && self.Position.X < hitKevin.Position.X + hitKevin.Width)
						{//player is above or below the kevin (or inside it lol)
						 //dirAsVector = (self.Position.Y <= hitKevin.Position.Y + (hitKevin.Height / 2)) ? new Vector2(0, 1) : new Vector2(0, -1);
							nailDirsKevin = (self.Position.Y <= hitKevin.Position.Y + (hitKevin.Height / 2)) ? Dirs.Down : Dirs.Up;
						}

						else if (self.Position.Y > hitKevin.Position.Y && self.Position.Y < hitKevin.Position.Y + hitKevin.Height)
						{//player is directly left, right, or inside kevin
							nailDirsKevin = (self.Position.X <= hitKevin.Position.X + (hitKevin.Width / 2)) ? Dirs.Right : Dirs.Left;
							//dirAsVector = (self.Position.X <= hitKevin.Position.X + (hitKevin.Width / 2)) ? new Vector2(1, 0) : new Vector2(-1, 0);
						}
						else
						{
							nailDirsKevin = nailDir;
							/*switch (nailDir)
							{
								case Dirs.Down:
									dirAsVector = new Vector2(0, 1);
									break;
								case Dirs.Up:
									dirAsVector = new Vector2(0, -1);
									break;
								case Dirs.Right:
									dirAsVector = new Vector2(1, 0);
									break;
								case Dirs.Left:
									dirAsVector = new Vector2(-1, 0);
									break;
								default://this SHOULD NEVER HAPPEN
									dirAsVector = new Vector2(-1, 0);
									break;
							}*/
						}

						Vector2 naildirskevinvec;
						switch (nailDirsKevin)
							{
							case Dirs.Down:
								naildirskevinvec = new Vector2(0, 1);
								break;
							case Dirs.Up:
								naildirskevinvec = new Vector2(0, -1);
								break;
							case Dirs.Right:
								naildirskevinvec = new Vector2(1, 0);
								break;
							case Dirs.Left:
								naildirskevinvec = new Vector2(-1, 0);
								break;
							default://this SHOULD NEVER HAPPEN
								naildirskevinvec = new Vector2(-1, 0);
								break;
							}

						//this is a little complicated, because I need to know if the player hit the kevin on the side it's already attacking towards; because if so the player
						//needs to NOT get momentum from hitting something with the nail (not kevin hit momentum). Player gets momentum if (kevin not attacking || returnstack.top != dashdir)
						//SoundSource kreturn = kevinData.Get<SoundSource>("returnLoopSfx");
						SoundSource kmoving = kevinData.Get<SoundSource>("currentMoveLoopSfx");
						bool kevinIsntAttacking = true;

						if (kmoving != null)
						{
							kevinIsntAttacking = !kmoving.InstancePlaying;//cursed. Checks that the kevin is not actively attacking
																		  //Console.WriteLine("sfx check: " + kevinIsReturning + " " + nailTimer);
						}

						if (kevinIsntAttacking)
						{
							nailHitSomething = true;
							self.StateMachine.State = 0;
							DoNailMomentum(self);
						}
						else
						{
							int retStackCount = kevinData.Get<IList>("returnStack").Count;
							if (retStackCount > 0)
							{
								//var kevindatadata = DynamicData.For(kevinData.Get<IList>("returnStack")[retStackCount - 1]);
								//Console.WriteLine("player direction " + dirAsVector + " kevin direction: " + kevindatadata.Get<Vector2>("Direction") + " " + nailTimer); //it is the direction the kevin goes once hit
								string facingdir = kevinData.Get<String>("nextFaceDirection");
								if ((facingdir == "left" & nailDirsKevin != Dirs.Right) | (facingdir == "right" & nailDirsKevin != Dirs.Left) | (facingdir == "up" & nailDirsKevin != Dirs.Down) | (facingdir == "down" & nailDirsKevin != Dirs.Up))
								//if (kevindatadata.Get<Vector2>("Direction") != -dirAsVector)//if the player hits the kevin left, then right, the right hit isn't added to the stack rip 
								{
									nailHitSomething = true;
									self.StateMachine.State = 0;
									DoNailMomentum(self);
								}
							}
						}
						kevinData.Invoke("OnDashed", self, naildirskevinvec);//OnDashed
					}

					//DASHBLOCKS
					if (hitDashblock != null && !nailHitSomething)
					{
						var dashblockdata = DynamicData.For(hitDashblock);//inefficient???
						if (dashblockdata.Get<bool>("canDash"))
						{
							Vector2 dirAsVector;
							switch (nailDir)
							{
								case Dirs.Down:
									dirAsVector = new Vector2(0, 1);
									break;
								case Dirs.Up:
									dirAsVector = new Vector2(0, -1);
									break;
								case Dirs.Right:
									dirAsVector = new Vector2(1, 0);
									break;
								case Dirs.Left:
									dirAsVector = new Vector2(-1, 0);
									break;
								default://this SHOULD NEVER HAPPEN
									dirAsVector = new Vector2(-1, 0);
									break;
							}
							dashblockdata.Invoke("OnDashed", self, dirAsVector);//OnDashed

							nailHitSomething = true;
							self.StateMachine.State = 0;
							DoNailMomentum(self);
						}
					}

					//DASH SWITCHES
					if (hitDashSwitch != null && !nailHitSomething)
					{
						var dashswitchdata = DynamicData.For(hitDashSwitch);//inefficient???
						Vector2 dirAsVector;
						switch (nailDir)
						{
							case Dirs.Down:
								dirAsVector = new Vector2(0, 1);
								break;
							case Dirs.Up:
								dirAsVector = new Vector2(0, -1);
								break;
							case Dirs.Right:
								dirAsVector = new Vector2(1, 0);
								break;
							case Dirs.Left:
								dirAsVector = new Vector2(-1, 0);
								break;
							default://this SHOULD NEVER HAPPEN
								dirAsVector = new Vector2(-1, 0);
								break;
						}
						dashswitchdata.Invoke("OnDashed", self, dirAsVector);//OnDashed

						if (dashswitchdata.Get<bool>("pressed"))
						{
							nailHitSomething = true;
							self.StateMachine.State = 0;
							DoNailMomentum(self);
						}
					}


					//SEEKERS
					if (hitSeeker != null && !nailHitSomething)
					{
						var SeekerData = DynamicData.For(hitSeeker);//inefficient???
						SeekerData.Invoke("GotBouncedOn", self);

						nailHitSomething = true;
						self.StateMachine.State = 0;
						DoNailMomentum(self);
					}


					//NAIL-COMPATIBLE SPRITE
					if (hitNailSprite != null && !nailHitSomething)
					{
						Vector2 dirAsVector;
						switch (nailDir)
						{
							case Dirs.Down:
								dirAsVector = new Vector2(0, 1);
								break;
							case Dirs.Up:
								dirAsVector = new Vector2(0, -1);
								break;
							case Dirs.Right:
								dirAsVector = new Vector2(1, 0);
								break;
							case Dirs.Left:
								dirAsVector = new Vector2(-1, 0);
								break;
							default://this SHOULD NEVER HAPPEN
								dirAsVector = new Vector2(-1, 0);
								break;
						}
						hitNailSprite.OnNail(dirAsVector);

						if (hitNailSprite.Refill)
						{
							self.RefillDash();
							self.RefillStamina();
						}
							nailHitSomething = true;
							self.StateMachine.State = 0;
						if (hitNailSprite.DoMomentum) {
							DoNailMomentum(self);
						}
					}


					if (nailHitSomething)
					{
						nailRechargeTimer = nailTimer + 0.06f;
					}

				}

				if (nailTimer <= 0 && !nailHitSomething)
				{
					nailHitSomething = true; //nail hit nothing I just don't want this timer to be set over and over
					nailRechargeTimer = 0.1f;
				}

				//NAIL TODO
				//momentum currently feels really jank
				//move spike interactions up on the interaction list, in general finilize order
				//spring/jump boost cancellations
				//fix collision with solids? (figure out what the farthest points of each collided spike is; then raycast from those to the player's middle, checking for solids)

				//direction buffering? ( not sure if this is needed now that it's four frames, but who knows.. no idea how)
			}
			orig.Invoke(self);
		}



		private static PlayerDeadBody NailPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
		{
			if (Class1.Settings.PlayerAlwaysHasNail || self.SceneAs<Level>().Session.GetFlag("flaglinesandsuch_nail_enabled")) {
				Sprite nailSprite = NailSpriteWrapper.GetOrCreateNailSprite(self);
				if (nailSprite != null) {
					nailSprite.Visible = false;
					self.Sprite.Visible = true;
				}
			}
			return orig(self, direction, evenIfInvincible, registerDeathInStats);
		}

		/*private static void nailPlayerRender(On.Celeste.Player.orig_Render orig, Player self) //used for debugging still
		{

			Collider tempCollider = self.Collider;

			if (nailTimer > 0f)
			{
				switch (nailDir)
				{
					case Dirs.Down:

						self.Collider = nailhitboxDown;
						//self.Sprite.Play(NAIL_DOWN);
						break;
					case Dirs.Up:
						self.Collider = nailhitboxUp;
						//self.Sprite.Play(NAIL_UP);
						break;
					case Dirs.Right:
						self.Collider = nailhitboxRight;
						//self.Sprite.Play(NAIL_SIDE);
						break;
					case Dirs.Left:
						self.Collider = nailhitboxLeft;
						//self.Sprite.Play(NAIL_SIDE);
						break;
				}
				Draw.HollowRect(self.Collider, Color.Yellow);
			}
			self.Collider = tempCollider;
			orig.Invoke(self);
		}*/


	}
}
