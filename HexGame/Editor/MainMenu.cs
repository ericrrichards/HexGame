namespace HexGame.Editor {
    using System;

    using GeonBit.UI.Entities;

    using Microsoft.Xna.Framework;

    public class MainMenu : GameScreen {
        public MainMenu(MapEditor editor, Action exit) {
            var panel = new Panel(Vector2.Zero, PanelSkin.Simple, Anchor.AutoCenter);
            panel.AddChild(new Header("HexGame", Anchor.AutoCenter));
            panel.AddChild(new HorizontalLine());
            var editorButton = new Button("Map Editor", ButtonSkin.Default, Anchor.AutoCenter);
            editorButton.OnClick += entity => { editor.Activate(); };
            panel.AddChild(editorButton);
            var quitButton = new Button("Exit", ButtonSkin.Default, Anchor.AutoCenter);
            quitButton.OnClick += entity => exit();
            panel.AddChild(quitButton);
            Interface.AddEntity(panel);
        }
    }
}