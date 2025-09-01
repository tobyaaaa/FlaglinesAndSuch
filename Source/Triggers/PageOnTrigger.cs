using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/PageTrigger")]
    public class PageTrigger : Trigger {

        private class PoemPage : Entity
        {
            private const float TextScale = 0.7f;

            private MTexture paper;
            private MTexture cursor;
            private Vector2 cursorPos;

            private VirtualRenderTarget target;

            private FancyText.Text text;
            private bool noText;

            private float alpha = 1f;

            private float scale = 1f;

            private float rotation = 0f;

            private float timer;

            private bool easingOut;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public PoemPage(string imgPath, string txtPath, Vector2 cursorPos)//todo: custom image, dialog optional
            {
                Console.WriteLine("FLaS: instantiated page/map trigger thing");

                base.Tag = Tags.HUD;


                cursor = GFX.Gui["x"];
                this.cursorPos = cursorPos;

                if (imgPath == "") paper = GFX.Gui["poempage"];
                else paper = GFX.Gui[imgPath];

                if (txtPath == "") noText = true;
                text = FancyText.Parse(Dialog.Get(txtPath), (int)((float)(paper.Width - 120) / 0.7f), -1, 1f, Color.Black * 0.6f);
                
                Add(new BeforeRenderHook(BeforeRender));//?
            }

            public IEnumerator EaseIn()//todo: optionalize ease in rotation
            {
                Console.WriteLine("FLaS: page/map trigger thing easein called");

                Audio.Play("event:/game/03_resort/memo_in");
                Vector2 vector = new Vector2(Engine.Width, Engine.Height) / 2f;
                //Vector2 from = vector + new Vector2(0f, 200f);
                //Vector2 to = vector;
                //float rFrom = -0.1f;
                //float rTo = 0.05f;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime)//todo: make this shorter/custom
                {
                    Position = vector; //from + (to - from) * Ease.CubeOut(p);
                    alpha = Ease.CubeOut(p);
                    //rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
                    yield return null;
                }
            }

            public IEnumerator EaseOut()//todo: optionalize ease out rotation
            {
                Audio.Play("event:/game/03_resort/memo_out");
                easingOut = true;
                //Vector2 from = Position;
                //Vector2 to = new Vector2(Engine.Width, Engine.Height) / 2f + new Vector2(0f, -200f);
                //float rFrom = rotation;
                //float rTo = rotation + 0.1f;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.5f)//todo: make this shorter/custom
                {
                    //Position = from + (to - from) * Ease.CubeIn(p);
                    alpha = 1f - Ease.CubeIn(p);
                    //rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
                    yield return null;
                }
                RemoveSelf();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void BeforeRender()
            {
                if (target == null)
                {
                    target = VirtualContent.CreateRenderTarget("flaglines-journal-poem", paper.Width, paper.Height);//todo; "name" string here could cause issues if not unique? no idea
                }
                Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                paper.Draw(Vector2.Zero);
                cursor.Draw(cursorPos, Vector2.Zero, Color.Lerp(Color.White, Color.Gray, (float)Math.Sin(timer * 3)));


                if (!noText) {
                    text.DrawJustifyPerLine(new Vector2(paper.Width, paper.Height) / 2f, new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, 1f);
                }
                Draw.SpriteBatch.End();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void Removed(Scene scene)
            {
                if (target != null)
                {
                    target.Dispose();
                }
                target = null;
                base.Removed(scene);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void SceneEnd(Scene scene)
            {
                if (target != null)
                {
                    target.Dispose();
                }
                target = null;
                base.SceneEnd(scene);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void Update()
            {
                timer += Engine.DeltaTime;
                base.Update();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void Render()
            {
                Level level = base.Scene as Level;
                if ((level == null || (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene)) && target != null)
                {
                    Draw.SpriteBatch.Draw((Texture2D)(RenderTarget2D)target, Position, (Rectangle?)target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, target.Height) / 2f, scale, SpriteEffects.None, 0f);

                    if (!easingOut)
                    {
                        GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height / 2 + ((timer % 1f < 0.25f) ? 6 : 0)));
                    }
                }
            }
        }

        //private string ReadOnceFlag; //todo: unique flag

        private Player player;
        Vector2 cursorOffset;
        float cursorScale;

        private PoemPage poem;

        bool active;

        bool doTextbox = false;

        string imgFilepath;
        string txtPath;

        public PageTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            //ReadOnceFlag = "poem_read" + id.Key;
            imgFilepath = data.Attr("ImageFilepath");
            txtPath = data.Attr("DialogId");
            cursorOffset = new Vector2(data.Float("CursorOffsetX"), data.Float("CursorOffsetY"));
            cursorScale = data.Float("CursorScale");
        }

        
        private IEnumerator Routine()
        {
            Console.WriteLine("FLaS: page/map trigger thing routine started");
            active = true;
            player.StateMachine.State = 11;
            //player.StateMachine.Locked = true; //don't lock statemachine, I don't care if something knocks the player out of this. TODO: close map if something changes state
            if (doTextbox)//todo: && !Level.Session.GetFlag("poem_read")
            {
                yield return Textbox.Say("ch2_journal");
                yield return 0.1f;
            }

            Vector2 pos = player.Position;
            pos.X = (float) Math.Floor(pos.X / 320) * 10 * cursorScale;
            pos.Y = (float) Math.Floor(pos.Y / 184) * 10 * cursorScale;
            pos += cursorOffset;

            poem = new PoemPage(imgFilepath, txtPath, pos);
            Scene.Add(poem);
            yield return poem.EaseIn();
            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            Audio.Play("event:/ui/main/button_lowkey");
            yield return poem.EaseOut();
            player.StateMachine.State = 0; //todo: can this happen just a hair earlier?
            poem = null;
            //EndCutscene(Level);
            Console.WriteLine("FLaS: page/map trigger thing routine finished");
            active = false;
        }

        public override void OnEnter(Player player) //todo: interact option?
        {

            Console.WriteLine("FLaS: page/map trigger thing OnEnter called");

            this.player = player; 
            if (!active && player.StateMachine.State == 0 && player.OnSafeGround) Add(new Coroutine(Routine()));
        }
    }
}
