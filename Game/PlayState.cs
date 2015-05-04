using HelloWorld.Mobs;
using Microsoft.Xna.Framework;
using Phantom;
using Phantom.Cameras;
using Phantom.Cameras.Components;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Physics;

namespace HelloWorld
{
    public class PlayState : GameState
    {
        public World World { get; private set; }
        public Player Player { get; private set; }
        public EntityLayer Entities { get; private set; }

        public PlayState()
        {
            var size = 160;
            this.Entities = new EntityLayer(
                size * HelloWorld.World.TileSize, 
                size * HelloWorld.World.TileSize, 
                new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas), 
                new TiledIntegrator(1, HelloWorld.World.TileSize)
            );

            this.Entities.AddComponent(this.World = new World(size, 5, PhantomGame.Randy.Next()));
            this.World.Generate();

            this.Entities.AddComponent(this.Player = new Player(this.World.SpawnPoint));

            this.AddComponent(new Camera());
            this.Camera.AddComponent(new RestrictCamera(this.Entities));
            this.Camera.AddComponent(new DeadZone(HelloWorld.World.TileSize * 20, HelloWorld.World.TileSize * 20));
            this.Camera.AddComponent(new FollowEntity(this.Player));


            AddComponent(this.Entities);
        }

    }
}
