using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    class World
    {
        public List<Player> players;
        public Arena arena;
        public Camera2d cam;

        /* An AABB that covers all of the players in the world. */
        public Rectangle playerBounds;

        public ParticleEngine blood;


        public World(List<Player> players, Arena arena, Camera2d cam, ParticleEngine blood)
        {
            this.cam = cam;
            this.players = players;
            this.arena = arena;
            playerBounds = new Rectangle(1, 1, 1, 1);

            this.blood = blood;

            // give every player a reference to the world
            foreach (Player player in this.players)
            {
                player.world = this;
            }


        }

        public void LoadContent(ContentManager content)
        {
            foreach (Player player in this.players)
            {
                player.LoadContent(content);
            }
        }



        public void Update(GameTime time)
        {
            updatePlayerBounds();
            cam.update(this);
            arena.update(time);

            List<Player> toRemove = new List<Player>();
            foreach (Player p in players)
            {
                p.Update(time);

                if (p.position.Y > arena.arenasize.Bottom)
                {
                    Console.WriteLine("PLAYER DIED");
                    toRemove.Add(p);
                }
            }

            foreach(Player p in toRemove)
            {
                players.Remove(p);
            }            
        }

        public void updatePlayerBounds()
        {
            Player leftMostPlayer = players[0];
            Player rightMostPlayer = players[0];
            Player upMostPlayer = players[0];
            Player bottomMostPlayer = players[0];

            foreach (Player p in players)
            {
                if (p.position.X < leftMostPlayer.position.X)
                {
                    leftMostPlayer = p;
                }

                if (p.position.Y < upMostPlayer.position.Y)
                {
                    upMostPlayer = p;
                }

                if (p.position.X > rightMostPlayer.position.X)
                {
                    rightMostPlayer = p;
                }

                if (p.position.Y > bottomMostPlayer.position.Y)
                {
                    bottomMostPlayer = p;
                }
            }


            float width = rightMostPlayer.position.X - leftMostPlayer.position.X;
            float height = bottomMostPlayer.position.Y - upMostPlayer.position.Y;

            playerBounds.X = (int) leftMostPlayer.position.X;
            playerBounds.Y = (int) upMostPlayer.position.Y;
            playerBounds.Width = (int) width;
            playerBounds.Height = (int) height;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(arena.background, arena.arenasize, Color.White);

            blood.DrawToRenderTargets(sb);
            blood.Draw();

            foreach (Block b in arena.blocks)
            {
                b.draw(sb);
            }

            foreach (Player p in players)
            {
                p.drawPlayer(sb);
            }
        }

    }
}
