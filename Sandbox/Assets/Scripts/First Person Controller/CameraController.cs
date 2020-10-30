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
	private const float STICK_LOOK_BASE_MULTIPLIER = 65f;
	private const float STICK_LOOK_HORIZ_MULTIPLIER = 1.47f;

	[SerializeField] private LookInputState lookInputState = null;
	[SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
	[SerializeField][ShowIf("NeverShow")] private ZoomManager zoomManager = null;
	[SerializeField][ShowIf("NeverShow")] private SwayManager swayManager = null;

	private float yaw;
	private float pitch;
	private Transform cameraPivotTransform;
	new private Camera camera;


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

	public void updateRunState(bool isReturningToWalk) {
		zoomManager.updateRunState(isReturningToWalk, this);
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
		Debug.Log(lookInputState.isStickAiming);
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

	private void calculateStickRotation() {
		yaw += lookInputState.inputVector.x
		       * STICK_LOOK_BASE_MULTIPLIER
		       * STICK_LOOK_HORIZ_MULTIPLIER
		       * firstPersonViewConfig.lookSensitivityStick.x
		       * Time.deltaTime;
		pitch -= lookInputState.inputVector.y
		         * STICK_LOOK_BASE_MULTIPLIER
		         * firstPersonViewConfig.lookSensitivityStick.y
		         * Time.deltaTime;

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
