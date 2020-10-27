using UnityEngine;
using NaughtyAttributes;


[System.Serializable]
public class SwayManager {


	/*--- Variables ---*/

	private FirstPersonViewConfig firstPersonViewConfig;

	private Transform cameraTransform;
	private float scrollSpeed;
	private float amountXThisFrame;
	private float amountXPreviousFrame;
	private bool isDifferentDirection;


	/*--- Methods ---*/

	public void initialize(Transform transform, FirstPersonViewConfig config) {
		cameraTransform = transform;
		firstPersonViewConfig = config;
	}

	public void updateSway(Vector3 inputVector, float inputRawX) {
		float amountX = inputVector.x;
		amountXThisFrame = inputRawX;

		// If we have input
		if (inputRawX != 0f) {

			// Account for direction change
			if (amountXThisFrame != amountXPreviousFrame && amountXPreviousFrame != 0)
				isDifferentDirection = true;
			float speedMultiplier = isDifferentDirection ? firstPersonViewConfig.changeDirectionMultiplier : 1f;

			scrollSpeed += (amountX * firstPersonViewConfig.swaySpeed * Time.deltaTime * speedMultiplier);

		} else {

			// Conditionally reset direction change
			if (amountXThisFrame == amountXPreviousFrame) isDifferentDirection = false;

			scrollSpeed = Mathf.Lerp(scrollSpeed, 0f, Time.deltaTime * firstPersonViewConfig.returnSpeed);
		}

		scrollSpeed = Mathf.Clamp(scrollSpeed, -1f, 1f);
		float swayFinalAmount;

		if (scrollSpeed < 0f)
			swayFinalAmount = firstPersonViewConfig.swayCurve.Evaluate(-scrollSpeed) * firstPersonViewConfig.swayAmount;
		else
			swayFinalAmount = firstPersonViewConfig.swayCurve.Evaluate(scrollSpeed) * -firstPersonViewConfig.swayAmount;

		Vector3 _swayVector;
		_swayVector.z = swayFinalAmount;

		cameraTransform.localEulerAngles = new Vector3(cameraTransform.localEulerAngles.x, cameraTransform.localEulerAngles.y, _swayVector.z);

		amountXPreviousFrame = amountXThisFrame;
	}
}
