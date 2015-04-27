using Microsoft.Xna.Framework;
using Phantom.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld
{
    public class World : Entity
    {
        const float Freq = 0.1f;

        private int frame;
        private float timer;

        public World()
            : base(new Vector2(0, 0))
        {
            this.frame = 0;
        }

        public override void Update(float elapsed)
        {
            this.timer += elapsed;
            if (this.timer > Freq)
            {
                this.frame += 1;
                this.timer -= Freq;
            }
            base.Update(elapsed);
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            Sprites.Roguelike.RenderFrame(info, this.frame, this.Position + new Vector2(8,8), 0, 1f);
            base.Render(info);
        }
    }
}
