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
    [CustomEntity("FlaglinesAndSuch/MusicParamOnVolume")]
    class MusicParamOnVolume : Entity
    {
        string parameter;
        bool is_music;
        public EntityID ID;

        public MusicParamOnVolume(EntityData data, Vector2 offset) : base()
        {
            parameter = data.Attr("parameter");
            is_music = data.Bool("is_music");
            ID = new EntityID(data.Level.Name, data.ID);
        }

        public override void Update()
        {
            base.Update();
            int toset = is_music ? Settings.Instance.MusicVolume : Settings.Instance.SFXVolume;
            Audio.SetMusicParam(parameter, toset);
        }


        /*
        //this is really stupid
        Vector2 cords = new Vector2(2,0);
        List<int> moves = [1,2,3,4,5,6,7];
        List<List<int>> foundMoves;
        public void stupid_maze_solver()
        {
            //for (int i...) loop through every possibility of move order
                //for (int j ...) loop through every possibility of positive and negative moves
                    //for (int k = 0; k < 6; k++):
                        //if (doMove(k, whatever)):
                            //k = 8
                        //if (k = 6):
                            //foundMoves.Append(moves);
            //Console.WriteLine(foundMoves.Count);
        }
        bool doMove(int num, bool rev) { 
            switch (num)
            {
                case 1:
                    return bad1(rev);
                case 2:
                    return bad2(rev);
                case 3:
                    return bad3(rev);
                case 4:
                    return bad4(rev);
                case 5:
                    return bad5(rev);
                case 6:
                    return bad6(rev);
                case 7:
                    return bad7(rev);
                default://bad
                    return true;
            }
        }
        public bool isOob()
        {
            return (cords.X < 0 || cords.X > 7 || cords.Y < 0 || cords.Y > 3);
        }

        public bool bad1(bool rev) {
            if (rev) { cords -= new Vector2(2, 1); }
            else { cords += new Vector2(2, 1); }
            return isOob();
        }
        public bool bad2(bool rev)
        {
            if (rev) { cords -= new Vector2(2, 0); }
            else { cords += new Vector2(2, 0); }
            return isOob();
        }
        public bool bad3(bool rev)
        {
            if (rev) { cords -= new Vector2(-1, 3); }
            else { cords += new Vector2(-1, 3); }
            return isOob();
        }
        public bool bad4(bool rev)
        {
            if (rev) { cords -= new Vector2(-3, 0); if (isOob()) { return true; } cords -= new Vector2(1, 1); }
            else { cords += new Vector2(-3, 0); if (isOob()) { return true; } cords += new Vector2(1, 1); }
            return isOob();
        }
        public bool bad5(bool rev)
        {
            if (rev) { cords -= new Vector2(1, 1); }
            else { cords += new Vector2(1, 1); }
            return isOob();
        }
        public bool bad6(bool rev)
        {
            if (rev) { cords -= new Vector2(-1, 2); }
            else { cords += new Vector2(-1, 2); }
            return isOob();
        }
        public bool bad7(bool rev)
        {
            if (rev) { cords -= new Vector2(-2, 2); }
            else { cords += new Vector2(-2, 2); }
            return isOob();
        }*/




    }
}
