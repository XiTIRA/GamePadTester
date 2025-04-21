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
    private bool _isResizing;
    private Atlas _atlas;

    private List<TestButton> _buttons = new();
    private List<TestAxis> _axes = new();

    private SpriteFont _font;
    private Texture2D _logo;

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

    private RenderTarget2D _renderTarget;
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _renderTarget = new RenderTarget2D(GraphicsDevice, 272,160);

        _font = Content.Load<SpriteFont>("defaultFont");
        _atlas = new Atlas(Content.Load<Texture2D>("buttons"), new Point(16, 16));
        _logo = Content.Load<Texture2D>("mg");

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

        var gs = GamePad.GetState(PlayerIndex.One);
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

    private string _down = "";
private string gamePadName = "";
    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.AntiqueWhite);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

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