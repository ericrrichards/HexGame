using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HexGame {
    public class Input {
        private KeyboardState _currentKeys;
        private KeyboardState _previousKeys;

        // TODO mouse and gamepad

        private GamePadState _currentPad;
        private GamePadState _previousPad;

        private Dictionary<string, List<Keys>> BindingMap { get; }


        public Input() {
            BindingMap = new Dictionary<string, List<Keys>>();
        }

        public void Update() {
            _previousKeys = _currentKeys;
            _currentKeys = Keyboard.GetState();

            _previousPad = _currentPad;
            _currentPad = GamePad.GetState(PlayerIndex.One);
        }

        public bool IsDown(string vkey) {
            if (BindingMap.TryGetValue(vkey, out var keys)) {
                return keys.Any(k => _currentKeys.IsKeyDown(k));
            }
            return false;
        }


        public void SetBinding(string vkey, params Keys[] keys) {
            BindingMap[vkey] = keys.ToList();
        }

        public void AddBinding(string vkey, params Keys[] keys) {
            AddBinding(vkey, keys);
        }

        public void AddBinding(string vkey, IEnumerable<Keys> keys) {
            if (!BindingMap.ContainsKey(vkey)) {
                BindingMap[vkey] = new List<Keys>();
            }

            BindingMap[vkey].AddRange(keys);
        }

        public void RemoveBinding(string vkey, Keys key) {
            if (BindingMap.TryGetValue(vkey, out var keys)) {
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