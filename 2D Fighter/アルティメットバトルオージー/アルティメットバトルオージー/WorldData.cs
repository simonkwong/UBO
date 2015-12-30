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
    class WorldData
    {
        public Dictionary<String, Animation> animations { get; private set; }
        public Dictionary<String, Arena> arenas { get; private set; }
        public Dictionary<String, Player> players { get; private set; }


        private static WorldData wData;


        private WorldData()
        {
            animations = new Dictionary<String, Animation>();
            arenas = new Dictionary<string, Arena>();
            players = new Dictionary<string, Player>();
        }

        public static WorldData GetInstance()
        {
            if (wData == null)
            {
                wData = new WorldData();
                string[] files = Directory.GetFiles("Content", "*.*", SearchOption.AllDirectories);

                // load all xml files
                foreach (string file in files)
                {
                    if (file.EndsWith("xml"))
                    {
                        wData.LoadData(file);
                    }
                }
            }
            return wData;
        }



        protected void AddData(XElement elem)
        {
            XMLParse.AddValueToClassInstance(elem, WorldData.GetInstance());
        }

        private void LoadData(String dataFile)
        {
            using (XmlReader reader = XmlReader.Create(new StreamReader(dataFile)))
            {
                XDocument xml = XDocument.Load(reader);
                XElement root = xml.Root;

                string rootName = root.Name.ToString();

                if (rootName == "Animation")
                {
                    XMLParse.loadAnimation(root, animations);
                }
                else if (rootName == "arena")
                {
                    XMLParse.loadArena(root, arenas);
                }
                else if (rootName == "player")
                {
                    XMLParse.loadPlayer(root, players);
                }
                else
                {
                    foreach (XElement elem in root.Elements())
                    {
                        AddData(elem);
                    }
                }
            }

        }



    }
}
