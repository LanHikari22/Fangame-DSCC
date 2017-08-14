using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.Dialog_Nodes;

namespace Assets.Scripts
{
    class DialogInterpreter
    {

        // the dialog nodes represents the instructions for the interpreter to act upon.
        Queue<DialogNode> dialogNodes;
        // determines whether the next dialog node can be processed, or if the current one is stil processing
        bool ready = true;

        public DialogInterpreter(Queue<DialogNode> dialogNodes)
        {
            this.dialogNodes = dialogNodes;
            ready = true;
        }


        /*
         * provided by a monobehavior. Runs every frame. TODO is this OK
         * @returns True if there are still dialogNodes to process, otherwise, false.
         */
        public bool Update()
        {
            bool stillRunning = true;

            // we are ready, if the chatbox is ready (TODO for now)
            if (ChatboxController.Ready())
                ready = true;

            // advance to next DialogNode
            if (ready)
            {
                if (dialogNodes.Count != 0)
                    step(dialogNodes.Dequeue());
                else
                    stillRunning = false;
            }
            
            return stillRunning;
        }

        private void step(DialogNode dn)
        {
            // we're not ready next frame... unless we are.
            ready = false;

            if(dn is TextNode)
            {
                var tn = (TextNode)dn;
                ChatboxController.setName(tn.name);
                ChatboxController.put(tn.content);
            } else
            {
                throw new Exception("Weird type somehow made it in");
            }
        }
    }
}
