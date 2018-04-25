namespace HexGame.UI {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using GeonBit.UI.Entities;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    public enum EditorTools {
        None = 0,
        Elevation = 1,
        Trees = 2,
    }

    public class MapEditorTools : Entity {
        private readonly Dictionary<EditorTools, Image> _tools = new Dictionary<EditorTools, Image>();
        public EditorTools ActiveTool { get; set; }
        private Panel Panel { get; }
        public Color ActiveToolOutline { get; set; }
        public MapEditorTools(ContentManager content) {
            Size = new Vector2(100, 600);
            
            ActiveToolOutline = Color.White;
            Panel = new Panel(Size, PanelSkin.Simple, Anchor.TopLeft);
            AddChild(Panel);
            foreach (var tool in Enum.GetValues(typeof(EditorTools)).Cast<EditorTools>().Where(e=>e!= EditorTools.None)) {
                var image = new Image(content.Load<Texture2D>($"{typeof(EditorTools).Name}.{tool}"), new Vector2(64,64), ImageDrawMode.Stretch, Anchor.AutoCenter) {
                    OutlineColor = Color.Transparent,
                    OutlineWidth = 2
                };
                image.OnClick += ToggleActiveTool;
                Panel.AddChild(image);
                _tools[tool] = image;
            }
            Draggable = true;
        }


        private void ToggleActiveTool(Entity entity) {
            foreach (var tool in _tools) {
                tool.Value.OutlineColor = Color.Transparent;
                if (tool.Value == entity) {
                    if (ActiveTool == tool.Key) {
                        ActiveTool = EditorTools.None;
                    } else {
                        tool.Value.OutlineColor = ActiveToolOutline;
                        ActiveTool = tool.Key;
                    }
                }
            }
        }
    }
}