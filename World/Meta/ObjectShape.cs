using Merchantry.UI.Developer.General;
using Merchantry.World.Meta.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectShape : IProperties
    {
        public WorldObject Object { get; set; }
        public Dictionary<string, Shape> Shapes { get; private set; } = new Dictionary<string, Shape>(StringComparer.OrdinalIgnoreCase);

        public void Add(string key, Shape shape)
        {
            if (!Shapes.ContainsKey(key))
            {
                shape.Name = key;
                shape.Object = Object;
                Shapes.Add(key, shape);
            }
        }
        public void Remove(string key)
        {
            if (Shapes.ContainsKey(key))
                Shapes.Remove(key);
        }

        public bool Contains(Vector2 position)
        {
            foreach (Shape s in Shapes.Values)
            {
                if (s.IsEnabled && s.Contains(position))
                    return true;
            }
            return false;
        }
        public bool Contains(Vector2 position, string group)
        {
            foreach (Shape s in Shapes.Values)
            {
                if (s.IsEnabled == true &&
                    s.IsGroup(group) &&
                    s.Contains(position))
                    return true;
            }
            return false;
        }
        public bool Intersects(Shape shape)
        {
            return Intersects(shape, null);
        }
        public bool Intersects(Shape shape, string group)
        {
            if (shape.IsEnabled)
            {
                foreach (Shape s in Shapes.Values)
                {
                    if (s.IsEnabled &&
                        s.IsGroup(shape) &&
                        s.Intersects(shape))
                        return true;
                }
            }
            return false;
        }

        public void DrawDebug(SpriteBatch sb, Texture2D pixel, string group)
        {
            foreach (Shape s in Shapes.Values)
            {
                if (s.IsGroup(group))
                    s.Draw(sb, pixel);
            }
        }

        private AetaLibrary.Elements.Text.TextElement countText;
        public void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader("Shapes");
            ui.PROPERTY_AddText("Owner: " + Object.ID).Fade(Color.Gray, 100);
            countText = ui.PROPERTY_AddText("Count: " + Shapes.Count);
            countText.Fade(Color.Gray, 100);
            ui.PROPERTY_AddSpacer(10);
            ui.PROPERTY_AddText("Types:");
            
            foreach (Shape shape in Shapes.Values)
            {
                AetaLibrary.Elements.Text.TextElement text = null;
                text = ui.PROPERTY_AddLabelButton("  " + shape.Name + " [" + shape.GetType().Name + "]", () =>
                {
                    if (Shapes.ContainsKey(shape.Name))
                        ui.SetSelected(shape);
                }, () =>
                {
                    if (Shapes.ContainsKey(shape.Name))
                    {
                        Remove(shape.Name);
                        text.Fade(ExtensionsLibrary.Extensions.ColorExt.Raspberry, 15);
                    }
                    else
                    {
                        Add(shape.Name, shape);
                        text.Fade(Color.Gray, 15);
                    }
                });
                text.Fade(Color.Gray, 100);
            }
        }
        public void RefreshProperties()
        {
            countText.Text = "Count: " + Shapes.Count;
        }
        public void NullifyProperties()
        {
            countText = null;
        }
    }
}
