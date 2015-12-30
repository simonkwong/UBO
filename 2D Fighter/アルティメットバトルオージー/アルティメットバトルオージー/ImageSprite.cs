using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    class ImageSprite : Sprite
    {
        Texture2D image;
        private Vector2 mOrigin;

        public ImageSprite(Texture2D image)
        {
            this.image = image;
            Position = Vector2.Zero;
            Scale = 1;
            Alpha = 255;
            Color = Color.White;
        }

        public override Vector2 Size()
        {
            return new Vector2(this.image.Width, this.image.Height);
        }



        public override void Draw(SpriteBatch sb)
        {

            sb.Draw(image, new Rectangle((int) this.Position.X, (int) this.Position.Y, image.Width, image.Height), this.Color);
        }

    }
}
