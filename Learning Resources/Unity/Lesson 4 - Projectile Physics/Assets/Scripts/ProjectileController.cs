using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {


	/*--- Veriables ---*/

	private float time;
	private float deleteTime = 10f;


	/*--- Methods ---*/

    void Update() {
        
        // Update Time
        time += Time.deltaTime;

        // Conditionally KYS
        if (time > deleteTime) {
        	Destroy(gameObject);
        }
    }
}
