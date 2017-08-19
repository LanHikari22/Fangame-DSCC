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
        public bool multi;
        public int exitBranchIndex;

        public ChoiceNode(string direction, string[] choices, Queue<DialogNode>[] branches, 
            bool multi, int exitBranchIndex)
        {
            if (choices == null || choices.Length < 0 || choices.Length > 3)
                throw new UnityException("Choices must not be null and must be withing range [0,2]");
            if (branches == null || choices.Length != branches.Length)
                throw new UnityException("number of branches don't match with number of choices");
            if (direction == null || (direction != "left" && direction != "right"))
                throw new UnityException("direction must be present. It must be either left or right.");
            if (exitBranchIndex != -1 && (exitBranchIndex < 0 || exitBranchIndex > choices.Length)) // -1 == None
                throw new UnityException("Out of bounds exit index " + exitBranchIndex);

            this.direction = direction;
            this.choices = choices;
            this.branches = branches;
            this.multi = multi;
            this.exitBranchIndex = exitBranchIndex;
        }

        public ChoiceNode(string direction, string c1, string c2, string c3, 
            Queue<DialogNode>[] branches, bool multi, int exitBranchIndex)
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
            if (exitBranchIndex != -1 && (exitBranchIndex < 0 || exitBranchIndex > numBranches)) // -1 = None
                throw new UnityException("Out of bounds exit index " + exitBranchIndex);


            this.direction = direction;
            string[] potentialChoices = new string[3] {c1, c2, c3};
            string[] choices = new string[numBranches];
            for (int i = 0; i < numBranches; i++)
                choices[i] = potentialChoices[i];
            this.choices = choices;
            this.branches = branches;
            this.multi = multi;
            this.exitBranchIndex = exitBranchIndex;
        }


    }
}
