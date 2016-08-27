using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace EmpireUWP
{
    class TiledTexture
    {
        private Texture2D _texture; 
        private string _fileName;
        public string FileName { get; }
        private float _speed;
        Rectangle sourceRect;

        public TiledTexture(string fileName, float speed)
        {
            _fileName = fileName;
            _speed = speed;
        }

        public void LoadContent(ContentManager content) {

            // TODO: implement error handling
            _texture = content.Load<Texture2D>(_fileName);

            sourceRect = new Rectangle(0, 0, _texture.Width, _texture.Height);
        }

        public void UnloadContent() {
            _texture.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, int screenWidth, int screenHeight) {
            Vector2 view = new Vector2();

            view.Y = screenHeight + y*_speed % _texture.Height;
            while (view.Y > -1*_texture.Width)
            {
                view.X = screenWidth + x * _speed % _texture.Width;
                while (view.X > -1*_texture.Width)
                {
                    spriteBatch.Draw(_texture, view, sourceRect, Color.White);
                    view.X -= _texture.Width;
                }

                view.Y -= _texture.Height;
            }
        }
    }
}
