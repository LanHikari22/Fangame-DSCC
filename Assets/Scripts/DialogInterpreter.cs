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
        // previous node executed. This is used to perform logic based on what node was processed.
        DialogNode previousNode;
        // determines whether the next dialog node can be processed, or if the current one is stil processing
        bool ready = true;

        public DialogInterpreter(Queue<DialogNode> dialogNodes)
        {
            this.dialogNodes = dialogNodes;
            ready = true;
        }

        private void step(DialogNode dn)
        {
            // we're not ready next frame... unless we are.
            ready = false;

            if (dn is TextNode)
            {
                var tn = (TextNode)dn;
                ChatboxController.setName(tn.name);
                ChatboxController.put(tn.content);
            }
            else if (dn is LeftNode)
            {
                handleLeftNode((LeftNode)dn);
            }
            else if (dn is RightNode)
            {
                handleRightNode((RightNode)dn);
            }
            else
            {
                throw new Exception("Weird type somehow made it in");
            }
            // Mark this dialogNode as the previously processed node
            previousNode = dn;
        }

        /**
         * Listens to user input, and determines whether we're ready to process next DialogNode.
         * @returns ready status
         */
        private bool handleReady()
        {
            var ready = false;

            // If we haven't processed anything yet
            if(previousNode == null)
            {
                ready = true;
            }

            // These nodes have no lag, they shouldn't halt the ready status.
            if(previousNode is LeftNode || previousNode is RightNode)
            {
                ready = true;
            }

            // when all text has been input, press key to continue
            if (ChatboxController.Ready() && Input.GetKeyDown(KeyCode.Z))
            {
                ready = true;
            }

            // if text is being processed, press key to speed up text to complete
            if (!ChatboxController.Ready() && Input.GetKeyDown(KeyCode.X))
            {
                // TODO should do on Z too. handle only first press
                ChatboxController.skipText();
            }

            return ready;
        }

        /**
         * <param name="ln">LeftNode node to perform configuration based on</param>
         */
         private void handleLeftNode(LeftNode ln)
        {
            var sr = GameObject.Find("L-Character").GetComponent<SpriteRenderer>();
            if (ln.character == "" || (ln.character != null && ln.emotion != null))
            {
                var sprite = DialogNode.LoadSprite(spriteName:
                    DialogNode.buildSpriteName(ln.character, ln.emotion, n:1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if(ln.character == null && ln.emotion != null)
            {
                // Keep same character, change emotion!
                // ensure emotion is valid
                if (!DialogNode.isValidEmotion(DialogNode.getCharName(sr.sprite.name), ln.emotion))
                    throw new UnityException("Promise emotion '" + ln.emotion + "' turns out to be invalid!");
                var sprite = DialogNode.LoadSprite(spriteName:
                    DialogNode.buildSpriteName(DialogNode.getCharName(sr.sprite.name), ln.emotion, n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if (ln.character != null && ln.emotion == null)
            {
                // Load character with neutral emotion
                var sprite = DialogNode.LoadSprite(spriteName:
                    DialogNode.buildSpriteName(ln.character, "neutral", n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
        }

        /**
        * <param name="rn">RightNode node to perform configuration based on</param>
        */
        private void handleRightNode(RightNode rn)
        {
            var sr = GameObject.Find("R-Character").GetComponent<SpriteRenderer>();
            if (rn.character == "" || (rn.character != null && rn.emotion != null))
            {
                var sprite = DialogNode.LoadSprite(spriteName:
                    DialogNode.buildSpriteName(rn.character, rn.emotion, n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if (rn.character == null && rn.emotion != null)
            {
                // Keep same character, change emotion!
                // ensure emotion is valid
                if (!DialogNode.isValidEmotion(DialogNode.getCharName(sr.sprite.name), rn.emotion))
                    throw new UnityException("Promise emotion '" + rn.emotion + "' turns out to be invalid!");
                var sprite = DialogNode.LoadSprite(spriteName: 
                    DialogNode.buildSpriteName(DialogNode.getCharName(sr.sprite.name),rn.emotion,n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if (rn.character != null && rn.emotion == null)
            {
                // Load character with neutral emotion
                var sprite = DialogNode.LoadSprite(spriteName:
                    DialogNode.buildSpriteName(rn.character, "neutral", n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
        }

        /*
         * provided by a monobehavior. Runs every frame. TODO is this OK
         * @returns True if there are still dialogNodes to process, otherwise, false.
         */
        public bool Update()
        {
            bool stillRunning = true;

            // If not ready, logic will be executed every frame to listen and change status when ready.
            ready = handleReady();

            if (ready)
            {
                // advance to next DialogNode if there is any. if not, we're done.
                if (dialogNodes.Count != 0)
                    step(dialogNodes.Dequeue());
                else
                    stillRunning = false;
            }

            return stillRunning;
        }

    }
}
