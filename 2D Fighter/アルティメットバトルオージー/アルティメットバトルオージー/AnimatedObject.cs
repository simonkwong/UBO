using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    class AnimatedObject
    {
        public bool Looping { get; set; }
        public Vector2 Position { get; set; }
        public int Width { get { return mWidth; } }
        public float SecondsPerFrame { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }

        public Texture2D mTexture;
        public int mWidth;
        public int mHeight;
        private int mNumFrames;
        private int mCurrentFrame;
        private Vector2 mCenter;
        private float mTimeSinceLastFrame;
        private int frameX;

        public AnimatedObject(Texture2D texture, int width, int height, Vector2 center = default(Vector2), bool looping = false, float secondsPerFrame = 0.1f, Vector2 position = default(Vector2))
        {
            mTexture = texture; 


            mWidth = width;
            mHeight = height;
            SecondsPerFrame = secondsPerFrame;
            Position = position;
            mCenter = center;
            mCurrentFrame = 0;
            Rotation = 0;
            mTimeSinceLastFrame = 0;
            Scale = 1;
            frameX = (texture.Width / width);
            mNumFrames = (texture.Width / width) * (texture.Height / height);
            Looping = looping;
        }

        public void Update(GameTime gameTime)
        {
            mTimeSinceLastFrame += (float) gameTime.ElapsedGameTime.TotalSeconds;
            
            if (mTimeSinceLastFrame > SecondsPerFrame)
            {
                if (mCurrentFrame == mNumFrames - 1)
                {
                    if (Looping)
                    {
                        mCurrentFrame = 0;
                    }
                }
                else
                {
                    mCurrentFrame++;
                }
                mTimeSinceLastFrame = 0;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            //  calculate position in spritesheet
            int curX = (this.mCurrentFrame % this.frameX) * mWidth;
            int curY = (int) (Math.Floor((double) (this.mCurrentFrame / this.frameX) * mHeight));

            sb.Draw(mTexture, Position, new Rectangle(curX, curY, mWidth, mHeight), Color.White, Rotation, mCenter, Scale, SpriteEffects.None, 0);
        }

        public void Draw(SpriteBatch sb, Vector2 offset, Color color, float zoom)
        {
            //  calculate position in spritesheet
            int curX = (this.mCurrentFrame % this.frameX) * mWidth;
            int curY = (int)(Math.Floor((double)(this.mCurrentFrame / this.frameX) * mHeight));

            sb.Draw(mTexture, Position + offset, new Rectangle(curX, curY, mWidth, mHeight), color, Rotation, mCenter, zoom, SpriteEffects.None, 0);
        }

    }
}
