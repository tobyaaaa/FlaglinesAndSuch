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
	[CustomEntity("FlaglinesAndSuch/CustomReflectionStatue")]
	class CustomReflectionStatue : Entity
	{
		public class Torch : Entity
		{


			public string[] Code;

			private Sprite sprite;

			private Session session;

			public string Flag => "heartTorch_" + Index;

			public bool Activated => session.GetFlag(Flag);


			public int Index
			{
				get;
				private set;
			}

			public Torch(Session session, Vector2 position, int index, string[] code, string path)
				: base(position)
			{


				Index = index;
				Code = code;
				base.Depth = 8999;
				this.session = session;
				Image image = new Image(GFX.Game.GetAtlasSubtextures(path)[index]);
				image.CenterOrigin();
				image.Position = new Vector2(0f, 28f);
				Add(image);
				Add(sprite = new Sprite(GFX.Game, "objects/reflectionHeart/torch"));
				sprite.AddLoop("idle", "", 0f, default(int));
				sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
				sprite.Play("idle");
				sprite.Origin = new Vector2(32f, 64f);
			}

			public override void Added(Scene scene)
			{
				base.Added(scene);
				if (Activated)
				{
					PlayLit();
				}
			}

			public void Activate()
			{
				session.SetFlag(Flag);
				Alarm.Set(this, 0.2f, delegate
				{
					Audio.Play("event:/game/06_reflection/supersecret_torch_" + (Index + 1), Position);
					PlayLit();
				});
			}

			private void PlayLit()
			{
				sprite.Play("lit");
				sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
				Add(new VertexLight(Color.LightSeaGreen, 1f, 24, 48));
				Add(new BloomPoint(0.6f, 16f));
			}
		}
		//END OF TORCH CLASS
		public static string[] Code = new string[6] //EDIT THIS
		{
		"U",
		"L",
		"DR",
		"UR",
		"L",
		"UL"
		};
		
		public static Dictionary<string, Color> OverrideColors = new Dictionary<string, Color>(ForsakenCitySatellite.Colors);// new Dictionary<string, Color>
		public bool HeartIsFake;
		//public static String[] CityOverrides;
		public bool DashFlavourEnabled;

		public static string[][] OverrideCodes; //EDIT THIS
		bool codesBlank = false;
		public string HintImgs = "objects/reflectionHeart/hint";

		private const string FlagPrefix = "heartTorch_";

		private List<string> currentInputs = new List<string>();

		private List<Torch> torches = new List<Torch>();

		private Vector2 offset;

		private Vector2[] nodes;

		private DashListener dashListener;

		private bool enabled;

		public CustomReflectionStatue(EntityData data, Vector2 offset)//                    <---- constructor here
			: base(data.Position + offset)
		{
			this.offset = offset;
			nodes = data.Nodes;
			base.Depth = 8999;

			HeartIsFake = data.Bool("fake_heart");
			DashFlavourEnabled = data.Bool("dash_flavour_sounds");

			HintImgs = data.Attr("hint_mural_sprites") == "" ? "objects/reflectionHeart/hint" : data.Attr("hint_mural_sprites");

			string[] CodeStrs = data.Attr("base_code").Replace(" ", String.Empty).ToUpper().Split(',');
			Code = new string[CodeStrs.Length];
			for (int i = 0; i < CodeStrs.Length; i++)
			{
				Code[i] = CodeStrs[i];
			}

			OverrideCodes = new string[4][];//override codes. final string[4 codes][? items per code]
			if (data.Attr("Override_codes") != "")
			{//if the plugin thing isn't blank
				string[] CodeStrs2 = data.Attr("Override_codes").Replace(" ", String.Empty).ToUpper().Split(';');//split into four sections, spaces removed
				codesBlank = true;
				for (int i = 0; i < CodeStrs2.Length; i++)//from 0 to 3; one loop per code
				{
					OverrideCodes[i] = CodeStrs2[i].Split(',');//split into directions
															   //for (int j = 0; j < CodeStrs2[i].Length; j++)//loop from 0 to (length of code)
															   //{
															   //	OverrideCodes[i][j] = CodeStrs2[j];//actually is this necessary?
															   //}
				}
			}

			if (!OverrideColors.ContainsKey("D"))
			{
				OverrideColors.Add("D", Color.White);
				OverrideColors.Add("DL", Color.White);
				OverrideColors.Add("R", Color.White);
			}
			string[] DictStrs = data.Attr("gem_override_colors").Replace(" ", String.Empty).Split(',');
			//CityOverrides = new String[DictStrs.Length];
			for (int i = 0; i < DictStrs.Length - 1; i += 2)
			{
				OverrideColors[DictStrs[i]] = Calc.HexToColor(DictStrs[i + 1]);
			}


		}

		public override void Added(Scene scene)
		{
			base.Added(scene);

			Session session = (base.Scene as Level).Session;
			Image image = new Image(GFX.Game["objects/reflectionHeart/statue"]);
			image.JustifyOrigin(0.5f, 1f);
			image.Origin.Y -= 1f;
			Add(image);
			List<string[]> list = new List<string[]>();


			if (!codesBlank)
			{
				list.Add(Code);
				list.Add(FlipCode(h: true, v: false));
				list.Add(FlipCode(h: false, v: true));
				list.Add(FlipCode(h: true, v: true));
			}
			else {
				list.Add(OverrideCodes[0]);
				list.Add(OverrideCodes[1]);
				list.Add(OverrideCodes[2]);
				list.Add(OverrideCodes[3]);
			}
			for (int i = 0; i < 4; i++)
			{
				Torch torch = new Torch(session, offset + nodes[i], i, list[i], HintImgs);
				base.Scene.Add(torch);
				torches.Add(torch);
			}
			int num = Code.Length;
			Vector2 value = nodes[4] + offset - Position;
			for (int j = 0; j < num; j++)
			{
				Image image2 = new Image(GFX.Game["objects/reflectionHeart/gem"]);
				image2.CenterOrigin();
				image2.Color = OverrideColors[Code[j]];  //EDIT THIS
				image2.Position = value + new Vector2(((float)j - (float)(num - 1) / 2f) * 24f, 0f);
				Add(image2);
				Add(new BloomPoint(image2.Position, 0.3f, 12f));
			}
			enabled = !session.HeartGem; //EDIT THIS?
			if (enabled)
			{
				Add(dashListener = new DashListener());
				dashListener.OnDash = delegate (Vector2 dir)
				{
					string text = "";
					if (dir.Y < 0f)
					{
						text = "U";
					}
					else if (dir.Y > 0f)
					{
						text = "D";
					}
					if (dir.X < 0f)
					{
						text += "L";
					}
					else if (dir.X > 0f)
					{
						text += "R";
					}
					int num2 = 0;
					if (dir.X < 0f && dir.Y == 0f)
					{
						num2 = 1;
					}
					else if (dir.X < 0f && dir.Y < 0f)
					{
						num2 = 2;
					}
					else if (dir.X == 0f && dir.Y < 0f)
					{
						num2 = 3;
					}
					else if (dir.X > 0f && dir.Y < 0f)
					{
						num2 = 4;
					}
					else if (dir.X > 0f && dir.Y == 0f)
					{
						num2 = 5;
					}
					else if (dir.X > 0f && dir.Y > 0f)
					{
						num2 = 6;
					}
					else if (dir.X == 0f && dir.Y > 0f)
					{
						num2 = 7;
					}
					else if (dir.X < 0f && dir.Y > 0f)
					{
						num2 = 8;
					}
					if (DashFlavourEnabled)
					{
						Audio.Play("event:/game/06_reflection/supersecret_dashflavour", base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero, "dash_direction", num2);
					}
					currentInputs.Add(text);
					if (currentInputs.Count > Code.Length)//change this from the code length to the max of the torch codes if override codes exists
					{
						currentInputs.RemoveAt(0);
					}
					foreach (Torch torch2 in torches)
					{
						if (!torch2.Activated && CheckCode(torch2.Code))
						{
							torch2.Activate();
						}
					}
					CheckIfAllActivated();
				};
				CheckIfAllActivated(skipActivateRoutine: true);
			}
		}

		private string[] FlipCode(bool h, bool v)
		{
			string[] array = new string[Code.Length];
			for (int i = 0; i < Code.Length; i++)
			{
				string text = Code[i];
				if (h)
				{
					text = (text.Contains('L') ? text.Replace('L', 'R') : text.Replace('R', 'L'));
				}
				if (v)
				{
					text = (text.Contains('U') ? text.Replace('U', 'D') : text.Replace('D', 'U'));
				}
				array[i] = text;
			}
			return array;
		}

		private bool CheckCode(string[] code)
		{
			if (currentInputs.Count < code.Length)
			{
				return false;
			}
			for (int i = 0; i < code.Length; i++)
			{
				if (!currentInputs[currentInputs.Count + i - code.Length].Equals(code[i]))
				{
					return false;
				}
			}
			return true;
		}

		private void CheckIfAllActivated(bool skipActivateRoutine = false)
		{
			if (enabled)
			{
				bool flag = true;
				foreach (Torch torch in torches)
				{
					if (!torch.Activated)
					{
						flag = false;
					}
				}
				if (flag)
				{
					Activate(skipActivateRoutine);
				}
			}
		}

		public void Activate(bool skipActivateRoutine)
		{
			enabled = false;
			if (skipActivateRoutine)
			{
				if (HeartIsFake)
				{
					base.Scene.Add(new FakeHeart(Position + new Vector2(0f, -52f)));
				}
				else
				{
					base.Scene.Add(new HeartGem(Position + new Vector2(0f, -52f)));
				}

			}
			else
			{
				Add(new Coroutine(ActivateRoutine()));
			}
		}

		private IEnumerator ActivateRoutine()
		{
			yield return 0.533f;
			Audio.Play("event:/game/06_reflection/supersecret_heartappear");
			Entity dummy = new Entity(Position + new Vector2(0f, -52f))
			{
				Depth = 1
			};
			Scene.Add(dummy);
			Image white = new Image(GFX.Game["collectables/heartgem/white00"]);
			white.CenterOrigin();
			white.Scale = Vector2.Zero;
			dummy.Add(white);
			BloomPoint glow = new BloomPoint(0f, 16f);
			dummy.Add(glow);
			List<Entity> absorbs = new List<Entity>();
			for (int i = 0; i < 20; i++)
			{
				AbsorbOrb absorbOrb = new AbsorbOrb(Position + new Vector2(0f, -20f), dummy);
				Scene.Add(absorbOrb);
				absorbs.Add(absorbOrb);
				yield return null;
			}
			yield return 0.8f;
			float duration = 0.6f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
			{
				white.Scale = Vector2.One * p;
				glow.Alpha = p;
				(Scene as Level).Shake();
				yield return null;
			}
			foreach (Entity item in absorbs)
			{
				item.RemoveSelf();
			}
			(Scene as Level).Flash(Color.White);
			Scene.Remove(dummy);

			if (HeartIsFake)
			{
				Scene.Add(new FakeHeart(Position + new Vector2(0f, -52f)));
			}
			else
			{
				Scene.Add(new HeartGem(Position + new Vector2(0f, -52f)));
			}
		}

		public override void Update()
		{
			if (dashListener != null && !enabled)
			{
				Remove(dashListener);
				dashListener = null;
			}
			base.Update();
		}
	} 
}





	//end of reflection heart statue code
	//yes I'm including two classes in one file
	//I don't want to have to send lily two cs files and these are essentially the same entity
	//catplant

	/*[CustomEntity("FlaglinesAndSuch/CustomCitySatellite")]
public class CustomCitySatellite : Entity
	{
		private class CodeBird : Entity
		{
			private Sprite sprite;

			private Coroutine routine;

			private float timer = Calc.Random.NextFloat();

			private Vector2 speed;

			private Image heartImage;

			private readonly string code;

			private readonly Vector2 origin;

			private readonly Vector2 dash;

			public CodeBird(Vector2 origin, string code)
				: base(origin)
			{
				this.code = code;
				this.origin = origin;
				Add(sprite = new Sprite(GFX.Game, "scenery/flutterbird/"));
				sprite.AddLoop("fly", "flap", 0.08f);
				sprite.Play("fly");
				sprite.CenterOrigin();
				sprite.Color = Colors[code];
				Vector2 zero = Vector2.Zero;
				zero.X = (code.Contains('L') ? (-1) : (code.Contains('R') ? 1 : 0));
				zero.Y = (code.Contains('U') ? (-1) : (code.Contains('D') ? 1 : 0));
				dash = zero.SafeNormalize();
				Add(routine = new Coroutine(AimlessFlightRoutine()));
			}

			public override void Update()
			{
				timer += Engine.DeltaTime;
				sprite.Y = (float)Math.Sin(timer * 2f);
				base.Update();
			}

			public void Dash()
			{
				routine.Replace(DashRoutine());
			}

			public void Transform(float duration)
			{
				base.Tag = Tags.FrozenUpdate;
				routine.Replace(TransformRoutine(duration));
			}

			private IEnumerator AimlessFlightRoutine()
			{
				speed = Vector2.Zero;
				while (true)
				{
					Vector2 target = origin + Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 16f + Calc.Random.NextFloat(40f));
					float reset = 0f;
					while (reset < 1f && (target - Position).Length() > 8f)
					{
						Vector2 value = (target - Position).SafeNormalize();
						speed += value * 420f * Engine.DeltaTime;
						if (speed.Length() > 90f)
						{
							speed = speed.SafeNormalize(90f);
						}
						Position += speed * Engine.DeltaTime;
						reset += Engine.DeltaTime;
						if (Math.Sign(value.X) != 0)
						{
							sprite.Scale.X = Math.Sign(value.X);
						}
						yield return null;
					}
				}
			}

			private IEnumerator DashRoutine()
			{
				for (float t3 = 0.25f; t3 > 0f; t3 -= Engine.DeltaTime)
				{
					speed = Calc.Approach(speed, Vector2.Zero, 200f * Engine.DeltaTime);
					Position += speed * Engine.DeltaTime;
					yield return null;
				}
				Vector2 from = Position;
				Vector2 to = origin + dash * 8f;
				if (Math.Sign(to.X - from.X) != 0)
				{
					sprite.Scale.X = Math.Sign(to.X - from.X);
				}
				for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime * 1.5f)
				{
					Position = from + (to - from) * Ease.CubeInOut(t3);
					yield return null;
				}
				Position = to;
				yield return 0.2f;
				if (dash.X != 0f)
				{
					sprite.Scale.X = Math.Sign(dash.X);
				}
				(Scene as Level).Displacement.AddBurst(Position, 0.25f, 4f, 24f, 0.4f);
				speed = dash * 300f;
				for (float t3 = 0.4f; t3 > 0f; t3 -= Engine.DeltaTime)
				{
					if (t3 > 0.1f && Scene.OnInterval(0.02f))
					{
						SceneAs<Level>().ParticlesBG.Emit(Particles[code], 1, Position, Vector2.One * 2f, dash.Angle());
					}
					speed = Calc.Approach(speed, Vector2.Zero, 800f * Engine.DeltaTime);
					Position += speed * Engine.DeltaTime;
					yield return null;
				}
				yield return 0.4f;
				routine.Replace(AimlessFlightRoutine());
			}

			private IEnumerator TransformRoutine(float duration)
			{
				Color colorFrom = sprite.Color;
				Color colorTo = Color.White;
				Vector2 target = origin;
				Add(heartImage = new Image(GFX.Game["collectables/heartGem/shape"]));
				heartImage.CenterOrigin();
				heartImage.Scale = Vector2.Zero;
				for (float t = 0f; t < 1f; t += Engine.DeltaTime / duration)
				{
					Vector2 value = (target - Position).SafeNormalize();
					speed += 400f * value * Engine.DeltaTime;
					float num = Math.Max(20f, (1f - t) * 200f);
					if (speed.Length() > num)
					{
						speed = speed.SafeNormalize(num);
					}
					Position += speed * Engine.DeltaTime;
					sprite.Color = Color.Lerp(colorFrom, colorTo, t);
					heartImage.Scale = Vector2.One * Math.Max(0f, (t - 0.75f) * 4f);
					if (value.X != 0f)
					{
						sprite.Scale.X = Math.Abs(sprite.Scale.X) * (float)Math.Sign(value.X);
					}
					sprite.Scale.X = (float)Math.Sign(sprite.Scale.X) * (1f - heartImage.Scale.X);
					sprite.Scale.Y = 1f - heartImage.Scale.X;
					yield return null;
				}
			}
		}

		private const string UnlockedFlag = "unlocked_satellite";

		public static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
	{
		{
			"U",
			Calc.HexToColor("f0f0f0")
		},
		{
			"L",
			Calc.HexToColor("9171f2")
		},
		{
			"DR",
			Calc.HexToColor("0a44e0")
		},
		{
			"UR",
			Calc.HexToColor("b32d00")
		},
		{
			"UL",
			Calc.HexToColor("ffcd37")
		}
	};

		public static readonly Dictionary<string, string> Sounds = new Dictionary<string, string>
	{
		{
			"U",
			"event:/game/01_forsaken_city/console_white"
		},
		{
			"L",
			"event:/game/01_forsaken_city/console_purple"
		},
		{
			"DR",
			"event:/game/01_forsaken_city/console_blue"
		},
		{
			"UR",
			"event:/game/01_forsaken_city/console_red"
		},
		{
			"UL",
			"event:/game/01_forsaken_city/console_yellow"
		}
	};

		public static readonly Dictionary<string, ParticleType> Particles = new Dictionary<string, ParticleType>();

		private static readonly string[] Code = new string[6]
		{
		"U",
		"L",
		"DR",
		"UR",
		"L",
		"UL"
		};

		private static List<string> uniqueCodes = new List<string>();

		private bool enabled;

		private List<string> currentInputs = new List<string>();

		private List<CodeBird> birds = new List<CodeBird>();

		private Vector2 gemSpawnPosition;

		private Vector2 birdFlyPosition;

		private Image sprite;

		private Image pulse;

		private Image computer;

		private Image computerScreen;

		private Sprite computerScreenNoise;

		private Image computerScreenShine;

		private BloomPoint pulseBloom;

		private BloomPoint screenBloom;

		private Level level;

		private DashListener dashListener;

		private SoundSource birdFlyingSfx;

		private SoundSource birdThrustSfx;

		private SoundSource birdFinishSfx;

		private SoundSource staticLoopSfx;

		public CustomCitySatellite(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(sprite = new Image(GFX.Game["objects/citysatellite/dish"]));
			Add(pulse = new Image(GFX.Game["objects/citysatellite/light"]));
			Add(computer = new Image(GFX.Game["objects/citysatellite/computer"]));
			Add(computerScreen = new Image(GFX.Game["objects/citysatellite/computerscreen"]));
			Add(computerScreenNoise = new Sprite(GFX.Game, "objects/citysatellite/computerScreenNoise"));
			Add(computerScreenShine = new Image(GFX.Game["objects/citysatellite/computerscreenShine"]));
			sprite.JustifyOrigin(0.5f, 1f);
			pulse.JustifyOrigin(0.5f, 1f);
			Add(new Coroutine(PulseRoutine()));
			Add(pulseBloom = new BloomPoint(new Vector2(-12f, -44f), 1f, 8f));
			Add(screenBloom = new BloomPoint(new Vector2(32f, 20f), 1f, 8f));
			computerScreenNoise.AddLoop("static", "", 0.05f);
			computerScreenNoise.Play("static");
			computer.Position = (computerScreen.Position = (computerScreenShine.Position = (computerScreenNoise.Position = new Vector2(8f, 8f))));
			birdFlyPosition = offset + data.Nodes[0];
			gemSpawnPosition = offset + data.Nodes[1];
			Add(dashListener = new DashListener());
			dashListener.OnDash = delegate (Vector2 dir)
			{
				string text = "";
				if (dir.Y < 0f)
				{
					text = "U";
				}
				else if (dir.Y > 0f)
				{
					text = "D";
				}
				if (dir.X < 0f)
				{
					text += "L";
				}
				else if (dir.X > 0f)
				{
					text += "R";
				}
				currentInputs.Add(text);
				if (currentInputs.Count > Code.Length)
				{
					currentInputs.RemoveAt(0);
				}
				if (currentInputs.Count == Code.Length)
				{
					bool flag = true;
					for (int j = 0; j < Code.Length; j++)
					{
						if (!currentInputs[j].Equals(Code[j]))
						{
							flag = false;
						}
					}
					if (flag && level.Camera.Left + 32f < gemSpawnPosition.X && enabled)
					{
						Add(new Coroutine(UnlockGem()));
					}
				}
			};
			string[] code = Code;
			foreach (string item in code)
			{
				if (!uniqueCodes.Contains(item))
				{
					uniqueCodes.Add(item);
				}
			}
			base.Depth = 8999;
			Add(staticLoopSfx = new SoundSource());
			staticLoopSfx.Position = computer.Position;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = (scene as Level);
			enabled = (!level.Session.HeartGem && !level.Session.GetFlag("unlocked_satellite"));
			if (enabled)
			{
				foreach (string uniqueCode in uniqueCodes)
				{
					CodeBird codeBird = new CodeBird(birdFlyPosition, uniqueCode);
					birds.Add(codeBird);
					level.Add(codeBird);
				}
				Add(birdFlyingSfx = new SoundSource());
				Add(birdFinishSfx = new SoundSource());
				Add(birdThrustSfx = new SoundSource());
				birdFlyingSfx.Position = birdFlyPosition - Position;
				birdFlyingSfx.Play("event:/game/01_forsaken_city/birdbros_fly_loop");
			}
			else
			{
				staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
			}
			if (!level.Session.HeartGem && level.Session.GetFlag("unlocked_satellite"))
			{
				HeartGem entity = new HeartGem(gemSpawnPosition);
				level.Add(entity);
			}
		}

		public override void Update()
		{
			base.Update();
			computerScreenNoise.Visible = !pulse.Visible;
			computerScreen.Visible = pulse.Visible;
			screenBloom.Visible = pulseBloom.Visible;
		}

		[IteratorStateMachine(typeof(< PulseRoutine > d__28))]
		private IEnumerator PulseRoutine()
		{
			return new < PulseRoutine > d__28(0)
			{
			<> 4__this = this
		};
		}

		private IEnumerator UnlockGem()
		{
			level.Session.SetFlag("unlocked_satellite");
			birdFinishSfx.Position = birdFlyPosition - Position;
			birdFinishSfx.Play("event:/game/01_forsaken_city/birdbros_finish");
			staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
			enabled = false;
			yield return 0.25f;
			level.Displacement.Clear();
			yield return null;
			birdFlyingSfx.Stop();
			level.Frozen = true;
			Tag = Tags.FrozenUpdate;
			BloomPoint bloom = new BloomPoint(birdFlyPosition - Position, 0f, 32f);
			Add(bloom);
			foreach (CodeBird bird in birds)
			{
				bird.Transform(3f);
			}
			while (bloom.Alpha < 1f)
			{
				bloom.Alpha += Engine.DeltaTime / 3f;
				yield return null;
			}
			yield return 0.25f;
			foreach (CodeBird bird2 in birds)
			{
				bird2.RemoveSelf();
			}
			ParticleSystem particles = new ParticleSystem(-10000, 100);
			particles.Tag = Tags.FrozenUpdate;
			particles.Emit(BirdNPC.P_Feather, 24, birdFlyPosition, new Vector2(4f, 4f));
			level.Add(particles);
			HeartGem gem = new HeartGem(birdFlyPosition)
			{
				Tag = Tags.FrozenUpdate
			};
			level.Add(gem);
			yield return null;
			gem.ScaleWiggler.Start();
			yield return 0.85f;
			SimpleCurve curve = new SimpleCurve(gem.Position, gemSpawnPosition, (gem.Position + gemSpawnPosition) / 2f + new Vector2(0f, -64f));
			for (float t = 0f; t < 1f; t += Engine.DeltaTime)
			{
				yield return null;
				gem.Position = curve.GetPoint(Ease.CubeInOut(t));
			}
			yield return 0.5f;
			particles.RemoveSelf();
			Remove(bloom);
			level.Frozen = false;
		}
	}



}*/
