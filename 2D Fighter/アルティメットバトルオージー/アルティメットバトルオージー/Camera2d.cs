using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    class Camera2d
    {
        protected float zoom; 
        public Matrix transform 
        {
            get { return  Matrix.CreateTranslation(new Vector3(-pos.X, -pos.Y, 0)) *
                                        Matrix.CreateRotationZ(Rotation) *
                                        Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                                        Matrix.CreateTranslation(new Vector3(viewPortbounds.Width * 0.5f, viewPortbounds.Height * 0.5f, 0));}
        }
        public Vector2 pos; 
        protected float rotation;
        public enum Modes { NOTRANSFORM, PLAY }
        public Modes mode;

        private Rectangle viewPortbounds;

        public Camera2d(Viewport viewport)
        {
            viewPortbounds = viewport.Bounds;
            zoom = 1.0f;
            rotation = 0.0f;
            pos = new Vector2(0, 0);
            mode = Modes.NOTRANSFORM;
        }

        // Sets and gets zoom
        public float Zoom
        {
            get { return zoom; }
            set { zoom = value; if (zoom < 0.00000001f) zoom = 0.1f; } // Negative zoom will flip image
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        // Get set position
        public Vector2 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public void updatePosition(Vector2 newPos)
        {
            pos = newPos;           
        }


        public Rectangle VisibleArea
        {
            get
            {
                var inverseViewMatrix = Matrix.Invert(transform);
                var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
                var tr = Vector2.Transform(new Vector2(viewPortbounds.Width, 0), inverseViewMatrix);
                var bl = Vector2.Transform(new Vector2(0, viewPortbounds.Height), inverseViewMatrix);
                var br = Vector2.Transform(new Vector2(viewPortbounds.Width, viewPortbounds.Height), inverseViewMatrix);
                var min = new Vector2(
                    MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                    MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
                var max = new Vector2(
                    MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                    MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
                return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
            }
        }


        public void update(World mWorld)
        {
            calcCameraPosZoom(mWorld);
        }


        public void calcCameraPosZoom(World mWorld)
        {   
            Rectangle bounds = mWorld.playerBounds;

            // place the camera at the center point between all players
            updatePosition(new Vector2(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2)));
        
            // zoom the camera so all players are visible

            float zoomW = ((float) viewPortbounds.Width) / (bounds.Width + 300);
            float zoomH = ((float) viewPortbounds.Height) / (bounds.Height + 300);

            if(zoomW < zoomH)
            {
                this.zoom = zoomW;
            }
            else
            {
                this.zoom = zoomH;
            }

            if (this.zoom > 1)
            {
                this.zoom = 1;
            }

            if (this.zoom < 0.7)
            {
                this.zoom = 0.7f;
            }



            Rectangle arenaSize = mWorld.arena.arenasize;

            // the worst algorithm ever
            // uses brute force to keep camera inside

            int moves = 0;
            float oldValue = this.pos.X;
            while (VisibleArea.Right > arenaSize.Right)
            {
                this.pos.X--;
                moves++;
                if (moves > arenaSize.Width)
                {
                    this.pos.X = oldValue;
                    break;
                }
            }
            
            moves = 0;
            oldValue = this.pos.X;
            while (VisibleArea.Left < arenaSize.Left)
            {
                this.pos.X++;
                moves++;
                if (moves > arenaSize.Width)
                {
                    this.pos.X = oldValue;
                    break;
                }
            }

            moves = 0;
            oldValue = this.pos.Y;   
            while (VisibleArea.Bottom > arenaSize.Bottom)
            {
                this.pos.Y--;
                moves++;
                if (moves > arenaSize.Height)
                {
                    this.pos.Y = oldValue;
                    break;
                }
            }
        }


        public Matrix getTransformation(GraphicsDevice graphicsDevice)
        {
            if (mode == Modes.NOTRANSFORM)
            {
                return Matrix.Identity;
            }
            else if (mode == Modes.PLAY)
            {
                return transform;
            }
            else
            {
                return Matrix.Identity;
            }
        }
    }
}
