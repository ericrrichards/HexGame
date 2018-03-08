namespace HexGame {
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;


    public class Input {
        private KeyboardState _current;
        private KeyboardState _previous;

        // TODO mouse and gamepad

        private Dictionary<string, List<Keys>> BindingMap { get; }


        public Input() {
            BindingMap = new Dictionary<string, List<Keys>>();
        }

        public void Update() {
            _previous = _current;
            _current = Keyboard.GetState();
        }

        public bool IsPressed(string vkey) {
            if (BindingMap.TryGetValue(vkey, out var keys)) {
                return keys.Any(k => _current.IsKeyDown(k));
            }
            return false;
        }


        public void SetBinding(string vkey, params Keys[] keys) {
            BindingMap[vkey] = keys.ToList();
        }

        public void AddBinding(string vkey, params Keys[] keys) {
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

        public void AddBindings(Dictionary<string, Keys[]> bindings) {
            foreach (var binding in bindings) {
                AddBinding(binding.Key, binding.Value);
            }
        }
    }

    public static class Commands {
        public const string CameraStrafeLeft = "Camera_StrafeLeft";
        public const string CameraStrafeRight = "Camera_StrafeRight";
        public const string CameraForward = "Camera_Forward";
        public const string CameraBackward = "Camera_Backward";
        public const string CameraZoomIn = "Camera_ZoomIn";
        public const string CameraZoomOut = "Camera_ZoomOut";

    }

    public class Camera {
        private Vector3 CameraTarget { get; set; }
        private Vector3 CameraPosition { get; set; }
        public Matrix ProjectionMatrix { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }
        private float CameraSpeed { get; set; } = 1f;
        private Input _input;


        public Camera(float aspectRatio, Input input) {
            _input = input;

            CameraTarget = Vector3.Zero;
            CameraPosition = new Vector3(0, 0, 3);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspectRatio, .01f, 100);
            ViewMatrix = Matrix.CreateLookAt(CameraPosition, CameraTarget, Vector3.Up);
            WorldMatrix = Matrix.CreateTranslation(0, 0, 0);
        }

        public void Update(GameTime gameTime) {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var dp = new Vector3();
            var dz = new Vector3();
            if (_input.IsPressed(Commands.CameraStrafeLeft)) {
                dp.X -= dt * CameraSpeed;
            }
            if (_input.IsPressed(Commands.CameraStrafeRight)) {
                dp.X += dt * CameraSpeed;
            }
            if (_input.IsPressed(Commands.CameraForward)) {
                dp.Y += dt * CameraSpeed;
            }
            if (_input.IsPressed(Commands.CameraBackward)) {
                dp.Y -= dt * CameraSpeed;
            }

            if (_input.IsPressed(Commands.CameraZoomIn)) {
                dz.Z -= dt * CameraSpeed;
            }
            if (_input.IsPressed(Commands.CameraZoomOut)) {
                dz.Z += dt * CameraSpeed;
            }
            CameraPosition += dp + dz;
            CameraTarget += dp;


            ViewMatrix = Matrix.CreateLookAt(CameraPosition, CameraTarget, Vector3.Up);
        }
    }
}