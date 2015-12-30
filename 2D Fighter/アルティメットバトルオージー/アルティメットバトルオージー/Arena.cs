using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    class Arena
    {
        public string name;
        public List<Block> blocks;
        public Texture2D background;
        public Rectangle arenasize;

        public Arena(String name, List<Block> blocks, String background, Vector2 arenasize)
        {
            this.name = name;
            this.blocks = blocks;
            this.background = Game1.contentM.Load<Texture2D>(background);
            this.arenasize = Vector2toRectangle(arenasize);
        }

        public Rectangle Vector2toRectangle(Vector2 v2)
        {
            return new Rectangle(0, 0, (int)v2.X, (int)v2.Y);
        }

        public void update(GameTime gametime)
        {
            foreach (Block b in blocks)
            {
                b.update(gametime);
            }
        }


    }
}
