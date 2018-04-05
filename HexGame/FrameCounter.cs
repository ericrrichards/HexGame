namespace HexGame {
    using System;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class FrameCounter {
        private int _totalFrames;
        private TimeSpan _elapsedTime;
        private float _fps;
        private readonly SpriteFont _font;
        public Color Color { get; set; }
        public Vector2 Position { get; set; }

        public FrameCounter(SpriteFont font) {
            _font = font;
            Position = new Vector2(0, 20);
            Color = Color.White;
        }

        public void Update(GameTime gameTime) {
            _elapsedTime += gameTime.ElapsedGameTime;
            if (_elapsedTime.TotalMilliseconds > 1000) {
                _fps = _totalFrames;
                _totalFrames = 0;
                _elapsedTime -= TimeSpan.FromSeconds(1);
            }
        }

        public void CountFrame() {
            _totalFrames++;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, $"FPS {_fps:F1}", Position, Color  );
            spriteBatch.End();
        }
    }
}