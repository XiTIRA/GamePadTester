using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Framework.Utilities;

namespace Xitira.GamePadTester;

public class GamePadTester : Game
{
    private readonly string _version = "3.8.4"  ;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Rectangle _finalDestination;
    private readonly Rectangle _renderDestination = new (0, 0, 272, 160);
    private readonly Rectangle _preferredRenderSize = new (0, 0, 272 * 4, 160 * 4);
    private bool _isResizing;

    private List<TestAxis> _axes = new();

    private SpriteFont _font;
    private SpriteFont _bigFont;
    private Texture2D _logo;
    private Texture2D _frame;
    private Texture2D _tileBackground;

    private GamePadSwitcher _switcher;
    private ColorModeButton _colorModeButton;
    private RenderTarget2D _renderTarget;
    private GamePadVisualiser _visualiser;

    private string _gamePadName = "";

    private readonly float _scrollSpeed = 20f; // Pixels per second
    private float _scrollOffset ;

    public GamePadTester()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        if (PlatformInfo.MonoGamePlatform == MonoGamePlatform.Android)
        {
            _graphics.IsFullScreen = true;
        }
        else
        {
            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = _preferredRenderSize.Width;
            _graphics.PreferredBackBufferHeight = _preferredRenderSize.Height;
            _graphics.ApplyChanges();
        }

        Window.ClientSizeChanged += (sender, args) =>
        {
            if (!_isResizing && Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _isResizing = true;
                CalculateRenderDestination();
                _isResizing = false;
            }
        };

        CalculateRenderDestination();
        Window.Title = $"GamePad Tester for MG {_version}";

        base.Initialize();
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _renderTarget = new RenderTarget2D(GraphicsDevice, _renderDestination.Width, _renderDestination.Height);

        var bitmapFont = new BitmapFont(Content.Load<Texture2D>("Unnamed"), "Content/Unnamed.fnt");
        var dogicaFont = new BitmapFont(Content.Load<Texture2D>("dogica"), "Content/dogica.fnt");
        _font = dogicaFont.Font;
        _bigFont = bitmapFont.Font;

        _tileBackground = Content.Load<Texture2D>("tile");
        var atlas = new Atlas(Content.Load<Texture2D>("buttons"), new Point(16, 16));
        _logo = Content.Load<Texture2D>("mg");
        _frame = Content.Load<Texture2D>("Frame");
        var onOff = new Atlas(Content.Load<Texture2D>("OnOff"), new Point(48, 16));
        var dot = Content.Load<Texture2D>("WhiteDot");

        var gamePadTexture = Content.Load<Texture2D>("xboxoutline");

        _switcher = new GamePadSwitcher(onOff, new Vector2(248, 45), this, _font);
        _colorModeButton = new ColorModeButton(dot, new Vector2(250, 8), this);

        _visualiser = new GamePadVisualiser(new Point(35, 20), gamePadTexture);
        _visualiser.AddButton(new TestButton(atlas, "1:2",  Buttons.X));
        _visualiser.AddButton(new TestButton(atlas, "1:3",  Buttons.A));
        _visualiser.AddButton(new TestButton(atlas, "1:4",  Buttons.Y));
        _visualiser.AddButton(new TestButton(atlas, "1:5",  Buttons.B));
        _visualiser.AddButton(new TestButton(atlas, "1:6",  Buttons.Back));
        _visualiser.AddButton(new TestButton(atlas, "1:7",  Buttons.Start));
        _visualiser.AddButton(new TestButton(atlas, "8:5",  Buttons.DPadUp));
        _visualiser.AddButton(new TestButton(atlas, "8:6", Buttons.DPadLeft));
        _visualiser.AddButton(new TestButton(atlas, "9:5",  Buttons.DPadDown));
        _visualiser.AddButton(new TestButton(atlas, "9:6",  Buttons.DPadRight));
        _visualiser.AddButton(new TestButton(atlas, "21:3",  Buttons.LeftTrigger));
        _visualiser.AddButton(new TestButton(atlas, "21:4",  Buttons.RightTrigger));
        _visualiser.AddButton(new TestButton(atlas, "21:5",  Buttons.LeftShoulder));
        _visualiser.AddButton(new TestButton(atlas, "21:6",  Buttons.RightShoulder));
        _visualiser.AddButton(new TestButton(atlas, "7:33",  Buttons.LeftStick));
        _visualiser.AddButton(new TestButton(atlas, "7:34",  Buttons.RightStick));
        _visualiser.UpdateButtonPositions();

        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(14, 16), "LeftTrigger",
            false));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(24, 16), "RightTrigger",
            false));

        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(5, 110), "LeftStickX"));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(14, 110), "LeftStickY"));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(24, 110), "RightStickX"));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(33, 110), "RightStickY"));
    }

    protected override void Update(GameTime gameTime)
    {
        if (_isResizing) return;

        _colorModeButton.Update(gameTime);
        _switcher.Update();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _scrollOffset += _scrollSpeed * deltaTime;
        _scrollOffset %= _tileBackground.Height;

        var gs = _switcher.ActivePadState;
        _visualiser.Update(gs, _colorModeButton.AltColor);

        foreach (var axis in _axes)
            axis.Update(gs);

        var capabilities = GamePad.GetCapabilities(_switcher.ActiveGamePad);
        if (!capabilities.IsConnected)
            _gamePadName = "Not Connected";
        else
            _gamePadName = capabilities.DisplayName ?? "Connected";

        base.Update(gameTime);
    }


    private void CalculateRenderDestination()
    {
        Point size = GraphicsDevice.Viewport.Bounds.Size;
        _finalDestination = new Rectangle(0, 0, size.X, size.Y);
    }

    public Point ConvertPoint(Point point)
    {
        float scaleX = (float)_renderDestination.Width / _finalDestination.Width;
        float scaleY = (float)_renderDestination.Height / _finalDestination.Height;
        return new Point(
            (int)(point.X * scaleX),
            (int)(point.Y * scaleY)
        );
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(_colorModeButton.CurrentColor);

        _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

        _spriteBatch.Draw(
            _tileBackground,
            _renderDestination,
            new Rectangle((int)_scrollOffset, (int)_scrollOffset, _renderDestination.Width, _renderDestination.Height),
            Color.White * 0.02f
        );

        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _colorModeButton.Draw(_spriteBatch);
        _switcher.Draw(_spriteBatch);

        _visualiser.Draw(_spriteBatch);

        foreach (var axis in _axes)
        {
            axis.Draw(_spriteBatch);
        }



        _spriteBatch.Draw(_frame, Vector2.Zero, Color.White * 0.5f);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, _finalDestination, Color.White);
        _spriteBatch.Draw(_logo, new Vector2(_finalDestination.Width-85, _finalDestination.Height-55), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None,
            0f );

        Vector2 textSize = _font.MeasureString(_gamePadName);
        float scalex =   (_finalDestination.Width/2f)/textSize.X;
        float scaley =  (_finalDestination.Height/2f)/textSize.Y ;
        float scale = Math.Min(scalex, scaley);

        if (scale < 3f) scale = 3f;
        if (scale > 5f) scale = 5f;

        _spriteBatch.DrawString(
            _font,
            _gamePadName,
            new Vector2(_finalDestination.Width / 2f , _finalDestination.Height-(textSize.Y*scale)),
            _colorModeButton.AltColor,
            0f,
            new Vector2(textSize.X/2,textSize.Y/2),
            scale,
            SpriteEffects.None, 0f);

        textSize = _font.MeasureString($"V {_version}");
        _spriteBatch.DrawString(_bigFont, $"V {_version}", new Vector2(_finalDestination.Width-10-textSize.X, _finalDestination.Height - 20), _colorModeButton.AltColor);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

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

public class GamePadVisualiser
{
    public List<TestButton> ButtonList = new();
    public Point Offset;
    public Texture2D Background;
    public Color OverlayColor = Color.White ;

    public GamePadVisualiser(Point offset, Texture2D background)
    {
        Offset = offset;
        Background = background;
    }

    public void AddButton(TestButton button)
    {
        ButtonList.Add(button);
    }

    public void Update(GamePadState gamePadState, Color overlayColor)
    {
        OverlayColor = overlayColor;

        foreach (var btn in ButtonList)
        {
            btn.Update(gamePadState);
        }

#if DEBUG
        UpdateButtonPositions();
#endif
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Background, Offset.ToVector2(), OverlayColor);
        foreach (var btn in ButtonList)
        {
            btn.Draw(spriteBatch, Offset);
        }
    }

    public void UpdateButtonPositions()
    {
        foreach (var btn in ButtonList)
        {
            switch (btn.ScanButton)
            {
                case Buttons.A:
                    btn.Position = new Point(147, 52);
                    break;
                case Buttons.B:
                    btn.Position = new Point(157, 41);
                    break;
                case Buttons.X:
                    btn.Position = new Point(137, 41);
                    break;
                case Buttons.Y:
                    btn.Position = new Point(147, 30);
                    break;
                case Buttons.Back:
                    btn.Position = new Point(84, 25);
                    break;
                case Buttons.Start:
                    btn.Position = new Point(117, 25);
                    break;
                case Buttons.LeftStick:
                    btn.Position = new Point(57, 33);
                    break;
                case Buttons.RightStick:
                    btn.Position = new Point(123, 62);
                    break;
                case Buttons.LeftShoulder:
                    btn.Position = new Point(57, 8);
                    break;
                case Buttons.RightShoulder:
                    btn.Position = new Point(144, 8);
                    break;
                case Buttons.LeftTrigger:
                    btn.Position = new Point(57, -3);
                    break;
                case Buttons.RightTrigger:
                    btn.Position = new Point(144, -3);
                    break;
                case Buttons.DPadUp:
                    btn.Position = new Point(78, 52);
                    break;
                case Buttons.DPadDown:
                    btn.Position = new Point(78, 66);
                    break;
                case Buttons.DPadLeft:
                    btn.Position = new Point(71, 60);
                    break;
                case Buttons.DPadRight:
                    btn.Position = new Point(85, 60);
                    break;

            }
        }
    }
}

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

public class TestButton
{
    private Atlas _atlas;
    private string _region;
    public Point Position;
    private Color _currentColour = Color.White;
    public Buttons ScanButton;

    public TestButton(Atlas atlas, string region,  Buttons scanButton)
    {
        _atlas = atlas;
        _region = region;
        Position = Point.Zero;
        ScanButton = scanButton;
    }

    public void Update(GamePadState state)
    {
        _currentColour = state.IsButtonDown(ScanButton) ? Color.Gray : Color.White;
    }

    public void Draw(SpriteBatch spriteBatch, Point offset)
    {
        _atlas.Draw(spriteBatch, _region, Position + offset, _currentColour, true);
    }
}

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
                scale = gs.ThumbSticks.Right.Y;
                break;
            case "LeftStickX":
                scale = gs.ThumbSticks.Left.X;
                break;
            case "LeftStickY":
                scale = gs.ThumbSticks.Left.Y;
                break;
        }

        var min = _fromCenter ? -1f : 0f;
        var mappedHeight = MapValue(scale, min, 1f, 0f, 30f);

        if (_horizontal)
            _rect = new Rectangle(_position.X + 1, _position.Y , 8, (int)mappedHeight);
        else
            _rect = new Rectangle(_position.X, _position.Y + 1, 8, (int)mappedHeight);
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

public struct Glyph
{
    public char Character;
    public Rectangle SourceRectangle;
    public int XOffset;
    public int YOffset;
}