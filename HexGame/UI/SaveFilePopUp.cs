namespace HexGame.UI {
    using System;
    using System.IO;

    using GeonBit.UI.Entities;
    using GeonBit.UI.Utils;

    using Microsoft.Xna.Framework;

    public class SaveFilePopUp : PopUpMenu {
        private readonly SelectList _saveListbox;
        private readonly TextInput _saveInput;
        public string SearchDirectory { get; set; }
        public string Extension { get;set; }

        public SaveFilePopUp(string directory, string extension="*",Action<string> save=null) : base("Save Map", new Vector2(0.5f, 0.6f)) {
            Extension = extension;
            SearchDirectory = directory;

            _saveInput = new TextInput(false, Anchor.AutoCenter);

            _saveListbox = new SelectList(new Vector2(-1, -1), Anchor.AutoCenter, null, PanelSkin.Simple);
            _saveListbox.OnValueChange += entity => { _saveInput.Value = _saveListbox.SelectedValue; };
            Panel.AddChild(_saveListbox);
            
            
            Panel.AddChild(_saveInput);
            var cancelButton = new Button("Cancel", ButtonSkin.Default, Anchor.BottomLeft, new Vector2(0.5f, -1));
            cancelButton.OnClick += entity => { Hide(); Parent?.Show(); };
            Panel.AddChild(cancelButton);
            var saveButton = new Button("Save", ButtonSkin.Default, Anchor.BottomRight, new Vector2(0.5f, -1));
            saveButton.OnClick += entity => {
                save?.Invoke(_saveInput.Value);
                MessageBox.ShowMsgBox("Map Saved", $"Map \"{_saveInput.Value}\" saved", new[] { new MessageBox.MsgBoxOption("OK", () => true) }, null, null, () => Hide());

            };
            
            Panel.AddChild(saveButton);
            
        }

        public override void Show() {
            _saveListbox.ClearItems();
            foreach (var mapFile in Directory.GetFiles(SearchDirectory, Extension)) {
                _saveListbox.AddItem(Path.GetFileNameWithoutExtension(mapFile));
            }
            _saveInput.IsFocused = true;
            base.Show();
        }
    }
}