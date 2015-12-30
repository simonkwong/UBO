using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace アルティメットバトルオージー
{

    class TextSprite : Sprite
    {
        public SpriteFont Font
        {
            get
            {
                return mFont;
            }
            set
            {
                mFont = value;
                if (Text != null)
                {
                    mOrigin = mFont.MeasureString(Text) / 2; 
                }
            }
        }


        public string Text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
                if (mFont != null)
                {
                    mOrigin = mFont.MeasureString(Text) / 2; 
                }
            }
        }

        
        public TextSprite(string text, SpriteFont font)
        {
            mFont = font;
            Text = text;
            Position = Vector2.Zero;
            Color = Color.Red;
            Scale = 1;
            Alpha = 255;
        }

        public override Vector2 Size()
        {
            return mFont.MeasureString(Text);
        }

        private SpriteFont mFont;
        private string mText;
        private Vector2 mOrigin;

        public override void Draw(SpriteBatch sb)
        {
            Vector2 origin;
            if (Centered)
            {
                origin = mOrigin;
            }
            else
            {
                origin = Vector2.Zero;
            }

            sb.DrawString(Font, mText, Position, new Color(Color.R, Color.G, Color.B, Alpha), Rotation, mOrigin, Scale, SpriteEffects.None, 0);
        }
    }
}
