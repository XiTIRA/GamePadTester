using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;

namespace Xitira.GamePadTester;

public class GamePadTester : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Rectangle _finalDestination;
    private Rectangle _renderDestination = new Rectangle(0,0,272,160);
    private bool _isResizing;
    private Atlas _atlas;

    private List<TestButton> _buttons = new();
    private List<TestAxis> _axes = new();

    private SpriteFont _font;
    private Texture2D _logo;

    private Texture2D _frame;

    private int[] _buttonStates = [0,0,0,0];
    private int _buttonSelected = 0;

    public Color BGCoolor = Color.LightSlateGray;

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
        Window.Title = "GamePad Tester for MG 3.1.3";

        base.Initialize();
    }

    private GamePadSwitcher _switcher;
    private ColorModeButton _colorModeButton;
    private RenderTarget2D _renderTarget;
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _renderTarget = new RenderTarget2D(GraphicsDevice, _renderDestination.Width,_renderDestination.Height);

        _font = Content.Load<SpriteFont>("defaultFont");
        _atlas = new Atlas(Content.Load<Texture2D>("buttons"), new Point(16, 16));
        _logo = Content.Load<Texture2D>("mg");
        _frame = Content.Load<Texture2D>("Frame");
        _onOff = new Atlas(Content.Load<Texture2D>("OnOff"), new Point(48, 16));
        _dot = Content.Load<Texture2D>("WhiteDot");

        _switcher = new GamePadSwitcher(_onOff, new Vector2(248,45), this);
        _colorModeButton = new ColorModeButton(_dot,new Vector2(250,8), this);

        _buttons.Add(new TestButton(_atlas, "1:2", new Point(196,96), Buttons.X));
        _buttons.Add(new TestButton(_atlas, "1:3", new Point(208,108), Buttons.A));
        _buttons.Add(new TestButton(_atlas, "1:4", new Point(208,84), Buttons.Y));
        _buttons.Add(new TestButton(_atlas, "1:5", new Point(220,96), Buttons.B));
        _buttons.Add(new TestButton(_atlas, "1:6", new Point(80,40), Buttons.Back));
        _buttons.Add(new TestButton(_atlas, "1:7", new Point(176,40), Buttons.Start));

        _buttons.Add(new TestButton(_atlas, "8:5", new Point(48,88), Buttons.DPadUp));
        _buttons.Add(new TestButton(_atlas, "8:6", new Point(40,96), Buttons.DPadLeft));
        _buttons.Add(new TestButton(_atlas, "9:5", new Point(48,102), Buttons.DPadDown));
        _buttons.Add(new TestButton(_atlas, "9:6", new Point(56,96), Buttons.DPadRight));

        _buttons.Add(new TestButton(_atlas, "21:3", new Point(48,32), Buttons.LeftTrigger));
        _buttons.Add(new TestButton(_atlas, "21:4", new Point(208,32), Buttons.RightTrigger));
        _buttons.Add(new TestButton(_atlas, "21:5", new Point(48,43), Buttons.LeftShoulder));
        _buttons.Add(new TestButton(_atlas, "21:6", new Point(208,43), Buttons.RightShoulder));
        _buttons.Add(new TestButton(_atlas, "21:7", new Point(128,48), Buttons.BigButton));
        _buttons.Add(new TestButton(_atlas, "7:33", new Point(112,96), Buttons.LeftStick));
        _buttons.Add(new TestButton(_atlas, "7:34", new Point(144,96), Buttons.RightStick));

        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(224+2, 32+6), "RightTrigger",false));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(32+4, 32+6), "LeftTrigger",false));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(160+8, 96), "RightStickX"));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(176, 96), "RightStickY"));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(96-8, 96), "LeftStickX"));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(96, 96), "LeftStickY"));

    }

    protected override void Update(GameTime gameTime)
    {

        if (_isResizing) return;

        _colorModeButton.Update();
        _switcher.Update();



        var gs = _switcher.ActivePadState;
        _down = string.Empty;
        foreach (var btn in _buttons)
        {
            btn.Update(gs, ref _down);
        }

        foreach (var axis in _axes)
            axis.Update(gs);
        
        var capabilities = GamePad.GetCapabilities(PlayerIndex.One);
        gamePadName = capabilities.DisplayName ?? "Unknown";

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
    
    private Texture2D _dot;

    private Atlas _onOff;
    private string _down = "";
private string gamePadName = "";
    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(BGCoolor); // antiquewhite,liteslategray

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _colorModeButton.Draw(_spriteBatch);

        _switcher.Draw(_spriteBatch);

        _spriteBatch.Draw(_frame, Vector2.Zero, Color.White*0.5f);

        foreach (var btn in _buttons)
            btn.Draw(_spriteBatch);

        foreach (var axis in _axes)
            axis.Draw(_spriteBatch);

        _spriteBatch.Draw(_logo, new Vector2(250, 140), null, Color.White,0f, Vector2.Zero, 0.05f, SpriteEffects.None, 0f );

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, _finalDestination, Color.White);
        _spriteBatch.DrawString(_font, $"Down: {_down}", new Vector2(16, 16), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);;
        _spriteBatch.DrawString(_font, $"Device: {gamePadName}", new Vector2(16, 32), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);;

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

public class GamePadSwitcher
{
    private GamePadTester _game;
    private Atlas _atlas;
    private const string _on = "0:1";
    private const string _off = "0:0";
    private float _transparency = 0.25f;
    private const int yOffset = 4;
    private const int xOffset = -5;
    private Vector2 _position;
    public PlayerIndex ActiveGamePad = PlayerIndex.One;
    public GamePadState ActivePadState => GamePad.GetState(ActiveGamePad);

    private MouseState _prevMouse = new MouseState();
    private bool[] _gamepadStates = [false,false,false,false];

    private List<Rectangle> _buttons = new();

    public GamePadSwitcher(Atlas atlas, Vector2 position, GamePadTester game)
    {
        _atlas = atlas;
        _position = position;
        _game = game;

        _buttons.Add(new Rectangle((int)_position.X, (int)_position.Y+(16+yOffset)*0, 48,16));
        _buttons.Add(new Rectangle((int)_position.X, (int)_position.Y+(16+yOffset)*1, 48,16));
        _buttons.Add(new Rectangle((int)_position.X, (int)_position.Y+(16+yOffset)*2, 48,16));
        _buttons.Add(new Rectangle((int)_position.X, (int)_position.Y+(16+yOffset)*3, 48,16));
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
            if(_buttons[1].Contains(pos))
                ActiveGamePad = PlayerIndex.Two;
            if(_buttons[2].Contains(pos))
                ActiveGamePad = PlayerIndex.Three;
            if(_buttons[3].Contains(pos))
                ActiveGamePad = PlayerIndex.Four;


        }_prevMouse = ms;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _atlas.Draw(spriteBatch, _gamepadStates[0] ? _on : _off, new Point(_buttons[0].X+(ActiveGamePad == PlayerIndex.One ? xOffset:0), _buttons[0].Y), ActiveGamePad == PlayerIndex.One ? Color.White : Color.White*0.25f);
        _atlas.Draw(spriteBatch, _gamepadStates[1] ? _on : _off, new Point(_buttons[1].X+(ActiveGamePad == PlayerIndex.Two ? xOffset:0), _buttons[1].Y), ActiveGamePad == PlayerIndex.Two ? Color.White : Color.White*0.25f);
        _atlas.Draw(spriteBatch, _gamepadStates[2] ? _on : _off, new Point(_buttons[2].X+(ActiveGamePad == PlayerIndex.Three ? xOffset:0), _buttons[2].Y), ActiveGamePad == PlayerIndex.Three ? Color.White : Color.White*0.25f);
        _atlas.Draw(spriteBatch, _gamepadStates[3] ? _on : _off, new Point(_buttons[3].X+(ActiveGamePad == PlayerIndex.Four ? xOffset:0), _buttons[3].Y), ActiveGamePad == PlayerIndex.Four ? Color.White : Color.White*0.25f);
    }
}

public class ColorModeButton
{
    private Texture2D _texture;
    private Vector2 _position;
    public Color CurrentColor;
    private Color _altColor;
    private MouseState _prevMouse = new MouseState();
    private GamePadTester _game;

    public ColorModeButton(Texture2D texture, Vector2 position, GamePadTester game)
    {
        _position = position;
        _texture = texture;
        CurrentColor = Color.LightSlateGray;
        _altColor = Color.AntiqueWhite;
        _game = game;
    }

    public void Update()
    {
        var ms = Mouse.GetState();

        if (ms.LeftButton == ButtonState.Pressed & _prevMouse.LeftButton != ButtonState.Pressed)
        {
            var pos = _game.ConvertPoint(ms.Position);

            if (new Rectangle((int)_position.X, (int)_position.Y, _texture.Width, _texture.Height).Contains(pos))
            {
                  CurrentColor = CurrentColor == Color.LightSlateGray ? Color.AntiqueWhite : Color.LightSlateGray;
                    _altColor = CurrentColor == Color.LightSlateGray ? Color.AntiqueWhite : Color.LightSlateGray;

                    _game.BGCoolor = CurrentColor;
            }

        }

        _prevMouse = ms;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            _position,
            _altColor
        );
    }
}

public class TestButton
{
    private Atlas _atlas;
    private string _region;
    private Point _position;
    private Color _currentColour = Color.White;
    private Buttons _scanButton;
    public TestButton(Atlas atlas, string region, Point position, Buttons scanButton)
    {
        _atlas = atlas;
        _region = region;
        _position = position;
        _scanButton = scanButton;
    }

    public void Update(GamePadState state,ref string down)
    {
        _currentColour = state.IsButtonDown(_scanButton) ? Color.Gray : Color.White;
        if (state.IsButtonDown(_scanButton))
            down += _scanButton.ToString();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _atlas.Draw(spriteBatch, _region, _position, _currentColour);
    }
}

public class TestAxis
{
    private Texture2D _texture;
    private Texture2D _pixel;
    private Point _position;
    private string _axis;
    private Rectangle _rect;
    private bool _fromCenter;
    
    public TestAxis(Texture2D texture, GraphicsDevice graphicsDevice, Point position, string axis, bool fromCenter = true)
    {
        _position = position;
        _texture = texture;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _axis = axis;
        _fromCenter = fromCenter;
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
        _rect = new Rectangle(_position.X, _position.Y+1, 8, (int)mappedHeight);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _pixel,
            _rect,
            Color.LightCoral
        );
        spriteBatch.Draw(
            _texture,
            _position.ToVector2(),
            Color.White
        );

    }
}



public class Atlas
{
    public Texture2D Texture { get; set; }
    public Dictionary<string,Rectangle> Rectangles { get; set; }
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
                Rectangles.Add($"{x}:{y}",rect);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, string region, Point position, Color color)
    {
        spriteBatch.Draw(
            Texture, 
            new Rectangle(position.X, position.Y, TileSize.X ,TileSize.Y ),
            Rectangles[region],
            color,
            0f,
            Vector2.One,
            SpriteEffects.None,
            0f
        );
    }
}