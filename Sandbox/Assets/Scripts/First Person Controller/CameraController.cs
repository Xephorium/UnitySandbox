﻿using NaughtyAttributes;
using UnityEngine;


public class CameraController : MonoBehaviour {


	/*--- Variables ---*/

	[Space, Header("Data")]
	[SerializeField] private LookInputState lookInputState = null;
	[SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
	[SerializeField][ShowIf("NeverShow")] private ZoomManager zoomManager = null;
	[SerializeField][ShowIf("NeverShow")] private SwayManager swayManager = null;

	private float yaw;
	private float pitch;
	private float desiredYaw;
	private float desiredPitch;
	private Transform pitchTranform;
	private Camera camera;


	/*--- Lifecycle Methods ---*/

	void Awake() {
		getComponents();
		initializeValues();
		initializeComponents();
		lockCursor();
	}

	void LateUpdate() {
		calculateRotation();
		smoothRotation();
		applyRotation();
		handleZoom();
	}


	/*--- Public Methods ---*/

	public void HandleSway(Vector3 inputVector, float inputRawX) {
		swayManager.updateSway(inputVector, inputRawX);
	}

	public void ChangeRunFOV(bool isReturningToWalk) {
		zoomManager.updateRunFov(isReturningToWalk, this);
	}


	/*--- Private Methods ---*/

	void getComponents() {
		pitchTranform = transform.GetChild(0).transform;
		camera = GetComponentInChildren<Camera>();
	}

	void initializeValues() {
		yaw = transform.eulerAngles.y;
		desiredYaw = yaw;
	}

	void initializeComponents() {
		zoomManager.initialize(camera, lookInputState, firstPersonViewConfig);
		swayManager.initialize(camera.transform, firstPersonViewConfig);
	}

	void calculateRotation() {
		desiredYaw += lookInputState.inputVector.x * firstPersonViewConfig.lookSensitivity.x * Time.deltaTime;
		desiredPitch -= lookInputState.inputVector.y * firstPersonViewConfig.lookSensitivity.y * Time.deltaTime;

		desiredPitch = Mathf.Clamp( desiredPitch, firstPersonViewConfig.verticalAngleClamp.x, firstPersonViewConfig.verticalAngleClamp.y);
	}

	void smoothRotation() {
		yaw = desiredYaw;
		pitch = desiredPitch;
	}

	void applyRotation() {
		transform.eulerAngles = new Vector3(0f, yaw, 0f);
		pitchTranform.localEulerAngles = new Vector3(pitch, 0f, 0f);
	}

	void handleZoom() {
		if (lookInputState.isZoomClicked || lookInputState.isZoomReleased)
			zoomManager.updateZoomFov(this);
	}

	void lockCursor() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}
