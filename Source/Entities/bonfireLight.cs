using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/BonfireLight")]
    class BonfireLight : Entity
    {

		public Color lightColor;
		//public float lightAlpha;
		//public int lightFadeStart;
		//public int lightFadeEnd;
		//public float BloomAlpha;
		//public float BloomRadius;


		private VertexLight light;
		private BloomPoint bloom;
		private float brightness;
		private float multiplier;
		private Wiggler wiggle; 

		float randBase = 0.5f;
		float randAdd = 0.5f;
		float randFreq = 0.25f;

		bool disableOnPhotosensitive;
		bool fadeInOnEntry;

		public BonfireLight(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			int lfadestart = 32;
			int lfadeend = 64;
			float bloomradius = 32;
			float wiggleF = 4f;
			float wiggleD = 0.2f;
			if (data.Has("brightnessVariance")) {
				lfadestart = data.Int("lightFadeStart");
				lfadeend = data.Int("lightFadeEnd");
				//float bloomalpha = data.Float("bloomAlpha");
				bloomradius = data.Float("bloomRadius");
	
				randBase = data.Float("baseBrightness");
				randAdd = data.Float("brightnessVariance");
				randFreq = data.Float("flashFrequency");

				wiggleF = data.Float("wigglerFrequency");
				wiggleD = data.Float("wigglerDuration");

				disableOnPhotosensitive = data.Bool("photosensitivityConcern");
			}
			fadeInOnEntry = !data.Has("fadeIn") || data.Bool("fadeIn"); //if FadeIn isn't there (back-compat), it's always true

            lightColor = Calc.HexToColor(data.Attr("lightColor"));
			base.Tag = Tags.TransitionUpdate;

			Add(light = new VertexLight(new Vector2(0f, 0f), lightColor, 1, lfadestart, lfadeend/*1f, 32, 64*/));
			Add(bloom = new BloomPoint(new Vector2(0f, 0f), 1f, bloomradius));
			Add(wiggle = Wiggler.Create(wiggleD, wiggleF, delegate (float wigglerVar)
			{
				light.Alpha = bloom.Alpha = Math.Min(1f, brightness + wigglerVar * 0.25f) * multiplier;
			}));
		}

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((disableOnPhotosensitive && Settings.Instance.DisableFlashes ) || randFreq <= 0)
			{
				return;
			}
			if (fadeInOnEntry) {
                wiggle.Start();
            }
        }

        public override void Update()
		{
			if (disableOnPhotosensitive && Settings.Instance.DisableFlashes) {
				base.Update();
				return;
			}
			multiplier = fadeInOnEntry ? Calc.Approach(multiplier, 1f, Engine.DeltaTime * 2f) : 1f;
				if (base.Scene.OnInterval(randFreq))
				{
					brightness = randBase + Calc.Random.NextFloat(randAdd);//0.5, 0.5
					wiggle.Start();
				}
			base.Update();
		}
	}
}
