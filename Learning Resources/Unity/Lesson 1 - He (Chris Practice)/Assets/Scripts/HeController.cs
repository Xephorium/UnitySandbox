using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* Lesson 1 - He
 * 
 * This script is a simple character controller. It detects player input and moves
 * the player character on a 2D plane.
 */

public class HeController : MonoBehaviour {


	/*--- Variables ---*/

	// Note: Prefixing our variable with "public" means it is available to Unity's
	//       UI, for quick editing outside our script.
	public float speed = 0.1f;


	/*--- Methods ---*/

	// Note: In this method, which is called once upon project load, we do nothing.
	//       Since we don't use it, it doesn't need to be included in our project,
	//       but I've done so for clarity and future reference.
	void Start() { }

	// Note: In this method, which is called once per frame, we do nothing.
	//       Since we don't use it, it doesn't need to be included in our project,
	//       but I've done so for clarity and future reference.
	void Update() { }


	// Note: In this method, which is called at a regular (fixed) interval, we
	//       detect player input as specific keypresses and move He accordingly.
    void FixedUpdate() {

    	// Get Player Input
    	bool movingForward = Input.GetKey("w");
    	bool movingBackward = Input.GetKey("s");
    	bool movingLeft = Input.GetKey("a");
    	bool movingRight = Input.GetKey("d");

    	// Calcululate Directional Transforms (Movement)
    	float transformForwardBackward = 0f;
    	if (movingForward) {
    		transformForwardBackward = speed;
    	} else if (movingBackward) {
    		transformForwardBackward = -speed;
    	}
    	float transformLeftRight = 0f;
    	if (movingLeft) {
    		transformLeftRight = -speed;
    	} else if (movingRight) {
    		transformLeftRight = speed;
    	}

    	// Move He
    	transform.position = new Vector3(
    			transform.position.x + transformLeftRight,
    			transform.position.y,
    			transform.position.z + transformForwardBackward
    		);

    	// Set He Rotation
   //  	transform.eulerAngles = new Vector3(
			// 	-90,
			// 	0,
			// 	0
			// );
    }
}
