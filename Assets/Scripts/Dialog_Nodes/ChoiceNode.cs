using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Dialog_Nodes
{
    class ChoiceNode : DialogNode
    {
        public string direction;
        public string[] choices;
        public Queue<DialogNode>[] branches;

        public ChoiceNode(string direction, string c1, string c2, string c3, Queue<DialogNode>[] branches)
        {
            if (c1 == null && c2 == null && c3 == null)
                throw new UnityException("There must be at least a choice");
            // count numBranches
            uint numBranches = 0;
            if (c1 != null) numBranches++;
            if (c2 != null) numBranches++;
            if (c3 != null) numBranches++;
            // ensure that there are just as many branches!
            if (branches == null || branches.Length != numBranches)
                throw new UnityException("number of branches don't match with number of choices");
            // only possibile directions: left and right
            if (direction == null || (direction != "left" && direction != "right"))
                throw new UnityException("direction must be present. It must be either left or right.");

            this.direction = direction;
            string[] potentialChoices = new string[3] {c1, c2, c3};
            string[] choices = new string[numBranches];
            for (int i = 0; i < numBranches; i++)
                choices[i] = potentialChoices[i];
            this.choices = choices;

            this.branches = branches;
            
        }


    }
}
