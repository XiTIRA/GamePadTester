using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Xitira.GamePadTester;

public class GamePadSwitcher
{
    private readonly GamePadTester _game;
    private readonly Atlas _atlas;
    private const string On = "0:1";
    private const string Off = "0:0";
    private readonly float _transparency = 0.25f;
    private const int YOffset = 4;
    private const int XOffset = -5;
    public PlayerIndex ActiveGamePad = PlayerIndex.One;
    public GamePadState ActivePadState => GamePad.GetState(ActiveGamePad);

    private MouseState _prevMouse ;
    private TouchLocation _prevTouch;
    private bool[] _gamepadStates = [false, false, false, false];
    private SpriteFont _font;

    private List<Rectangle> _buttons = new();

    public GamePadSwitcher(Atlas atlas, Vector2 position, GamePadTester game, SpriteFont font)
    {
        _atlas = atlas;
        _game = game;
        _font = font;

        _buttons.Add(new Rectangle((int)position.X, (int)position.Y + (16 + YOffset) * 0, 48, 16));
        _buttons.Add(new Rectangle((int)position.X, (int)position.Y + (16 + YOffset) * 1, 48, 16));
        _buttons.Add(new Rectangle((int)position.X, (int)position.Y + (16 + YOffset) * 2, 48, 16));
        _buttons.Add(new Rectangle((int)position.X, (int)position.Y + (16 + YOffset) * 3, 48, 16));
    }

    public void Update()
    {
        _gamepadStates[0] = GamePad.GetState(PlayerIndex.One).IsConnected;
        _gamepadStates[1] = GamePad.GetState(PlayerIndex.Two).IsConnected;
        _gamepadStates[2] = GamePad.GetState(PlayerIndex.Three).IsConnected;
        _gamepadStates[3] = GamePad.GetState(PlayerIndex.Four).IsConnected;

        var ms = Mouse.GetState();
        if (ms.LeftButton == ButtonState.Pressed & _prevMouse.LeftButton != ButtonState.Pressed)
        {
            var pos = _game.ConvertPoint(ms.Position);

            if (_buttons[0].Contains(pos))
                ActiveGamePad = PlayerIndex.One;
            if (_buttons[1].Contains(pos))
                ActiveGamePad = PlayerIndex.Two;
            if (_buttons[2].Contains(pos))
                ActiveGamePad = PlayerIndex.Three;
            if (_buttons[3].Contains(pos))
                ActiveGamePad = PlayerIndex.Four;
        }

        var tc = TouchPanel.GetCapabilities();
        if (tc.IsConnected)
        {
            var touch = TouchPanel.GetState().FirstOrDefault();
            var pos = _game.ConvertPoint(touch.Position.ToPoint());
            if (_buttons[0].Contains(pos))
                ActiveGamePad = PlayerIndex.One;
            if (_buttons[1].Contains(pos))
                ActiveGamePad = PlayerIndex.Two;
            if (_buttons[2].Contains(pos))
                ActiveGamePad = PlayerIndex.Three;
            if (_buttons[3].Contains(pos))
                ActiveGamePad = PlayerIndex.Four;

            _prevTouch = touch;
        }

        _prevMouse = ms;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float alpha;

        alpha = ActiveGamePad == PlayerIndex.One ? 1f : _transparency;
        _atlas.Draw(spriteBatch, _gamepadStates[0] ? On : Off,
            new Point(_buttons[0].X + (ActiveGamePad == PlayerIndex.One ? XOffset : 0), _buttons[0].Y),
            Color.White * alpha);
        spriteBatch.DrawString(_font, "1", new Vector2(_buttons[0].X + 13, _buttons[0].Y +3 ), Color.White * alpha);

        alpha = ActiveGamePad == PlayerIndex.Two ? 1f : _transparency;
        _atlas.Draw(spriteBatch, _gamepadStates[1] ? On : Off,
            new Point(_buttons[1].X + (ActiveGamePad == PlayerIndex.Two ? XOffset : 0), _buttons[1].Y),
            Color.White * alpha);
        spriteBatch.DrawString(_font, "2", new Vector2(_buttons[1].X + 13, _buttons[1].Y +3), Color.White * alpha);

        alpha = ActiveGamePad == PlayerIndex.Three ? 1f : _transparency;
        _atlas.Draw(spriteBatch, _gamepadStates[2] ? On : Off,
            new Point(_buttons[2].X + (ActiveGamePad == PlayerIndex.Three ? XOffset : 0), _buttons[2].Y),
            Color.White * alpha);
        spriteBatch.DrawString(_font, "3", new Vector2(_buttons[2].X + 13, _buttons[2].Y +3 ), Color.White * alpha);

        alpha = ActiveGamePad == PlayerIndex.Four ? 1f : _transparency;
        _atlas.Draw(spriteBatch, _gamepadStates[3] ? On : Off,
            new Point(_buttons[3].X + (ActiveGamePad == PlayerIndex.Four ? XOffset : 0), _buttons[3].Y),
            Color.White * alpha);
        spriteBatch.DrawString(_font, "4", new Vector2(_buttons[3].X + 13, _buttons[3].Y +3 ), Color.White * alpha);
    }
}