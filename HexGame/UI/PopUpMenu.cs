namespace HexGame.UI {
    using System.Collections.Generic;
    using System.Linq;

    using GeonBit.UI;
    using GeonBit.UI.Entities;

    using Microsoft.Xna.Framework;

    public abstract class PopUpMenu {
        protected Panel Panel { get; }
        private List<PopUpMenu> ChildMenus { get; }
        protected PopUpMenu Parent { get; private set; }
        public bool IsActive => Panel.IsVisible() || ChildMenus.Any(m=>m.IsActive);
        private readonly UserInterface _root;

        protected PopUpMenu(string header, Vector2? size=null) {
            _root = new UserInterface();
            ChildMenus = new List<PopUpMenu>();
            Panel = new Panel(size??new Vector2(0.5f, 0.5f), PanelSkin.Simple) {
                Visible = false
            };

            var saveHeader = new Header(header);
            Panel.AddChild(saveHeader);
            var hr = new HorizontalLine();
            Panel.AddChild(hr);

            _root.AddEntity(Panel);
        }

        public virtual void Show() {
            InterfaceStack.PushInterface(_root);
            Panel.Visible = true;
        }

        public virtual void Hide() {
            InterfaceStack.PopInterface();
            Panel.Visible = false;
        }

        public void AddChildMenu(PopUpMenu child) {
            ChildMenus.Add(child);
            child.Parent = this;
        }
    }
}