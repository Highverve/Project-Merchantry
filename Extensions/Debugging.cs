using AetaLibrary.Extensions;
using ExtensionsLibrary.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Extensions
{
    public class Debugging
    {
        public Texture2D Pixel { get; private set; }
        public Texture2D Circle { get; private set; }
        public SpriteFont TinyFont { get; private set; }
        public SpriteFont SmallFont { get; private set; }
        public Color ObjectBox { get; private set; } = Color.White;
        public Color ObjectText { get; private set; } = Color.White;

        public bool IsDeveloperMode { get; set; } = false;
        public bool IsDebugCamera { get; set; } = false;
        public bool IsNavGridRender { get; set; } = false;
        public bool IsTileMouseRender { get; set; } = false;
        public bool IsSelectShapeRender { get; set; } = false;

        private string version;
        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
                }

                return version;
            }
        }

        private References references;

        public void Load(ContentManager cm)
        {
            Pixel = cm.Load<Texture2D>("Debug/pixel");
            Circle = cm.Load<Texture2D>("Debug/circle");
            TinyFont = cm.Load<SpriteFont>("UI/Fonts/tinyFont");
            SmallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");
            //Load assets for debugging purposes here
        }
        
        public void Update(GameTime gt)
        {
            //UpdateShortcuts();
        }
        private void UpdateShortcuts()
        {
            if (GameManager.References().Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F1))
            {
                IsDeveloperMode = !IsDeveloperMode;
                RefreshDebugInfo();
            }
            if (GameManager.References().Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F2))
            {
                IsDebugCamera = !IsDebugCamera;
                RefreshDebugInfo();
            }
            if (GameManager.References().Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F3))
            {
                IsTileMouseRender = !IsTileMouseRender;
                RefreshDebugInfo();
            }
            if (GameManager.References().Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F4))
            {
                IsNavGridRender = !IsNavGridRender;
                RefreshDebugInfo();
            }
            if (GameManager.References().Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F5))
            {
                IsSelectShapeRender = !IsSelectShapeRender;
                RefreshDebugInfo();
            }
        }

        private string info = string.Empty;
        public void RefreshDebugInfo()
        {
            info = string.Join("\n", "Developer Mode (F1): " + IsDeveloperMode,
                                     "Free Camera (F2 -- LAlt): " + IsDebugCamera,
                                     "Mouse Tile (F3): " + IsTileMouseRender,
                                     "Navigation Grid (F4): " + IsNavGridRender,
                                     "Select Shapes (F5): " + IsSelectShapeRender,
                                     "\nVersion: " + Version);
        }
        public void Draw(SpriteBatch sb)
        {
            if (IsDeveloperMode)
            {
                sb.Begin(samplerState: SamplerState.PointClamp);

                sb.DrawString(TinyFont, info, new Vector2(GameManager.References().Settings.Resolution.X - 10, 10), Color.White, 0, new Vector2(TinyFont.MeasureString(info).X, 0), 2, SpriteEffects.None, 1);

                sb.End();
            }
        }

        public void DrawWireframe(SpriteBatch sb, int x, int y, int width, int height,
                                 string topText, string bottomText, Color boxColor, Color textColor)
        {
            sb.DrawSquare(Pixel, Pixel.Bounds, new Vector2(x, y), new Vector2(width, height), boxColor, 1, 1);

            if (!string.IsNullOrEmpty(topText))
                sb.DrawString(TinyFont, topText, new Vector2(x, y + 1), textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            if (!string.IsNullOrEmpty(bottomText))
                sb.DrawString(TinyFont, bottomText, new Vector2(x, y + (height - TinyFont.MeasureString(bottomText).Y)), textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        }
        public void DrawWireframe(SpriteBatch sb, int x, int y, int width, int height,
                                 string topText, string bottomText)
        {
            DrawWireframe(sb, x, y, width, height, topText, bottomText, ObjectBox, ObjectText);
        }
        public void DrawPoint(SpriteBatch sb, Vector2 position, float scale, Color color)
        {
            sb.Draw(Pixel, position, Pixel.Bounds, color, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
        }
        public void DrawCircle(SpriteBatch sb, Vector2 position, float radius, Color color, float depth = 1)
        {
            sb.Draw(Circle, position, Circle.Bounds, color, 0, Circle.Bounds.Center.ToVector2(), radius / 512, SpriteEffects.None, depth);
        }

        public void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, float width, Color color)
        {
            DrawLine(sb, start, end, width, color, 1f);
        }
        public void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, float width, Color color, float depth)
        {
            sb.DrawLine(Pixel, Pixel.Bounds, start, end, width, color, depth);
        }
    }
}
