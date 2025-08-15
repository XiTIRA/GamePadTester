using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Xitira.GamePadTester;

public class ColorModeButton
{
    private Texture2D _texture;
    private Vector2 _position;
    public Color CurrentColor;
    public Color AltColor;
    private MouseState _prevMouse;
    private TouchLocation _prevTouch;
    private GamePadTester _game;

    private float _lerpPosition = 1f;
    private float _lerpSpeed;


    private Color _lerpFrom = Color.AntiqueWhite;
    private Color _lerpTo = Color.LightSlateGray;

    public ColorModeButton(Texture2D texture, Vector2 position, GamePadTester game)
    {
        _position = position;
        _texture = texture;
        CurrentColor = _lerpTo;
        AltColor = _lerpFrom;
        _game = game;
    }

    public void Update(GameTime gameTime)
    {
        var ms = Mouse.GetState();
        var touch = TouchPanel.GetState().FirstOrDefault();

        var mouseClick = ms.LeftButton == ButtonState.Pressed & _prevMouse.LeftButton != ButtonState.Pressed;
        var touchRelease = touch.State == TouchLocationState.Released;

        if (mouseClick || touchRelease)
        {
            var mousePos = _game.ConvertPoint(ms.Position);
            var touchPos = _game.ConvertPoint(touch.Position.ToPoint());;
            var rect = new Rectangle((int)_position.X, (int)_position.Y, _texture.Width, _texture.Height);

            if (rect.Contains(mousePos) | rect.Contains(touchPos))
            {
                if (_lerpSpeed > 0)
                    _lerpSpeed = -1f;
                else if (_lerpSpeed < 0)
                    _lerpSpeed = 1f;
                else
                {
                    if (CurrentColor == _lerpFrom)
                        _lerpSpeed = 1f;
                    else
                        _lerpSpeed = -1f;
                }
            }
        }

        _prevMouse = ms;
        _prevTouch = touch;

        if (_lerpSpeed == 0) return;

        _lerpPosition += _lerpSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 5f;
        if (_lerpPosition > 1f)
        {
            _lerpPosition = 1f;
            _lerpSpeed = 0;
        }
        else if (_lerpPosition < 0f)
        {
            _lerpPosition = 0f;
            _lerpSpeed = 0;
        }
        CurrentColor = Color.Lerp(_lerpFrom, _lerpTo, _lerpPosition);
        AltColor = Color.Lerp(_lerpTo, _lerpFrom, _lerpPosition);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            _position,
            AltColor
        );
    }
}