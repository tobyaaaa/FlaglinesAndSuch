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
    [CustomEntity("FlaglinesAndSuch/PlatformJelly")]
    public class PlatformJelly : Actor
    {
		private JumpThru ridablePlatform; // [!] !NEW CODE! [!]
		Vector2 PlatformOffset = new Vector2(-12,-16);// [!] !NEW CODE! [!]
		Vector2 PlatformOffsetBig = new Vector2(-16, -16);// [!] !NEW CODE! [!]
		int platformSoundIndex = 40;
		bool PermaNoGrab;
		bool HitboxWhenHeld = false;
		bool noTilt = false;

		bool biggerHitbox;

	public static ParticleType P_Glide;

	public static ParticleType P_GlideUp;

	public static ParticleType P_Platform;

	public static ParticleType P_Glow;

	public static ParticleType P_Expand;

	private const float HighFrictionTime = 0.5f;

	public Vector2 Speed;

	public Holdable Hold;

	private Level level;

	private Collision onCollideH;

	private Collision onCollideV;

	private Vector2 prevLiftSpeed;

	private Vector2 startPos;

	private float noGravityTimer;

	private float highFrictionTimer;

	private bool bubble;

	private bool tutorial;

	private bool destroyed;

	private Sprite sprite;

	private Wiggler wiggler;

	private SineWave platformSine;

	private SoundSource fallingSfx;

	private BirdTutorialGui tutorialGui;

	public PlatformJelly(Vector2 position, bool bubble, bool tutorial, int PlatSound, bool nograbs, bool nograbhitbox, string OverrideText, bool bigbox, bool oldHitbox, bool notilt)// [!] !Altered code! [!]
		: base(position)
	{
		this.bubble = bubble;
		this.tutorial = tutorial;
		startPos = Position;
		base.Collider = nograbhitbox ? new Hitbox(8f, 8, -4f, -16f) : new Hitbox(8f, 10f, -4f, -10f); //If true have the hitbox be higher to match the nograb sprite
		onCollideH = OnCollideH;
		onCollideV = OnCollideV;

		if (OverrideText != "")
		{
			Add(sprite = GFX.SpriteBank.Create(OverrideText));
		}
		else {
			Add(sprite = Class1.spriteBank.Create("PlatformJelly"));// [!] !Altered code! [!]
		}
		Add(wiggler = Wiggler.Create(0.25f, 4f));
		base.Depth = -5;
		Add(Hold = new Holdable(0.3f));
		Hold.PickupCollider = new Hitbox(20f, 22f, -10f, -16f);
		Hold.SlowFall = true;
		Hold.SlowRun = false;
		Hold.OnPickup = OnPickup;
		Hold.OnRelease = OnRelease;
		Hold.SpeedGetter = (() => Speed);
		Hold.OnHitSpring = HitSpring;
		platformSine = new SineWave(0.3f, 0f);
		Add(platformSine);
		fallingSfx = new SoundSource();
		Add(fallingSfx);
		Add(new WindMover(WindMode));

		platformSoundIndex = PlatSound; // [!] !NEW CODE! [!]
		PermaNoGrab = nograbs;
		HitboxWhenHeld = oldHitbox;
		biggerHitbox = bigbox;
		noTilt = notilt;
	}

	public PlatformJelly(EntityData e, Vector2 offset)
		: this(e.Position + offset, e.Bool("bubble"), e.Bool("tutorial"), e.Int("platformSound"), e.Bool("noGrabbing"), e.Bool("restLower"), e.Attr("OverrideSprite"), e.Bool("bigHitbox"), e.Bool("NewHitboxBehavior"), e.Bool("noRotation"))// [!] !Altered code! [!]
		{
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		level = SceneAs<Level>();
		if (tutorial)
		{
			tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), Input.Grab);
			tutorialGui.Open = true;
			base.Scene.Add(tutorialGui);

            
		}
		scene.Add(ridablePlatform = new JumpThru(biggerHitbox ? (this.ExactPosition + PlatformOffsetBig) : (this.ExactPosition + PlatformOffset), biggerHitbox ? 32 : 24, false)); // [!] !NEW CODE! [!]
			ridablePlatform.SurfaceSoundIndex = platformSoundIndex;
	}

	public override void Update()
	{
			if (ridablePlatform.Scene != null)// [!] !NEW CODE! [!]
			{
				if (ridablePlatform.HasPlayerRider() || PermaNoGrab)
				{// [!] !NEW CODE! [!]
					this.Collidable = false;
				}// [!] !NEW CODE! [!]
				else if (!destroyed && !PermaNoGrab)
				{// [!] !NEW CODE! [!]
					this.Collidable = true;
				}
				if ((!HitboxWhenHeld && Hold.IsHeld) || (HitboxWhenHeld && Hold.Holder != null && Hold.Holder.Sprite.CurrentAnimationID == "pickUp"))
				{// [!] !NEW CODE! [!]
					ridablePlatform.Collidable = false;
				}
				else
				{// [!] !NEW CODE! [!]
					ridablePlatform.Collidable = true;
				}
			}// [!] !NEW CODE! [!]


			if (base.Scene.OnInterval(0.05f))
		{
			level.Particles.Emit(Glider.P_Glow, 1, base.Center + Vector2.UnitY * -9f, new Vector2(10f, 4f));
		}
		float target = (!Hold.IsHeld) ? 0f : ((!Hold.Holder.OnGround()) ? Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, (float)Math.PI / 3f, -(float)Math.PI / 3f) : Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 0.6981317f, -0.6981317f));
		if (!noTilt)
		{ 
			sprite.Rotation = Calc.Approach(sprite.Rotation, target, (float)Math.PI * Engine.DeltaTime);
		}
		if (Hold.IsHeld && !Hold.Holder.OnGround() && (sprite.CurrentAnimationID == "fall" || sprite.CurrentAnimationID == "fallLoop"))
		{
			if (!fallingSfx.Playing)
			{
				Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
				fallingSfx.Play("event:/new_content/game/10_farewell/glider_movement");
			}
			Vector2 speed = Hold.Holder.Speed;
			Vector2 vector = new Vector2(speed.X * 0.5f, (speed.Y < 0f) ? (speed.Y * 2f) : speed.Y);
			float value = Calc.Map(vector.Length(), 0f, 120f, 0f, 0.7f);
			fallingSfx.Param("glider_speed", value);
		}
		else
		{
			fallingSfx.Stop();
		}
		base.Update();
		if (!destroyed)
		{
			foreach (SeekerBarrier entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
			{
				entity.Collidable = true;
				bool num = CollideCheck(entity);
				entity.Collidable = false;
				if (num)
				{
					destroyed = true;
					Collidable = false;
					if (Hold.IsHeld)
					{
						Vector2 speed2 = Hold.Holder.Speed;
						Hold.Holder.Drop();
						Speed = speed2 * 0.333f;
						Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					}
					Add(new Coroutine(DestroyAnimationRoutine()));
					return;
				}
			}
				if (Hold.IsHeld)
				{
					prevLiftSpeed = Vector2.Zero;
				}
				else if (!bubble)
				{
					if (highFrictionTimer > 0f)
					{
						highFrictionTimer -= Engine.DeltaTime;
					}
					if (OnGround())
					{
						float target2 = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
						Speed.X = Calc.Approach(Speed.X, target2, 800f * Engine.DeltaTime);
						Vector2 liftSpeed = base.LiftSpeed;
						if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
						{
							Speed = prevLiftSpeed;
							prevLiftSpeed = Vector2.Zero;
							Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
							if (Speed.X != 0f && Speed.Y == 0f)
							{
								Speed.Y = -60f;
							}
							if (Speed.Y < 0f)
							{
								noGravityTimer = 0.15f;
							}
						}
						else
						{
							prevLiftSpeed = liftSpeed;
							if (liftSpeed.Y < 0f && Speed.Y < 0f)
							{
								Speed.Y = 0f;
							}
						}
					}
					else if (Hold.ShouldHaveGravity)
					{
						float num2 = 200f;
						if (Speed.Y >= -30f)
						{
							num2 *= 0.5f;
						}
						float num3 = (Speed.Y < 0f) ? 40f : ((!(highFrictionTimer <= 0f)) ? 10f : 40f);
						Speed.X = Calc.Approach(Speed.X, 0f, num3 * Engine.DeltaTime);
						if (noGravityTimer > 0f)
						{
							noGravityTimer -= Engine.DeltaTime;
						}
						else if (level.Wind.Y < 0f)
						{
							Speed.Y = Calc.Approach(Speed.Y, 0f, num2 * Engine.DeltaTime);
						}
						else
						{
							Speed.Y = Calc.Approach(Speed.Y, 30f, num2 * Engine.DeltaTime);
						}
					}
					MoveH(Speed.X * Engine.DeltaTime, onCollideH);
					MoveV(Speed.Y * Engine.DeltaTime, onCollideV);

					
					//ridablePlatform.MoveTo(this.ExactPosition + PlatformOffset); // [!] !NEW CODE! [!]
				

					CollisionData data;
				if (base.Left < (float)level.Bounds.Left)
				{
					base.Left = level.Bounds.Left;
					data = new CollisionData
					{
						Direction = -Vector2.UnitX
					};
					OnCollideH(data);
				}
				else if (base.Right > (float)level.Bounds.Right)
				{
					base.Right = level.Bounds.Right;
					data = new CollisionData
					{
						Direction = Vector2.UnitX
					};
					OnCollideH(data);
				}
				if (base.Top < (float)level.Bounds.Top)
				{
					base.Top = level.Bounds.Top;
					data = new CollisionData
					{
						Direction = -Vector2.UnitY
					};
					OnCollideV(data);
				}
				else if (base.Top > (float)(level.Bounds.Bottom + 16))
				{

					ridablePlatform.DestroyStaticMovers(); // [!] !NEW CODE! [!]
					ridablePlatform.RemoveSelf(); // [!] !NEW CODE! [!]
					RemoveSelf();

					return;
				}
				Hold.CheckAgainstColliders();
			}
			else
			{
				Position = startPos + Vector2.UnitY * platformSine.Value * 1f;
			}
			Vector2 one = Vector2.One;
			if (!Hold.IsHeld)
			{
				if (level.Wind.Y < 0f)
				{
					PlayOpen();
				}
				else
				{
					sprite.Play(PermaNoGrab ? "alt_idle" : "idle");
				}
			}
			else if (Hold.Holder.Speed.Y > 20f || level.Wind.Y < 0f)
			{
				if (level.OnInterval(0.04f))
				{
					if (level.Wind.Y < 0f)
					{
						level.ParticlesBG.Emit(Glider.P_GlideUp, 1, Position - Vector2.UnitY * 20f, new Vector2(6f, 4f));
					}
					else
					{
						level.ParticlesBG.Emit(Glider.P_Glide, 1, Position - Vector2.UnitY * 10f, new Vector2(6f, 4f));
					}
				}
				PlayOpen();
				if (Input.GliderMoveY.Value > 0)
				{
					one.X = 0.7f;
					one.Y = 1.4f;
				}
				else if (Input.GliderMoveY.Value < 0)
				{
					one.X = 1.2f;
					one.Y = 0.8f;
				}
				Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
			}
			else
			{
				sprite.Play("held");
			}
			sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, one.Y, Engine.DeltaTime * 2f);
			sprite.Scale.X = Calc.Approach(sprite.Scale.X, (float)Math.Sign(sprite.Scale.X) * one.X, Engine.DeltaTime * 2f);
			if (tutorialGui != null)
			{
				tutorialGui.Open = (tutorial && !Hold.IsHeld && (OnGround(4) || bubble));
			}
		}
		else
		{
			Position += Speed * Engine.DeltaTime;
		}
			if (ridablePlatform.Scene != null)// [!] !NEW CODE! [!]
			{
				ridablePlatform.MoveTo(this.ExactPosition + (biggerHitbox ? PlatformOffsetBig: PlatformOffset)); // [!] !NEW CODE! [!]
			}
	}

	private void PlayOpen()
	{
		if (sprite.CurrentAnimationID != "fall" && sprite.CurrentAnimationID != "fallLoop")
		{
			sprite.Play("fall");
			sprite.Scale = new Vector2(1.5f, 0.6f);
			level.Particles.Emit(Glider.P_Expand, 16, base.Center + (Vector2.UnitY * -12f).Rotate(sprite.Rotation), new Vector2(8f, 3f), -(float)Math.PI / 2f + sprite.Rotation);
			if (Hold.IsHeld)
			{
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
			}
		}
	}

	public override void Render()
	{
		if (!destroyed)
		{
			sprite.DrawSimpleOutline();
		}
		base.Render();
		if (bubble)
		{
			for (int i = 0; i < 24; i++)
			{
				Draw.Point(Position + PlatformAdd(i), PlatformColor(i));
			}
		}
	}

	private void WindMode(Vector2 wind)
	{
		if (!Hold.IsHeld)
		{
			if (wind.X != 0f)
			{
				MoveH(wind.X * 0.5f);
			}
			if (wind.Y != 0f)
			{
				MoveV(wind.Y);
			}
		}
	}

	private Vector2 PlatformAdd(int num)
	{
		return new Vector2(-12 + num, -5 + (int)Math.Round(Math.Sin(base.Scene.TimeActive + (float)num * 0.2f) * 1.7999999523162842));
	}

	private Color PlatformColor(int num)
	{
		if (num <= 1 || num >= 22)
		{
			return Color.White * 0.4f;
		}
		return Color.White * 0.8f;
	}

	private void OnCollideH(CollisionData data)
	{
		if (data.Hit is DashSwitch)
		{
			(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
		}
		if (Speed.X < 0f)
		{
			Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", Position);
		}
		else
		{
			Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);
		}
		Speed.X *= -1f;
		sprite.Scale = new Vector2(0.8f, 1.2f);
	}

	private void OnCollideV(CollisionData data)
	{
		if (Math.Abs(Speed.Y) > 8f)
		{
			sprite.Scale = new Vector2(1.2f, 0.8f);
			Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
		}
		if (Speed.Y < 0f)
		{
			Speed.Y *= -0.5f;
		}
		else
		{
			Speed.Y = 0f;
		}
	}

	private void OnPickup()
	{
		if (bubble)
		{
			for (int i = 0; i < 24; i++)
			{
				level.Particles.Emit(Glider.P_Platform, Position + PlatformAdd(i), PlatformColor(i));
			}
		}
		AllowPushing = false;
		Speed = Vector2.Zero;
		AddTag(Tags.Persistent);
			ridablePlatform.AddTag(Tags.Persistent); // [!] !NEW CODE! [!]
			highFrictionTimer = 0.5f;
		bubble = false;
		wiggler.Start();
		tutorial = false;
	}

	private void OnRelease(Vector2 force)
	{
		if (force.X == 0f)
		{
			Audio.Play("event:/new_content/char/madeline/glider_drop", Position);
		}
		AllowPushing = true;
		RemoveTag(Tags.Persistent);
		ridablePlatform.RemoveTag(Tags.Persistent); // [!] !NEW CODE! [!]

		force.Y *= 0.5f;
		if (force.X != 0f && force.Y == 0f)
		{
			force.Y = -0.4f;
		}
		Speed = force * 100f;
		wiggler.Start();
	}

	protected override void OnSquish(CollisionData data)
	{
		if (!TrySquishWiggle(data))
		{

			ridablePlatform.DestroyStaticMovers(); // [!] !NEW CODE! [!]
			ridablePlatform.RemoveSelf(); // [!] !NEW CODE! [!]
			RemoveSelf();
		}
	}

	public bool HitSpring(Spring spring)
	{
		if (!Hold.IsHeld)
		{
			if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
			{
				Speed.X *= 0.5f;
				Speed.Y = -160f;
				noGravityTimer = 0.15f;
				wiggler.Start();
				return true;
			}
			if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
			{
				MoveTowardsY(spring.CenterY + 5f, 4f);
				Speed.X = 160f;
				Speed.Y = -80f;
				noGravityTimer = 0.1f;
				wiggler.Start();
				return true;
			}
			if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
			{
				MoveTowardsY(spring.CenterY + 5f, 4f);
				Speed.X = -160f;
				Speed.Y = -80f;
				noGravityTimer = 0.1f;
				wiggler.Start();
				return true;
			}
		}
		return false;
	}

	private IEnumerator DestroyAnimationRoutine()
	{
		Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
		sprite.Play("death");
		ridablePlatform.DestroyStaticMovers(); // [!] !NEW CODE! [!]
		ridablePlatform.RemoveSelf(); // [!] !NEW CODE! [!]
		yield return 1f;
		RemoveSelf();

		}
}

}
