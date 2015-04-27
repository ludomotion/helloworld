using Microsoft.Xna.Framework;
using Phantom.Core;
using System;
using System.Runtime.CompilerServices;

namespace HelloWorld
{
    public class World : Component
    {
        const float Freq = 0.1f;

        const short NONE = 0;
        const short WALL = 1;
        const short TREE = 2;

        private int size;
        private int iterations;
        private int seed;
        private Random rand;
        private short[] cells;

        public World(int size = 160, int iterations=4, int seed=0)
        {
            this.size = size;
            this.iterations = iterations;
            this.seed = seed;
            this.rand = new Random(seed);
            this.cells = new short[size * size];
        }

        public void Generate()
        {
            var length = this.cells.Length;
            for (int i = 0; i < length; i++)
                this.cells[i] = rand.NextDouble() > 0.55 ? WALL : NONE;

            var a = this.cells;
            var b = new short[length];
            for (int _ = 0; _ < this.iterations; _++)
            {
                for (int i = 0; i < length; i++)
                {
                    var numberOfNeighborsAreWall = CountNeighnors(a, i, WALL);
                    var isWall = a[i] == WALL;
                    b[i] = ((isWall && numberOfNeighborsAreWall >= 4) || (!isWall && numberOfNeighborsAreWall >= 5)) ? WALL : NONE;
                }
                var c = a; a = b; b = c;
            }
            this.cells = a;

            var last = (this.size - 1) * this.size;
            for (int x = 0; x < this.size; x++)
            {
                this.cells[x] = WALL;
                this.cells[last + x] = WALL;

                this.cells[x * this.size] = WALL;
                this.cells[x * this.size + this.size-1] = WALL;
            }


           
            var dress = new short[] {
                517, 518, 519,
                523, 524, 525,
                527, 528,

            };
            for (int i = 0; i < length; i++)
            {
                var isNone = a[i] == NONE;
                var numberOfNeighborsAreNone = CountNeighnors(this.cells, i, NONE);
                if (isNone && numberOfNeighborsAreNone >= 5 && rand.NextDouble() > 0.85)
                {
                    this.cells[i] = dress[rand.Next(dress.Length)];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CountNeighnors(short[] data, int ix, int type)
        {
            var x = ix % this.size;
            var y = ix / this.size;

            int[] offsets = {
                -1, -1,
                0, -1,
                1, -1,
                -1, 0,
                1, 0,
                -1, 1,
                0, 1,
                1, 1,
            };

            int count = 0;
            for (int i = 0; i < offsets.Length; i += 2)
            {
                if (ValueByCoords(data, x + offsets[i], y + offsets[i+1]) == type)
                    count += 1;
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ValueByCoords(short[] data, int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.size || y >= this.size)
                return WALL;
            return data[(y * this.size) + x];
        }
    
        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            Vector2 p;
            Vector2 offset = new Vector2(8, 8);
            Random r = new Random(this.seed);
            for (int i = 0; i < this.cells.Length; i++)
            {
                p = offset + new Vector2(i % this.size, i / this.size)*16;
                switch (this.cells[i])
                {
                    case NONE:
                        Sprites.Roguelike.RenderFrame(info, 5+(r.Next(2)*56), p, 0, 1f);
                        break;

                    case WALL:
                        // 26
                        Sprites.Roguelike.RenderFrame(info, (2 + r.Next(2)) * 56 + 6, p, 0, 1f);
                        break;

                    default:
                        Sprites.Roguelike.RenderFrame(info, 5 + (r.Next(2) * 56), p, 0, 1f);
                        Sprites.Roguelike.RenderFrame(info, this.cells[i], p, 0, 1f);
                        break;
                }
            }
            base.Render(info);
        }
    }
}
