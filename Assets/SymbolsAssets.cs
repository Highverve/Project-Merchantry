using EmberiumLibrary._2D.Textures;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets
{
    public class SymbolAssets
    {
        public TextureAtlas Gamepad { get; private set; }

        public void Load(ContentManager cm)
        {
            Gamepad = new TextureAtlas(cm.Load<Texture2D>("UI/Icons/gamepadPlaystation"));

            Gamepad.Add("CrossReleased", 0, 0, 22, 26);
            Gamepad.Add("CrossPressed", 22, 0, 22, 26);
            Gamepad.Add("SquareReleased", 44, 0, 22, 26);
            Gamepad.Add("SquarePressed", 66, 0, 22, 26);
            Gamepad.Add("CircleReleased", 88, 0, 22, 26);
            Gamepad.Add("CirclePressed", 110, 0, 22, 26);
            Gamepad.Add("TriangleReleased", 132, 0, 22, 26);
            Gamepad.Add("TrianglePressed", 154, 0, 22, 26);

            Gamepad.Add("StickLeft", 0, 26, 32, 56);
            Gamepad.Add("StickLeftReleased", 32, 26, 32, 56);
            Gamepad.Add("StickLeftPressed", 64, 26, 32, 56);
            Gamepad.Add("StickRight", 96, 26, 32, 56);
            Gamepad.Add("StickRightReleased", 128, 26, 32, 56);
            Gamepad.Add("StickRightPressed", 160, 26, 32, 56);

            Gamepad.Add("L1Released", 0, 82, 35, 27);
            Gamepad.Add("L1Pressed", 35, 82, 35, 27);
            Gamepad.Add("R1Released", 70, 82, 35, 27);
            Gamepad.Add("R1Pressed", 105, 82, 35, 27);

            Gamepad.Add("L1Released", 0, 82, 35, 36);
            Gamepad.Add("L1Pressed", 35, 82, 35, 36);
            Gamepad.Add("R1Released", 70, 82, 35, 36);
            Gamepad.Add("R1Pressed", 105, 82, 35, 36);

            Gamepad.Add("SelectReleased", 0, 145, 56, 36);
            Gamepad.Add("SelectPressed", 56, 145, 56, 36);
            Gamepad.Add("StartReleased", 0, 181, 53, 36);
            Gamepad.Add("StartPressed", 53, 181, 53, 36);

            Gamepad.Add("DPad", 0, 217, 43, 47);
            Gamepad.Add("DPadUp", 43, 217, 43, 47);
            Gamepad.Add("DPadDown", 86, 217, 43, 47);
            Gamepad.Add("DPadLeft", 129, 217, 43, 47);
            Gamepad.Add("DPadRight", 172, 217, 43, 47);
        }
    }
}
