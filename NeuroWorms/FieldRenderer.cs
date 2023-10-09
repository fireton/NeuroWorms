using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeuroWorms.Core;
using System;

namespace NeuroWorms
{
    public class FieldRenderer
    {
        private readonly Texture2D whitePixel;
        private const int CellSize = 4;
        private const int CellPadding = 1;

        public FieldRenderer(GraphicsDevice graphicsDevice)
        {
            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });
        }
        public void Render(int posX, int posY, Field field, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Constants.FieldWidth; x++)
            {
                for (int y = 0; y < Constants.FieldHeight; y++)
                {
                    var cellType = field[x, y];
                    var color = GetColor(cellType);
                    var rectangle = new Rectangle(x * (CellSize + CellPadding) + posX, y * (CellSize + CellPadding) + posY, CellSize, CellSize);
                    spriteBatch.Draw(whitePixel, rectangle, color);
                }
            }
        }

        private static Color GetColor(CellType cellType)
        {
            return cellType switch
            {
                CellType.Empty => Color.White,
                CellType.WormHead => Color.DarkCyan,
                CellType.WormBody => Color.Navy,
                CellType.Food => Color.OrangeRed,
                _ => throw new NotImplementedException(),
            };
        }
    }
}