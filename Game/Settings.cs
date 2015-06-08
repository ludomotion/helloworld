using HelloWorld.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld
{
    public class Settings
    {

        [Saved(defaultValue = true)]
        public static bool FullScreen;

        [Saved(defaultValue = "Kevin")]
        public static string PlayerName;

    }
}
