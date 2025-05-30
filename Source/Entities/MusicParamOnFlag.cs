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
using static Celeste.TrackSpinner;


namespace FlaglinesAndSuch
{
    /// <summary>
    /// Sets a music parameter to equal the amount of activated flags (of a given set)
    /// </summary>
    [CustomEntity("FlaglinesAndSuch/MusicParamOnFlag")]
    class MusicParamOnFlag : Entity
    {
        string parameter;
        string[] eventFlags;
        bool isGlobal = false;
        public EntityID ID;

        public MusicParamOnFlag(EntityData data,  Vector2 offset) : base()
        {
            eventFlags = data.Attr("eventFlags").Split(',');
            parameter = data.Attr("parameter");
            isGlobal = data.Bool("throughWholeMap");
            ID = new EntityID(data.Level.Name, data.ID);
        }

        public override void Awake(Scene scene)
        {
            Session session = SceneAs<Level>().Session;
            if (isGlobal)
            {
                AddTag(Tags.Global);
                session.DoNotLoad.Add(ID);
            }
        }

        public override void Update()
        {
            base.Update();
            int numFlags = 0;
            //Console.WriteLine("this isnt workign why");
            foreach (string flag in eventFlags)
            {
                numFlags += (!string.IsNullOrEmpty(flag) && (Scene as Level).Session.GetFlag(flag)) ? 1 : 0;
                //Console.WriteLine("current flag:" + flag + "; flags found" + numFlags);
            }

            //Console.WriteLine("set music param to" + numFlags);
            Audio.SetMusicParam(parameter, numFlags);
        }

    }
}
