using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Assets.Scripts.Dialog_Nodes
{
    class DialogNode
    {
        /* Database for characters/emotions and character to asset URL conversions */
        // registered characters and their corresponding emotions
        // TODO (lol how do you create a const dictionary?
        // TODO this is cool for now, but update so that it automatically finds valid chars/emotions from asssets?
        // might not be so fast.
        private static Dictionary<string, string[]> characterEmotions = new Dictionary<string, string[]>
        {
            {"Hero",   new string[] {"neutral", "happy", "sad", "angry", "evil", "pleased", "front"} },
            {"Atsuro", new string[] {"neutral", "happy", "sad", "excited", "serious"} },
            {"Yuzu",   new string[] {"neutral", "happy", "sad", "angry", "surprised", "scared", "blush" , "frightened", "startled" } },
            {"Kid",    new string[] {"neutral" } },
            {"Heehoo", new string[] {"neutral" } },
            {"",       new string[] {} }
        };
        // character to characterURL data in the format <spreadsheet>_<charName>
        private static Dictionary<string, string> characterPaths = new Dictionary<string, string>
        {
            {"Hero",    "Hero_hero"},
            {"Atsuro",  "Atsuro_atsuro" },
            {"Yuzu",    "Yuzu_yuzu" },
            {"Kid",     "NPCs_kid" },
            {"Heehoo",  "NPCs_heehoo"},
            {"",        "None_none" }
        };

            
        /**
         * @returns character path in the resources folder
         */
         public static string getCharacterPath(string character)
        {
            if (characterPaths.ContainsKey(character))
                return characterPaths[character];
            return null;
        }

        /**
         * builds the character path according to the specified format:
         * <spritesheet>_<charName>_<emotion>_<n>
         */
         public static string buildSpriteName(string character, string emotion, int n)
        {
            // empty character is the absence of one; no emotion.
            if (character == "")
            {
                if (emotion != "" && emotion != null) throw new UnityException("provided emotion to nochar!");
                return getCharacterPath(character) + "_eNONE_" + n;
            }

            if (getCharacterPath(character) != null && isValidEmotion(character, emotion))
            {
                return getCharacterPath(character) + "_e" + emotion.ToUpper() + "_" + n;
            }
            return null;
        }

        /*
         * checks whether a character is registered 
         * 
         */
        public static bool isValidCharacter(string character)
        {
            return characterEmotions.ContainsKey(character);
        }

        /**
         * checks whether a character has a certain emotion
         */
        public static bool isValidEmotion(string character, string emotion)
        {
            if (isValidCharacter(character))
                foreach(string em in characterEmotions[character])
                {
                    if (em == emotion)
                        return true;
                }
            return false;
        }

        /**
         * <summary>Returns capitalized character name by performing a dictionary search</summary>
         */
        public static string getCharName(string spritePathFormat)
        {
            // I know I could do this differently but dictionary searches are fun pffft
            var snFirst_ = spritePathFormat.IndexOf("_");
            var snSecond_ = spritePathFormat.IndexOf("_", snFirst_ + 1);
            var strippedSpritePath = spritePathFormat.Substring(0, snSecond_); // <spritesheet>_<charName>
            string output = null;
            foreach(var character in characterPaths)
            {
                if (character.Value == strippedSpritePath)
                {
                    var v = character.Value;
                    var temp = v.Substring(v.IndexOf('_') + 1); // remove spreadsheet from string
                    output = temp.ToUpper()[0] + temp.Substring(1); // capitalize first character
                }
            }
            return output;
        }

        /**
         * Loads sprite from resources given the sprite path
         * @returns the sprite or null
         */
        public static Sprite LoadSprite(string spriteName)
        {
            var spritesheet = spriteName.Substring(0, spriteName.IndexOf("_"));
            var sprites = Resources.LoadAll<Sprite>("Sprites/" + spritesheet);
            var sprite = CharacterAnimator.searchSpriteArray(sprites, spriteName); 
            return sprite;
        }


    }

}
