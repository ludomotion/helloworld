using Microsoft.Xna.Framework.Graphics;
using Phantom;
using System;
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

#if DEBUG // Play window-mode when debugging:
            this.SetResolution((int)this.Width, (int)this.Height, false);
            XnaGame.IsMouseVisible = true;
#endif

        }

        protected override void Initialize()
        {
            base.Initialize();
            XnaGame.Content.RootDirectory = "./Assets";

            PushState(new PlayState());

            Debug.WriteLine("HelloWorld initialized!");
        }
    }
}
