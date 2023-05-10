using AetaLibrary.Elements.Text;
using Merchantry.UI.Developer.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Shapes
{
    public class Shape : IProperties
    {
        public string Name { get; set; }
        public string Group { get; set; } = null;
        public bool IsEnabled { get; set; } = true;
        public WorldObject Object { get; set; }

        //Variables
        private Vector2 offset;
        public Vector2 Offset { get { return offset; } set { offset = value; } }
        public float X { get { return offset.X; } set { offset.X = value; } }
        public float Y { get { return offset.Y; } set { offset.Y = value; } }
        public Vector2 Position() { return Object.Position + Offset; }

        public Shape(Vector2 Offset) { this.Offset = Offset; }

        //Methods
        public virtual bool Contains(Vector2 position) { return false; }
        public bool Contains(Point position) { return Contains(position.ToVector2()); }
        public virtual bool Intersects(Shape shape) { return false; }
        public virtual Vector2 Closest(Vector2 position) { return Vector2.Zero; }
        public virtual float Gap(Shape shape) { return 0; }

        public bool IsGroup(Shape shape)
        {
            return IsGroup(shape.Group);
        }
        public bool IsGroup(string group)
        {
            return Group == group;
        }

        public virtual void Draw(SpriteBatch sb, Texture2D pixel)
        {
            sb.Draw(pixel, Object.Position, pixel.Bounds, Color.White, 0, Vector2.Zero, new Vector2(2, 2), SpriteEffects.None, 1);
        }

        private TextElement enabledText;
        private UI.Elements.NumberElement offsetXNumber, offsetYNumber;
        public void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader(GetType().Name);
            ui.PROPERTY_AddText("Owner: " + Object.ID).Fade(Color.Gray, 15);
            ui.PROPERTY_AddText("Group: " + Group).Fade(Color.Gray, 15);
            offsetXNumber = ui.PROPERTY_AddNumber("Offset.X: ", X, null, () => X += 1, () => X -= 1);
            offsetXNumber.Fade(Color.Gray, 15);
            offsetYNumber = ui.PROPERTY_AddNumber("Offset.Y: ", Y, null, () => Y += 1, () => Y -= 1);
            offsetYNumber.Fade(Color.Gray, 15);
            ui.PROPERTY_AddSpacer(10);
            enabledText = ui.PROPERTY_AddButton((IsEnabled ? "Enabled" : "Disabled"), () => IsEnabled = !IsEnabled);
        }
        public void RefreshProperties()
        {
            enabledText.Text = (IsEnabled ? "Enabled" : "Disabled");
            offsetXNumber.Number = X;
            offsetYNumber.Number = Y;
        }
        public void NullifyProperties()
        {
            enabledText = null;
            offsetXNumber = null;
            offsetYNumber = null;
        }
    }
}
