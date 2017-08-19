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
        /**
         * Parses an XElement based on the DialogNode scripting specification
         */
        public static Queue<DialogNode> Parse(XElement xelem)
        {
            var nodeQueue = new Queue<DialogNode>();

            // parse either an XDocument or an XElement. XElement for recursive-ness.
            var elements = xelem.Elements();

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
                    case "choice":
                        nodeQueue.Enqueue(handleChoice(el));
                        break;
                    default:
                        
                        break;
                }
            }

            return nodeQueue;
        }

        /**
         * <summary>Parses data out of the choice commands</summary>
         * <param name="el">choice element to be parsed</param>
         */
        private static DialogNode handleChoice(XElement el)
        {
            //ex <choice direction="left" c1="Yeah." c2="Not really.">
            string direction = null;
            if (el.Attribute("direction") != null) direction = el.Attribute("direction").Value;
            string c1 = null;
            if (el.Attribute("c1") != null) c1 = el.Attribute("c1").Value;
            string c2 = null;
            if (el.Attribute("c2") != null) c2 = el.Attribute("c2").Value;
            string c3 = null;
            if (el.Attribute("c3") != null) c3 = el.Attribute("c3").Value;
            string multiStr = null;
            if (el.Attribute("multi") != null) multiStr = el.Attribute("multi").Value.ToLower();
            bool multi = multiStr == "true";

            Queue<DialogNode>[] potentialBranches = new Queue<DialogNode>[3];
            // get potential branches, could be 3 or less.
            int branchCursor = 0;
            List<string> branchNamesChecklist = new List<string> { "c1", "c2", "c3"}; // each can only appear ONCE
            int exitBranchIndex = -1;
            foreach (XElement element in el.Elements())
            {
                var elName = element.Name.ToString();

                if (!branchNamesChecklist.Contains(elName) && (elName == "c1" || elName == "c2" || elName == "c3"))
                    throw new UnityException("Cannot contain two elements of the same branch!");
                
                // if branch found, it should not be found again. No duplicate branch tags allowed!
                if (branchNamesChecklist.Contains(elName))
                {
                    branchNamesChecklist.Remove(elName); // check this one off. Shouldn't parse another one...
                    // Check if this node is an exit node. There should be only one!
                    if (element.Attribute("exit") != null && element.Attribute("exit").Value.ToLower() == "true")
                    {
                        if (exitBranchIndex != -1)
                            throw new UnityException("Cannot have two choices being assigned exit!");
                        exitBranchIndex = branchCursor;
                    }

                    // parse parse parse!
                    potentialBranches[branchCursor++] = Parse(element);
                }
            }
            // just take the actual branches
            Queue<DialogNode>[] branches = new Queue<DialogNode>[branchCursor];
            for (int i = 0; i < branchCursor; i++)
                branches[i] = potentialBranches[i];

            return new ChoiceNode(direction, c1, c2, c3, branches, multi, exitBranchIndex);
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
            string content = el.Value.Trim();

            return new TextNode(name, content);
        }
    }
}
