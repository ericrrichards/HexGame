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

    using Keys = Keys;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont _font;
        private Panel _settingsPanel;
        private Panel _saveDialog;
        private Panel _loadDialog;

        // Camera
        private Camera Camera { get; set; }

        private Input Input { get; set; }

        private BasicEffect BasicEffect { get; set; }

        private HexMap Map { get; set; }
        private string DisplayText { get; set; }

        private FrameCounter FrameCounter { get; set; }

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
                [Commands.TogglePickMode] = new List<Keys> { Keys.P },
                [Commands.ToggleHexHeights] = new List<Keys> { Keys.H },

                [Commands.SaveMap] = new List<Keys> { Keys.S },
                [Commands.LoadMap] = new List<Keys> { Keys.L }
            };

            Input.AddBindings(bindings);


            Camera = new Camera(Input);
            Camera.SetLens(MathHelper.ToRadians(45), GraphicsDevice.DisplayMode.AspectRatio, .01f, 1000f);
            Camera.LookAt(new Vector3(0, 10, 1), Vector3.Zero, Vector3.Up);

            BasicEffect = new BasicEffect(GraphicsDevice);


            UserInterface.Initialize(Content);

            _settingsPanel = new Panel(new Vector2(0.5f, 0.5f), PanelSkin.Simple) {
                Visible = false
            };
            var menuHeader = new Header("Menu");
            _settingsPanel.AddChild(menuHeader);
            var hr = new HorizontalLine();
            _settingsPanel.AddChild(hr);

            var btnSaveMap = new Button("Save Map", ButtonSkin.Default, Anchor.AutoCenter);
            var btnLoadMap = new Button("Load Map", ButtonSkin.Default, Anchor.AutoCenter);

            var exitButton = new Button("Exit", ButtonSkin.Default, Anchor.AutoCenter);
            exitButton.OnClick += entity => Exit();


            _saveDialog = new Panel(new Vector2(0.5f, 0.5f), PanelSkin.Simple) {
                Visible = false
            };
            var saveHeader = new Header("Save Map");
            _saveDialog.AddChild(saveHeader);
            hr = new HorizontalLine();
            _saveDialog.AddChild(hr);
            var saveInput = new TextInput(false, Anchor.AutoCenter);
            _saveDialog.AddChild(saveInput);
            var cancelButton = new Button("Cancel", ButtonSkin.Default, Anchor.BottomLeft, new Vector2(0.5f, -1));
            cancelButton.OnClick += entity => _saveDialog.Visible = false;
            _saveDialog.AddChild(cancelButton);
            var saveButton = new Button("Save", ButtonSkin.Default, Anchor.BottomRight, new Vector2(0.5f, -1));
            saveButton.OnClick += entity => SaveMap(saveInput.Value, _saveDialog);
            _saveDialog.AddChild(saveButton);
            btnSaveMap.OnClick += entity => {
                _saveDialog.Visible = true;
                _settingsPanel.Visible = false;
            };

            _loadDialog = new Panel(new Vector2(0.5f, 0.5f), PanelSkin.Simple, Anchor.AutoCenter) {
                Visible = false
            };
            var loadHeader = new Header("Load Map", Anchor.AutoCenter);
            _loadDialog.AddChild(loadHeader);
            hr = new HorizontalLine();
            _loadDialog.AddChild(hr);
            var loadListBox = new SelectList(new Vector2(-1, -1), Anchor.AutoCenter, null, PanelSkin.Simple);
            foreach (var mapFile in Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame"), "*.mapp")) {
                loadListBox.AddItem(Path.GetFileNameWithoutExtension(mapFile));
            }
            _loadDialog.AddChild(loadListBox);

            cancelButton = new Button("Cancel", ButtonSkin.Default, Anchor.BottomLeft, new Vector2(0.5f, -1));
            cancelButton.OnClick += entity => _loadDialog.Visible = false;
            _loadDialog.AddChild(cancelButton);
            var loadButton = new Button("Load", ButtonSkin.Default, Anchor.BottomRight, new Vector2(0.5f, -1));
            loadButton.OnClick += entity => LoadMap(loadListBox.SelectedValue, _loadDialog);
            _loadDialog.AddChild(loadButton);

            btnSaveMap.OnClick += entity => {
                _saveDialog.Visible = true;
                _settingsPanel.Visible = false;
            };

            btnLoadMap.OnClick += entity => {
                _loadDialog.Visible = true;
                _settingsPanel.Visible = false;
            };


            _settingsPanel.AddChild(btnSaveMap);
            _settingsPanel.AddChild(btnLoadMap);
            _settingsPanel.AddChild(exitButton);





            UserInterface.Active.AddEntity(_saveDialog);
            UserInterface.Active.AddEntity(_loadDialog);

            UserInterface.Active.AddEntity(_settingsPanel);


            base.Initialize();
        }

        private void LoadMap(string filename, Entity parent) {
            Task.Run(() => {
                var newMap = HexMap.LoadFromFileProto(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame", filename + ".mapp"), GraphicsDevice, Content, _font);
                Map = newMap;
                MessageBox.ShowMsgBox("Map Loaded", $"Map \"{filename}\" Loaded", new[] { new MessageBox.MsgBoxOption("OK", () => true) }, null, null, () => parent.Visible = false);

            });

        }

        private void SaveMap(string filename, Entity parent) {
            Map.SaveToFileProto(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame", filename + ".mapp"));
            MessageBox.ShowMsgBox("Map Saved", $"Map \"{filename}\" saved", new[] { new MessageBox.MsgBoxOption("OK", () => true) }, null, null, () => parent.Visible = false);
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
            if (!_settingsPanel.IsVisible() && !_saveDialog.IsVisible() && !_loadDialog.IsVisible()) {


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
                    _settingsPanel.Visible = !_settingsPanel.IsVisible();
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
