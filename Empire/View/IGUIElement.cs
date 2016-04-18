using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.View
{
    // Interface for the top-level GUI elements.
    // Textboxes, StatusBars, minimap, etc.
    interface IGUIElement
    {
        bool Visible { get; set; }
        string Name { get; set; }

        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
        void LoadContent();
    }
}
