using Microsoft.Xna.Framework.Graphics;
using Phantom;
using Phantom.Core;
using Phantom.Graphics;
using System;
using System.Diagnostics;

namespace HelloWorld
{
    public class Sprites
    {

        public static readonly Sprite Roguelike;

        static Sprites()
        {
            Content content = PhantomGame.Game.Content;
            if (content == null)
                throw new InvalidProgramException("You've seem to called `Sprites' statically before the game was initialized, please refrain from doing that.");
            Debug.WriteLine("Loading Sprites");
            Roguelike = new Sprite(content.Load<Texture2D>("Sprites/roguelike"), 16, 16, 1);
        }

    }
}
