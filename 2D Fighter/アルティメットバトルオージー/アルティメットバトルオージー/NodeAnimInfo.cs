using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace アルティメットバトルオージー
{
    /* Represents info about the state of a node in an animation. The node is identified by the field name. */
    class NodeAnimInfo
    {
        public String name;
        public Vector2 positionInParent;
        public float localOrientation;
        public Texture2D texture;
        public String state;

        public NodeAnimInfo(Node node)
        {
            this.name = String.Copy(node.name);
            this.positionInParent = new Vector2(node.positionInParent.X, node.positionInParent.Y);
            this.localOrientation = node.localOrientation;
            this.texture = node.texture;
        }

        public NodeAnimInfo(String name, Vector2 positionInParent, float localOrientation, Texture2D texture, String state)
        {
            this.name = String.Copy(name);
            this.positionInParent = new Vector2(positionInParent.X, positionInParent.Y);
            this.localOrientation = localOrientation;
            this.texture = texture;
            this.state = String.Copy(state);
        }


        public static NodeAnimInfo lerpNodes(NodeAnimInfo nodeA, NodeAnimInfo nodeB, float lerpAmount)
        {

            float newOrienation = MathHelper.Lerp(nodeA.localOrientation, nodeB.localOrientation, lerpAmount);

            // hacky attempt at flipping angles to change 


            // sets the node state, its the same throughout the whole animation
            // so it doesnt matter if we use the left or right keyframe.
            String newNodeState = nodeA.state;
            Vector2 newNodePosInParent = nodeA.positionInParent;
            /* We can add more attributes to lerp on here that are in the NodeAnimInfo */
            Texture2D newNodeTexture = nodeA.texture;
            NodeAnimInfo newAnimNode = new NodeAnimInfo(nodeA.name, newNodePosInParent, newOrienation, newNodeTexture, newNodeState);

            return newAnimNode;
        }
    }
}
