using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xitira.GamePadTester;

public class Atlas
{
    public Texture2D Texture { get; set; }
    public Dictionary<string, Rectangle> Rectangles { get; set; }
    public Point TileSize { get; set; }

    public Atlas(Texture2D texture, Point tileSize)
    {
        Texture = texture;
        TileSize = tileSize;

        int divSizeX = texture.Width / tileSize.X;
        int divSizeY = texture.Height / tileSize.Y;

        Rectangles = new();

        for (int x = 0; x < divSizeX; x++)
        {
            for (int y = 0; y < divSizeY; y++)
            {
                Rectangle rect = new(x * tileSize.X, y * tileSize.Y, tileSize.X, tileSize.Y);
                Rectangles.Add($"{x}:{y}", rect);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, string region, Point position, Color color, bool origin = false)
    {
        var offset = Point.Zero;
        if (origin) offset = new Point(-8, -8);

        spriteBatch.Draw(
            Texture,
            new Rectangle(position.X + offset.X, position.Y + offset.Y, TileSize.X , TileSize.Y ),
            Rectangles[region],
            color,
            0f,
            Vector2.One,
            SpriteEffects.None,
            0f
        );
    }
}