using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace アルティメットバトルオージー
{
    class Poser
    {
        Game1 game;
        SpriteFont menuFont;
        SpriteFont poserFont;
        SpriteBin Sprites;
        MenuManager mMenus;
        String pipelineMode;
        String[] possibleKeyBoardMappings;
        Dictionary<String, String> playerKeyMap;
        String lastSelectedNodeName;
        int currentFrameNumber;
        bool oneKeyPressLock = true;
        Keys lastKeyPressed = Keys.Tab;
        bool playingAnimation;
        Animation currentAnimation;
        Player currentPlayer;
        enum PoseType
        {
            Position, Rotation
        }
        PoseType poseType;

        /* What is currently displayed in the poser */
        Dictionary<String, Node> currentFrameBodyParts;

        public Poser(Game1 game)
        {
            poseType = PoseType.Rotation;
            currentFrameBodyParts = new Dictionary<string, Node>();
            playingAnimation = false;            
            this.game = game;
            currentFrameNumber = 0;
            pipelineMode = "menu";
            possibleKeyBoardMappings = new String[] {"d0", "d1", "d2", "d3", "d4", "d5", 
                "d6", "d7", "d8", "d9", "q", "w" , "e", "r", "t", "y", "u", "i", "o", "p"};
            LoadContent(game.Content);

            currentAnimation = new Animation();
        }

        public void LoadContent(ContentManager content)
        {
            menuFont = content.Load<SpriteFont>("Fonts/PoserFont");
            poserFont = content.Load<SpriteFont>("Fonts/PoserFont");
            Sprites = new SpriteBin(menuFont);
            CreateMenus(menuFont);
        }

        /* Maps bodyPart Names to keys*/
        public Dictionary<String, String> generatePlayerKeyMappings(Dictionary<String, Node> bodyParts,String[] possibleKeyMappings)
        {
            Dictionary<String, String> bodyPartKeyMap = new Dictionary<String, String>();
            // maps bodyparts to keys and outputs mapping

            int i = 0;
            foreach (Node node in bodyParts.Values)
            {
                bodyPartKeyMap.Add(possibleKeyBoardMappings[i], node.name);
                i++;
            }

            return bodyPartKeyMap;
        }

        protected void CreateMenus(SpriteFont menuFont)
        {
            Menu mainPoserMenu = new Menu(menuFont, "Poser 420", true);
            Menu selectPlayerMenu = new Menu(menuFont, "Poser 420 Character Select", false);
            Menu animationSelectMenu = new Menu(menuFont, "Poser 420 Select Animation", false);

            mainPoserMenu.AddMenuItem("Select Character", () => { pipelineMode = "menu"; mainPoserMenu.Enabled = false; selectPlayerMenu.Enabled = true; });
            mainPoserMenu.AddMenuItem("Open Poser", () => { pipelineMode = "playposer"; mainPoserMenu.Enabled = false; selectPlayerMenu.Enabled = false; });
            mainPoserMenu.AddMenuItem("Load Existing Animation", () => { pipelineMode = "menu"; mainPoserMenu.Enabled = false; animationSelectMenu.Enabled = true; selectPlayerMenu.Enabled = false; });

            int i = 1;
            foreach (String playerName in WorldData.GetInstance().players.Keys)
            {
                selectPlayerMenu.AddMenuItem(playerName, () =>
                {
                    pipelineMode = "menu";
                    mainPoserMenu.Enabled = true; 
                    selectPlayerMenu.Enabled = false;
                    currentPlayer = WorldData.GetInstance().players[playerName];
                    this.playerKeyMap = generatePlayerKeyMappings(WorldData.GetInstance().players[playerName].currentBodyParts, this.possibleKeyBoardMappings);
                    lastSelectedNodeName = WorldData.GetInstance().players[playerName].currentBodyParts.Values.ElementAt(0).name;
                    WorldData.GetInstance().players[playerName].LoadContent(Game1.contentM);
                });
                i++;
            }

            foreach (String aniName in WorldData.GetInstance().animations.Keys)
            {
                animationSelectMenu.AddMenuItem(aniName, () => { 
                    pipelineMode = "playposer";
                    mainPoserMenu.Enabled = false;
                    selectPlayerMenu.Enabled = false;
                    this.currentAnimation = WorldData.GetInstance().animations[aniName]; 
                });
            }

            mMenus = new MenuManager();
            mMenus.AddMenu(selectPlayerMenu);
            mMenus.AddMenu(animationSelectMenu);
            mMenus.AddMenu(mainPoserMenu);

        }

        public void Update(GameTime gameTime)
        {
            switch (pipelineMode)
            {
                case "playposer":
                    updateEditBody();
                    updateEditKeys(gameTime);
                    break;
                case "menu":
                    mMenus.Update(gameTime);
                    break;
                case "renameAnimation":
                    updateRenameAnimation();
                    break;
            }
        }

        public void updateEditKeys(GameTime gameTime)
        { 
            /* Prevents multiple input from held down keys */
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            if (pressedKeys.Length > 0 && oneKeyPressLock)
            {
                oneKeyPressLock = false;
                lastKeyPressed = pressedKeys[0];

                if ( (lastKeyPressed == Keys.K))
                {
                    pipelineMode = "renameAnimation";
                }

                if ((lastKeyPressed == Keys.OemCloseBrackets) && !playingAnimation)
                {
                    this.currentFrameNumber++;
                    Animation.lerpAndSwitchFrame(this.currentAnimation, this.currentFrameNumber, this.currentPlayer);
                }

                if ((lastKeyPressed == Keys.OemOpenBrackets) && !playingAnimation)
                {
                    if (this.currentFrameNumber > 0)
                    {
                        this.currentFrameNumber--;
                        Animation.lerpAndSwitchFrame(this.currentAnimation, this.currentFrameNumber, this.currentPlayer);
                    }
                }

                /* Add a keyframe to the animation */
                if ((lastKeyPressed == Keys.Enter) && !playingAnimation)
                {
                    currentAnimation.insertKeyFrame(this.currentFrameNumber, new KeyFrame(currentPlayer.currentBodyParts));
                }

                /* Delete the current keyFrame */
                if ((lastKeyPressed == Keys.D) && !playingAnimation)
                {
                    currentAnimation.deleteKeyFrame(this.currentFrameNumber);
                }

                /* Toggle play the animation */
                if ((lastKeyPressed == Keys.Space) && (this.currentAnimation.keyFrames1.Keys.Count != 0))
                {
                    if (playingAnimation)
                    {
                        playingAnimation = false;
                    }
                    else
                    {
                        playingAnimation = true;
                    }
                }

                /* Write the animatin to xml */
                if ((lastKeyPressed == Keys.Insert))
                {
                    currentAnimation.exportToXML();
                    Console.WriteLine("WROTE TO XML!");
                }

                /* Toggle edit mode */
                if ((lastKeyPressed == Keys.Tab) && poseType == PoseType.Position)
                {
                    poseType = PoseType.Rotation;
                } else if ((lastKeyPressed == Keys.Tab) && poseType == PoseType.Rotation)
                {
                    poseType = PoseType.Position;
                }
            }

            if (Keyboard.GetState().IsKeyUp(lastKeyPressed))
            {
                oneKeyPressLock = true;
            }

            if (playingAnimation)
            {
                int timeStep = 1;  // frame update every timeStep milliseconds

                if (gameTime.TotalGameTime.Milliseconds % timeStep == 0)
                {
                    if (this.currentFrameNumber++ >= this.currentAnimation.keyFrames1.Keys.ElementAt(this.currentAnimation.keyFrames1.Keys.Count - 1))
                    {
                        this.currentFrameNumber = 0;
                    }       
                    Animation.lerpAndSwitchFrame (this.currentAnimation, this.currentFrameNumber, this.currentPlayer);
                }                
            }
        }


        public void drawNodeNumbers(SpriteBatch sb)
        {

            // draw the number associated with every node over the node

            // playerKeyMap mapping is as follows
            // <keyboard key, node name>
            foreach (String controllerKey in this.playerKeyMap.Keys)
            {
                // hacky fix to make keys that start with the d look nice
                // from this point forward keyOutputToScreen is the name of
                // the associated keyboard with a node, NOTE this is not the
                // actual key, it is cleaned up by this if statement
                // for printing to the screen.
                String keyOutputToScreen = controllerKey;

                if (controllerKey.Length > 1)
                {
                    keyOutputToScreen = controllerKey.Substring(1);
                }

                // find associated node
                Node associatedNode = this.currentPlayer.currentBodyParts[this.playerKeyMap[controllerKey]];

                if (associatedNode.name == this.lastSelectedNodeName)
                {
                    sb.DrawString(poserFont, keyOutputToScreen, new Vector2(associatedNode.positionInWorld.X, associatedNode.positionInWorld.Y), Color.Red);
                }
                else
                {
                    sb.DrawString(poserFont, keyOutputToScreen, new Vector2(associatedNode.positionInWorld.X, associatedNode.positionInWorld.Y), Color.Yellow);
                }

            }

        }

        public void drawPoser(SpriteBatch sb)
        {
            sb.DrawString(menuFont, "Currently Selected Node: " + this.lastSelectedNodeName, new Vector2(800, 50), Color.Red);
            sb.DrawString(menuFont, "Current Frame Number: " + currentFrameNumber, new Vector2(20, 50), Color.Red);
            sb.DrawString(menuFont, "<Use [ and ] keys to choose frame>", new Vector2(20, 80), Color.Red);

            sb.DrawString(menuFont, "Pose Type Being Edited: " + poseType.ToString() + " (TAB to switch)", new Vector2(20, 170), Color.Red);

            if (poseType == PoseType.Rotation)
            {
                sb.DrawString(menuFont, "Use Left and Right to adjust rotaton.", new Vector2(20, 200), Color.Red);
            }
            if (poseType == PoseType.Position)
            {
                sb.DrawString(menuFont, "Use arrow keys to adjust position.", new Vector2(20, 200), Color.Red);
            }

            sb.DrawString(menuFont, "Press k to edit animation name", new Vector2(20, 230), Color.Red);
            sb.DrawString(menuFont, "Press Insert To Export", new Vector2(20, 260), Color.Red);


            if (!this.currentAnimation.keyFrames1.ContainsKey(this.currentFrameNumber))
            {
                sb.DrawString(menuFont, "<Press ENTER to save frame>", new Vector2(20, 110), Color.Red);
            }
            else
            {
                sb.DrawString(menuFont, "<Press D to delete frame>", new Vector2(20, 110), Color.Red);
            }


            if (playingAnimation)
            {
                sb.DrawString(menuFont, "<Press SPACE to play animation>", new Vector2(20, 140), Color.Red);
            }
            else
            {
                sb.DrawString(menuFont, "<Press SPACE to pause animation>", new Vector2(20, 140), Color.Red);
            }

            int i = 0;
            int xPos = 0;
            int yPos = 640;

            foreach (int frameNum in this.currentAnimation.keyFrames1.Keys)
            {
                if (xPos > this.game.graphics.PreferredBackBufferWidth - 100)
                {
                    xPos = 0;
                    yPos += 30;
                }

                if (this.currentAnimation.keyFrames1.Keys.ElementAt(i) == this.currentFrameNumber)
                {
                    sb.DrawString(this.poserFont, "<" + frameNum + ">", new Vector2(xPos, yPos), Color.Green);
                }
                else
                {
                    sb.DrawString(menuFont, "<" + frameNum + ">", new Vector2(xPos, yPos), Color.Red);
                }
                
                xPos += 70;
                i++;
            }
        }


        public void updateEditBody()
        {
            // find out if any of the mapped bodypart keys were pressed
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();

            // see if any of the keys that were pressed are mapped to a bodypart in the currentPlayer
            foreach (Keys key in pressedKeys)
            {
                if (this.playerKeyMap.ContainsKey(key.ToString().ToLower()))
                {
                    lastSelectedNodeName = this.currentPlayer.currentBodyParts[this.playerKeyMap[key.ToString().ToLower()]].name;
                    break;
                }
            }

            if (poseType == PoseType.Rotation)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    this.currentPlayer.currentBodyParts[lastSelectedNodeName].localOrientation -= (float)(1.0f * Math.PI / 180);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    this.currentPlayer.currentBodyParts[lastSelectedNodeName].localOrientation += (float)(1.0f * Math.PI / 180);
                }
            }

            if (poseType == PoseType.Position && lastSelectedNodeName.Equals("torso"))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    this.currentPlayer.currentBodyParts[lastSelectedNodeName].positionInParent += new Vector2(-2, 0);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    this.currentPlayer.currentBodyParts[lastSelectedNodeName].positionInParent += new Vector2(2, 0);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    this.currentPlayer.currentBodyParts[lastSelectedNodeName].positionInParent += new Vector2(0, -2);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    this.currentPlayer.currentBodyParts[lastSelectedNodeName].positionInParent += new Vector2(0, 2);
                }
            }
        }


        public void drawRenameAnimation(SpriteBatch sb)
        {
            sb.DrawString(menuFont, "Type the new animation name. Press ENTER to save.", new Vector2(200, 200), Color.Red);
            sb.DrawString(menuFont, ">", new Vector2(200, 230), Color.Red);

            // draw the name
            sb.DrawString(menuFont, this.currentAnimation.name, new Vector2(220, 230), Color.LimeGreen);
        }

        String validCharacters = "abcdefghijklmnopqrstuvwxyz";
        public void updateRenameAnimation()
        {
            /* GET CHARACTERS FOR NAME */
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();

            String newLetter;
            if (pressedKeys.Length > 0 && oneKeyPressLock)
            {
                newLetter = pressedKeys[0].ToString();

                oneKeyPressLock = false;
                lastKeyPressed = pressedKeys[0];


                if (validCharacters.Contains(newLetter.ToLower()))
                {
                    this.currentAnimation.name += newLetter.ToLower();
                }
                else if (lastKeyPressed == Keys.Back)
                {
                    if (this.currentAnimation.name.Length > 0)
                    {
                        this.currentAnimation.name = this.currentAnimation.name.Substring(0, this.currentAnimation.name.Length - 1);
                    }
                }
                else if (lastKeyPressed == Keys.Space)
                {
                    this.currentAnimation.name += " "; 
                }
                else if (lastKeyPressed == Keys.OemMinus)
                {
                    this.currentAnimation.name += "-";
                }
                else if (lastKeyPressed == Keys.Enter)
                {
                    /* Save animation name */
                    pipelineMode = "playposer";
                }
            }

            if (Keyboard.GetState().IsKeyUp(lastKeyPressed))
            {
                oneKeyPressLock = true;
            }
        }


        public void draw(SpriteBatch sb)
        {
            switch (pipelineMode)
            {
                case "playposer":
                    drawPoser(sb);
                    this.currentPlayer.drawPlayer(sb);
                    drawNodeNumbers(sb);
                    break;
                case "menu":
                    mMenus.Draw(sb);
                    break;
                case "renameAnimation":
                    drawRenameAnimation(sb);
                    break;
            }
        }
    }
}
