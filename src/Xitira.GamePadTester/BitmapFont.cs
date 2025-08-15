using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xitira.GamePadTester;

public class BitmapFont
{
    public readonly SpriteFont Font;

    public BitmapFont(Texture2D texture, string descriptorFile)
    {
        var glyphs = new List<Glyph>();

        var stream = TitleContainer.OpenStream(descriptorFile);

        if (stream is null) return;

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        
        foreach (var line in content.Split('\n'))
        {
            if (!line.StartsWith("char ")) continue;

            var parts = line.Split([ ' ' ]);


            var character = (char)int.Parse(parts[1][3..]);
            var x = int.Parse(parts[2][2..]);
            var y = int.Parse(parts[3][2..]);
            var width = int.Parse(parts[4][6..]);
            var height = int.Parse(parts[5][7..]);
            var xOffset = int.Parse(parts[6][8..]);
            var yOffset = int.Parse(parts[7][8..]);

            glyphs.Add( new Glyph
            {
                Character = character,
                SourceRectangle = new Rectangle(x, y, width, height),
                XOffset = xOffset,
                YOffset = yOffset
            });
        }

        List<char> characters = new List<char>();
        List<Rectangle> rectangles = new List<Rectangle>();
        List<Rectangle> cropings = new List<Rectangle>();
        List<Vector3> kerning = new List<Vector3>();

        foreach (var glyph in glyphs)
        {
            characters.Add(glyph.Character);
            rectangles.Add(glyph.SourceRectangle);
            cropings.Add(new Rectangle(glyph.XOffset, glyph.YOffset, glyph.SourceRectangle.Width,
                glyph.SourceRectangle.Height));
            kerning.Add(new Vector3(0, glyph.SourceRectangle.Width, 0));
        }

        Font = new SpriteFont(texture, rectangles, cropings, characters, 2, 0, kerning, '*');
    }
}