using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Game_Exceptions;

public class ChatboxController : MonoBehaviour {

    // chatbox gameobjects
    static GameObject chatbox;
    static GameObject charName;
    static GameObject body;
    // whether or not the chatbox is visible
    static bool isVisible = true;
    // dialog to be processed as the frames progress.
    static Queue<char> dialog;
    // frameclock to adust speed of dialog. advances to no bounds!
    static uint frameClock = 0;
  
    // number of frames it takes to print a character. Speed determinator.
    const uint FRAMES_PER_CHAR = 3;
    // chatbox pause amount for inactivating and activating to set name
    const uint FRAMES_PER_NAMECHANGE = 10;

    // original location of the chatbox
    static Vector3 pos;
    // counter until chatbox wakes up. 0 is awake.
    static uint sleepCounter = 0;
    // delegate to be passed to sleep. function to be executed on awake.
    public delegate void onAwake();
    // stored onAwake delegate to be executed. This is used by the sleep function.
    private static onAwake sleepFunc;
    private static bool ready;

    // this can be invoked to generate the choice system given the right parameters
    private static ChoiceSystem choiceSystem;
    // choice retrieved through the choice system! [0, ChoiceSystem.CHOICE_NUMBER_LIMIT)
    private static int playerChoice;
   
    // Use this for initialization
    void Start () {
		chatbox = GameObject.Find ("Chatbox");
        charName = chatbox.transform.GetChild(1).gameObject;
		body = chatbox.transform.GetChild (2).gameObject;
        dialog = new Queue<char>();
        pos = chatbox.transform.position;
        ready = true;
    }

    // Update is called once per frame
    void Update()
    {

        // always advance frameClock!
        frameClock++;

        // handles sleep counter, no action performed unless this is 0.
        if (sleepCounter != 0)
        {
            sleepCounter--;
            if (sleepCounter == 0)
                sleepFunc();
            return;
        }

        // if the dialog queue has anything, it needs to be handled 
        if (dialog.Count != 0 && frameClock % FRAMES_PER_CHAR == 0)
        {
            printChar(dialog.Dequeue());

            // ready to print more text!
            if (dialog.Count == 0)
                ready = true;
        }

        // if the choice system is initiated, have the player make a choice!
        if (choiceSystem != null)
        {
            playerChoice = choiceSystem.handleUpdate(); // will return INVALID_CHOICE until complete!
            if(playerChoice != ChoiceSystem.INVALID_CHOICE)
            {
                // RIP. Thank you very much. Good bye.
                choiceSystem = null;
                ready = true;
            }
        }

    }

    /**
     * returns whether the component is ready to accept more text to be displayed
     */
    public static bool Ready()
    {
        return ready;
    }

    /**
     * fills in the rest of the text from the queue
     */
    public static void skipText()
    {
        // if the dialog queue has anything, it needs to be handled 
        while (dialog.Count != 0)
        {
            printChar(dialog.Dequeue());
            
            // ready to print more text!
            if (dialog.Count == 0)
                ready = true;
        }

    }

    /**
     * toggles visibility of chatbox
     */
    public static void toggleVisible()
    {
        setVisible(!isVisible);
    }

    /**
     * activates or deactivates the chatbox, not the gameobjct.
     * SORT OF A HACK. Just transports it out.
     */
    public static void setVisible(bool visible){
        if (chatbox != null)
        {
            if (visible)
                chatbox.transform.position = pos;
            else
                chatbox.transform.position = new Vector3(0, 0, 999);
            isVisible = visible;
        }
	}

    /**
     * sleeps for a number of frames. Sets delegate func forr execcution upon awake
     */
    public static void sleep(uint nFrames, onAwake func)
    {
        sleepCounter = nFrames+1; // if it was 1, on update, that decreases to 0 instantly.
        sleepFunc = func;
    }


    /**
     * clears text in chatbox and puts the string into the chatbox, one character a time.
     */
    public static void put(string s)
    {
        clear();
        print(s);
    }

    /**
     * registers the string's characters in the dialog queue, which is printed one character at a time.
     */
    public static void print(string s)
    {
        if (!ready)
            throw new NotReadyException("Chatbox controller is not ready");

        // there's nothing to print, really.
        if (s == null || s == "")
            return;

        // Text might not be safe to be entered into the chatbox. Filter it.
        s = filterGameText(s);

        // We are now working to the best of our ability to deliver. Please wait.
        ready = false;

        foreach(char c in s)
            dialog.Enqueue(c);
        
    }

    /**
     * Filters game text by removing tabs and by trimming the ends.
     * Tabs are either removed or replaced with a space, depending on if there is a space before/after them.
     * Extra spaces are trimmed down.
     */
    private static string filterGameText(string s)
    {
        Debug.Log("Executed: s=" + s);
        s = s.Trim();
        while(s.Contains("\t"))
        {
            if (   (s.IndexOf('\t') != 0 && s[s.IndexOf('\t') - 1] == ' ')
                || (s.IndexOf('\t') != s.Length - 1 && s[s.IndexOf('\t') + 1] == ' '))
            {
                s = s.Remove(s.IndexOf('\t'));
            }
            else
            {
                s = s.Replace('\t', ' ');
            }
        }
        while(s.Contains("  "))
        {
            s = s.Remove(s.IndexOf("  "), 1);
        }

        while(s.Contains("\n "))
        {
            s = s.Replace("\n ", "\n");
        }

        if(s.Length > 30) Debug.Log("s[30]=" + s[30]);

        return s;
    }

    /*
     * Appends one character to the Text UI of the chatbox's body.
     */
    private static void printChar(char c)
    {
        body.GetComponent<Text>().text += c;
    }

    /**
     * sets avatar name. This never happens on screen, an animation is triggered! TODO really?
     */
    public static void setName(string name)
    {
        if (charName.GetComponent<Text>().text == name)
            return;
        setVisible(false);
        charName.GetComponent<Text>().text = name;
        sleep(FRAMES_PER_NAMECHANGE, toggleVisible);
    }

    public static void clear()
    {
        body.GetComponent<Text>().text = "";
    }

    /**
     * starts up the choice system, which takes over the chatbox until the user makes a decision
     */
    public static void invokeChoiceSystem(string[] choices)
    {
        if (!ready)
            throw new NotReadyException("Chatbox controller is not ready");

        // TODO text cutting workaround??? GOOD???
        string headerText = body.GetComponent<Text>().text;
        if (headerText.Length > ChoiceSystem.HEADER_TEXT_CHAR_LIMIT)
            headerText = headerText.Substring(0, ChoiceSystem.HEADER_TEXT_CHAR_LIMIT - 3) + "...";

        // Ay. Let's get to work!
        ready = false;
        choiceSystem = new ChoiceSystem(headerText, choices);

    }

    /**
     * returns choice made by user, and clears that field so it can't be accessed again.
     * Handle with care.
     */
    public static int retrievePlayerChoice()
    {
        if (!ready)
            throw new NotReadyException("Chatbox controller is not ready");
        int output = playerChoice;
        playerChoice = -1;
        return output;

    }

    private class ChoiceSystem
    {
        // yellow text at the top of the choice initiator
        string headerText;
        // choices for player to choose from, only three are given at a time.
        string[] choices;
        // you can't have infinite choices for the player to choose from. That'd be dilematic to deal with.
        const int CHOICE_NUMBER_LIMIT = 3;
        // character length limits, it really needs to fit into a chatbox, y'know...
        public const int HEADER_TEXT_CHAR_LIMIT = 30;
        public const int CHOICE_TEXT_CHAR_LIMIT = 30;
        // number of frames it takes to pop up chatbox window for choice making
        const uint FRAMES_PER_CHOICE_CHATBOX_POPUP = FRAMES_PER_NAMECHANGE;
        // color of headerText. used in the text component's rich text feature <color=HEADER_TEXT_COLOR>...</color>
        const string HEADER_TEXT_COLOR = "yellow";
        // choice that the user will eventually make. Range 0 to CHOICE_NUMBER_LIMIT, exclusive.
        int choice;
        // value of choice, unless it's valid.
        public const int INVALID_CHOICE = -1;
        // indicates at what choice the player currently is. UI Controlled.
        int choiceCsr;
        // scaling needed to increment by 1 unit globally is TRANSFORM_SCALING*1.
        const float TRANSFORM_SCALE = 1.408654f;
        // First line is highest in the chatbox! Next lines are lower by a multiple of LNE_HEIGHT
        const float FIRST_CHOICE_Y_POS = TRANSFORM_SCALE * -160;
        // height of a chatbox line of text. Used to move from a line to another.
        const float CHOICE_HEIGHT = TRANSFORM_SCALE * 30;
        // used at the start to indicate that the player didn't interact with the UI yet.
        const int INVALID_PLAYER_CURSOR_POSITION = -1;

        /**
         * <summary>headerText and choices must all be within their character number constrains.
         * the number of choices must also be within limit, as well.</summary>
         */
        public ChoiceSystem(string headerText, string[] choices)
        {
            // error-checking. Beep beep.
            if (headerText == null || choices == null)
                throw new ArgumentNullException("The choice system must contain a headertext and choices!");
            if (choices.Length == 0 || choices.Length > CHOICE_NUMBER_LIMIT)
                throw new ArgumentException("choices must be between 1 and " + CHOICE_NUMBER_LIMIT);
            if (headerText.Length > HEADER_TEXT_CHAR_LIMIT)
                throw new ArgumentException("headerText too big. Length must be <= " + HEADER_TEXT_CHAR_LIMIT);
            foreach(string choice in choices)
            {
                if(choice.Length > CHOICE_TEXT_CHAR_LIMIT)
                    throw new ArgumentException("Text length of a choice Must be <= " + CHOICE_TEXT_CHAR_LIMIT);
            }

            this.headerText = headerText;
            this.choices = choices;
            choiceCsr = -1;

            // hide chatbox, set text, and then pop up.
            charName.GetComponent<Text>().text = "";
            setVisible(false);
            clear();
            body.GetComponent<Text>().text = "<color=" + HEADER_TEXT_COLOR + ">" + headerText + "</color>\n";
            foreach (string choice in choices)
                body.GetComponent<Text>().text += choice + "\n";
            // removing that last '\n'
            var temp = body.GetComponent<Text>().text;
            temp = temp.Substring(0, temp.Length - 1);
            body.GetComponent<Text>().text = temp;
            // Ay, sleep then pop up!
            sleep(nFrames: FRAMES_PER_CHOICE_CHATBOX_POPUP, func: toggleVisible);

        }

        /**
         * Handles User decision making! This will be called as long as it return INVALID_CHOICE.
         * Once it returns a valid choice, bam, goodbye.
         */
        public int handleUpdate()
        {
            // chatbox poped up. Now to handle user input. See what they choose, track their movements.
            // this will return INVALID_CHOICE until the user makes a choice.
            int choice = INVALID_CHOICE;
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                choiceCsr = (choiceCsr + 1) % choices.Length;
                putPlayerCursor(choiceCsr);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) // shouldn't be able to go up at start
            {
                if (choiceCsr != INVALID_PLAYER_CURSOR_POSITION)
                {
                    // -1 in context of % choices.Length... Except, C# doesn't do % that way.
                    choiceCsr = (choiceCsr + choices.Length - 1) % choices.Length;
                    putPlayerCursor(choiceCsr);
                }
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                choice = assignPlayerChoice(choiceCsr);
                if (choice != INVALID_CHOICE)
                    hideChoiceCursor(); // in the off-event that it 
            }

            if (choice != INVALID_CHOICE) Debug.Log("choice=" + choice);
            return choice;
        }

        /**
         * ensures the playerCursor is at a valid location and returns the choice it's assigned.
         */
        private int assignPlayerChoice(int playerCursor)
        {
            if (playerCursor == INVALID_PLAYER_CURSOR_POSITION)
                return INVALID_CHOICE;

            // it maps perfectly, oops
            return playerCursor;
        }

        /**
         * Enables the cursor texture game obect and sets it to the specified location, (assuming it's legal).
         */
        private void putPlayerCursor(int choiceCsr)
        {
            if (choiceCsr < 0 || choiceCsr > 2)
                throw new ArgumentOutOfRangeException("choiceCsr " + choiceCsr + " must be in range [0,2]");
            
            var choiceMarker = GameObject.Find("Choice Marker");
            showChoiceCursor();
            float y = FIRST_CHOICE_Y_POS - choiceCsr * CHOICE_HEIGHT;
            var pos = choiceMarker.transform.position;
            pos.y = y;
            choiceMarker.transform.position = pos;
        }

        private void hideChoiceCursor()
        {
            var cm = GameObject.Find("Choice Marker");
            var pos = cm.transform.position;
            pos.y = 999;
            cm.transform.position = pos;
        }

        private void showChoiceCursor()
        {
            var cm = GameObject.Find("Choice Marker");
            var pos = cm.transform.position;
            pos.y = FIRST_CHOICE_Y_POS;
            cm.transform.position = pos;
        }
    }
}