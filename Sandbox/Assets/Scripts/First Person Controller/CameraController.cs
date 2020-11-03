using System.Collections;
using System;
using NaughtyAttributes;
using UnityEngine;

/* CameraController
 *
 * This class encapsulates the view logic of the First Person Character. It
 * drives player looking and zooming by translating raw input (as read from
 * LookInputState) to rotation and FOV changes in the FPS camera.
 */

public class CameraController : MonoBehaviour {


	/*--- Variables ---*/

	private const float MOUSE_LOOK_BASE_MULTIPLIER = 5f;

	[SerializeField] private LookInputState lookInputState = null;
	[SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
	[SerializeField] [ShowIf("NeverShow")] private GamepadLookAdapter gamepadLookAdapter = null;
	[SerializeField] [ShowIf("NeverShow")] private ZoomManager zoomManager = null;
	[SerializeField] [ShowIf("NeverShow")] private SwayManager swayManager = null;

	private float yaw;
	private float pitch;
	private Transform cameraPivotTransform;
	new private Camera camera;

	private IEnumerator horizAccRoutine;
	private IEnumerator vertAccRoutine;


	/*--- Lifecycle Methods ---*/

	void Awake() {
		getComponents();
		initializeValues();
		initializeComponents();
		initializeCamera();
		lockCursor();
	}

	void LateUpdate() {
		updateLookState();
		updateZoomState();
	}


	/*--- Public Methods ---*/

	public void updateCameraSway(Vector3 inputVector, float inputRawX) {
		swayManager.updateSway(inputVector, inputRawX);
	}

	public void beginRunFovAnimation(bool isBeginningRun) {
		zoomManager.beginRunFovAnimation(isBeginningRun, this);
	}


	/*--- Private Setup Methods ---*/

	private void getComponents() {
		cameraPivotTransform = transform.GetChild(0).transform;
		camera = GetComponentInChildren<Camera>();
	}

	private void initializeValues() {
		yaw = transform.eulerAngles.y;
	}

	private void initializeComponents() {
		gamepadLookAdapter = new GamepadLookAdapter(this, firstPersonViewConfig, lookInputState);
		zoomManager.initialize(camera, lookInputState, firstPersonViewConfig);
		swayManager.initialize(camera.transform, firstPersonViewConfig);
	}

	private void initializeCamera() {
		camera.fieldOfView = firstPersonViewConfig.defaultFOV;
	}

	private void lockCursor() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}


	/*--- Private Look Methods ---*/

	private void updateLookState() {
		if (lookInputState.isStickAiming) {

			// Process Stick Input
			calculateStickRotation();
			applyStickRotation();

		} else {

			// Process Mouse Input
			calculateMouseRotation();
			applyMouseRotation();
		}
	}


	/*--- Stick Look Methods ---*/

	private void calculateStickRotation() {
		Vector2 rotation = gamepadLookAdapter.calculatePlayerRotation();
		yaw += rotation.x;
		pitch -= rotation.y;
		pitch = Mathf.Clamp(
			pitch,
			firstPersonViewConfig.verticalAngleClamp.x,
			firstPersonViewConfig.verticalAngleClamp.y
		);
	}

	private void applyStickRotation() {
		transform.eulerAngles = new Vector3(0f, yaw, 0f);
		cameraPivotTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
	}


	/*--- Mouse Look Methods ---*/

	private void calculateMouseRotation() {
		yaw += lookInputState.inputVector.x
		       * MOUSE_LOOK_BASE_MULTIPLIER
		       * firstPersonViewConfig.lookSensitivityMouse.x
		       * Time.deltaTime;
		pitch -= lookInputState.inputVector.y
		         * MOUSE_LOOK_BASE_MULTIPLIER
		         * firstPersonViewConfig.lookSensitivityMouse.y
		         * Time.deltaTime;

		pitch = Mathf.Clamp(
			pitch,
			firstPersonViewConfig.verticalAngleClamp.x,
			firstPersonViewConfig.verticalAngleClamp.y
		);
	}

	private void applyMouseRotation() {
		transform.eulerAngles = new Vector3(0f, yaw, 0f);
		cameraPivotTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
	}

	private void updateZoomState() {
		zoomManager.updateZoomState(this);
	}
}
