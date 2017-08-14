using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    // constant determining the number of frames it takes to print a character. Speed determinator.
    static uint FRAMES_PER_CHAR = 3;
    // chatbox pause amount for inactivating and activating to set name
    static uint FRAMES_PER_NAMECHANGE = 20;
    // original location of the chatbox
    static Vector3 pos;
    // counter until chatbox wakes up. 0 is awake.
    static uint sleepCounter = 0;
    // delegate to be passed to sleep. function to be executed on awake.
    public delegate void onAwake();
    // stored onAwake delegate to be executed. This is used by the sleep function.
    private static onAwake sleepFunc;
    private static bool ready;
   
    // Use this for initialization
    void Start () {
		chatbox = GameObject.Find ("Chatbox");
        charName = chatbox.transform.GetChild(0).gameObject;
		body = chatbox.transform.GetChild (1).gameObject;
        dialog = new Queue<char>();
        pos = chatbox.transform.position;
        ready = true;
    }

    /**
     * returns whether the component is ready to accept more text to be displayed
     */
    public static bool Ready()
    {
        return ready;
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
            throw new UnityException("Not yet ready to accept text");
        // We are now working to the best of our ability to deliver. Please wait.
        ready = false;

        foreach(char c in s)
            dialog.Enqueue(c);
        
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
	
	// Update is called once per frame
	void Update () {

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
            // only ready to accept more text if done printing.
            if (dialog.Count == 0)
                ready = true;
        }

    }
}
