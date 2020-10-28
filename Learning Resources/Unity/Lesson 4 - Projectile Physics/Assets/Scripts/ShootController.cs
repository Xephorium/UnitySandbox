using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour {


	/*--- Variables ---*/

	public GameObject projectile;
	public Transform camera;
	public float speed = 10f;

	private float time = 0f;
	private float shootDelay = 0.2f;
	private float lastShoot = 0f;



	/*--- Methods ---*/

    void Update() {

    	// Update Time
    	time += Time.deltaTime;
     
    	// Get Fire Input
    	float shoot = Input.GetAxis("Fire1");

    	// Shoot Projectile
    	if (shoot == 1f && time > lastShoot + shootDelay) {

    		// Fire Projectile
    		GameObject newProjectile = Instantiate(projectile, camera.transform.position + (camera.transform.forward * 2f), Quaternion.identity);

    		// Give Projectile Velocity
    		(newProjectile.GetComponent(typeof(Rigidbody)) as Rigidbody).velocity = camera.transform.forward * speed;

    		// Update Last Shoot
    		lastShoot = time;
    	}

    }
}
