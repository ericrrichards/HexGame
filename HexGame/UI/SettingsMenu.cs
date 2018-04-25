using System;

namespace HexGame.UI {
    using System.IO;

    using GeonBit.UI.Entities;

    public class SettingsMenu : PopUpMenu {
        


        public SettingsMenu(Action exit=null, Action<string>save=null, Action<string> load=null) : base("Menu") {
            
            var btnReturn = new Button("Close Menu", ButtonSkin.Default, Anchor.AutoCenter);
            btnReturn.OnClick += entity => Hide();

            var btnSaveMap = new Button("Save Map", ButtonSkin.Default, Anchor.AutoCenter);
            var btnLoadMap = new Button("Load Map", ButtonSkin.Default, Anchor.AutoCenter);

            var exitButton = new Button("Exit", ButtonSkin.Default, Anchor.AutoCenter);
            exitButton.OnClick += entity => { exit?.Invoke(); };

            var saveMenu = new SaveFilePopUp(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame"), "*.mapp", save);
            AddChildMenu(saveMenu);
            
            btnSaveMap.OnClick += entity => {
                saveMenu.Show();
            };

            var loadMenu = new LoadFilePopUp(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexGame"), "*.mapp", load);
            AddChildMenu(loadMenu);
            
            btnLoadMap.OnClick += entity => {
                loadMenu.Show();
            };

            Panel.AddChild(btnReturn);
            Panel.AddChild(btnSaveMap);
            Panel.AddChild(btnLoadMap);
            Panel.AddChild(exitButton);

        }
    }
    
}
