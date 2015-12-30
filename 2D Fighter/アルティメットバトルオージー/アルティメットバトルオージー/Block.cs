using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    /**
     * This represents a block in the world.
     * A block is basicly a wrapper class to the animated object,
     * it adds on the ability to be collided with.
     */
    class Block
    {
        public Rectangle AABB;
        public bool collidable;
        public AnimatedObject anim;

        // for debugging
        public Color color;

        public Vector2 position
        {
            get
            {
                return this.anim.Position;
            }
            set
            {
                this.anim.Position = value;
            }
        }

        public Block(AnimatedObject anim, bool collidable, Vector2 position)
        {
            this.anim = anim;
            this.position = position;
            this.collidable = collidable;
            this.AABB = new Rectangle((int) anim.Position.X, (int) anim.Position.Y, (int) anim.mWidth , (int) anim.mHeight);
        }

        public void update(GameTime gametime)
        {
            color = Color.White;
            updateBoundingBox();
            anim.Update(gametime);
        }

        public void draw(SpriteBatch sb)
        {
            anim.Draw(sb);
        }

        private void updateBoundingBox()
        {
            this.AABB.X = (int) this.position.X;
            this.AABB.Y = (int) this.position.Y;
        }

        public void draw(SpriteBatch sb, Vector2 offSet, float zoom)
        {
            anim.Draw(sb, offSet, color, zoom);
        }
    }
}
