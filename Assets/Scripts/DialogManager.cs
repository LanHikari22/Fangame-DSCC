using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using Assets.Scripts;
using Assets.Scripts.Dialog_Nodes;

public class DialogManager : MonoBehaviour {

    // queue of dialogNodes to be used by interpreter
    Queue<DialogNode> dialogNodes;
    // interpreter to await commands from every frame.
    DialogInterpreter interpreter;
    

	// Use this for initialization
	void Start () {
		
		GameObject[] chars = new GameObject[2];
		chars [0] = GameObject.Find ("L-Character");
		chars [1] = GameObject.Find ("R-Character");

        //Load xml to be handled by parser and obtain DialogNode queue
        var xml = Resources.Load("Dialog/DS-Scene1").ToString();
        //var xml = Resources.Load("Dialog/test").ToString();

        XDocument xdoc = XDocument.Parse(xml);
        dialogNodes = DialogParser.Parse(xdoc.Element("DSCC_Dialog"));

        // load DialogNode queue into the interpreter.
        interpreter = new DialogInterpreter(dialogNodes);		

	}
	
	// Update is called once per frame
	void Update () {
        // Ensure the interpreter's Update() method runs once every frame.
        interpreter.Update();
        
        // TODO perhaps listen to the interpreter? it will have to make higher level scene calls.
	}
}
