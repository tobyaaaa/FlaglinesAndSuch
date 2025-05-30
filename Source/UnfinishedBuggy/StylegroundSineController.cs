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
    [CustomEntity("FlaglinesAndSuch/StylegroundSineController")]
    class StylegroundSineController : Entity
    {
        static Dictionary<string, float> storedThetas;
        string TagID;
        float sinValue;
        float amplitude;
        float frequency;
        float xOffset;
        bool isVert;
        //float theta = 0;

        static StylegroundSineController() {
            storedThetas = new Dictionary<string, float>();
        }
        
        public StylegroundSineController(EntityData data, Vector2 offset)
        {
            Tag = Tags.FrozenUpdate;
            TagID = data.Attr("Tag");
            amplitude = data.Float("SineAmplitude");
            frequency = data.Float("SineFrequency");
            xOffset = data.Float("SineOffset");
            isVert = data.Bool("VerticalMovement");

            if (!storedThetas.ContainsKey(TagID)) {
                storedThetas[TagID] = 0;
            }
        }


        private struct thisBackdrop
        {
            public Backdrop drop;
            public float initialPos;
        }
        List<thisBackdrop> TheseBackdrops = new List<thisBackdrop>();

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = base.Scene as Level;

            List<Backdrop> tempList = new List<Backdrop>();//this whole little block is to poulate the backdrop list
            tempList.AddRange(level.Background.GetEach<Backdrop>(TagID));
            for (int i = 0; i < tempList.Count; i++)
            {

                thisBackdrop tempBackdrop;
                tempBackdrop.drop = tempList[i];
                if (isVert)
                {
                    tempBackdrop.initialPos = tempList[i].Position.Y;
                    
                }
                else {
                    tempBackdrop.initialPos = tempList[i].Position.X;
                }
                TheseBackdrops.Add(tempBackdrop);
            }
        }

        public override void Update()
        {
            base.Update();
            //sinValue = (sinValue + (angular velocity in rad / s * Engine.DeltaTime)) % (Math.Pi * 2);
            sinValue = amplitude * (float) Math.Sin(frequency * storedThetas[TagID] + (Math.PI * xOffset));

            if (isVert)
            {
                foreach (thisBackdrop backdrop in TheseBackdrops)
                {
                    backdrop.drop.Position.Y = sinValue + backdrop.initialPos;
                }
            }
            else
            {
                foreach (thisBackdrop backdrop in TheseBackdrops)
                {
                    backdrop.drop.Position.X = sinValue + backdrop.initialPos;
                }
            }
            storedThetas[TagID] = (storedThetas[TagID] + Engine.DeltaTime) % (float)(Math.PI * 2 / frequency);

        }

    }
}
