using Phantom;
using System.Diagnostics;

namespace HelloWorld
{
    public class HelloWorldGame : PhantomGame
    {
        public HelloWorldGame()
            : base(1280, 720, "HelloWorld")
        {
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

        protected override void Initialize()
        {
            base.Initialize();
            XnaGame.Content.RootDirectory = "./Assets";


            Debug.WriteLine("HelloWorld initialized!");
            PushState(new PlayState());
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
