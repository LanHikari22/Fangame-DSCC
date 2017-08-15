using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


namespace Assets.Scripts.Dialog_Nodes
{
    class RightNode : DialogNode
    {
        // character and emotion configuration
        public string character { set; get; }
        public string emotion { set; get; }

        public RightNode() : base()
        {

        }

        public RightNode(string character, string emotion)
        {
            // TODO those exceptions BREAK unity, there is some async magic going on here
            if (character != null && !isValidCharacter(character))
                throw new UnityException("Unregistered character '" + character + "'  found in dialog");
            if (character != null && emotion != "" && emotion != null && !isValidEmotion(character, emotion))
                throw new UnityException("emotion '" + emotion + "' is not registered for character '" + character + "'");

            this.character = character;
            this.emotion = emotion;
        }

    }
}
