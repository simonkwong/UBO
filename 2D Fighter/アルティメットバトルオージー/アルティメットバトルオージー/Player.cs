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
    class Player
    {

        public String name;
        public String iconPath;
        public Texture2D icon;

        public Dictionary<String, Node> currentBodyParts;

        public Dictionary<String, Node> bodyPartsLeft;
        public Dictionary<String, Node> bodyPartsRight;

        /* Stored for iterating through the players nodes */
        public World world;
        
        /* Used to indicate a flip in direction */
        public bool isFlipped;

        public float rotation
        {
            get
            {
                return this.currentRootNode.orientationInWorld;
            }
            set
            {
                this.currentRootNode.orientationInWorld = value;
            }
        }

        public Vector2 position
        {
            get
            {
                return this.currentRootNode.positionInParent;
            }
            set
            {
                this.currentRootNode.positionInParent = value;
            }
        }

        public Node currentRootNode;
        public Node leftRootNode;
        public Node rightRootNode;


        /* Animation stuff */
        public Animation currentAnimation;
        public int currentAnimFrame;
        public Animation oldAnimation;
        public int oldAnimFrame;
        int lerpFramesCompleted;
        bool lerpState;

        PrimitiveBatch pb;
        ParticleEngine particleEngine;
        List<Texture2D> textures = new List<Texture2D>();

        /* Stuff used setting the mode of player, the mode 
         * determines what the player's update does, using a switch
         * statement kind of thing.
         */
        public Modes mode;
        public enum Modes { PLAYEYCONTROLLED, DUMMY };

        /* The current players health */
        public int health;

        // if true the current animation will play through to completion without interruption.
        public bool aniNoIntMode;

        private KeyboardState prevKeyState;
        private KeyboardState currKeyState;

        /* used for physics */
        public Vector2 velocity;
        // AABB that minimaly covers the whole player
        public Rectangle bigAABB;
        bool isMidAir;
        public Player()
        {
            this.iconPath = "";
            this.name = "";
            mode = Modes.DUMMY;
            isFlipped = false;
            currentBodyParts = new Dictionary<String, Node>();
            aniNoIntMode = false;
            health = 100;
            bigAABB = new Rectangle(0, 0, 0, 0);
            isMidAir = false;
            lerpState = false;

            bodyPartsLeft = new Dictionary<string, Node>();
            bodyPartsRight = new Dictionary<string, Node>();

            currKeyState = Keyboard.GetState();
            prevKeyState = currKeyState;
            
            // possibly add bool to specify flip left or right
            this.bodyPartsLeft = orderDrawingParts(this.bodyPartsLeft, "right");
            this.bodyPartsRight = orderDrawingParts(this.bodyPartsRight, "left");
            
            pb = new PrimitiveBatch(Game1.gDevice);
            textures.Add(Game1.contentM.Load<Texture2D>("Art/Particles/blood"));
            particleEngine = new ParticleEngine(textures);

        }

        public void LoadContent(ContentManager content)
        {
            foreach (Node node in bodyPartsLeft.Values)
            {
                node.LoadContent(content);
            }
            foreach (Node node in bodyPartsRight.Values)
            {
                node.LoadContent(content);
            }

            icon = content.Load<Texture2D>(iconPath);
        }

        private static Dictionary<String, Node> orderDrawingParts(Dictionary<String, Node> bodyParts, String side)
        {
            Dictionary<String, Node> leftNodes = new Dictionary<String, Node>();
            Dictionary<String, Node> otherNodes = new Dictionary<String, Node>();

            foreach (Node node in bodyParts.Values)
            {
                if (node.name.Contains(side))
                    leftNodes.Add(node.name, node);
                else
                    otherNodes.Add(node.name, node);
            }

            foreach (Node node in otherNodes.Values)
            {
                leftNodes.Add(node.name, node);
            }

            return leftNodes;
        }

        public void updateBigAABB()
        {

            Rectangle tempAABB = this.currentBodyParts.Values.ToList()[0].aabb;
            int minX = tempAABB.Left;
            int minY = tempAABB.Top;
            int maxX = tempAABB.Right;
            int maxY = tempAABB.Bottom;
            foreach (Node n in this.currentBodyParts.Values)
            {
                if(n.aabb.Left < minX)
                {
                    minX = n.aabb.Left;
                }

                if (n.aabb.Top < minY)
                {
                    minY = n.aabb.Top;
                }

                if (n.aabb.Right > maxX)
                {
                    maxX = n.aabb.Right;
                }

                if (n.aabb.Bottom > maxY)
                {
                    maxY = n.aabb.Bottom;
                }
            }

            this.bigAABB.X = minX;
            this.bigAABB.Y = minY;
            this.bigAABB.Width = maxX - minX;
            this.bigAABB.Height = maxY - minY;
        }

        public void updateModePlayerControlled(GameTime time)
        {
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            currKeyState = Keyboard.GetState();

            if(aniNoIntMode)   // animation must play to completion without interruption
            {
                updateAnimation(WorldData.GetInstance().animations[this.currentAnimation.name]);
            }
            else if (pressedKeys.Length == 0)
            {
                // no keys are pressed right now
                updateAnimation(WorldData.GetInstance().animations["idle"]);
            }
            else
            {
                updatePressedKeys();
            }

            prevKeyState = currKeyState;
        }

        public void Update(GameTime time)
        {
            updateBigAABB();

            particleEngine.UpdateMeta();

            foreach (Node n in this.currentBodyParts.Values)
            {
                // resets the node color to white
                n.update();
            }

            if (this.health <= 0)
            {
                health = 0;
                die();
            }
            else
            {
                if (this.mode == Player.Modes.DUMMY)
                {
                    updateAnimation(WorldData.GetInstance().animations["idle"]);
                }
                else if (this.mode == Player.Modes.PLAYEYCONTROLLED)
                {
                    updateModePlayerControlled(time);
                    if (isFlipped)
                    {
                        this.currentBodyParts = bodyPartsLeft;
                        this.currentRootNode = leftRootNode;
                        this.rightRootNode.positionInWorld = this.position;
                    }
                    else
                    {
                        this.currentBodyParts = bodyPartsRight;
                        this.currentRootNode = rightRootNode;
                        this.leftRootNode.positionInWorld = this.position;
                    }
                }

                // check for collision with other player            
                checkCollisions();

                updatePhysics();
            }
        }

        public void checkCollisions()
        {
            // first update all of the nodes in the player aabb
            foreach (Node n in this.currentBodyParts.Values)
            {
                n.updateAABB();
            }

            checkPlayerOnPlayerCollision();
        }

        public void updatePhysics()
        {
            isMidAir = true;
            // check if it is colliding with anything in the world            
            foreach (Block b in this.world.arena.blocks)
            {
                foreach (MetaParticle p in particleEngine.metaParticles)
                {
                    if (p.aabb().Intersects(b.AABB) && p.lifeTime > 0)
                    {
                        world.blood.Splash(p.position.X, p.position.Y);
                        p.lifeTime = 0;
                    }
                }

                if (b.collidable)
                {
                    // colliding left
                    if (this.bigAABB.Left <= b.AABB.Right + 35
                        && this.bigAABB.Right >= b.AABB.Right + 35
                        && this.bigAABB.Top <= b.AABB.Bottom
                        && this.bigAABB.Bottom >= (b.AABB.Top + 20)
                        )
                    {
                        if (velocity.X < 0)
                        {
                            velocity = new Vector2(0, velocity.Y);
                        }
                    }
                    // colliding right
                    if (this.bigAABB.Right >= b.AABB.Left - 35
                        && this.bigAABB.Left <= b.AABB.Left - 35
                        && this.bigAABB.Top <= b.AABB.Bottom
                        && this.bigAABB.Bottom >= (b.AABB.Top + 20)
                        )
                    {
                        if (velocity.X > 0)
                        {
                            velocity = new Vector2(0, velocity.Y);
                        }
                    }

                    // standing on platform
                    if (this.bigAABB.Bottom >= b.AABB.Top
                        && this.bigAABB.Right > (b.AABB.Left + 25)
                        && this.bigAABB.Left < (b.AABB.Right - 25)
                        && this.bigAABB.Bottom < b.AABB.Bottom
                        )
                    {
                        isMidAir = false;

                        // only apply friction if not walking
                        if (!Keyboard.GetState().IsKeyDown(Keys.Left) && !Keyboard.GetState().IsKeyDown(Keys.Right))
                        {
                            // friction
                            if (velocity.X > 0)
                            {
                                velocity.X -= 1;
                            }
                            else if (velocity.X < 0)
                            {
                                velocity.X += 1;
                            }
                        }
                        
                        if (velocity.Y > 0)
                        {
                            velocity.Y = 0;
                        }

                        // move to top if player is partially in platform
                        if ((this.bigAABB.Bottom - b.AABB.Top) > 20)
                        {
                            this.position = new Vector2(position.X, position.Y - 5);
                            this.velocity = new Vector2(0, 0);
                        }
                    }
                }               
            }

            if(isMidAir)
            {
                velocity.Y += 0.5f;
            }
            
            this.position += velocity;
        }

        public void checkPlayerOnPlayerCollision()
        {
            Rectangle arenasize = world.arena.arenasize;

            float playerWidth = 0.0f;

            // check player on player collisions
            foreach (Player oPlayer in world.players)
            {
                playerWidth = oPlayer.bigAABB.Right - oPlayer.bigAABB.Left;

                if (oPlayer == this)
                {
                    // dont check itself
                    continue;
                }

                foreach (Node mN in this.currentBodyParts.Values)
                {
                    foreach (Node oN in oPlayer.currentBodyParts.Values)
                    {
                        if (mN.aabb.Intersects(oN.aabb))
                        {
                            if (oN.state == "attack" && mN.state == "passive")
                            {
                                mN.color = Color.Red;
                                if (!mN.playersThatHitMe.Contains(oPlayer))
                                {
                                    mN.playersThatHitMe.Add(oPlayer);
                                    particleEngine.GenerateNewParticle(mN.positionInWorld, 5);
                                    this.health--;

                                    if (this.health <= 0)
                                    {
                                        particleEngine.GenerateNewParticle(mN.positionInWorld, 100);
                                        world.blood.Splash(position.X, position.Y);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void updatePressedKeys()
        {
            // if this method is called at least one key is 
            // being pressed

            /* The following are key bindings for the player */
            if (currKeyState.IsKeyDown(Keys.Right))
            {

                if (velocity.X <= 8f)
                {
                    velocity.X = 8f;
                }

                // set animation walking right
                updateAnimation(WorldData.GetInstance().animations["walkingright"]);
                this.isFlipped = false;
            }

            if (currKeyState.IsKeyDown(Keys.Left))
            {
                if (velocity.X >= -8f)
                {
                    velocity.X = -8f;
                }

                velocity.X = -8f;
                updateAnimation(WorldData.GetInstance().animations["walkingright"]);
                this.isFlipped = true;
            }

            if (currKeyState.IsKeyDown(Keys.Up) && !prevKeyState.IsKeyDown(Keys.Up) && !isMidAir)
            {
                isMidAir = true;
                velocity.Y = -10f;
                updateAnimation(WorldData.GetInstance().animations["idle"]);
            }


            if (currKeyState.IsKeyDown(Keys.A))
            {
                updateAnimation(WorldData.GetInstance().animations["squat"]);
            }


            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                kick();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                punch();
            }

            prevKeyState = currKeyState;

        }

        public void die()
        {
            updateAnimation(WorldData.GetInstance().animations["dead"], 20);
        }

        public void punch()
        {
            // play the kick until animation has finished
            aniNoIntMode = true;
            updateAnimation(WorldData.GetInstance().animations["punch"]);
        }

        public void kick()
        {
            // play the kick until animation has finished
            aniNoIntMode = true;
            updateAnimation(WorldData.GetInstance().animations["kicka"]);
        }

        public static Player deepClonePlayer(Player p)
        {
            Player clonedPlayer = new Player();
            clonedPlayer.name = String.Copy(p.name);
            clonedPlayer.icon = p.icon;
            clonedPlayer.iconPath = p.iconPath;

            clonedPlayer.bodyPartsLeft = deepCloneBodyParts(p.bodyPartsLeft);
            clonedPlayer.bodyPartsRight = deepCloneBodyParts(p.bodyPartsRight);

            clonedPlayer.leftRootNode = clonedPlayer.bodyPartsLeft[p.leftRootNode.name];
            clonedPlayer.rightRootNode = clonedPlayer.bodyPartsRight[p.rightRootNode.name];

            clonedPlayer.isFlipped = p.isFlipped;
            clonedPlayer.currentBodyParts = clonedPlayer.bodyPartsRight;
            clonedPlayer.currentRootNode = clonedPlayer.rightRootNode;

            return clonedPlayer;
        }

        public static Dictionary<String, Node> deepCloneBodyParts(Dictionary<String, Node> toClonebodyParts)
        {
            /* Assumes nodes always come after parent in array! */
            Dictionary<String, Node> bodyPartsCopy = new Dictionary<string, Node>();

            foreach (String key in toClonebodyParts.Keys)
            {
                /* Construct the new node! */
                String newName = String.Copy(toClonebodyParts[key].name);
                Vector2 newPositionInParent = new Vector2(toClonebodyParts[key].positionInParent.X, toClonebodyParts[key].positionInParent.Y);
                float newLocalOrientation = toClonebodyParts[key].localOrientation;
                Boolean bnewIsLeaf = toClonebodyParts[key].isLeaf;
                String snewIsLeaf;
                if (bnewIsLeaf)
                {
                    snewIsLeaf = "T";
                }
                else
                {
                    snewIsLeaf = "F";
                }

                String newTextureName = toClonebodyParts[key].nodeTextureName;
                Vector2 newTextureOrigin = new Vector2(toClonebodyParts[key].textureOrigin.X, toClonebodyParts[key].textureOrigin.Y);
                float newZoom = toClonebodyParts[key].zoom;
                String newParentName;
                if (toClonebodyParts[key].parent == null)
                {
                    newParentName = "";
                }
                else
                {
                    newParentName = String.Copy(toClonebodyParts[key].parent.name);
                }

                Node newParent;
                // search through bodyPartsCopy array for parent that should have already been added
                if (bodyPartsCopy.ContainsKey(newParentName))
                {
                    newParent = bodyPartsCopy[newParentName];
                }
                else
                {
                    newParent = null;
                }

                Node copyNode = new Node(newName, newPositionInParent, newLocalOrientation, newParent, snewIsLeaf, newTextureOrigin, newTextureName);
                copyNode.LoadContent(Game1.contentM);
                bodyPartsCopy.Add(String.Copy(key), copyNode);
            }

            return bodyPartsCopy;
        }

        public void updateBody(KeyFrame keyFrame)
        {
            foreach (NodeAnimInfo nodeAnimInfo in keyFrame.nodesAnimInfo.Values)
            {
                if (this.isFlipped)
                {
                    this.currentBodyParts[nodeAnimInfo.name].localOrientation = nodeAnimInfo.localOrientation * -1;
                }
                else
                {
                    this.currentBodyParts[nodeAnimInfo.name].localOrientation = nodeAnimInfo.localOrientation;
                }
                this.currentBodyParts[nodeAnimInfo.name].state = nodeAnimInfo.state;
                // for now do not update the texture!!!!
                // Need to come up with a clever way to use it on different players
            }
        }

        public void drawPlayer(SpriteBatch sb)
        {
            // uncomment to draw node aabb's
            //foreach (Node pp in this.bodyparts.Values)
            //{
            //    sb.Draw(Game1.contentM.Load<Texture2D>("Art/yellowpixel"), pp.aabb , pp.color);
            //}

            // uncomment to draw players bigAABB
            // sb.Draw(Game1.contentM.Load<Texture2D>("Art/yellowpixel"), this.bigAABB , Color.Green);

            foreach (Node pp in this.currentBodyParts.Values)
            {
                Texture2D nodeTexture = pp.texture;

                sb.Draw(nodeTexture, pp.positionInWorld, null, pp.color, pp.orientationInWorld, pp.textureOrigin, 1, SpriteEffects.None, 0);
            }

            particleEngine.DrawSingle(sb);

        }

        /* Animation Functions */

        /* Animation data only applied to upper body*/
        public void setUpperBodyAnimation(Animation ani)
        {

        }

        public void updateAnimation(Animation ani)
        {
            updateAnimation(ani, -1);
        }


        public void updateAnimation(Animation ani, int restartAtFrame, int lerpNumFrames = 10)
        {
            if (this.currentAnimation == null)
            {
                // this should only happen the very first call to 
                // update animation
                this.currentAnimation = ani;
                lerpState = false;
            }

            if (this.currentAnimation != ani)
            {
                // this occurs when we update to a new animation
                // this setups the variables needed for the animation lerp
                // to occur

                // we keep track of how far along we are in a variable called
                // lerpFramesCompleted
                // and signal that we are in a lerpState by setting
                // lerpState to true

                lerpState = true;
                lerpFramesCompleted = 1;

                // We are lerping between the old and the current
                // animation (the one we changed to)

                // the old ani frame is frozen at whatever
                // frame it was at when the change of animation occured
                this.oldAnimation = this.currentAnimation;
                this.oldAnimFrame = this.currentAnimFrame;

                // this is will be updated as usual
                this.currentAnimation = ani;
                this.currentAnimFrame = 0;
            }

            if (this.currentAnimFrame++ >= this.currentAnimation.keyFrames1.Keys.ElementAt(this.currentAnimation.keyFrames1.Keys.Count - 1))
            {
                if (aniNoIntMode)
                {
                    // signals that an animation has played to completion
                    aniNoIntMode = false;

                    // remove myself from nodes list of players who attacked me.
                    // now that Ive finished my animation
                    foreach (Player oPlayer in world.players)
                    {
                        if (oPlayer == this)
                        {
                            // ignore myself
                            continue;
                        }

                        foreach (Node n in oPlayer.currentBodyParts.Values)
                        {
                            if (n.playersThatHitMe.Contains(this))
                            {
                                n.playersThatHitMe.Remove(this);
                            }
                        }
                    }
                }

                if (restartAtFrame == -1)
                {
                    this.currentAnimFrame = 0;
                }
                else
                {
                    this.currentAnimFrame = restartAtFrame;
                }
            }

            if (lerpState)
            {
                float lerpAmount = ((float)lerpFramesCompleted / lerpNumFrames);

                Animation.lerpAndSwitchFrame(this.oldAnimation, this.oldAnimFrame, this.currentAnimation, this.currentAnimFrame, lerpAmount, this);

                if (lerpFramesCompleted++ >= lerpNumFrames)
                {
                    // check to see if lerping is completed
                    lerpState = false;
                }
            }
            else
            {
                Animation.lerpAndSwitchFrame(this.currentAnimation, this.currentAnimFrame, this);
            }
        }

        public void lineDraw(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(Game1.contentM.Load<Texture2D>("Art/yellowpixel"), r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
