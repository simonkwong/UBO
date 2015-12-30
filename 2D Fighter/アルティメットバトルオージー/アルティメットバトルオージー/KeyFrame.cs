using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    /* Represents a single keyframe in an animation. */
    class KeyFrame
    {
        public Dictionary<String, NodeAnimInfo> nodesAnimInfo;

        public KeyFrame(List<NodeAnimInfo> animInfoNodes)
        {
            nodesAnimInfo = new Dictionary<String, NodeAnimInfo>();
            foreach (NodeAnimInfo n in animInfoNodes)
            {
                nodesAnimInfo.Add(n.name, n);
            }
        }


        public KeyFrame(Dictionary<String, Node> bodyParts)
        {
            nodesAnimInfo = new Dictionary<String, NodeAnimInfo>();

            /* Convert each Node in bodyParts to NodeAnimInfo and populate the keyframe. */
            foreach (Node curNode in bodyParts.Values)
            {
                nodesAnimInfo.Add(String.Copy(curNode.name), new NodeAnimInfo(curNode));

                if (curNode.name == "torso")
                {
                    Console.WriteLine("SAVING NODE: " + curNode.name + " pos in parent: " + curNode.positionInParent);
                }
            }
        }





    }
}