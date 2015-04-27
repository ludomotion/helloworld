using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (HelloWorldGame game = new HelloWorldGame())
            {
                game.Run();
            }
        }
    }
}
