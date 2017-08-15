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
                        nodeQueue.Enqueue(handleConfig(el, isLeft: true));
                        break;
                    case "right":
                        nodeQueue.Enqueue(handleConfig(el, isLeft: false));
                        break;
                    default:
                        // Debug.Log(el);
                        break;
                }
            }

            return nodeQueue;
        }

        /**
         * @param el        Left/Right node element to parse data from
         * @param isLeft    Whether it's the character in the left side, or right side.
         * @returns the instantiaated node as a DialogNode
         */
        private static DialogNode handleConfig(XElement el, bool isLeft)
        {
            string character = null;
            if (el.Attribute("char") != null) character = el.Attribute("char").Value;
            string emotion = null;
            if (el.Attribute("emotion") != null) emotion = el.Attribute("emotion").Value;

            if (isLeft)
                return new LeftNode(character, emotion);
            else
                return new RightNode(character, emotion);
        }

        /**
         * @param el        TextNode element to parse name/content from
         * @return the instantiated node as a DialogNode.
         */
        private static DialogNode handleText(XElement el)
        {
            string name = null;
            if (el.Attribute("name") != null) name = el.Attribute("name").Value;
            string content = el.Value;

            return new TextNode(name, content);
        }
    }
}
