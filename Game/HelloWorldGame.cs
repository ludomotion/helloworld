using Microsoft.Xna.Framework.Audio;
using Phantom;
using Phantom.Audio;
using System.Diagnostics;

namespace HelloWorld
{
    public class HelloWorldGame : PhantomGame
    {
        public HelloWorldGame()
            : base(1280, 720, "HelloWorld")
        {

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
            this.SetResolution((int)this.Width, (int)this.Height, false);

        }

        protected override void Initialize()
        {
            base.Initialize();

            this.Content.SwitchContext("game");
            var sprites = typeof(Sprites); // Loads the class, which loads the sprites!


            Debug.WriteLine("HelloWorld initialized!");
            PushState(new PlayState());
        }

        protected override void LoadContent(Phantom.Core.Content content)
        {
            Audio.RegisterSound("game", "Audio/Sounds/strike");

            base.LoadContent(content);
        }

        public override void Update(float elapsed)
        {

#if DEBUG
            // Easy restart when working with visualstudio:
            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F5))
                this.Exit();
#endif

            base.Update(elapsed);
        }

    }
}
