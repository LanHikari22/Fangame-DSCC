using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    class DialogCharacter
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

        private GameObject character;
        public const int R_CHARACTER = 0;
        public const int L_CHARACTER = 1;

        /**
         * Creates an instance that either controls the right character gameobject or the left one.
         * Use constants R_CHARACTER and L_CHARACTER.
         */
        public DialogCharacter(int character)
        {
            if (character != R_CHARACTER && character != L_CHARACTER)
                throw new UnityException("Character given is not defined");
            if (character == R_CHARACTER)
                this.character = GameObject.Find("R-Character");
            if (character == L_CHARACTER)
                this.character = GameObject.Find("L-Character");
        }

        /**
         * returns current character name of this instnace
         */
        public string currCharName()
        {
            var sr = character.GetComponent<SpriteRenderer>();
            return getCharName(sr.sprite.name);
        }

        /**
         * Sets the sprite for the specified character
         */
        public void SetSprite(string charName, string emotion)
        {
            var path = buildSpriteName(charName, emotion, n: 1);
            var sprite = LoadSprite(path);
            character.GetComponent<SpriteRenderer>().sprite = sprite;
        }

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
                foreach (string em in characterEmotions[character])
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
            foreach (var character in characterPaths)
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
            var sprite = searchSpriteArray(sprites, spriteName);
            return sprite;
        }

        /**
	 * searches array of sprites, checks if one of the sprites has, name, as its name, if so, return it.
     * The name includes the spreadsheet. The format: <spreadsheet>_<name>_e<emotion>_n
	 * @param name		the name to check sprites for. It should adhere to the naming format.
	 * @return sprite with name @param name, or null
	 */
        public static Sprite searchSpriteArray(Sprite[] sprites, string name)
        {
            Sprite output = null;
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name == name)
                {
                    output = sprite;
                }
            }
            return output;
        }
    }

    class SpriteNameFormat
    {
        public string spritesheet { get; set; }
        public string character { get; set; }
        public string emotion { get; set; }
        public int n { get; set; }

        public SpriteNameFormat(string spritesheet, string character, string emotion, int n)
        {
            this.spritesheet = spritesheet;
            this.character = character;
            this.emotion = emotion;
            this.n = n;
        }

        public static SpriteNameFormat Create(string formattedString)
        {
            throw new NotImplementedException();
        }
    }
}
