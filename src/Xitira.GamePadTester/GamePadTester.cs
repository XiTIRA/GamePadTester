using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;

namespace Xitira.GamePadTester;

public class GamePadTester : Game
{
    private readonly string _version = "3.8.4.1"  ;
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
        _visualiser.AddButton(new TestButton(atlas, "7:33",  Buttons.LeftStick, isLeftStick:true));
        _visualiser.AddButton(new TestButton(atlas, "7:34",  Buttons.RightStick, isRightStick:true));
        _visualiser.UpdateButtonPositions();

        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(14, 16), "LeftTrigger",
            false));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(24, 16), "RightTrigger",
            false));

        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(5, 110), "LeftStickX"));
        _axes.Add(new TestAxis(Content.Load<Texture2D>("battery"), GraphicsDevice, new Point(14, 110), "LeftStickY"));;
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