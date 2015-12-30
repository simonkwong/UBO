using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace アルティメットバトルオージー
{
    class Node
    {
        public String name;
        public Vector2 positionInParent;
        public float localOrientation;
        public Node parent;
        public Boolean isLeaf;
        public String nodeTextureName;
        public Texture2D texture;
        public Texture2D flipTexture;
        public Vector2 textureOrigin;
        public float zoom;
        public Color color;
        public String state;
        public Rectangle aabb;
        public List<Player> playersThatHitMe;

        public Node(String name, Vector2 positionInParent, float orientation, Node parent, String isLeaf, Vector2 textureOrigin, String texture) 
        {
            playersThatHitMe = new List<Player>();
            color = Color.White;
            this.zoom = 1.0f;
            this.name = name;
            this.localOrientation = orientation;
            this.parent = parent;
            this.textureOrigin = textureOrigin;
            this.positionInParent = positionInParent;

            if (isLeaf.Equals("T"))
                this.isLeaf = true;
            else
                this.isLeaf = false;

            this.nodeTextureName = texture;
            state = "passive";
            aabb = new Rectangle((int) this.positionInWorld.X, (int) this.positionInWorld.Y, 2, 2);
        }

        public Vector2 rotate(Vector2 vec, float angle)
        {
            return new Vector2(vec.X * (float) Math.Cos(angle) - vec.Y * (float) Math.Sin(angle),
                               vec.X * (float) Math.Sin(angle) + vec.Y * (float) Math.Cos(angle));
        }

        public Vector2 worldToLocal(Vector2 worldPos, Node obj)
        {
            if (obj == null)
            {
                return worldPos;
            }

            Vector2 parentPos = worldToLocal(worldPos, obj.parent);
            Vector2 localPos = rotate(parentPos - obj.positionInParent, -obj.localOrientation);
            return localPos;
        }

        public Vector2 positionInWorld
        {
            get
            {
                Vector2 worldPosition = positionInParent;
                Node temp = parent;

                while (temp != null)
                {
                    worldPosition = rotate(worldPosition, temp.localOrientation);
                    worldPosition = worldPosition + temp.positionInParent;
                    temp = temp.parent;
                }
                return worldPosition;
            }
            set
            {
                positionInParent = worldToLocal(value, parent);
            }
        }

        public float orientationInWorld
        {
            get
            {
                // Get the rotation of the object in world space
                float rotation = localOrientation;
                Node temp = parent;
                while (temp != null)
                {
                    rotation = rotation + temp.localOrientation;
                    temp = temp.parent;
                }
                return rotation;
            }
            set
            {
                float ancestorRotation = 0.0f;
                Node temp = parent;
                while (temp != null)
                {
                    ancestorRotation = ancestorRotation + temp.localOrientation;
                    temp = temp.parent;
                }
                localOrientation = (int) (value - ancestorRotation);
            }
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(nodeTextureName);
            texture.Name = nodeTextureName;            

            // generate texture for going backwards and forwards

            flipTexture =  new Texture2D(Game1.gDevice, texture.Width, texture.Height, false, SurfaceFormat.Color);

            Color[] nonFlippedPixels = new Color[texture.Width * texture.Height];
            texture.GetData(nonFlippedPixels);

            Color[] flippedPixels = new Color[texture.Width * texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    flippedPixels[ (texture.Width - x - 1) + (y * texture.Width)] = nonFlippedPixels[x + (y * texture.Width)];
            flipTexture.SetData(flippedPixels);
        }

        public void update()
        {
            this.color = Color.White;
        }

        public void updateAABB()
        {
           
            // calculate axis lengths of rotated texture
            float textureWidth = this.texture.Width;
            float textureHeight = this.texture.Height;

            // contruct points box around the unrotated texture
            // with its texture origin at (0,0)
            // we then rotate it around its texture origin and finds the new
            // aabb dimensions

            Vector2 tl = new Vector2(this.textureOrigin.X * -1, this.textureOrigin.Y * -1);
            Vector2 tr = new Vector2(texture.Width - this.textureOrigin.X, this.textureOrigin.Y * -1);
            Vector2 bl = new Vector2(this.textureOrigin.X * -1, texture.Height - textureOrigin.Y);
            Vector2 br = new Vector2(texture.Width - this.textureOrigin.X, texture.Height - textureOrigin.Y);

            tl = this.rotate(tl, this.orientationInWorld);
            tr = this.rotate(tr, this.orientationInWorld);
            bl = this.rotate(bl, this.orientationInWorld);
            br = this.rotate(br, this.orientationInWorld);

            // find the min and max x and y
            Vector2[] points = { tl, tr, bl, br };

            float minX =  tl.X;
            float minY = tl.Y;
            float maxX = tl.X;
            float maxY = tl.Y;

            foreach (Vector2 point in points)
            {
                if (point.X < minX)
                {
                    minX = point.X;
                }

                if (point.X > maxX)
                {
                    maxX = point.X;
                }

                if (point.Y < minY)
                {
                    minY = point.Y;
                }

                if (point.Y > maxY)
                {
                    maxY = point.Y;
                }
            }

            int newWidth = (int) (maxX - minX);
            int newHeight = (int) (maxY - minY);

            // we have the dimensions of the aabb, now we need to position it
            aabb.X = (int) (positionInWorld.X + minX);
            aabb.Y = (int) (positionInWorld.Y + minY);
            aabb.Width = newWidth;
            aabb.Height = newHeight;
        }

        public override string ToString()
        {
            String temp = "<";
            temp += name + ", ";
            temp += "(" + positionInParent.X + ", " + positionInParent.Y + "), ";
            temp += localOrientation + ", ";
            temp += nodeTextureName + ", ";

            if (this.parent == null)
            {
                temp += "parent =  NULL"; 
            }
            else
            {
                temp += "parent =  " + this.parent.name; 
            }

            temp += ">";


            return temp;
        }
    }
}
