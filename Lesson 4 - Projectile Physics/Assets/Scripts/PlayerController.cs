using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour {


	/*--- Variables ---*/

	public CharacterController controller;
	
	public float lookSensitivity = 100f;
	public Transform mainCamera;
	public float playerSpeed = 9f;
	public float playerGravity = -9.81f;
	public Transform groundCheck;
	public float groundDistance = 0.1f;
	public LayerMask groundMask;
	public float jumpHeight = 2.0f;

	private float xRotation = 0f;
	private Vector3 fallVelocity;
	private bool isGrounded;


	/*--- Methods ---*/

    void Start() {

    	// Lock & Hide Cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {

        // Get Look Input
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        // Calculate Vertical Rotation
        xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply Rotations
        mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

    	// Perform Ground Check
    	isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

    	// Conditionally Reset Velocity
    	if (isGrounded && fallVelocity.y < 0) {
    		fallVelocity.y = -1f;
    	}

        // Get Move Input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate Move Direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Account for Diagonal Motion
        if (moveX != 0f && moveZ != 0f) {
        	move *= 0.70710678f; // sin(45), or the 45 degree vector strength. 
        }

        // Move Player
        controller.Move(move * playerSpeed * Time.deltaTime);

        // Conditionally Perform Jump
        if (Input.GetButtonDown("Jump") && isGrounded) {
        	fallVelocity.y = Mathf.Sqrt(jumpHeight * -2f * playerGravity);
        }

        // Calculate Fall
        fallVelocity.y += playerGravity * Time.deltaTime;

        // Apply Fall
        controller.Move(fallVelocity * Time.deltaTime);
    }
}
