﻿using System;

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

        private bool InField(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}