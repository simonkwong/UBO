using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace アルティメットバトルオージー
{
    class Animation
    {
        public String name;
        public SortedDictionary<int, KeyFrame> keyFrames1;

        public int numFrames
        {
            get
            {
                return keyFrames1.Keys.ElementAt(keyFrames1.Keys.Count - 1);
            }
        }

        public Animation()
        {
            name = "";
            keyFrames1 = new SortedDictionary<int, KeyFrame>();
        }

        public void insertKeyFrame(int frameNum, KeyFrame newKeyFrame)
        {
            if (this.keyFrames1.ContainsKey(frameNum))
            {
                this.keyFrames1.Remove(frameNum);
            }

            keyFrames1.Add(frameNum, newKeyFrame);
        }

        public void deleteKeyFrame(int frameNum)
        {
            if (keyFrames1.ContainsKey(frameNum))
            {
                keyFrames1.Remove(frameNum);
            }
        }

        /* Writes animation to an xml file. The xml file is exported into a folder called PoserExports on the desktop. */
        public void exportToXML()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\PoserExports";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }


            using (XmlWriter writer = XmlWriter.Create(path + "\\" + name + ".xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Animation");

                writer.WriteAttributeString("name", name);

                writer.WriteStartElement("KeyFrames");

                foreach (int frameNum in keyFrames1.Keys)
                {
                    writer.WriteStartElement("KeyFrame");
                    writer.WriteAttributeString("number", frameNum.ToString());

                    writer.WriteStartElement("AnimationNodes");

                    foreach (String nodeAnimName in keyFrames1[frameNum].nodesAnimInfo.Keys)
                    {
                        writer.WriteStartElement("AnimationNode");
                        writer.WriteAttributeString("name", nodeAnimName);

                        NodeAnimInfo curAnimNode = keyFrames1[frameNum].nodesAnimInfo[nodeAnimName];

                        writer.WriteStartElement("Texture");
                        writer.WriteString(curAnimNode.texture.Name);
                        writer.WriteEndElement();
                        writer.WriteStartElement("LocalOrientation");
                        writer.WriteString(curAnimNode.localOrientation.ToString());
                        writer.WriteEndElement();
                        writer.WriteStartElement("PositionInParent");
                        writer.WriteStartElement("x");
                        writer.WriteString(curAnimNode.positionInParent.X.ToString());
                        writer.WriteEndElement();
                        writer.WriteStartElement("y");
                        writer.WriteString(curAnimNode.positionInParent.Y.ToString());
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static void lerpAndSwitchFrame(Animation animOld, int animOldFrameNum, Animation animNew, int animNewFrameNum, float lerpAmount, Player player)
        {
            KeyFrame oldKeyFrame = getLerpedKeyFrame(animOld, animOldFrameNum, player);
            KeyFrame newKeyFrame = getLerpedKeyFrame(animNew, animNewFrameNum, player);

            List<NodeAnimInfo> nodeAniInfoList = new List<NodeAnimInfo>();
            foreach (String aniNodeName in oldKeyFrame.nodesAnimInfo.Keys)
            {
                NodeAnimInfo oldAniNode = oldKeyFrame.nodesAnimInfo[aniNodeName];
                NodeAnimInfo newAniNode = newKeyFrame.nodesAnimInfo[aniNodeName];

                // calculate lerped node
                NodeAnimInfo newAnimNode = NodeAnimInfo.lerpNodes(oldAniNode, newAniNode, lerpAmount);
                nodeAniInfoList.Add(newAnimNode);
            }

            KeyFrame lerpedKeyFrame = new KeyFrame(nodeAniInfoList);
            player.updateBody(lerpedKeyFrame);
        }

        public static KeyFrame getLerpedKeyFrame(Animation anim, int currentFrameNum, Player player)
        {
            int leftNeighbor = -1;
            int rightNeighbor = -1;

            List<int> aniKeys = anim.keyFrames1.Keys.ToList();

            // find first key that is greater than or equal to the frame number.
            for (int i = 0; i < aniKeys.Count; i++)
            {
                if (aniKeys[i] >= currentFrameNum)
                {
                    // we found it
                    rightNeighbor = aniKeys[i];

                    if (aniKeys[i] == 0)
                    {
                        leftNeighbor = aniKeys[i];
                    }
                    else
                    {
                        leftNeighbor = aniKeys[i - 1];
                    }

                    KeyFrame leftKeyFrame = anim.keyFrames1[leftNeighbor];
                    KeyFrame rightKeyFrame = anim.keyFrames1[rightNeighbor];

                    float lerpAmount = ((float)(currentFrameNum - leftNeighbor)) / (rightNeighbor - leftNeighbor);

                    if (rightNeighbor == leftNeighbor)
                    {
                        lerpAmount = 0;
                    }


                    List<NodeAnimInfo> animNodes = new List<NodeAnimInfo>();
                    // lerp every body part accordingly
                    foreach (String nodeName in leftKeyFrame.nodesAnimInfo.Keys)
                    {
                        // ignore angle of root node, this should not be determined by the animation
                        if (player.currentRootNode == player.currentBodyParts[nodeName])
                        {
                            continue;
                        }

                        NodeAnimInfo newAnimNode = NodeAnimInfo.lerpNodes(leftKeyFrame.nodesAnimInfo[nodeName], rightKeyFrame.nodesAnimInfo[nodeName], lerpAmount);

                        animNodes.Add(newAnimNode);
                    }

                    return new KeyFrame(animNodes);
                }
            }
            // this will never be reached
            return null;
        }


        public static void lerpAndSwitchFrame(Animation anim, int currentFrameNum, Player player)
        {
            KeyFrame keyFrame = getLerpedKeyFrame(anim, currentFrameNum, player);
            player.updateBody(keyFrame);
            player.updateBody(keyFrame);
        }
    }
}
