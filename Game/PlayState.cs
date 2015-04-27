using Phantom;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Physics;

namespace HelloWorld
{
    public class PlayState : GameState
    {
        private World World;
        public EntityLayer Entities { get; private set; }

        public PlayState()
        {
            this.Entities = new EntityLayer(new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas), new TiledIntegrator(1, 16));

            this.Entities.AddComponent(this.World = new World(160, 5, PhantomGame.Randy.Next()));

            this.World.Generate();

            AddComponent(this.Entities);
        }
    }
}
