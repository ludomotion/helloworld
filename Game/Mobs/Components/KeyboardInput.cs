using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Phantom.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld.Mobs.Components
{
    public class KeyboardInput : EntityComponent
    {
        const float Speed = 50;
        const Keys Attack1 = Keys.Space;
        const Keys Attack2 = Keys.C;

        private KeyboardState current;
        private KeyboardState previous;

        public override void OnAdd(Component parent)
        {
            this.current = Keyboard.GetState();
            this.previous = Keyboard.GetState();
            base.OnAdd(parent);
        }

        public override void Update(float elapsed)
        {
            this.current = Keyboard.GetState();
            this.Entity.Mover.Velocity = Vector2.Zero;
            if (current.IsKeyDown(Keys.Left))
            {
                this.Entity.Mover.Velocity += new Vector2(-1, 0);
            }
            if (current.IsKeyDown(Keys.Right))
            {
                this.Entity.Mover.Velocity += new Vector2(1, 0);
            }
            if (current.IsKeyDown(Keys.Up))
            {
                this.Entity.Mover.Velocity += new Vector2(0, -1);
            }
            if (current.IsKeyDown(Keys.Down))
            {
                this.Entity.Mover.Velocity += new Vector2(0, 1);
            }
            this.Entity.Mover.Velocity *= Speed;

            if (current.IsKeyDown(Attack1) && previous.IsKeyUp(Attack1))
                this.Parent.HandleMessage(HelloWorldMessages.Attack1);
            if (current.IsKeyDown(Attack2) && previous.IsKeyUp(Attack2))
                this.Parent.HandleMessage(HelloWorldMessages.Attack2);

            previous = current;
            base.Update(elapsed);
        }
    }
}
