using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* GamepadLookAdapter
 *
 * This class contains the logic required to translate analog toggle stick
 * input to satisfying first person look motion. It does this by treating
 * input differently based on which of two zones it activates: "Look" or
 * "Turn".
 * 
 *               _.-""""-._
 *             .'\        /`.
 *            /   :      :   \
 *           |   /        \   |
 *     Turn ---> :  Look   : <--- Turn
 *           |   \        /   |
 *            \   :      :   /
 *             `./        \.'
 *                `-....-'
 *
 * "Look" simply rotates the camera based on an acceleration curve.
 * This area makes up the majority of the stick. However, input of greater
 * magnitude than config.stickTurnThreshold along the right and left edges
 * of the stick assumes the player's intention to "Turn" and accelerates
 * the player's rotation. The turn strength is also clamped to a radial
 * falloff, where the turn speed decreases as input strays from the
 * horizontal axis.
 *
 * I derived this approach from a YouTube comment. :)
 */

public class GamepadLookAdapter {


	/*--- Variables ---*/

	private const float STICK_LOOK_BASE_MULTIPLIER = 80f;
	private const float STICK_LOOK_HORIZ_MULTIPLIER = 1.17f;
	private const float STICK_LOOK_VERT_MULTIPLIER = 0.85f;
	private const float STICK_ZOOM_SENSITIVITY = 0.6f;

	private MonoBehaviour parent = null;
	private FirstPersonViewConfig viewConfig = null;
	private LookInputState lookInput = null;

	private Vector2 input;
	private float turnAccelerationStrength;
	private float turnSpeed; // Float [0, 1] representing gradually increasing factor

	private IEnumerator turnSpeedRoutine;


	/*--- Constructor ---*/

	public GamepadLookAdapter(
		MonoBehaviour monoBehavior,
		FirstPersonViewConfig config,
		LookInputState lookInputState) {
		parent = monoBehavior;
		viewConfig = config;
		lookInput = lookInputState;

		turnAccelerationStrength = STICK_LOOK_BASE_MULTIPLIER
								   * Mathf.Clamp(viewConfig.stickTurnAccelerationStrength - 1f, 0f, 1000f);
		turnSpeed = 0f;
	}


	/*--- Public Methods ---*/

	public Vector2 calculatePlayerRotation() {
		input = lookInput.inputVector;

		Vector2 lookRotation = calculateLookRotation();

		updateTurnSpeed();
		Vector2 turnRotation = calculateTurnRotation();

		return (lookRotation + turnRotation) * Time.deltaTime;
	}


	/*--- Private Look Methods ---*/

	private Vector2 calculateLookRotation() {

		// Setup Local Variables
		Vector2 signedNormalVector = input.normalized;
		Vector2 aspectRatioMultiplier = new Vector2(STICK_LOOK_HORIZ_MULTIPLIER, STICK_LOOK_VERT_MULTIPLIER);
		Vector2 stickSensitivity = viewConfig.lookSensitivityStick;
		float interpolatedSpeed = viewConfig.stickLookAcceleration.Evaluate(input.magnitude);
		float zoomFactor = lookInput.isZooming ? STICK_ZOOM_SENSITIVITY : 1f;

		// Return Look Speed
		return signedNormalVector
		       * STICK_LOOK_BASE_MULTIPLIER
		       * aspectRatioMultiplier
		       * stickSensitivity
		       * interpolatedSpeed
		       * zoomFactor;
	}


	/*--- Private Turn Methods ---*/

	private Vector2 calculateTurnRotation() {

		// Setup Local Variables
		float lookHorizSign = input.x < 0f ? -1f : 1f;
		float interpolatedTurnSpeed = viewConfig.stickTurnAcceleration.Evaluate(turnSpeed);
		float zoomFactor = lookInput.isZooming ? 0f : 1f;

		// Return Turn Speed
		return (new Vector2(lookHorizSign * 1f, 0f))
			   * interpolatedTurnSpeed
			   * turnAccelerationStrength
		       * zoomFactor;
	}

	private bool isInTurnZone() {

		// Look magnitude must be > viewConfig.stickTurnThreshold.
		bool magnitudeCheck = Mathf.Abs(input.x) >= Mathf.Abs((input.normalized * viewConfig.stickTurnThreshold).x);

		// Look angle must be < viewConfig.stickTurnFalloffAngle.
		float angle = Mathf.Abs(Mathf.Atan(input.y / input.x) * Mathf.Rad2Deg);
		bool angleCheck = angle <= viewConfig.stickTurnFalloffAngle;

		return magnitudeCheck && angleCheck;
	}

	private float getRadialFalloff() {

		// Turn experiences radial falloff as input strays from vector (1, 0).
		float angle = Mathf.Abs(Mathf.Atan(input.y / input.x) * Mathf.Rad2Deg);
		float radialPercent = Mathf.Clamp(1f - (angle / viewConfig.stickTurnFalloffAngle), 0f, 1f);
		float radialFalloff = viewConfig.stickTurnFalloff.Evaluate(TypeUtility.getValidFloat(radialPercent));

		return radialFalloff;
	}

	private void updateTurnSpeed() {

		if (isInTurnZone()) {

			float turnFactor = getRadialFalloff();
			if (turnFactor > turnSpeed && turnSpeedRoutine == null) {

				// Begin New Turn Speed Routine
				invokeTurnSpeedRoutine(0f);


			} else if (turnSpeed > turnFactor) {

				// Clamp TurnSpeed by Radial Falloff
				cancelTurnSpeedRoutine();
				turnSpeed = turnFactor;
				invokeTurnSpeedRoutine(turnSpeed);
			
			} else {

				// Already Running Routine, do Nothing
			}

		} else {

			// Reset Turn Speed
			cancelTurnSpeedRoutine();
			turnSpeed = 0f;
		}
	}

	private void cancelTurnSpeedRoutine() {
		if (turnSpeedRoutine != null) turnSpeedRoutine = null;
	}

	private void invokeTurnSpeedRoutine(float startingPercent) {
		turnSpeedRoutine = TurnSpeedRoutine(startingPercent);
		parent.StartCoroutine(turnSpeedRoutine);
	}

	protected virtual IEnumerator TurnSpeedRoutine(float startingPercent) {

		// Setup Local Variables
		float percent = startingPercent;
		float speed = 1f / viewConfig.stickTurnAccelerationLength;

		// Animate Acceleration Factor
		while (percent < 1f) {
			percent += Time.deltaTime * speed;
			turnSpeed = viewConfig.stickTurnAcceleration.Evaluate(percent);
			yield return null;
		}
	}
}
