using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.View
{
    class TextBlock : IGUIElement
    {
        public bool Visible { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public Color Color{ get; set; }
        public TextProperties TextAlignment = TextProperties.Left;

        private SpriteFont _font;
        private string _fontName;
        private Vector2 ViewportLocation;

        public TextBlock(string name, string fontName, int x = 0, int y = 0)
        {
            Name = name;
            _fontName = fontName;
            ViewportLocation.X = x;
            ViewportLocation.Y = y;
            Color = Color.Wheat;
            Text = "You forgot to set the text";
            Visible = true;
        }

        // TODO:  Need to figure out the formatting... got frustrated, moving on for now.
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            string formattedText = String.Format("{0,10:N}", Text);
            spriteBatch.DrawString(_font, formattedText, new Vector2((int)ViewportLocation.X, (int)ViewportLocation.Y), Color);
        }

        public void LoadContent()
        {
            _font = ViewHelper.GetFont(_fontName);
        }

        public void Update(GameTime gameTime) { } // Textblock doesn't actually change on updates
    }
}
