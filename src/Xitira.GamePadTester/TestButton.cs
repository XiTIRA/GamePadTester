using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Xitira.GamePadTester;

public class TestButton
{
    private Atlas _atlas;
    private string _region;
    public Point Position;
    private Color _currentColour = Color.White;
    public Buttons ScanButton;
    public bool IsLeftStick;
    public bool IsRightStick;
    
    public Vector2 StickPos = Vector2.Zero;

    public TestButton(Atlas atlas, string region,  Buttons scanButton, bool isLeftStick = false, bool isRightStick = false)
    {
        _atlas = atlas;
        _region = region;
        Position = Point.Zero;
        ScanButton = scanButton;
        IsLeftStick = isLeftStick;
        IsRightStick = isRightStick;
    }

    public void Update(GamePadState state)
    {
        _currentColour = state.IsButtonDown(ScanButton) ? Color.Gray : Color.White;
        
        if (IsLeftStick || IsRightStick)
        {
            var scale = IsLeftStick ? state.ThumbSticks.Left : state.ThumbSticks.Right;
            scale.Y = -scale.Y;
            var pos = Position.ToVector2();
            var reach = 15f;
            var mappedHeight = (scale.Y * reach) + pos.Y;
            var mappedWidth = (scale.X * reach) + pos.X;
            StickPos = new Vector2(mappedWidth, mappedHeight);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Point offset)
    {
        _atlas.Draw(spriteBatch, _region, Position + offset, _currentColour, true);

        if (IsLeftStick || IsRightStick)
        {
            _atlas.Draw(spriteBatch, _region, StickPos.ToPoint() + offset , _currentColour*.5f, true);
        }
    }
}