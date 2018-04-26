namespace HexGame.UI {
    using System.Collections.Generic;

    using GeonBit.UI;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public static class InterfaceStack {
        private static readonly Stack<UserInterface> _stack = new Stack<UserInterface>();

        public static void PushInterface(UserInterface userInterface) {
            _stack.Push(userInterface);
            UserInterface.Active = userInterface;
        }

        public static void PopInterface() {
            _stack.Pop();
            UserInterface.Active = _stack.Peek();
        }

        public static void Update(GameTime gameTime) {
            UserInterface.Active.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch) {
            UserInterface.Active.Draw(spriteBatch);
        }
    }
}