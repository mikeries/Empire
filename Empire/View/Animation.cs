using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Empire.View
{
    class Animation
    {
        private int _elapsedTime;       // accumulate number of milliseconds since last frame switch
        private int _frameDuration;     // the duration of each frame in milliseconds
        private int _frameCount;        // the number of frames in this sprite strip
        private int _currentFrame;      // the currently displayed frame
        private int _frameWidth;        // frame width in texture pixels
        private int _frameHeight;       // frame height in texture pixels
        private Texture2D _spriteStrip; // a texture with the frames arranged horizontally in one strip
        private Rectangle _sourceRect = new Rectangle();        // where in the spritestrip to pull the frame from
        private Rectangle _destinationRect = new Rectangle();   // where on the screen to display the frame
        private CanvasLayer _canvasLayer;   // draw order -- lower numbers draw before higher numbers.  Range: 0 to CanvasLayer.Count
        private float _zOrder;          // the _cavasLayer variable scaled to a float between 0 and 1 (required by game engine)

        private bool _active;           // animation will not draw or advance while this is false
        private bool _looping;

        internal float Scale { get; set; }    // scales the frame size

        internal Animation(Texture2D texture, int frameWidth, int frameHeight, int frameCount, int frameDuration, bool looping, CanvasLayer canvasLayer)
        {
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _frameCount = frameCount;
            _frameDuration = frameDuration;

            _looping = looping;
            _spriteStrip = texture;
            _canvasLayer = canvasLayer;

            // Monogame requires a float for zOrder, so we pre-compute this based on the desired canvas layer
            _zOrder = (float)canvasLayer / (float)CanvasLayer.Count;

            _elapsedTime = 0;
            _currentFrame = 0;

            Scale = 1;              // default scale to 1
            _active = true;
        }

        internal Animation Clone()
        {
           return new Animation(_spriteStrip, _frameWidth, _frameHeight, _frameCount, _frameDuration, _looping, _canvasLayer);
        }

        internal void Update(GameTime gameTime)
        {
            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            // If the elapsed time is larger than the frame time
            // we need to switch frames
            if (_elapsedTime > _frameDuration)
            {
                _currentFrame++;
                _elapsedTime = 0;

                if (_currentFrame == _frameCount)
                {
                    _currentFrame = 0;

                    // if we've completed the entire animation and it's not looping, we stop.
                    if (_looping == false)
                    {
                        _active = false;
                    }
                }

            }
        }

        // Draw the Animation Strip
        // rotation will be around the center of the frame
        // position is to be in view coords
        // Zorder ranges from 0 to 1, with lower values being shown behind higher values.
        internal void Draw(SpriteBatch spriteBatch, Vector2 position, float orientation)
        {
            if (_active)
            {
                _sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
                _destinationRect = new Rectangle(
                    (int)position.X, 
                    (int)position.Y,
                    (int)(_frameWidth*Scale),
                    (int)(_frameHeight*Scale)
                );

                spriteBatch.Draw(
                    _spriteStrip, 
                    _destinationRect, 
                    _sourceRect, 
                    Color.White, 
                    orientation, 
                    new Vector2(_frameWidth/2,_frameHeight/2),
                    SpriteEffects.None, 
                    _zOrder
                );
            }
        }
    }
}