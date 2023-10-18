using System;

namespace NeuroWorms.Core
{
    public class Field
    {
        public int Width { get; }
        public int Height { get; }

        private readonly CellType[,] cells;

        public Field(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new CellType[width, height];
            Clear();
        }

        public void Clear()
        {
            Array.Clear(cells, (int)CellType.Empty, cells.Length);
        }

        public CellType this[int x, int y]
        {
            get => InField(x, y) ? cells[x, y] : CellType.Wall;
            set {
                if (InField(x, y))
                {
                    cells[x, y] = value;
                }
            }
        }

        public CellType this[Position position]
        {
            get => this[position.X, position.Y];
            set => this[position.X, position.Y] = value;
        }

        public Position RoundUp(Position position)
        {
            var x = position.X;
            var y = position.Y;
            if (x < 0)
            {
                x = Width - 1;
            }
            else if (x >= Width)
            {
                x = 0;
            }

            if (y < 0)
            {
                y = Height - 1;
            }
            else if (y >= Height)
            {
                y = 0;
            }

            return new Position(x, y);
        }

        private bool InField(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
