using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ElevatorController : MonoBehaviour {


	/*--- Variables ---*/

	private float currentTime;
	private float cycleFactor;
	private Vector3 initialPosition;

    public float cycleLength;
    public float riseHeight;


    /*--- Lifecycle Methods ---*/


    void Start() {
    	initialPosition = transform.position;
    }
 
    void Update () {

    	// Update Time
        currentTime += Time.deltaTime;
        cycleFactor = currentTime % cycleLength / cycleLength;

        // Update Rotation
        transform.position = new Vector3(
        	initialPosition.x,
        	initialPosition.y + Mathf.Abs(0.5f - cycleFactor) * (riseHeight * 2f),
        	initialPosition.z
        );
    }
}