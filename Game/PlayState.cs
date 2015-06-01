using HelloWorld.Mobs;
using HelloWorld.Statics;
using Microsoft.Xna.Framework;
using Phantom;
using Phantom.Cameras;
using Phantom.Cameras.Components;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Misc.Components;
using Phantom.Physics;
using Phantom.Shapes;
using System.Diagnostics;

namespace HelloWorld
{
    public class PlayState : GameState
    {
        public World World { get; private set; }
        public Player Player { get; private set; }
        public EntityLayer Entities { get; private set; }

        public PlayState(Player player = null)
        {
            var size = 160;
            this.Entities = new EntityLayer(
                size * HelloWorld.World.TileSize, 
                size * HelloWorld.World.TileSize, 
                new Renderer(1, Renderer.ViewportPolicy.None, Renderer.RenderOptions.Canvas), 
                new TiledIntegrator(1, HelloWorld.World.TileSize)
            );

            this.World = new World(size, 5, PhantomGame.Randy.Next());
            this.Entities.AddComponent(this.World);
            this.World.Generate();

            this.Player = player ?? new Player();
            this.Entities.AddComponent(this.Player);
            this.Player.Position = this.World.SpawnPoint;

            this.Entities.AddComponent(new Exit(this.World.ExitPoint));

            this.AddComponent(new Camera());
            this.Camera.AddComponent(new RestrictCamera(this.Entities));
            this.Camera.AddComponent(new DeadZone(HelloWorld.World.TileSize * 20, HelloWorld.World.TileSize * 20));
            this.Camera.AddComponent(new FollowEntity(this.Player));


            AddComponent(this.Entities);
            Debug.WriteLine("PlayState constructed");
        }

        public override void HandleMessage(Message message)
        {
            if (message == HelloWorldMessages.Exit)
            {
                PhantomGame.Game.PopAndPushState(new PlayState(this.Player));
            }
            base.HandleMessage(message);
        }

    }
}
