using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    //using System.Windows.Forms;

    using GeonBit.UI;
    using GeonBit.UI.Entities;
    using GeonBit.UI.Utils;

    using HexGame.UI;

    using Keys = Keys;


    public enum EditorTools {
        None = 0,
        RaiseTerrain = 1,
        Trees = 2,
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont _font;
        private SettingsMenu _settingsMenu;

        // Camera
        private Camera Camera { get; set; }

        private Input Input { get; set; }

        private BasicEffect BasicEffect { get; set; }

        private HexMap Map { get; set; }
        private string DisplayText { get; set; } = string.Empty;

        private FrameCounter FrameCounter { get; set; }
        private EditorTools ActiveTool { get; set; }

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

                [Commands.OpenMenu] = new List<Keys> { Keys.Escape },

                [Commands.ToggleHexCoordinates] = new List<Keys> { Keys.C },
                [Commands.ToggleHexGrid] = new List<Keys> { Keys.G },
                [Commands.ToggleWireframe] = new List<Keys> { Keys.W },
                [Commands.ToggleHexHeights] = new List<Keys> { Keys.H },
                [Commands.CmdRaiseTerrain] = new List<Keys>{Keys.F1},
                [Commands.CmdTrees] = new List<Keys>{Keys.F2},


                [Commands.SaveMap] = new List<Keys> { Keys.S },
                [Commands.LoadMap] = new List<Keys> { Keys.L }
            };

            Input.AddBindings(bindings);


            Camera = new Camera(Input);
            Camera.SetLens(MathHelper.ToRadians(45), GraphicsDevice.DisplayMode.AspectRatio, .01f, 1000f);
            Camera.LookAt(new Vector3(0, 10, 1), Vector3.Zero, Vector3.Up);

            BasicEffect = new BasicEffect(GraphicsDevice);


            UserInterface.Initialize(Content);

            _settingsMenu = new SettingsMenu(Exit, SaveMap, LoadMap);



            base.Initialize();
        }

        private void LoadMap(string filename) {
            var newMap = HexMap.LoadFromFileProto(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame", filename + ".mapp"), GraphicsDevice, Content, _font);
            Map = newMap;

        }

        private void SaveMap(string filename) {
            Map.SaveToFileProto(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame", filename + ".mapp"));
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
            Map = new HexMap(GraphicsDevice, 100, 100, texture, _font);
            FrameCounter = new FrameCounter(_font);

            MapResources.LoadContent(Content);
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
            if (!_settingsMenu.IsActive) {


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

                if (Input.IsPressed(Commands.OpenMenu)) {
                    _settingsMenu.Show();
                }

                if (Input.IsPressed(Commands.CmdRaiseTerrain)) {
                    ActiveTool = EditorTools.RaiseTerrain;
                }
                if (Input.IsPressed(Commands.CmdTrees)) {
                    ActiveTool = EditorTools.Trees;
                }

                DisplayText = "Over: ";

                var mouse = Mouse.GetState();
                var mouseLoc = mouse.Position.ToVector2();
                var viewPort = GraphicsDevice.Viewport;
                if (viewPort.Bounds.Contains(mouseLoc)) {
                    var ray = Camera.CalculateRay(mouseLoc, viewPort);
                    if (ActiveTool == EditorTools.RaiseTerrain) {
                        var vertex = Map.PickVertex(ray);
                        if (vertex != null) {
                            var mapDirty = false;
                            if (Input.MouseClicked(true)) {
                                Map.RaiseVertex(vertex.Value);
                                mapDirty = true;
                            } else if (Input.MouseDown(true)) {
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
                    } else if (ActiveTool == EditorTools.Trees) {
                        var hex = Map.PickHex(ray);
                        if (hex != null) {
                            if (Input.MouseClicked(true)) {
                                hex.IsForest = true;
                            } else if (Input.MouseDown(true)) {
                                hex.IsForest = true;
                            } else if (Input.MouseClicked(false)) {
                                hex.IsForest = false;
                            } else if (Input.MouseDown(false)) {
                                hex.IsForest = false;
                            }
                        }
                    }

                }
                Camera.Update(gameTime);
            }

            

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
