using HelloWorld.Mobs;
using Microsoft.Xna.Framework;
using Phantom.Core;
using Phantom.Physics;
using Phantom.Shapes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld.Statics
{
    public class Exit : Entity
    {
        private GameState state;

        public Exit(Vector2 position)
            :base(position)
        {
            AddComponent(new Circle(5));
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.state = this.GetAncestor<GameState>();
        }

        public override void AfterCollisionWith(Entity other, CollisionData collision)
        {
            if (other is Player)
                state.HandleMessage(HelloWorldMessages.Exit, this);
            base.AfterCollisionWith(other, collision);
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            Sprites.Roguelike.RenderFrame(info, 12, this.Position, 0, 1f);
            base.Render(info);
        }

    }
}
