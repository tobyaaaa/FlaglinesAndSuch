using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/EffectAttrFade")]
    class EffectAttrFade : Trigger
    {/*
        String TagID;
        String Attr;//what attribute to change
        String Value;//the new value
        float speed;//sBeed
        String flag;
        bool flagState;
        bool resetFlag;
        List<Backdrop> GivenBackdrops;

        public EffectAttrFade(EntityData data, Vector2 offset) : base(data, offset)
        {

            speed = data.Float("speed");
            Attr = data.Attr("Attribute");
            TagID = data.Attr("Tag");
            Value = data.Attr("Value");
            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
            
            




                    /*how it works:
            A. find effects of given tag
            A1.figure out how to find effects

            (base.Scene as Level).Background.Get<BlackholeBG>()?.NextStrength(base.Scene as Level, strength);

            B. Start a coroutine easing between two values
            C. Yell at them to change the value     

            there's gonna be an enum for the different data types
            it'll have:
            -number (float)
            -color
            -array of above
            
            types:
            -color
            -float
            -array of (color, float)
            -range of floats (min-max)
            should I have a bool for "fade randomly" or "fade relatively"?
            * /
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            GivenBackdrops = getEffectTypeByTag(TagID, "temp lol");
        }
        public override void OnEnter(Player player)
        {
            foreach (customDreamStars ToChange in GivenBackdrops) {
                this.Add(new Coroutine(SingleColorFade(ToChange, Calc.HexToColor(Value), speed)));
            }
        }




        List<Backdrop> getEffectTypeByTag(String EffectTag, String EffectType) {
            Level level = Scene as Level;
            List<Backdrop> ArrayToReturn = new List<Backdrop>();
            foreach (Backdrop item in level.Foreground.GetEach<Backdrop>(TagID))
            {
                customDreamStars typeDreamstars = item as customDreamStars;
                if (typeDreamstars != null)
                {
                    //typeDreamstars.Add(new Coroutine(GradualColorFade(Calc.HexToColor(Value), speed)));
                    ArrayToReturn.Add(typeDreamstars);
                    //GradualColorFade(Calc.HexToColor(Value),speed);
                }

            }
            return ArrayToReturn;
        }

        private IEnumerator SingleColorFade(customDreamStars thisEffect, Color newColor, float fadeTime)
        {
            Color oldColor = thisEffect.StarColor;
            if (newColor == oldColor) {
                yield break;
            }
            float Percent = 0f;
            //Console.WriteLine("pancakes for dinner");
            while (true)
            {
                Percent += Engine.DeltaTime / fadeTime;
                thisEffect.StarColor = Color.Lerp(oldColor, newColor, Percent);
                if (Percent >= 1.0)
                {
                    //Console.WriteLine("pancakes for lunch " + thisEffect.StarColor);
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerator SingleFloatFade(customDreamStars thisEffect, float newFloat, float fadeTime) {
            float oldFloat = thisEffect.StarColor;
            if (newFloat == oldFloat)
            {
                yield break;
            }
            float Percent = 0f;
            //Console.WriteLine("pancakes for dinner");
            while (true)
            {
                Percent += Engine.DeltaTime / fadeTime;
                thisEffect.StarColor = float.Lerp(oldFloat, newFloat, Percent);
                if (Percent >= 1.0)
                {
                    //Console.WriteLine("pancakes for lunch " + thisEffect.StarColor);
                    yield break;
                }
                yield return null;
            }
        }
        
        private IEnumerator RangeEffectFade(customDreamStars thisEffect, float newMin, float newMax, float fadeTime) {
            //thisEffect.count;
            float[] oldspeeds;


            DynamicData data2 = new DynamicData(thisEffect);
            ??? = data2.Get("EffectName");
            DynData<customDreamStars> data = new DynData<customDreamStars>(thisEffect);
            data.Get<customDreamStars>("variableName");
            

            float oldMin = thisEffect.SpeedMin;
            float oldMax = thisEffect.SpeedMax;
            for (int i = 0; i < oldspeeds.Length; i++) {
                newspeeds[i] = (oldspeeds[i]) / Math.Abs(oldMax-oldMin) * Math.Abs(newMax-newMin)//maybe only works for values above 0? anyway
            }
            float Percent = 0f;
            while (true)
            {
                Percent += Engine.DeltaTime / fadeTime;
                thisEffect.StarColor = Color.Lerp(oldmins, newColor, Percent);
                if (Percent >= 1.0)
                {
                    //Console.WriteLine("pancakes for lunch " + thisEffect.StarColor);
                    yield break;
                }
                yield return null;
            }
            yield return null;
        }*/

        //coloursofnoise epic

        enum DataTypes
        {
            Int, Float, Color, Auto
        }

        DataTypes DataType;

        string TagID;
        string Field;
        object Value;
        float FadeTime;

        List<Backdrop> GivenBackdrops;


        public EffectAttrFade(EntityData data, Vector2 offset) : base(data, offset)
        {

            FadeTime = data.Float("speed");
            Field = data.Attr("Attribute");
            TagID = data.Attr("Tag");

            AddTag(Tags.Persistent);

            //data.Attr("max_attribute");
            //data.Attr("struct_name");
            /* 
             * This could be done without user input, but would require reflection and one of two things that are less ideal:
             * 1. The user would have to supply the type name for something like
             * `Type type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).First(t => t.FullName == "").GetField(Field).FieldType;`
             * 2. The type could be gotten from each backdrop, possibly in Added, with something like 
             * `Type type = GivenBackdrops.First().GetType().GetField(Field).FieldType;`
             */
            DataType = data.Enum<DataTypes>("dataType");
            // Parsing
            /*Value = DataType switch
            {
                DataTypes.Int => data.Int("value"),
                DataTypes.Float => data.Float("value"),
                DataTypes.Color => data.HexColor("value"),
                _ => throw new NotImplementedException("Unsupported value Type"),
            };*/
            switch (DataType) {
                case DataTypes.Int:
                    Value = data.Int("Value");
                    break;
                case DataTypes.Float:
                    Value = data.Float("Value");
                    break;
                case DataTypes.Color:
                    Value = data.HexColor("Value");
                    Console.WriteLine("FlaglinesAndSuch log: color achieved!");

                    break;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            GivenBackdrops = SceneAs<Level>().Foreground.GetEach<Backdrop>(TagID).ToList();
            GivenBackdrops.AddRange(SceneAs<Level>().Background.GetEach<Backdrop>(TagID).ToList());
        }

        public override void OnEnter(Player player)
        {
            Console.WriteLine("FlaglinesAndSuch log: OnEnter ");
            foreach (Backdrop ToChange in GivenBackdrops)
            {

                Console.WriteLine("FlaglinesAndSuch log: OnEnter switch case start");
                switch (DataType)
                {
                    /* 
                     * Special Case for field contained in `Backdrop` that would not be available through DynamicData
                     * This could be done with some sort of lookup table, or by doing `new DynamicData(typeof(Backdrop), effect)`
                     * But since there are a set number of these they could just be handled manually.
                     */
                    case DataTypes.Auto:

                        //this.Add(new Coroutine(SingleVector2ComponentFade(ToChange, Field, (float)Value, FadeTime, false)));//X
                        break;


                    case DataTypes.Float:
                        Console.WriteLine("FlaglinesAndSuch log: OnEnter switch case: float, when field");
                        this.Add(new Coroutine(SingleFloatFade(ToChange, Field, (float)Value, FadeTime)));
                        break;

                    case DataTypes.Color when Field == "Color":
                        Console.WriteLine("FlaglinesAndSuch log: OnEnter switch case: color, when field");
                        this.Add(new Coroutine(SingleColorFade(ToChange, (Color)Value, FadeTime)));
                        //SingleColorFade(ToChange, (Color)Value, FadeTime);
                        break;
                    case DataTypes.Color:
                        Console.WriteLine("FlaglinesAndSuch log: OnEnter switch case: color, not field");
                        this.Add(new Coroutine(SingleColorFade(ToChange, Field, (Color)Value, FadeTime)));
                        //SingleColorFade(ToChange, Field, (Color)Value, FadeTime);
                        break;

                }
            }
        }

        private IEnumerator SingleColorFade(Backdrop effect, Color newColor, float fadeTime)
        {
            Console.WriteLine("FlaglinesAndSuch log: fade beginning!");
            Color oldColor = effect.Color;
            if (newColor == oldColor)
            {
                yield break;
            }
            float Percent = 0f;
            while (Percent < 1.0)
            {
                Percent += Engine.DeltaTime / fadeTime;
                effect.Color = Color.Lerp(oldColor, newColor, Percent);
                yield return null;
            }
        }

        private IEnumerator SingleColorFade(Backdrop effect, string fieldName, Color newColor, float fadeTime)
        {
            Console.WriteLine("FlaglinesAndSuch log: fade beginning!");
            var effectData = new DynamicData(effect);

            Color oldColor;
            try
            {
                oldColor = effectData.Get<Color>(fieldName);
            }
            catch (NullReferenceException) {
                Console.WriteLine("FlaglinesAndSuch log: no variable found!");
                yield break;
            }
            
            if (newColor == oldColor)
            {
                yield break;
            }
            float Percent = 0f;
            while (Percent < 1.0)
            {
                Percent += Engine.DeltaTime / fadeTime;
                effectData.Set(fieldName, Color.Lerp(oldColor, newColor, Percent));
                yield return null;
            }
        }


        private IEnumerator SingleFloatFade(Backdrop effect, string fieldName, float newVal, float fadeTime)
        {
            Console.WriteLine("FlaglinesAndSuch log: fade beginning!");
            var effectData = new DynamicData(effect);

            float oldVal;
            try
            {
                oldVal = effectData.Get<float>(fieldName);
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("FlaglinesAndSuch log: no variable found!");
                yield break;
            }

            if (newVal == oldVal)
            {
                yield break;
            }
            float Percent = 0f;
            while (Percent < 1.0)
            {
                Percent += Engine.DeltaTime / fadeTime;
                effectData.Set(fieldName, (newVal * Percent + oldVal * (1 - Percent))); //Color.Lerp(oldVal, newVal, Percent)
                yield return null;
            }
        }

        private IEnumerator SingleVector2ComponentFade(Backdrop effect, string fieldName, float newVal, float fadeTime, bool direction)
        {
            Console.WriteLine("FlaglinesAndSuch log: fade beginning!");
            var effectData = new DynamicData(effect);

            float oldVal;
            try
            {
                if (direction)
                {
                    oldVal = effectData.Get<Vector2>(fieldName).Y;
                }
                else {
                    oldVal = effectData.Get<Vector2>(fieldName).X;
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("FlaglinesAndSuch log: no variable found!");
                yield break;
            }

            if (newVal == oldVal)
            {
                yield break;
            }
            float Percent = 0f;
            while (Percent < 1.0)
            {
                Percent += Engine.DeltaTime / fadeTime;
                effectData.Set(fieldName, (newVal * Percent + oldVal * (1 - Percent))); //Color.Lerp(oldVal, newVal, Percent)
                yield return null;
            }
        }



        /*private IEnumerator RangeEffectFade(Backdrop effect, string fieldNameMin, string fieldNameMax, float fadeTime)
        {
            //thisEffect.count;
            float[] oldspeeds;
            var effectData = new DynamicData(effect);

            float oldMin;
            float oldMax;
            try
            {
                oldMin = effectData.Get<float>(fieldNameMin);
                oldMax = effectData.Get<float>(fieldNameMax);
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("FlaglinesAndSuch log: no variable found!");
                yield break;
            }

            for (int i = 0; i < oldspeeds.Length; i++)
            {
                newspeeds[i] = (oldspeeds[i]) / Math.Abs(oldMax - oldMin) * Math.Abs(newMax - newMin)//maybe only works for values above 0? anyway
            }
            float Percent = 0f;
            while (true)
            {
                Percent += Engine.DeltaTime / fadeTime;
                effect.StarColor = Color.Lerp(oldmins, newColor, Percent);
                if (Percent >= 1.0)
                {
                    //Console.WriteLine("pancakes for lunch " + thisEffect.StarColor);
                    yield break;
                }
                yield return null;
            }
            yield return null;
        }*/

    }
}
