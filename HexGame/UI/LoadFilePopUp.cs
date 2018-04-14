namespace HexGame.UI {
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using GeonBit.UI.Entities;
    using GeonBit.UI.Utils;

    using Microsoft.Xna.Framework;

    public class LoadFilePopUp : PopUpMenu {
        private readonly SelectList _loadListBox;
        public string SearchDirectory { get; set; }
        public string Extension { get;set; }
        public LoadFilePopUp(string directory, string extension="*", Action<string> load=null) : base("Load Map") {
            Extension = extension;
            SearchDirectory = directory;
            _loadListBox = new SelectList(new Vector2(-1, -1), Anchor.AutoCenter, null, PanelSkin.Simple);
            foreach (var mapFile in Directory.GetFiles(directory, extension)) {
                _loadListBox.AddItem(Path.GetFileNameWithoutExtension(mapFile));
            }
            Panel.AddChild(_loadListBox);

            var cancelButton = new Button("Cancel", ButtonSkin.Default, Anchor.BottomLeft, new Vector2(0.5f, -1));
            cancelButton.OnClick += entity => {
                Hide();
                Parent?.Show();
            };
            Panel.AddChild(cancelButton);
            var loadButton = new Button("Load", ButtonSkin.Default, Anchor.BottomRight, new Vector2(0.5f, -1));
            loadButton.OnClick += entity => {
                Task.Run(
                         () => {
                             load?.Invoke(_loadListBox.SelectedValue);
                             MessageBox.ShowMsgBox("Map Loaded", $"Map \"{_loadListBox.SelectedValue}\" Loaded", new[] { new MessageBox.MsgBoxOption("OK", () => true) }, null, null, () => Hide());

                         });
            };
            Panel.AddChild(loadButton);
        }
        public override void Show() {
            _loadListBox.ClearItems();
            foreach (var mapFile in Directory.GetFiles(SearchDirectory, Extension)) {
                _loadListBox.AddItem(Path.GetFileNameWithoutExtension(mapFile));
            }
            _loadListBox.IsFocused = true;
            if (_loadListBox.Count > 0) {
                _loadListBox.SelectedIndex = 0;
            }
            base.Show();
        }
    }
}