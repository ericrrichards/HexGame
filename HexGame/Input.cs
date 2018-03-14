using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HexGame {
    public class Input {
        private KeyboardState _currentKeys;
        private KeyboardState _previousKeys;

        private MouseState _currentMouse;
        private MouseState _previousMouse;

        // TODO mouse and gamepad

        private GamePadState _currentPad;
        private GamePadState _previousPad;

        private Dictionary<string, List<Keys>> KeyBindingMap { get; }


        public Input() {
            KeyBindingMap = new Dictionary<string, List<Keys>>();
        }

        public void Update() {
            _previousKeys = _currentKeys;
            _currentKeys = Keyboard.GetState();

            _previousPad = _currentPad;
            _currentPad = GamePad.GetState(PlayerIndex.One);

            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
        }

        public bool IsDown(string vkey) {
            if (KeyBindingMap.TryGetValue(vkey, out var keys)) {
                return keys.Any(k => _currentKeys.IsKeyDown(k));
            }
            return false;
        }
        public bool IsPressed(string vkey) {
            if (KeyBindingMap.TryGetValue(vkey, out var keys)) {
                return keys.Any(k => _currentKeys.IsKeyDown(k) && _previousKeys.IsKeyUp(k));
            }
            return false;
        }

        public bool MouseClicked(bool left) {
            if (left) {
                return _currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;
            } else {
                return _currentMouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released;
            }
        }


        public void SetBinding(string vkey, params Keys[] keys) {
            KeyBindingMap[vkey] = keys.ToList();
        }

        public void AddBinding(string vkey, params Keys[] keys) {
            AddBinding(vkey, keys);
        }

        public void AddBinding(string vkey, IEnumerable<Keys> keys) {
            if (!KeyBindingMap.ContainsKey(vkey)) {
                KeyBindingMap[vkey] = new List<Keys>();
            }

            KeyBindingMap[vkey].AddRange(keys);
        }

        public void RemoveBinding(string vkey, Keys key) {
            if (KeyBindingMap.TryGetValue(vkey, out var keys)) {
                keys.Remove(key);
            }
        }

        public void AddBindings(Dictionary<string, List<Keys>> bindings) {
            foreach (var binding in bindings) {
                AddBinding(binding.Key, binding.Value);
            }
        }

        
    }
}