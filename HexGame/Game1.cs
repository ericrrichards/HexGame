using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HexGame {
    using System.Collections.Generic;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Camera
        private Camera Camera { get; set; }

        private Input Input { get; set; }

        private BasicEffect BasicEffect { get; set; }

        private Hexagon Hexagon { get; set; }


        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here

            base.Initialize();

            Input = new Input();
            Input.AddBindings(new Dictionary<string, Keys[]> {
                [Commands.CameraStrafeLeft] = new []{Keys.Left},

            });


            Camera = new Camera(GraphicsDevice.DisplayMode.AspectRatio, Input);
            

            BasicEffect = new BasicEffect(GraphicsDevice) {
                VertexColorEnabled = true,
            };

            Hexagon = new Hexagon(GraphicsDevice, 0.5f);
        }
        


        

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

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
            var ks = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                Exit();
            
            Camera.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {

            BasicEffect.Projection = Camera.ProjectionMatrix;
            BasicEffect.View = Camera.ViewMatrix;
            BasicEffect.World = Camera.WorldMatrix;

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetVertexBuffer(Hexagon.VertexBuffer);
            GraphicsDevice.Indices = Hexagon.IndexBuffer;

            var rasterState = new RasterizerState { CullMode = CullMode.None };
            GraphicsDevice.RasterizerState = rasterState;

            foreach (var pass in BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 6);
            }


            base.Draw(gameTime);
        }
    }
}
