using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace アルティメットバトルオージー
{
    class SpriteBin
    {
        List<Sprite> sprites;

        private SpriteFont _font;
        public SpriteBin(SpriteFont font)
        {
            sprites = new List<Sprite>();
            _font = font;
        }

        public ImageSprite AddImageSprite(Texture2D content)
        {
            ImageSprite s = new ImageSprite(content);
            sprites.Add(s);
            return s;
        }

        public TextSprite AddTextSprite(string content)
        {
            TextSprite s = new TextSprite(content, _font);
            sprites.Add(s);
            return s;
        }

        public void Add(Sprite s)
        {
            sprites.Add(s);
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (Sprite sprite in sprites)
            {
                sprite.Draw(sb);
            }
        }

    }
}
