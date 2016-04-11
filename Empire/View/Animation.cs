using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Empire.View
{
    class Animation
    {
        private int _elapsedTime;
        private int _frameDuration;
        private int _frameCount;
        private int _currentFrame;
        private Rectangle _sourceRect = new Rectangle();
        private Rectangle _destinationRect = new Rectangle();
        private int _frameWidth;
        private int _frameHeight;
        private bool _active;
        private bool _looping;
        private Texture2D _spriteStrip;
        public Texture2D SpriteStrip
        {
            get { return _spriteStrip; }
            set { _spriteStrip = value; }
        }
        public float scale { get; set; }

        public float Zorder { get; set; }

        public Animation(Texture2D texture, int frameWidth, int frameHeight, int frameCount, int frameDuration, bool looping)
        {

            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _frameCount = frameCount;
            _frameDuration = frameDuration;

            _looping = looping;
            _spriteStrip = texture;

            _elapsedTime = 0;
            _currentFrame = 0;
            scale = 1;  // default scale to 1
            _active = true;
        }

        public Animation Clone()
        {
           return new Animation(_spriteStrip, _frameWidth, _frameHeight, _frameCount, _frameDuration, _looping);
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            // If the elapsed time is larger than the frame time
            // we need to switch frames
            if (_elapsedTime > _frameDuration)
            {
                _currentFrame++;
                if (_currentFrame == _frameCount)
                {
                    _currentFrame = 0;
                    if (_looping == false)
                        _active = false;
                }
                _elapsedTime = 0;
            }
        }

        // Draw the Animation Strip
        // rotation will be around the center of the frame
        // position is to be in view coords
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float orientation)
        {
            // Only draw the animation when we are active
            if (_active)
            {
                _sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
                _destinationRect = new Rectangle(
                   // (int)position.X - (int)(_frameWidth*scale / 2),
                    //(int)position.Y - (int)(_frameHeight*scale / 2),
                    (int)position.X, (int)position.Y,
                    (int)(_frameWidth*scale),
                    (int)(_frameHeight*scale));
                spriteBatch.Draw(_spriteStrip, _destinationRect, _sourceRect, Color.White, orientation, new Vector2(_frameWidth/2,_frameHeight/2),SpriteEffects.None, Zorder);
            }
        }
    }
}