namespace HexGame {
    using System;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Camera {
        
        private Vector3 Position { get; set; }
        private Vector3 Right { get; set; }
        private Vector3 Up { get; set; }
        private Vector3 Look { get; set; }




        public Matrix ProjectionMatrix { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public Matrix WorldMatrix { get; private set; }
        public BoundingFrustum Frustum { get; private set; }

        private float CameraSpeed { get; set; } = 3f;
        private readonly Input _input;

        public float NearZ { get; protected set; }
        public float FarZ { get; protected set; }
        public float Aspect { get; protected set; }
        public float FovY { get; protected set; }
        public float FovX {
            get {
                var halfWidth = 0.5f * NearWindowWidth;
                return 2.0f * (float)Math.Atan(halfWidth / NearZ);
            }
        }
        public float NearWindowWidth => Aspect * NearWindowHeight;
        public float NearWindowHeight { get; protected set; }
        public float FarWindowWidth => Aspect * FarWindowHeight;
        public float FarWindowHeight { get; protected set; }

        public Vector3 Target { get; set; }
        private float _radius;
        private float _alpha;
        private float _beta;

        public Camera(Input input) {
            _input = input;

            Position = new Vector3();
            Right = new Vector3(1, 0, 0);
            Up = new Vector3(0, 1, 0);
            Look = new Vector3(0, 0, 1);

            _radius = 10.0f;
            Target = new Vector3();


            ProjectionMatrix = Matrix.Identity;
            ViewMatrix = Matrix.Identity;
            WorldMatrix = Matrix.Identity;
        }
        public void LookAt(Vector3 pos, Vector3 target, Vector3 up) {
            Target = target;
            Position = pos;
            Look = Vector3.Normalize(target - pos);
            Right = Vector3.Normalize(Vector3.Cross(up, Look));
            Up = Vector3.Cross(Look, Right);
            _radius = (target - pos).Length();

            _beta = (float)Math.Asin((pos.Y - target.Y) / _radius);

            var sideRadius = _radius * (float)Math.Cos(_beta);
            _alpha = (float)Math.Acos((pos.X - target.X) / sideRadius);
            UpdateViewMatrix();

        }
        public void SetLens(float fovY, float aspect, float zn, float zf) {
            FovY = fovY;
            Aspect = aspect;
            NearZ = zn;
            FarZ = zf;

            NearWindowHeight = 2.0f * NearZ * (float)Math.Tan(0.5f * FovY);
            FarWindowHeight = 2.0f * FarZ * (float)Math.Tan(0.5f * FovY);

            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FovY, Aspect, NearZ, FarZ);
        }

        private void UpdateViewMatrix() {
            var sideRadius = _radius * (float)Math.Cos(_beta);
            var height = _radius * (float)Math.Sin(_beta);

            Position = new Vector3(
                                   Target.X + sideRadius * (float)Math.Cos(_alpha),
                                   Target.Y + height,
                                   Target.Z + sideRadius * (float)Math.Sin(_alpha)
                                  );


            ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
            Frustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);

            Right = new Vector3(ViewMatrix.M11, ViewMatrix.M21, ViewMatrix.M31);
            Right.Normalize();

            Look = new Vector3(ViewMatrix.M13, ViewMatrix.M23, ViewMatrix.M33);
            Look.Normalize();
        }

        public void Update(GameTime gameTime) {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_input.IsDown(Commands.CameraStrafeLeft)) {
                Strafe(-dt*CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraStrafeRight)) {
                Strafe(dt*CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraForward)) {
                Walk(-dt*CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraBackward)) {
                Walk(dt*CameraSpeed);
            }

            if (_input.IsDown(Commands.CameraZoomIn)) {
                Zoom(-dt * CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraZoomOut)) {
                Zoom(dt * CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraOrbitRight)) {
                Yaw(-dt*CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraOrbitLeft)) {
                Yaw(dt*CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraOrbitUp)) {
                Pitch(dt*CameraSpeed);
            }
            if (_input.IsDown(Commands.CameraOrbitDown)) {
                Pitch(-dt*CameraSpeed);
            }
            var mouseScroll = _input.MouseScrolled();
            if (mouseScroll != 0) {
                Zoom(dt * CameraSpeed * mouseScroll/10.0f);
            }
            

            UpdateViewMatrix();
        }
        public void Strafe(float d) {
            var dt = Vector3.Normalize(new Vector3(Right.X, 0, Right.Z)) * d;
            Target += dt;
        }
        public void Walk(float d) {
            Target += Vector3.Normalize(new Vector3(Look.X, 0, Look.Z)) *d;
        }
        public void Zoom(float dr) {
            _radius += dr;
            _radius = MathHelper.Clamp(_radius, 2.0f, 150.0f);
        }
        public  void Yaw(float angle) {
            _alpha = (_alpha + angle) % ((float)Math.PI*2.0f);
        }
        public  void Pitch(float angle) {
            _beta += angle;
            _beta = MathHelper.Clamp(_beta, 0.05f, (float)Math.PI/2.0f - 0.01f);
        }


        public Ray CalculateRay(Vector2 screenPos, Viewport viewport) {
            var nearPoint = viewport.Unproject(new Vector3(screenPos, 0), ProjectionMatrix, ViewMatrix, WorldMatrix);
            var farPoint = viewport.Unproject(new Vector3(screenPos, 1.0f), ProjectionMatrix, ViewMatrix, WorldMatrix);

            var direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }
    }
}