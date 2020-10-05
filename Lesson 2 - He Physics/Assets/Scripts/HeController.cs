using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* Lesson 2 - He Physics
 * 
 * This script is an update to our simple character controller. It now uses generic
 * input for player movement (controller support!), locks the cursor, and rotates
 * the player's view left/right based on mouse movement.
 *
 * This incomplete script will be our last attempt at building a player controller
 * from scratch, but has provided many lessons on what we get from the Character
 * Controller Component for free. :)
 *
 * Our Upcoming Approach:
 * https://www.youtube.com/watch?v=_QajrabyTJc&list=PLQ_viMC2OQB900E4Bl5nAYh_6jui8Pibn&index=1
 *
 * Other Unity Notes
 *   - Collider Component: Necessary for every object with collision in a Unity scene.
 *                         Tells the physics engine about the object's bounding box.
 *                         For custom meshes, we use "Mesh Collider" with "convex"
 *                         checked.
 *   - Rigidbody Component: Gives proper physics to the object it's attached to!
 *                          Gravity, tumbling, speed falloff, angular drag, the works.
 *                          Used for game props and physics-enabled scenery.
 *   - Character Controller Component: Built to dramatically simplify character control.
 *                                     Used in conjunction with movement and view scripting
 *                                     to give us slope traversal, stepping, collision, etc.
 *
 */

public class HeController : MonoBehaviour {


	/*--- Variables ---*/

	public float speed = 0.1f;

	// Note: This variable is used to track our player's left/right rotation as a
	//       float angle.
	private float rotationY = 0f;


	/*--- Methods ---*/

	// Note: Called once before the first frame.
	void Start() {

		// Lock & Hide Cursor
    	Cursor.lockState = CursorLockMode.Locked;
	}

	// Note: In this method, which is called at a regular (fixed) interval, we
	//       detect player input as generic directions and move He accordingly.
    void FixedUpdate() {

    	// Get Player Movement Input
    	float transformZ = Input.GetAxis("Vertical");
    	float transformX = Input.GetAxis("Horizontal");

    	// Get Player Look Input
    	float rotateY = Input.GetAxis("Mouse X");

    	// Move He
    	transform.position = new Vector3(
    		transform.position.x + (transformX * speed), // + transformLeftRight,
    		transform.position.y,
    		transform.position.z + (transformZ * speed)  // + transformForwardBackward
    	);

    	// Update Rotation Variable
    	rotationY += rotateY;

    	// Rotate He
    	transform.localRotation = Quaternion.Euler(-90f, rotationY, 0f);
    }
}
