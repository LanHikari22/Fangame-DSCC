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
        DialogNode processedNode;
        // status of interpreter across frames. used to coordinate action.
        int status;
        // determines whether the next dialog node can be processed, or if the current one is stil processing
        const int STATUS_READY = 1;
        const int STATUS_PROCESSING = 2;
        const int STATUS_CHOICE_PROMPT = 30;

        public DialogInterpreter(Queue<DialogNode> dialogNodes)
        {
            this.dialogNodes = dialogNodes;
            status = STATUS_READY;
        }

        private void step(DialogNode dn)
        {
            // we're not ready next frame... unless we are.
            status = STATUS_PROCESSING;

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
            else if (dn is ChoiceNode)
            {
                status = STATUS_CHOICE_PROMPT;
                handleChoiceNode((ChoiceNode)dn);
            }
            else
            {
                throw new Exception("Weird dialogNode somehow made it in");
            }
            // Mark this dialogNode as the previously processed node
            processedNode = dn;
        }

        private void handleChoiceNode(ChoiceNode cn)
        {
            // change sprite in the specified direction to Hero
            DialogCharacter character;
            if (cn.direction == "left")
                character = new DialogCharacter(DialogCharacter.L_CHARACTER);
            else
                character = new DialogCharacter(DialogCharacter.R_CHARACTER);
            if(character.currCharName() != "Hero")
                character.SetSprite("Hero", "neutral");

            // initiate chatbox choice system and await till getting a choice
            // response to this and the execution of the branches will be handled on update
            ChatboxController.invokeChoiceSystem(cn.choices);

            // this node has been initiated
            processedNode = cn;

        }

        /**
         * Listens to user input, and determines whether we're ready to process next DialogNode.
         * @returns operation status
         */
        private int handleUpdateAction()
        {
            // If we haven't processed anything yet
            if(processedNode == null)
            {
                status = STATUS_READY;
            }

            // These nodes have no lag, they shouldn't halt the ready status.
            if(processedNode is LeftNode || processedNode is RightNode)
            {
                if(CharacterAnimator.NoSwappingIsHappening())
                    status = STATUS_READY;
            }

            if (processedNode is TextNode)
            {
                // when all text has been input, press key to continue
                if (ChatboxController.Ready() && Input.GetKeyDown(KeyCode.Z))
                {
                    status = STATUS_READY;
                }

                // if text is being processed, press key to speed up text to complete
                if (!ChatboxController.Ready() && Input.GetKeyDown(KeyCode.X))
                {
                    // TODO should do on Z too. handle only first press
                    ChatboxController.skipText();
                }
            }

            if(processedNode is ChoiceNode)
            {
                if(status == STATUS_CHOICE_PROMPT && ChatboxController.Ready())
                {
                    // yay! ChatboxController got an answer!
                    // Let's add the queue of the branch into the main queue to be executed!
                    insertBranchIntoQueue(processedNode as ChoiceNode, ChatboxController.retrievePlayerChoice());

                    // Cool! Now just have the new instructions execute!
                    status = STATUS_READY;
                    
                }
            }

            return status;
        }

        /**
         * Inserts the DialogNode Queue of the choice branch into the main DialogNode Queue for execution
         */
        private void insertBranchIntoQueue(ChoiceNode cn, int choice)
        {
            Queue<DialogNode> branchQueue = cn.branches[choice];
            while(dialogNodes.Count != 0)
                branchQueue.Enqueue(dialogNodes.Dequeue());
            dialogNodes = branchQueue;
        }

        /**
         * <param name="ln">LeftNode node to perform configuration based on</param>
         */
        private void handleLeftNode(LeftNode ln)
        {
            var sr = GameObject.Find("L-Character").GetComponent<SpriteRenderer>();
            if (ln.character == "" || (ln.character != null && ln.emotion != null))
            {
                var sprite = DialogCharacter.LoadSprite(spriteName:
                    DialogCharacter.buildSpriteName(ln.character, ln.emotion, n:1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if(ln.character == null && ln.emotion != null)
            {
                // Keep same character, change emotion!
                // ensure emotion is valid
                if (!DialogCharacter.isValidEmotion(DialogCharacter.getCharName(sr.sprite.name), ln.emotion))
                    throw new UnityException("Promise emotion '" + ln.emotion + "' turns out to be invalid!");
                var sprite = DialogCharacter.LoadSprite(spriteName:
                    DialogCharacter.buildSpriteName(DialogCharacter.getCharName(sr.sprite.name), ln.emotion, n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if (ln.character != null && ln.emotion == null)
            {
                // Load character with neutral emotion
                var sprite = DialogCharacter.LoadSprite(spriteName:
                    DialogCharacter.buildSpriteName(ln.character, "neutral", n: 1));
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
                var sprite = DialogCharacter.LoadSprite(spriteName:
                    DialogCharacter.buildSpriteName(rn.character, rn.emotion, n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if (rn.character == null && rn.emotion != null)
            {
                // Keep same character, change emotion!
                // ensure emotion is valid
                if (!DialogCharacter.isValidEmotion(DialogCharacter.getCharName(sr.sprite.name), rn.emotion))
                    throw new UnityException("Promise emotion '" + rn.emotion + "' turns out to be invalid!");
                var sprite = DialogCharacter.LoadSprite(spriteName: 
                    DialogCharacter.buildSpriteName(DialogCharacter.getCharName(sr.sprite.name),rn.emotion,n: 1));
                if (sprite != null)
                    sr.sprite = sprite;
            }
            else if (rn.character != null && rn.emotion == null)
            {
                // Load character with neutral emotion
                var sprite = DialogCharacter.LoadSprite(spriteName:
                    DialogCharacter.buildSpriteName(rn.character, "neutral", n: 1));
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
            status = handleUpdateAction();

            if (status == STATUS_READY)
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