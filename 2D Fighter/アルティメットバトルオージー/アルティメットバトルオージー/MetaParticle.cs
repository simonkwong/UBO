using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace アルティメットバトルオージー
{
    class MetaParticle
    {
        public Texture2D texture { get; set; }
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public float angle { get; set; }
        public float angularVelocity { get; set; }
        public Color color { get; set; }
        public float size { get; set; }
        public int lifeTime { get; set; }

        public MetaParticle(Texture2D texture, Vector2 position, Vector2 velocity,
                         float angle, float angularvelocity, Color color,
                         float size, int lifeTime)
        {
            this.texture = texture;
            this.position = position;
            this.velocity = velocity;
            this.angle = angle;
            this.angularVelocity = angularVelocity;
            this.color = color;
            this.size = size;
            this.lifeTime = lifeTime;
        }

        public Rectangle aabb()
        {
            int x = (int)this.position.X;
            int y = (int)this.position.Y;
            int width = texture.Bounds.Width;
            int height = texture.Bounds.Height;

            return new Rectangle(x, y, width, height);
        }

        public void Update()
        {
            lifeTime--;
            position += velocity;
            angle += angularVelocity;
        }

        public void Draw(SpriteBatch sb)
        {
            Rectangle particleRect = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            sb.Draw(texture, position, particleRect, color, angle, origin, size, SpriteEffects.None, 0f);
        }
    }
}
