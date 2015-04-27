using Phantom.Core;
using Phantom.Graphics;
using Phantom.Physics;
using System;

namespace HelloWorld
{
    public class PlayState : GameState
    {
        public EntityLayer Entities { get; private set; }

        public PlayState()
        {
            this.Entities = new EntityLayer(new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas), new TiledIntegrator(1, 16));

            this.Entities.AddComponent(new World());

            AddComponent(this.Entities);
        }
    }
}
