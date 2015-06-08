using HelloWorld.Utils;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Phantom;
using Phantom.Assets;
using Phantom.Audio;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Misc;
using Phantom.Utils;
using System.Diagnostics;

namespace HelloWorld
{
    public class HelloWorldGame : PhantomGame
    {
        private KeyboardState previousKeyboardState;

        public HelloWorldGame()
            : base(1280, 720, "HelloWorld")
        {
            Debug.WriteLine("Testing output: Debug");
            Trace.WriteLine("Testing output: Trace");
            System.Console.WriteLine("Testing output: Console");

            XnaGame.Content.RootDirectory = "./Assets";
            Audio.Initialize(this);
        }

        public override void SetupGraphics()
        {
            base.SetupGraphics();
            graphics.PreferMultiSampling = false;

#if DEBUG // Play window-mode when debugging:
            this.SetResolution((int)this.Width, (int)this.Height, false);
            XnaGame.IsMouseVisible = true;
#endif

        }

        public void ToggleFullscreen()
        {
            if (this.graphics.IsFullScreen)
            {
                this.SetResolution((int)this.Width, (int)this.Height, false);
            }
            else
            {
                this.SetResolution(0, 0, true);
            }
            Settings.FullScreen = this.graphics.IsFullScreen;
        }

        protected override void Initialize()
        {
            base.Initialize();
            AddComponent(new Storage());
            Debug.WriteLine("StateFile: " + GetComponentByType<Storage>().StateFile);
            Debug.WriteLine("LastSaved: " + Storage.LastSaved);

#if !DEBUG
            if (this.graphics.IsFullScreen != Settings.FullScreen)
                ToggleFullscreen();
#endif

            AddComponent(Asynchrony.Instance);
            AddComponent(new Konsoul());

            // Example usage of the Konsoul and the asynchrony:
            PhantomGame.Game.Console.Register("crash", "cause an exception", delegate(string[] argv){
                Asynchrony.Instance.Dispatch(() =>
                {
                    int j = 0;
                    int i = 5 / j;
                });
            });

            this.Content.SwitchContext("game");
            var sprites = typeof(Sprites); // Loads the class, which loads the sprites!

            Debug.WriteLine("HelloWorld initialized!");
            PushState(new PlayState());
        }

        protected override void LoadContent(Content content)
        {
            Audio.RegisterSound("game", "Audio/Sounds/strike");

            base.LoadContent(content);
        }

        public override void Update(float elapsed)
        {
            KeyboardState keyboardState = Keyboard.GetState();
#if DEBUG
            // Easy restart when working with visualstudio:
            if (keyboardState.IsKeyDown(Keys.F5))
                this.Exit();
#endif

            if (keyboardState.IsKeyDown(Keys.F11) && previousKeyboardState.IsKeyUp(Keys.F11))
                this.ToggleFullscreen();



            previousKeyboardState = keyboardState;
            base.Update(elapsed);
        }

    }
}
