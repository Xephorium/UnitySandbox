using System.Collections;
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
	private const float STICK_LOOK_HORIZ_MULTIPLIER = 1.26f;
	private const float DIAGONAL_VECTOR_MEGNITUDE = 0.7071f;

	// TODO - Extract to Config
	private const float ACCELERATION_ZONE = 0.95f;
	private const float ACCELERATION_LENGTH = 0.8f;
	private const float HORIZ_ACC_FACTOR = 2f;
	private const float VERT_ACC_FACTOR = 1.7f;
	private const float ZOOM_FACTOR = 0.8f;

	[SerializeField] private LookInputState lookInputState = null;
	[SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
	[SerializeField][ShowIf("NeverShow")] private ZoomManager zoomManager = null;
	[SerializeField][ShowIf("NeverShow")] private SwayManager swayManager = null;

	private float yaw;
	private float pitch;
	private float currentHorizTurnSpeed = 0f;
	private float currentVertTurnSpeed = 0f;
	private float horizAccZone;   // Value [-1, 0, 1] representing current zone [left, none, right]
	private float vertAccZone;    // [down, none, up]
	private float horizAccFactor; // Float [-1, 1] representing current acceleration factor
	private float vertAccFactor;
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
		horizAccFactor = 0f;
		horizAccZone = 0f;
		vertAccFactor = 0f;
		vertAccFactor = 0f;
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
		if (lookInputState.isStickAiming) {

			// Process Stick Input
			updateHorizAccFactor();
			updateVertAccFactor();
			calculateHorizStickRotation();
			calculateVertStickRotation();
			applyStickRotation();

		} else {

			// Process Mouse Input
			resetLookAcc();
			calculateMouseRotation();
			applyMouseRotation();
		}
	}


	/*--- Stick Look Methods ---*/

	private void updateHorizAccFactor() {

		// Update Horiz State
		// TODO - Radial Acceleration Zone Calculation
		if (lookInputState.inputVector.x > ACCELERATION_ZONE) {

			// Handle Right Acceleration
			if (horizAccZone == -1f) {

				// Cancel Left Acceleration, Begin Right Acceleration
				cancelHorizAccRoutine();
				horizAccZone = 1f;
				horizAccFactor = 0f;
				invokeHorizAccRoutine();

			} else if (horizAccZone == 0f) {

				// Begin Right Acceleration
				horizAccZone = 1f;
				invokeHorizAccRoutine();
			}

		} else if (lookInputState.inputVector.x < -ACCELERATION_ZONE) {

			// Handle Left Acceleration
			if (horizAccZone == 1f) {

				// Cancel Right Acceleration, Begin Left Acceleration
				cancelHorizAccRoutine();
				horizAccZone = -1f;
				horizAccFactor = 0f;
				invokeHorizAccRoutine();

			} else if (horizAccZone == 0f) {

				// Begin Left Acceleration
				horizAccZone = -1f;
				invokeHorizAccRoutine();

			}

		} else {

			// Cancel Horizontal Acceleration
			horizAccZone = 0f;
			horizAccFactor = 0f;
		}
	}

	private void cancelHorizAccRoutine() {
		if (horizAccRoutine != null) horizAccRoutine = null;
	}

	private void invokeHorizAccRoutine() {
		horizAccRoutine = HorizAccRoutine();
		StartCoroutine(horizAccRoutine);
	}

	protected virtual IEnumerator HorizAccRoutine() {

		// Setup Local Variables
		float percent = 0f;
		float smoothPercent = 0f;
		float speed = 1f / ACCELERATION_LENGTH;
		float startSpeed = currentHorizTurnSpeed;
		float endSpeed = STICK_LOOK_BASE_MULTIPLIER
		                 * STICK_LOOK_HORIZ_MULTIPLIER
		                 * firstPersonViewConfig.lookSensitivityStick.x
		                 * HORIZ_ACC_FACTOR;

		// Animate Acceleration Factor
		while (percent < 1f) {
			percent += Time.deltaTime * speed;
			horizAccFactor = Mathf.Lerp(startSpeed, endSpeed, firstPersonViewConfig.stickTurnAcceleration.Evaluate(percent));
			yield return null;
		}
	}

	private void calculateHorizStickRotation() {

		// Local Variables
		float horizTurn;
		float horizSign = lookInputState.inputVector.x >= 0f ? 1f : -1f;
		float zoomFactor = lookInputState.isZooming ? ZOOM_FACTOR : 1f;
		currentHorizTurnSpeed = STICK_LOOK_BASE_MULTIPLIER
		                        * STICK_LOOK_HORIZ_MULTIPLIER
		                        * firstPersonViewConfig.lookSensitivityStick.x;

		// Calculate Horiz Look/Turn Speed
		if (horizAccZone == 0f || lookInputState.isZooming) {
			horizTurn = horizSign
			            * currentHorizTurnSpeed
			            * firstPersonViewConfig.stickLookAcceleration.Evaluate(Mathf.Abs(lookInputState.inputVector.x))
			            * zoomFactor
			            * Time.deltaTime;
		} else {
			horizTurn = horizSign
			            * horizAccFactor
			            * Time.deltaTime;
		}

		// Record Rotation
		yaw += horizTurn;
	}

	private void updateVertAccFactor() {

		// Update Horiz State
		// TODO - Radial Acceleration Zone Calculation
		if (lookInputState.inputVector.y > ACCELERATION_ZONE) {

			// Handle Up Acceleration
			if (vertAccZone == -1f) {

				// Cancel Down Acceleration, Begin Up Acceleration
				cancelVertAccRoutine();
				vertAccZone = 1f;
				vertAccFactor = 0f;
				invokeVertAccRoutine();

			} else if (vertAccZone == 0f) {

				// Begin Up Acceleration
				vertAccZone = 1f;
				invokeVertAccRoutine();
			}

		} else if (lookInputState.inputVector.y < -ACCELERATION_ZONE) {

			// Handle Down Acceleration
			if (vertAccZone == 1f) {

				// Cancel Up Acceleration, Begin Down Acceleration
				cancelVertAccRoutine();
				vertAccZone = -1f;
				vertAccFactor = 0f;
				invokeVertAccRoutine();

			} else if (vertAccZone == 0f) {

				// Begin Doen Acceleration
				vertAccZone = -1f;
				invokeVertAccRoutine();

			}

		} else {

			// Cancel Vertical Acceleration
			vertAccZone = 0f;
			vertAccFactor = 0f;
		}
	}

	private void cancelVertAccRoutine() {
		if (vertAccRoutine != null) vertAccRoutine = null;
	}

	private void invokeVertAccRoutine() {
		vertAccRoutine = VertAccRoutine();
		StartCoroutine(vertAccRoutine);
	}

	protected virtual IEnumerator VertAccRoutine() {

		// Setup Local Variables
		float percent = 0f;
		float smoothPercent = 0f;
		float speed = 1f / ACCELERATION_LENGTH;
		float startSpeed = currentVertTurnSpeed;
		float endSpeed = STICK_LOOK_BASE_MULTIPLIER
		                 * firstPersonViewConfig.lookSensitivityStick.y
		                 * VERT_ACC_FACTOR;

		// Animate Acceleration Factor
		while (percent < 1f) {
			percent += Time.deltaTime * speed;
			vertAccFactor = Mathf.Lerp(startSpeed, endSpeed, firstPersonViewConfig.stickTurnAcceleration.Evaluate(percent));
			yield return null;
		}
	}

	private void calculateVertStickRotation() {

		// Local Variables
		float vertTurn;
		float vertSign = lookInputState.inputVector.y >= 0f ? 1f : -1f;
		float zoomFactor = lookInputState.isZooming ? ZOOM_FACTOR : 1f;
		currentVertTurnSpeed = STICK_LOOK_BASE_MULTIPLIER
		                       * firstPersonViewConfig.lookSensitivityStick.y;

		// Calculate Vert Look/Turn Speed
		if (vertAccZone == 0f || lookInputState.isZooming) {
			vertTurn = vertSign
			           * currentVertTurnSpeed
			           * firstPersonViewConfig.stickLookAcceleration.Evaluate(Mathf.Abs(lookInputState.inputVector.y))
			           * zoomFactor
			           * Time.deltaTime;
		} else {
			vertTurn = vertSign
			           * vertAccFactor
			           * Time.deltaTime;
		}

		// Record Rotation
		pitch -= vertTurn;

		// Clamp Vertical Rotation
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

	private void resetLookAcc() {
		cancelHorizAccRoutine();
		horizAccZone = 0f;
		horizAccFactor = 0f;
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
