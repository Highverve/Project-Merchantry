using AetaLibrary.Extensions;
using ExtensionsLibrary.Input;
using Merchantry.Assets;
using Merchantry.Culture;
using Merchantry.Extensions;
using Merchantry.Particles;
using Merchantry.UI;
using Merchantry.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Windows.Forms;

namespace Merchantry
{
    public class GameManager : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region Classes

        private static References references;
        public static References References() { return references; }

        WorldManager world;
        ScreenManager screens;
        ParticleManager particles;
        AssetManager assets;
        Calendar calendar;

        ExtensionsLibrary.Input.Controls controls;
        Debugging debugging;
        Settings settings;

        #endregion

        public GameManager()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1600;//MathHelper.Min(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, 1920);
            graphics.PreferredBackBufferHeight = 900;//MathHelper.Min(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 1080);
            graphics.IsFullScreen = false;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            IsMouseVisible = true;

            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            references = new References();
            references.Game = this;

            controls = new ExtensionsLibrary.Input.Controls();
            references.Controls = controls;

            debugging = new Debugging();
            references.Debugging = debugging;

            settings = new Settings();
            references.Settings = settings;
            settings.Resolution = GraphicsDevice.Viewport.Bounds.Size.ToVector2();

            world = new WorldManager(GraphicsDevice);
            references.World = world;

            screens = new ScreenManager(GraphicsDevice);
            references.Screens = screens;

            particles = new ParticleManager();
            references.Particles = particles;

            calendar = new Calendar(776, 1, 1, 11, 0);
            references.Calendar = calendar;

            assets = new AssetManager();
            references.Assets = assets;
            assets.Load(Content);

            world.Initialize();
            screens.Initialize();
            particles.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            debugging.Load(Content);

            screens.SetContentManager(Content);
            world.Load(Content);
            particles.Load(Content);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive == true)
            {
                controls.UpdateDelay(gameTime);
                controls.UpdateLast();
                controls.UpdateCurrent();

                world.Update(gameTime);
                screens.Update(gameTime);
                particles.Update(gameTime);
                calendar.Update(gameTime);

                debugging.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);
            screens.Prerender(spriteBatch);

            GraphicsDevice.Clear(Color.Black);
            world.Draw(spriteBatch);
            screens.Draw(spriteBatch);
            debugging.Draw(spriteBatch);

            base.Draw(gameTime);
        }
    }
}
