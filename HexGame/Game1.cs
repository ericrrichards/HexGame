using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HexGame {
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;

    using GeonBit.UI;

    using Keys = Keys;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont _font;

        // Camera
        private Camera Camera { get; set; }

        private Input Input { get; set; }

        private BasicEffect BasicEffect { get; set; }

        private HexMap Map { get; set; }
        private string DisplayText { get; set; }

        private FrameCounter FrameCounter { get; set; }

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
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

            
            
            Input = new Input();

            var bindings = new Dictionary<string, List<Keys>> {
                [Commands.CameraStrafeLeft] = new List<Keys> { Keys.Left },
                [Commands.CameraStrafeRight] = new List<Keys> { Keys.Right },
                [Commands.CameraForward] = new List<Keys> { Keys.Up },
                [Commands.CameraBackward] = new List<Keys> { Keys.Down },
                [Commands.CameraZoomIn] = new List<Keys> { Keys.OemPlus, Keys.Add },
                [Commands.CameraZoomOut] = new List<Keys> { Keys.OemMinus, Keys.Subtract },
                [Commands.CameraOrbitRight] = new List<Keys> { Keys.PageDown },
                [Commands.CameraOrbitLeft] = new List<Keys> { Keys.Delete },
                [Commands.CameraOrbitDown] = new List<Keys> { Keys.End },
                [Commands.CameraOrbitUp] = new List<Keys> { Keys.Home },

                [Commands.GameExit] = new List<Keys> { Keys.Escape },

                [Commands.ToggleHexCoordinates] = new List<Keys> { Keys.C },
                [Commands.ToggleHexGrid] = new List<Keys> { Keys.G },
                [Commands.ToggleWireframe] = new List<Keys> { Keys.W },
                [Commands.TogglePickMode] = new List<Keys> { Keys.P },
                [Commands.ToggleHexHeights] = new List<Keys>{Keys.H},

                [Commands.SaveMap] = new List<Keys>{Keys.S},
                [Commands.LoadMap] = new List<Keys>{Keys.L}
            };

            Input.AddBindings(bindings);


            Camera = new Camera(Input);
            Camera.SetLens(MathHelper.ToRadians(45), GraphicsDevice.DisplayMode.AspectRatio, .01f, 1000f);
            Camera.LookAt(new Vector3(0, 10, 1), Vector3.Zero, Vector3.Up);

            BasicEffect = new BasicEffect(GraphicsDevice);


            UserInterface.Initialize(Content, BuiltinThemes.hd);

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
            var texture = Content.Load<Texture2D>("Dry Grass 2");
            Map = new HexMap(GraphicsDevice, 100, 100,texture, _font, MeshType.Flat );
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

            UserInterface.Active.Update(gameTime);

            Input.Update(gameTime);

            if (Input.IsDown(Commands.GameExit)) {
                Exit();
            }
            if (Input.IsPressed(Commands.ToggleHexCoordinates)) {
                Map.ShowCoords = !Map.ShowCoords;
            }
            if (Input.IsPressed(Commands.ToggleHexGrid)) {
                Map.ShowGrid = !Map.ShowGrid;
            }
            if (Input.IsPressed(Commands.ToggleWireframe)) {
                Map.Wireframe = !Map.Wireframe;
            }
            if (Input.IsPressed(Commands.ToggleHexHeights)) {
                Map.ShowHexHeights = !Map.ShowHexHeights;
            }
            if (Input.IsPressed(Commands.SaveMap)) {
                var saveFileDialog = new SaveFileDialog{ DefaultExt = ".map", Filter = "Map files|*.map|Binary Map|*.mapb|Proto Map|*.mapp", Title = "Save Map"};
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    var extension = Path.GetExtension(saveFileDialog.FileName);
                    if (extension == ".map") {
                        Map.SaveToFile(saveFileDialog.FileName);
                    } else if (extension == ".mapb") {
                        Map.SaveToFileBinary(saveFileDialog.FileName);
                    } else if (extension == ".mapp") {
                        Map.SaveToFileProto(saveFileDialog.FileName);
                    }
                }
            }
            if (Input.IsPressed(Commands.LoadMap)) {
                var loadFileDialog = new OpenFileDialog { DefaultExt = "map", Filter = "Map files|*.map|Binary Map|*.mapb|Proto Map|*.mapp", Title = "Load Map"};
                if (loadFileDialog.ShowDialog() == DialogResult.OK) {
                    var extension = Path.GetExtension(loadFileDialog.FileName);
                    if (extension == ".map") {
                        Map = HexMap.LoadFromFile(loadFileDialog.FileName, GraphicsDevice, Content, _font);
                    } else if (extension == ".mapb") {
                        Map = HexMap.LoadFromFileBinary(loadFileDialog.FileName, GraphicsDevice, Content, _font);
                    } else if (extension == ".mapp") {
                        Map = HexMap.LoadFromFileProto(loadFileDialog.FileName, GraphicsDevice, Content, _font);
                    }
                }
            }


            DisplayText = "Over: ";

            var mouse = Mouse.GetState();
            var mouseLoc = mouse.Position.ToVector2();
            var viewPort = GraphicsDevice.Viewport;
            if (viewPort.Bounds.Contains(mouseLoc)) {
                var ray = Camera.CalculateRay(mouseLoc, viewPort);
                var vertex = Map.PickVertex(ray);
                if (vertex != null) {
                    DisplayText = "Over: " + vertex;
                    var mapDirty = false;
                    if (Input.MouseClicked(true)) {
                        Map.RaiseVertex(vertex.Value);
                        mapDirty = true;
                    }
                    else if (Input.MouseDown(true)) {
                        Map.RaiseVertex(vertex.Value);
                        mapDirty = true;
                    } else if (Input.MouseClicked(false)) {
                        Map.LowerVertex(vertex.Value);
                        mapDirty = true;
                    } else if (Input.MouseDown(false)) {
                        Map.LowerVertex(vertex.Value);
                        mapDirty = true;
                    }
                    if (mapDirty) {
                        Map.Rebuild(GraphicsDevice);
                    }
                }


            }


            Camera.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            FrameCounter.CountFrame();
            

            BasicEffect.Projection = Camera.ProjectionMatrix;
            BasicEffect.View = Camera.ViewMatrix;

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //BasicEffect.EnableDefaultLighting();

            Map.Draw(GraphicsDevice, BasicEffect, spriteBatch, Camera);

            DrawDebugText();

            FrameCounter.Draw(spriteBatch);

            UserInterface.Active.Draw(spriteBatch);

            base.Draw(gameTime);
        }

        private void DrawDebugText() {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, DisplayText, Vector2.Zero, Color.White);

            spriteBatch.End();
        }
    }
}
