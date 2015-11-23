//You can write text with this one
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace namespace_default
{
    class TextWriter
    {
        Game1 Game;

        SpriteFont font;


        public TextWriter(Game1 newGame, string fontname)
        {
            Game = newGame;

            font = Game.Content.Load<SpriteFont>(fontname);
        }

        public void Write(string text, Vector2 position, Color color)
        {
            Game.spriteBatch.DrawString(font, text, position, color);
        }
    }
}
