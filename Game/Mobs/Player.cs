using HelloWorld.Mobs.Components;
using Microsoft.Xna.Framework;
using Phantom.Audio;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Misc;
using Phantom.Shapes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld.Mobs
{
    public class Player : Entity
    {
        const float AnimFreq = 0.1f;
        const int North = 0;
        const int East = 1;
        const int South = 2;
        const int West = 3;

        public int Facing { get; private set; }
        public bool Attacking
        {
            get
            {
                return this.attack > 0;
            }
        }

        private float timer;
        private int frame;
        private float attack = 0;

        public Player(Vector2? position = null)
            : base(position ?? Vector2.Zero)
        {
            AddComponent(new Mover(Vector2.Zero, .9f, 0.9f, 0));
            AddComponent(new Circle(5));
            AddComponent(new KeyboardInput());

            this.Orientation = MathHelper.PiOver2;
        }

        public override void HandleMessage(Message message)
        {
            if (message == HelloWorldMessages.Attack1 && this.attack <= 0)
            {
                this.attack = 0.5f;
                Sound.Play("strike");
            }
            base.HandleMessage(message);
        }

        public override void Update(float elapsed)
        {
            this.timer += elapsed;

            if (this.Mover.Velocity.LengthSquared() > 0)
            {
                this.Orientation = this.Mover.Velocity.Angle();
                this.frame = (int)((this.timer / AnimFreq) % 3);
            }

            if (Math.Abs(Orientation) < MathHelper.PiOver4)
                this.Facing = East;
            else if (Math.Abs(Orientation - MathHelper.PiOver2) < MathHelper.PiOver4)
                this.Facing = South;
            else if (Math.Abs(Orientation - MathHelper.Pi) < MathHelper.PiOver4)
                this.Facing = West;
            else if (Math.Abs(Orientation - -MathHelper.PiOver2) < MathHelper.PiOver4)
                this.Facing = North;

            if (this.attack > 0 && (this.attack -= elapsed) <= 0)
            {
                this.attack = 0;
                this.frame = (int)((this.timer / AnimFreq) % 3);
            }

            base.Update(elapsed);
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            Vector2 pos = this.Position + new Vector2(0, -4);
            switch (this.Facing)
            {
                case North:
                    if (this.Attacking)
                        Sprites.Character.RenderFrame(info, 8, pos);
                    else
                        Sprites.Character.RenderFrame(info, 0 + this.frame, pos);
                    break;
                case East:
                    if (this.Attacking)
                        Sprites.Character.RenderFrame(info, 13, pos);
                    else
                        Sprites.Character.RenderFrame(info, 5 + this.frame, pos);
                    break;
                case South:
                    if (this.Attacking)
                        Sprites.Character.RenderFrame(info, 4, pos);
                    else
                        Sprites.Character.RenderFrame(info, 10 + this.frame, pos);
                    break;
                case West:
                    if (this.Attacking)
                        Sprites.Character.RenderFrame(info, 19, pos);
                    else
                        Sprites.Character.RenderFrame(info, 15 + this.frame, pos);
                    break;
            }
            base.Render(info);
        }

    }
}
