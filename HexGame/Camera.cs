namespace HexGame {
    using Microsoft.Xna.Framework;


    public class Camera {
        private Vector3 CameraTarget { get; set; }
        private Vector3 CameraPosition { get; set; }
        public Matrix ProjectionMatrix { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }
        private float CameraSpeed { get; set; } = 1f;
        private readonly Input _input;


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