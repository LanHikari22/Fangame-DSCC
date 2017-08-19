using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts;

public class CharacterAnimator : MonoBehaviour {

	// clocked used for animations
	uint frameClock = 0;

	/* Blinking Animation */
	// start time reference for blinking animation.
	uint t0_blinking;
	// Two characters won't blank at the same time, set one apart from the other.
	public uint blinkingLag = 0;
	// sprites for 
	Sprite openEyesSprite;
	Sprite closedEyesSprite;

	/* Swapping Animation */
	// start time reference or swapping animation.
	uint t0_swapping;
	// swapping velocity
	int v_swapping;

	// initial x-location of sprite
	float init_x;
	// swapping x-location of sprite, this is where the swap occurs. it's not on screen.
	float swap_x;
	// direction of transition, after swap, it reverses. (-1 -> 1, or 1 -> -1).
	int swapDir;
	// reference to new sprite to be swapped in
	Sprite savedSprite;

	/* Animation Triggering */
	// trigger flags determine what animations should be running at any given instant
	public uint triggerFlags;
	// used to 0store previous state for animations that pause everything else until they're complete
	uint savedTriggerFlags;
	const uint BLINKING = 0x01;
	const uint SWAPPING = 0x02;
	const uint DEFAULT_TRIGGERFLAGS = BLINKING;

	// keeps track of what the currently recorded sprite is, action to be taken if it changes.
	Sprite currSprite;
	// sprite renderer of this gameobject
	SpriteRenderer sr;



	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer>();
		t0_blinking = frameClock = 0;
		init_x = sr.transform.position.x;
		triggerFlags = DEFAULT_TRIGGERFLAGS;
		swapDir = sr.flipX ? -1 : 1;
		swap_x = sr.transform.position.x + swapDir * 390;  // w = 390. at x +- w, sprite is completely out. TODO MAGIC
		v_swapping = swapDir * 30;
	}
	
	// Update is called once per frame
	void Update () {
		// frameClock always advances! overflow, if you must.
		frameClock++;

		// sprite name without the animation tag: "_0" or "_1".
		var spriteName = "";
		if (sr.sprite)
			spriteName = sr.sprite.name.Substring (0, sr.sprite.name.Length - 2);

		// if the currSprite isn't part of the blinking animation
		if (sr.sprite && !(currSprite && (currSprite.name.Equals(spriteName + "_0") || currSprite.name.Equals(spriteName + "_1")))){
				
			// Sprite changed, resync closedEysesSprite and openEyesSprite, resnap t0 for animateBlinking().
			if (sr.sprite.name.IndexOf ("_") == -1)
				throw new MissingReferenceException ("Expected formatted sprite to contain '_' but it didn't.");
			
			// get sprites from resources. They are formatted as such: "<spritesheet>_<name>_e<emotion>_<n>". n=0 being opened eyes, n=1 closed eyes.
			// since Resources.LoadAll() returns an array, the array is searched for the corresponding closed/open eyes variation of the sprite.
			string spritesheet = sr.sprite.name.Substring (0, sr.sprite.name.IndexOf ("_")); // <spritesheet>_<name>_e<emotion>_<n> -> <spritesheet>
			var characterSprites = Resources.LoadAll<Sprite> ("Sprites/" + spritesheet);
			openEyesSprite = DialogCharacter.searchSpriteArray (characterSprites, spriteName + "_1"); // "*_0" or *_1".
			closedEyesSprite = DialogCharacter.searchSpriteArray (characterSprites, sr.sprite.name.Substring (0, sr.sprite.name.Length - 2) + "_0");

			// take new time reference for the blinkingAnimation.
			t0_blinking = frameClock;

			// Since this is a new sprite, swapping must happen, unless this is the very first run. Trigger.
			if (currSprite && triggerFlags != SWAPPING && isNewCharacter()) {
				savedTriggerFlags = triggerFlags;
				triggerFlags = SWAPPING; // Don't execute any other animations for now.
				t0_swapping = frameClock; // We're now starting the swapping animation!
				// save the new sprite, and swap it back with currSprite. We need to perform animation first.
				savedSprite = sr.sprite;
				sr.sprite = currSprite;
			}
				
			// Awaiting next update...
			currSprite = sr.sprite;


		}
			
		// Animations, they only execute if triggered. This is but a frame advance.
		animateBlinking (t0_blinking);
		animateSwapping (t0_swapping);
	}

	/**
	 * toggles current gameobject's sprite "*_0" with its corresponding "*_1"* and vice versa.
	 * Open eyes = 2.2s (132 frames) , closed eyes = 0.3s (18 frames), total animation time = 2.5s (180 frames)
	 * @param t0		initial time the animation started. used to disassociate the clock's state from the animation state
	 */
	void animateBlinking(uint t0){
		// if flag is OFF, skip.
		if ((triggerFlags & BLINKING) == 0) // TODO == order of operation higher than &????
			return;

		uint animationTime = 180;
		uint blinkTime = 18;
		uint t = (frameClock - t0 + blinkingLag) % animationTime; // 60 frames = 1 second
		if (t < animationTime - blinkTime)
			sr.sprite = openEyesSprite;
		else
			sr.sprite = closedEyesSprite;
	}

	/**
	 * Animates swapping animation CHARACTERS. 
	 * This should not occur if the sprite change was not a <name> change in the following format:
	 * <spritesheet>_<name>_e<emotion>_<n>.
	 */
	void animateSwapping(uint t0){
		// Check if triggered: nothing else can be triggered when swapping
		if (triggerFlags != SWAPPING)
			return;

		// specify upper limit of transition based on direction.
		bool reachedSwapPoint = (swapDir == -1) ? sr.transform.position.x <= swap_x : sr.transform.position.x >= swap_x; 
		// back to the beginning! Again, the bounds are dependent on the swap direction.
		bool back2square1 = (frameClock != t0) && (swapDir==-1 ? sr.transform.position.x >= init_x : sr.transform.position.x <= init_x);


		if (reachedSwapPoint) {
			// perform swap and reverse transition direction
			sr.sprite = savedSprite;
			v_swapping *= -1;
		}

		// frame transition
		var loc = sr.transform.position;
		loc.x += v_swapping;
		sr.transform.position = loc;

		// Done swapping. Restore saved trigger state
		if (back2square1) {
			// make sure we're at +- init_x
			loc = sr.transform.position;
			loc.x = init_x;
			sr.transform.position = loc;
			// restore v_swapping to its original state for the next swap
			v_swapping *= -1;
			// restore back old animation triggers
			triggerFlags = savedTriggerFlags;
		}
	}


    public static bool NoSwappingIsHappening()
    {
        var lc = GameObject.Find("L-Character").GetComponent<CharacterAnimator>();
        var rc = GameObject.Find("R-Character").GetComponent<CharacterAnimator>();

        return (lc.triggerFlags != SWAPPING) && (rc.triggerFlags != SWAPPING);
    }

    /**
	 * Checks whether sr.sprite is a different character from currSprite in accordinace to the naming format:
	 * "<spritesheet>_<name>_e<emotion>_<n>".
	 * It should be the same,if "<spritesheet>_<name>" is identical.
	 * @return true if sr.sprite has the same character as currSprite. false otherwise.
	 */
    bool isNewCharacter()
    {
        bool sameCharacter = true; // innocent until proven guilty
        var sn = sr.sprite.name;
        var snFirst_ = sn.IndexOf("_");
        var snSecond_ = sn.IndexOf("_", snFirst_ + 1);
        var rFirst_ = currSprite.name.IndexOf("_"); // first '_' in recordedSprite
        var rSecond_ = currSprite.name.IndexOf("_", rFirst_ + 1);

        // If spritesheet is different, it's guaranteed to be a different character.
        // <spritesheet>_<name>_e<emotion>_<n> -> <spritesheet>
        string spritesheet = sn.Substring(0, snFirst_);
        string rSpritesheet = currSprite.name.Substring(0, rFirst_); // [<spritesheet>]_<name>_e<emotion>_<n>
        sameCharacter &= spritesheet == rSpritesheet;

        // if <name> is different, it's also a different character.
        // <spritesheet>_<name>_e<emotion>_<n> -> <name>
        string name = sr.sprite.name.Substring(snFirst_ + 1, snSecond_ - snFirst_); // <spritesheet>_<name>_e<emotion>_<n> -> <spritesheet>
        string rName = currSprite.name.Substring(rFirst_ + 1, rSecond_ - rFirst_); //			   01234567890123456789012345678901234
        sameCharacter &= name == rName;

        return !sameCharacter; // output = True: same spreadsheet, same name.
    }

}
