using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class SpinController : MonoBehaviour {


	/*--- Variables ---*/

	private float currentTime;
	private float cycleFactor;

    public float cycleLength;


    /*--- Lifecycle Methods ---*/
 
    void Update () {

    	// Update Time
        currentTime += Time.deltaTime;
        cycleFactor = currentTime % cycleLength / cycleLength;

        // Update Rotation
        transform.localRotation = Quaternion.Euler(-90f, 360f * cycleFactor, 0f);
    }
}