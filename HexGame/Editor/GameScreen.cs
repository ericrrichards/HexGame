namespace HexGame.Editor {
    using GeonBit.UI;

    using HexGame.UI;

    using Microsoft.Xna.Framework;

    public abstract class GameScreen {
        public bool Active { get; protected set; }
        protected UserInterface Interface { get; }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(GameTime gameTime) { }

        public GameScreen() {
            Interface = new UserInterface();
        }

        public void Activate() {
            Active = true;
            InterfaceStack.PushInterface(Interface);
        }

        public void Deactivate() {
            Active = false;
            InterfaceStack.PopInterface();
        }
    }
}