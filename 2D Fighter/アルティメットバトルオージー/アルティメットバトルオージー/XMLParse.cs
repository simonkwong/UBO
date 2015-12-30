using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace アルティメットバトルオージー
{
    class XMLParse
    {
        /// <summary>
        /// Returns the value of an attribute of an XElement, or "" if no such
        /// attribute exists
        /// </summary>
        /// <param name="elem">Element to get attribute from</param>
        /// <param name="AttributeName">Name of the attribute to extract</param>
        /// <returns>Value of the attribute, or "" if it doesn't exist</returns>
        public static string GetAttributeValue(XElement elem, String AttributeName)
        {
            foreach (XAttribute attr in elem.Attributes())
            {
                if (attr.Name.ToString() == AttributeName)
                {
                    return attr.Value;
                }
            }
            return "";
        }

        /// <summary>
        /// Adds a property value to an object, based on an XML node.  Use the
        /// 'type' attribute of the XML node to determine what type the element
        /// we are adding is -- float, Vector2, string, etc.
        /// </summary>
        /// <param name="elem">XML element to read name / value of property from</param>
        /// <param name="obj">Object to set proprerty on</param>
        public static void AddValueToClassInstance(XElement elem, Object obj)
        {
            string type = GetAttributeValue(elem, "type").ToLower();
            if (type == "playerlist")
            {
                addPlayers(elem, obj);
            }
            else if (type == "animationlist")
            {
                addAnimations(elem, obj);
            }
            else if (type == "animation")
            {

            }

            /*
            else
            {
                throw new FormatException("Unknown Type attribute " + type + " in XML file");
            }
            */
        }

        private static List<String> ParseStringList(XElement elem)
        {
            List<String> stringList = new List<String>();
            foreach (XElement pathPoint in elem.Elements())
            {
                String nextElem = pathPoint.Value;

                stringList.Add(nextElem);
            }
            return stringList;
        }

        private static Vector2 ParseVector(XElement elem)
        {
            Vector2 valueToSet = new Vector2(float.Parse(elem.Element("x").Value),
                                             float.Parse(elem.Element("y").Value));
            return valueToSet;
        }

        private static Node getDaddy(String daddy, Dictionary<String, Node> playerNodes)
        {
            if (playerNodes.ContainsKey(daddy))
            {
                return playerNodes[daddy];
            }
            else
            {
                return null;
            }
        }


        /* Loads a player from xml file*/
        public static void loadPlayer(XElement root, Dictionary<String, Player> players)
        {
            Player temp = new Player();
            String name = XMLParse.GetAttributeValue(root, "name");
            String icon = root.Element("icon").Value;
            temp.name = name;
            temp.iconPath = icon;

            Dictionary<String, Node> playerNodesLeft = new Dictionary<String, Node>();
            Dictionary<String, Node> playerNodesRight = new Dictionary<String, Node>();

            bool firstIter = true;
            // parse left
            foreach (XElement node in root.Element("left").Elements())
            {
                Node daddyD = getDaddy(node.Element("parent").Value, playerNodesLeft);

                Node dick = new Node(node.Element("name").Value,
                                     ParseVector(node.Element("pspace_pos")),
                                     float.Parse(node.Element("orientation").Value),
                                     daddyD,
                                     node.Element("isLeaf").Value,
                                     ParseVector(node.Element("textureOrigin")),
                                     node.Element("texture").Value);
                playerNodesLeft.Add(dick.name, dick);

                // save the root, which is always the first node in the xml file
                if (firstIter)
                {
                    firstIter = false;
                    temp.leftRootNode = dick;
                }
            }

            temp.bodyPartsLeft = playerNodesLeft;

            firstIter = true;
            // parse right
            foreach (XElement node in root.Element("right").Elements())
            {
                Node daddyD = getDaddy(node.Element("parent").Value, playerNodesRight);

                Node dick = new Node(node.Element("name").Value,
                                     ParseVector(node.Element("pspace_pos")),
                                     float.Parse(node.Element("orientation").Value),
                                     daddyD,
                                     node.Element("isLeaf").Value,
                                     ParseVector(node.Element("textureOrigin")),
                                     node.Element("texture").Value);
                playerNodesRight.Add(dick.name, dick);

                // save the root, which is always the first node in the xml file
                if (firstIter)
                {
                    firstIter = false;
                    temp.rightRootNode = dick;
                }

            }
            temp.bodyPartsRight = playerNodesRight;

            temp.isFlipped = false;
            temp.currentBodyParts = temp.bodyPartsRight;
            temp.currentRootNode = temp.rightRootNode;

            // at this point the left bodyparts and right bodyparts have been loaded
            players.Add(name, temp);
        }

        /* Loads an arena from xml file */
        public static void loadArena(XElement root, Dictionary<String, Arena> arenas)
        {
            string arenaName = XMLParse.GetAttributeValue(root, "name");

            XElement backgroundImage = root.Element("background");

            String background = backgroundImage.Value;
            Vector2 arenasize = XMLParse.ParseVector(root.Element("arenasize"));

            XElement blocks = root.Element("blockList");

            bool collidable;
            Texture2D texture;
            int width;
            int height;
            float secondsPerFrame;
            Vector2 position;
            AnimatedObject aniObj;
            Block tempBlock;

            List<Block> blockList = new List<Block>();

            /* load blocks */
            foreach (XElement block in blocks.Elements())
            {
                if (block.Element("collidable").Value == "true")
                {
                    collidable = true;
                }
                else
                {
                    collidable = false;
                }

                texture = Game1.contentM.Load<Texture2D>(block.Element("texture").Value);

                width = int.Parse(block.Element("width").Value);
                height = int.Parse(block.Element("height").Value);
                secondsPerFrame = float.Parse(block.Element("secondsPerFrame").Value);
                position = XMLParse.ParseVector(block.Element("position"));

                aniObj = new AnimatedObject(texture, width, height, Vector2.Zero, true, secondsPerFrame, position);
                tempBlock = new Block(aniObj, collidable, position);

                blockList.Add(tempBlock);
            }

            arenas.Add(arenaName, new Arena(arenaName, blockList, background, arenasize));
        }


        /* Loads an animation from xml file into animationList. */
        public static void loadAnimation(XElement root, Dictionary<String, Animation> animationList)
        {
            string aniName = XMLParse.GetAttributeValue(root, "name");
            string curKeyFrameNumber = "";
            string curAnimNodeName = "";
            string texture;
            string localOrientation;
            KeyFrame tempKeyFrame;
            NodeAnimInfo tempNodeAnimInfo;
            Animation newAnimation = new Animation();
            newAnimation.name = aniName;
            List<NodeAnimInfo> tempAniInfos;

            XElement nodeStates = root.Element("NodeStates");


            Dictionary<String, String> nodeStateDict = new Dictionary<String, String>();
            foreach (XElement nodeState in nodeStates.Elements())
            {
                nodeStateDict.Add(XMLParse.GetAttributeValue(nodeState, "name"), nodeState.Value);
            }

            XElement KeyFrames = root.Element("KeyFrames");
            foreach (XElement KeyFrame in KeyFrames.Elements())
            {
                curKeyFrameNumber = XMLParse.GetAttributeValue(KeyFrame, "number");
                XElement AnimationNodes = KeyFrame.Element("AnimationNodes");
                tempAniInfos = new List<NodeAnimInfo>();
                foreach (XElement AnimationNode in AnimationNodes.Elements())
                {
                    curAnimNodeName = XMLParse.GetAttributeValue(AnimationNode, "name");
                    texture = AnimationNode.Element("Texture").Value.ToString();
                    localOrientation = AnimationNode.Element("LocalOrientation").Value.ToString();

                    // look up state of node for the animation, for now a node has the same state during
                    // the entire animation, later it would be nice to have it change...
                    String nodeStateValue = nodeStateDict[curAnimNodeName];
                    tempNodeAnimInfo = new NodeAnimInfo(curAnimNodeName, XMLParse.ParseVector(AnimationNode.Element("PositionInParent")), float.Parse(localOrientation), Game1.contentM.Load<Texture2D>(texture), nodeStateValue);
                    tempAniInfos.Add(tempNodeAnimInfo);
                }
                tempKeyFrame = new KeyFrame(tempAniInfos);
                newAnimation.insertKeyFrame(int.Parse(curKeyFrameNumber), tempKeyFrame);
            }
            animationList.Add(newAnimation.name, newAnimation);
        }




        protected static void addPlayers(XElement elem, Object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty("players");

            List<Player> players = new List<Player>();

            foreach (XElement player in elem.Elements())
            {

                Player temp = new Player();

                Dictionary<String, Node> playerNodes = new Dictionary<String, Node>();

                bool firstIter = true;

                foreach (XElement node in player.Elements())
                {
                    Node daddyD = getDaddy(node.Element("parent").Value, playerNodes);

                    Node dick = new Node(node.Element("name").Value,
                                         ParseVector(node.Element("pspace_pos")),
                                         float.Parse(node.Element("orientation").Value),
                                         daddyD,
                                         node.Element("isLeaf").Value,
                                         ParseVector(node.Element("textureOrigin")),
                                         node.Element("texture").Value);
                    playerNodes.Add(dick.name, dick);

                    // save the root, which is always the first node in the xml file
                    if (firstIter)
                    {
                        firstIter = false;
                        temp.currentRootNode = dick;
                    }
                }

                temp.currentBodyParts = playerNodes;

                players.Add(temp);
            }

            propertyInfo.SetValue(obj, players, null);
        }

        protected static void addAnimations(XElement elem, Object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty("animations");

            Dictionary<String, Animation> animations = new Dictionary<String, Animation>();

            foreach (XElement anim in elem.Elements())
            {
                String animationName = anim.Element("animName").Value;

                SortedDictionary<int, Dictionary<String, Node>> keyFrames = new SortedDictionary<int, Dictionary<String, Node>>();

                foreach (XElement frame in anim.Element("Frames").Elements())
                {
                    int frameNum = int.Parse(frame.Element("frameNum").Value);

                    Dictionary<String, Node> bodyparts = new Dictionary<String, Node>();

                    foreach (XElement bodypart in frame.Element("BodyParts").Elements())
                    {
                        Node daddyD = getDaddy(bodypart.Element("parent").Value, bodyparts);

                        Node dick = new Node(bodypart.Element("name").Value,
                                             ParseVector(bodypart.Element("pspace_pos")),
                                             float.Parse(bodypart.Element("orientation").Value),
                                             daddyD,
                                             bodypart.Element("isLeaf").Value,
                                             ParseVector(bodypart.Element("textureOrigin")),
                                             bodypart.Element("texture").Value);

                        bodyparts.Add(dick.name, dick);
                    }

                    keyFrames.Add(frameNum, bodyparts);

                }

                // Animation animation = new Animation(animationName, keyFrames);

                // animations.Add(animationName, animation);
            }

            propertyInfo.SetValue(obj, animations, null);
        }
    }
}
