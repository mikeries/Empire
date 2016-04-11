using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.View
{
    interface IGUIElement
    {
        bool Visible { get; set; }
        string Name { get; set; }

        void Update(GameTime gameTime);
        void Initialize();
        void Draw(SpriteBatch spriteBatch);
    }
}
