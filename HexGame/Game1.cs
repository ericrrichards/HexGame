using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using GeonBit.UI;
    using GeonBit.UI.Entities;

    using HexGame.Editor;
    using HexGame.UI;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont _font;
        private string DisplayText { get; set; } = string.Empty;

        private FrameCounter FrameCounter { get; set; }

        private List<GameScreen> Screens { get; } = new List<GameScreen>();

        public Game1() {
            graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1600, PreferredBackBufferHeight = 900 };
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame"))) {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame"));
            }

            //graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;
            Content.RootDirectory = "Content";


        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here

            UserInterface.Initialize(Content);

            var editor = new MapEditor(GraphicsDevice, Content);

            var mainMenu =new MainMenu(editor, Exit);

            Screens.Add(editor);
            Screens.Add(mainMenu);

            mainMenu.Activate();
            


            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("default");
            FrameCounter = new FrameCounter(_font);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            FrameCounter.Update(gameTime);

            foreach (var screen in Screens) {
                screen.Update(gameTime);    
            }
            InterfaceStack.Update(gameTime);
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            FrameCounter.CountFrame();


            

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach (var screen in Screens) {
                screen.Draw(gameTime);
            }
            

            DrawDebugText();

            FrameCounter.Draw(spriteBatch);

            InterfaceStack.Draw(spriteBatch);


            

            base.Draw(gameTime);
        }

        

        private void DrawDebugText() {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, DisplayText, Vector2.Zero, Color.White);

            spriteBatch.End();
        }
    }
}
