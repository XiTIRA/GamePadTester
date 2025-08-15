using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Xitira.GamePadTester;

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