using HelloWorld.Misc;
using Microsoft.Xna.Framework;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Misc;
using Phantom.Shapes;
using System;
using System.Runtime.CompilerServices;

namespace HelloWorld
{
    public class World : Component
    {
        public const int TileSize = 15;

        public const float Freq = 0.1f;

        public const short NONE = 0;
        public const short WALL = 1;
        public const short TREE = 2;

        public const int FrameGrass = 5;
        public const int FrameDirt = 6;
        public const int FrameStone = 7;
        public const int FrameSalt = 8;

        public Vector2 SpawnPoint
        {
            get
            {
                return new Vector2(this.spawnTile % this.size, this.spawnTile / this.size) * TileSize + Vector2.One * TileSize * .5f;
            }
        }

        private int size;
        private int iterations;
        private int seed;
        private Random rand;
        private int[] cells;
        private int[] rooms;
        private int[] groundFrames = { FrameGrass, FrameDirt, FrameStone, FrameSalt };
        private int spawnTile;

        public World(int size = 160, int iterations=4, int seed=0)
        {
            this.size = size;
            this.iterations = iterations;
            this.seed = seed;
            this.rand = new Random(seed);
            this.cells = new int[size * size];
        }

        public void Generate()
        {
            var length = this.cells.Length;
            for (int i = 0; i < length; i++)
                this.cells[i] = rand.NextDouble() > 0.55 ? WALL : NONE;

            // Execute 4/5 rule:
            var a = this.cells;
            var b = new int[length];
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

            // Wall in edges:
            var last = (this.size - 1) * this.size;
            for (int x = 0; x < this.size; x++)
            {
                this.cells[x] = WALL;
                this.cells[last + x] = WALL;

                this.cells[x * this.size] = WALL;
                this.cells[x * this.size + this.size-1] = WALL;
            }

            // CCL, find rooms:
            int largest;
            this.rooms = CCL.ccl(this.cells, this.size, out largest);

            if (rand.NextDouble() > 0.1f)
            {
                var tmp = this.groundFrames[largest % this.groundFrames.Length];
                this.groundFrames[largest % this.groundFrames.Length] = this.groundFrames[0];
                this.groundFrames[0] = tmp;
            }


            var dress = new short[] {
                517, 518, 519,
                523, 524, 525,
                527, 528,

            };

            
            for (int i = 0; i < length; i++)
            {
                if (this.groundFrames[this.rooms[i] % 4] == FrameGrass)
                {
                    var isNone = a[i] == NONE;
                    var numberOfNeighborsAreNone = CountNeighnors(this.cells, i, NONE);
                    if (isNone && numberOfNeighborsAreNone >= 5 && rand.NextDouble() > 0.85)
                    {
                        this.cells[i] = dress[rand.Next(dress.Length)];
                    }
                }
                if (a[i] == NONE && this.rooms[i] == largest)
                {
                    this.spawnTile = i;
                }
            }


            this.Populate(this.Parent);
        }

        private void Populate(Component component)
        {
            Vector2 p;
            Vector2 offset = Vector2.One * TileSize * .5f;
            for (int i = 0; i < this.cells.Length; i++)
            {
                Entity e;
                p = offset + new Vector2(i % this.size, i / this.size) * TileSize;
                if (this.cells[i] == WALL && this.CountNeighnors(this.cells, i, WALL) < 8)
                {
                    e = new Entity(p);
                    e.InitiateCollision = false;
                    e.AddComponent(new OABB(offset));
                }
                else if (this.cells[i] > TREE)
                {
                    e = new Entity(p+new Vector2(0,4));
                    e.InitiateCollision = false;
                    e.AddComponent(new Circle(4));
                }
                else
                {
                    continue;
                }
                component.AddComponent(e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CountNeighnors(int[] data, int ix, int type)
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
        private int ValueByCoords(int[] data, int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.size || y >= this.size)
                return WALL;
            return data[(y * this.size) + x];
        }
    
        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            Vector2 diagonal = new Vector2(info.Width, info.Height) * .5f * (1 / info.Camera.Zoom);
            Vector2 topleft = info.Camera.Position - diagonal - Vector2.One * TileSize;
            Vector2 bottomright = info.Camera.Position + diagonal + Vector2.One * TileSize;

            Vector2 p;
            Vector2 offset = Vector2.One * TileSize * .5f;
            Random r = new Random(this.seed);
            for (int i = 0; i < this.cells.Length; i++)
            {
                var room = this.rooms[i];
                p = offset + new Vector2(i % this.size, i / this.size)*(TileSize);
                if (p.X < topleft.X || p.Y < topleft.Y || p.X > bottomright.X || p.Y > bottomright.Y)
                {
                    continue;
                }
                switch (this.cells[i])
                {
                    case NONE:
                        var tile = this.groundFrames[this.rooms[i] % this.groundFrames.Length];
                        Sprites.Roguelike.RenderFrame(info, tile + (i * 1327 % 2 * 56), p, 0, 1f);
                        break;

                    case WALL:
                        // 26
                        Sprites.Roguelike.RenderFrame(info, (2 + i * 1327 % 2) * 56 + 6, p, 0, 1f);
                        break;

                    default:
                        tile = this.groundFrames[this.rooms[i] % this.groundFrames.Length];
                        Sprites.Roguelike.RenderFrame(info, tile + (i * 1327 % 2 * 56), p, 0, 1f);
                        Sprites.Roguelike.RenderFrame(info, this.cells[i], p, 0, .8f);
                        break;
                }
            }
            base.Render(info);
        }
    }
}
