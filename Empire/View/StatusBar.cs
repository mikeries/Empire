using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Empire.View
{
    // Displays a bar that fills from left to right based on the Value, which is a percentage 
    // of the max (0 to 1f)

    internal class StatusBar : IGUIElement
    {
        public string Name { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }  // length of a 'full' bar, in viewport pixels
        public int Height { get; set; }  // bar thickness, in viewport pixels
        public Color ForeColor { get; set; }    // bar foreground color
        public Color BackColor { get; set; }    // bar background color
        private Vector2 ViewportLocation;   // location of the leftmost end
        public float Value;               // percent full

        public StatusBar(string name, int x = 0, int y = 0)
        {
            Name = name;
            ViewportLocation.X = x;
            ViewportLocation.Y = y;
            Height = 4;  // default value
            Width = 100; // default value
            Value = 1f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ViewHelper.DrawRectangle(spriteBatch, new Rectangle((int)ViewportLocation.X, (int)ViewportLocation.Y, Width, Height), BackColor);
            ViewHelper.DrawRectangle(spriteBatch, new Rectangle((int)ViewportLocation.X, (int)ViewportLocation.Y, (int)(Width*Value), Height), ForeColor);
        }

        public void LoadContent()
        {
            
        }

        public void Update(GameTime gameTime) { }
    }
}
