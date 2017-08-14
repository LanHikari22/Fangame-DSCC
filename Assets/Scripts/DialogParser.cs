using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Assets.Scripts;
using Assets.Scripts.Dialog_Nodes;


namespace Assets.Scripts
{
    class DialogParser
    {
        public static Queue<DialogNode> Parse(XDocument xdoc)
        {
            var nodeQueue = new Queue<DialogNode>();

            var elements = xdoc.Element("DSCC_Dialog").Elements();
            
            foreach(XElement el in elements)
            {
                switch (el.Name.ToString())
                {
                    case "text":
                        nodeQueue.Enqueue(handleText(el));
                        break;
                    case "left":
                        break;
                    case "right":
                        break;
                    default:
                        // Debug.Log(el);
                        break;
                }
            }

            return nodeQueue;
        }

        private static DialogNode handleText(XElement el)
        {
            string name = null;
            if (el.Attribute("name") != null) name = el.Attribute("name").Value;
            string content = el.Value;

            return new TextNode(name, content);
        }
    }
}
