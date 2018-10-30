using System;
using System.Collections.Generic;
using GeonBit.UI;

namespace HexGame.Editor {
    using System.IO;

    using HexGame.UI;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class MapEditor : GameScreen {
        private Input Input { get; }
        private Camera Camera { get; set; }

        private HexMap Map { get; set; }

        private SettingsMenu SettingsMenu { get; }
        private MapEditorTools EditorPanel { get; }

        private readonly SpriteFont _font;
        private GraphicsDevice GraphicsDevice { get; }
        private ContentManager Content { get; }
        private SpriteBatch SpriteBatch { get; }

        public MapEditor(GraphicsDevice graphicsDevice, ContentManager content) {
            GraphicsDevice = graphicsDevice;
            Content = content;
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
            Camera.SetLens(MathHelper.ToRadians(45), graphicsDevice.DisplayMode.AspectRatio, .01f, 1000f);
            Camera.LookAt(new Vector3(0, 10, 1), Vector3.Zero, Vector3.Up);

            SettingsMenu = new SettingsMenu(Exit, SaveMap, LoadMap);

            EditorPanel = new MapEditorTools(content);
            Interface.AddEntity(EditorPanel);

            

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("default");

            var texture = Content.Load<Texture2D>("Dry Grass 2");
            Map = new HexMap(GraphicsDevice, 100, 100, texture, _font);
            MapResources.LoadContent(Content);
        }

        private void Exit() {
            Deactivate();
        }

        private void LoadMap(string filename) {
            var loader = new MapLoader();
            var newMap = loader.LoadFromFileProto(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame", filename + ".mapp"), GraphicsDevice, Content, _font);
            Map = newMap;

        }

        private void SaveMap(string filename) {
            var loader = new MapLoader();
            loader.SaveToFileProto(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame", filename + ".mapp"), Map);
        }

        public override void Update(GameTime gameTime) {
            if (!Active) {
                return;
            }
            
            Input.Update(gameTime);
            
            if (!SettingsMenu.IsActive) {
                if (Input.IsDown(Commands.GameExit)) {
                    Deactivate();
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
                    SettingsMenu.Show();
                }

                if (Input.IsPressed(Commands.CmdRaiseTerrain)) {
                    EditorPanel.ActiveTool = EditorTools.Elevation;
                }
                if (Input.IsPressed(Commands.CmdTrees)) {
                    EditorPanel.ActiveTool = EditorTools.Trees;
                }
                
                var mouse = Mouse.GetState();
                var mouseLoc = mouse.Position.ToVector2();
                var viewPort = GraphicsDevice.Viewport;
                if (viewPort.Bounds.Contains(mouseLoc)) {
                    var ray = Camera.CalculateRay(mouseLoc, viewPort);
                    if (EditorPanel.ActiveTool == EditorTools.Elevation) {
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
                    } else if (EditorPanel.ActiveTool == EditorTools.Trees) {
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
        }

        public override void Draw(GameTime gameTime) {
            if (!Active) {
                return;
            }
            Map.Draw(GraphicsDevice, SpriteBatch, Camera);
        }
    }
}
