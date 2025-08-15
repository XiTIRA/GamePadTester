using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Xitira.GamePadTester;

public class TestAxis
{
    private readonly Texture2D _texture;
    private readonly Texture2D _pixel;
    private Point _position;
    private readonly string _axis;
    private Rectangle _rect;
    private readonly bool _fromCenter;
    private readonly bool _horizontal = false;

    public Vector2 Position => _position.ToVector2();

    public TestAxis(Texture2D texture, GraphicsDevice graphicsDevice, Point position, string axis,
        bool fromCenter = true)
    {
        _position = position;
        _texture = texture;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _axis = axis;
        _fromCenter = fromCenter;
        //horizontal = axis.Contains("X") ?  true :  false;
        if (_horizontal) _position.Y += 20;
        if (_horizontal) _position.X -= 12;
    }

    private float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromRange = fromMax - fromMin;
        var toRange = toMax - toMin;
        var valueScaled = (value - fromMin) / fromRange;
        return toMin + (valueScaled * toRange);
    }

    public void Update(GamePadState state)
    {
        var gs = state;
        float scale = 0f;

        switch (_axis)
        {
            case "RightTrigger":
                scale = gs.Triggers.Right;
                break;
            case "LeftTrigger":
                scale = gs.Triggers.Left;
                break;
            case "RightStickX":
                scale = gs.ThumbSticks.Right.X;
                break;
            case "RightStickY":
                scale = -gs.ThumbSticks.Right.Y;
                break;
            case "LeftStickX":
                scale = gs.ThumbSticks.Left.X;
                break;
            case "LeftStickY":
                scale = -gs.ThumbSticks.Left.Y;
                break;
        }

        var range = _fromCenter ? 15f : 30f;
        var mappedHeight = MapValue(scale, -1f, 1f, -range, range);
        float offset = mappedHeight < 0 ? mappedHeight : 0f;
        if (!_fromCenter) offset = -15;
        _rect = new Rectangle(_position.X, _position.Y + 16 + (int)offset, 8, (int)Math.Abs(mappedHeight));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float rotation = 0f;
        if (_horizontal)
        {
            rotation = MathHelper.ToRadians(270f);
        }

        spriteBatch.Draw(
            _pixel,
            _rect,
            null,
            Color.LightCoral,
            rotation,
            Vector2.Zero,
            SpriteEffects.None,
            0f
        );
        spriteBatch.Draw(
            _texture,
            _position.ToVector2(),
            null,
            Color.White,
            rotation,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            0f
        );
    }
}