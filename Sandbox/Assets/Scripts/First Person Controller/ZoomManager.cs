using System.Collections;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class ZoomManager {


	/*--- Variables ---*/

	public FirstPersonViewConfig firstPersonViewConfig;
	private LookInputState lookInputState;

	private Camera camera;
	private bool isRunning;
	private bool isZooming;
	private IEnumerator zoomRoutine;
	private IEnumerator runRoutine;


	/*--- Initialization Method ---*/

	public void initialize(Camera cam, LookInputState state, FirstPersonViewConfig config) {
		camera = cam;
		lookInputState = state;
		firstPersonViewConfig = config;
	}


	/*--- Zoom Methods ---*/

	public void updateZoomState(MonoBehaviour monoBehavior) {

		// Begin Zoom
		if (lookInputState.isZoomClicked && !lookInputState.isZooming && !isRunning) {
			lookInputState.isZooming = true;
			lookInputState.isZoomClicked = false;
			invokeZoomRoutine(monoBehavior, true);

		// Return From Zoom
		} else if (lookInputState.isZooming && (lookInputState.isZoomReleased || isRunning)) {
			lookInputState.isZooming = false;
			lookInputState.isZoomReleased = false;
			invokeZoomRoutine(monoBehavior, false);

		}

		// Disregard Input When Running
		if (isRunning) {
			lookInputState.isZoomClicked = false;
			lookInputState.isZoomReleased = false;
		}
	}

	private void invokeZoomRoutine(MonoBehaviour monoBehavior, bool isBeginningZoom) {

		// Cancel Animations
		if (runRoutine != null) monoBehavior.StopCoroutine(runRoutine);
		if (zoomRoutine != null) monoBehavior.StopCoroutine(zoomRoutine);

		zoomRoutine = ZoomRoutine(isBeginningZoom);
		monoBehavior.StartCoroutine(zoomRoutine);
	}

	IEnumerator ZoomRoutine(bool isBeginningZoom) {

		// Setup Local Variables
		float percent = 0f;
		float smoothPercent = 0f;
		float speed = 1f / firstPersonViewConfig.zoomTransitionDuration;
		float currentFov = camera.fieldOfView;
		float targetFov = isBeginningZoom ? firstPersonViewConfig.zoomFOV : firstPersonViewConfig.defaultFOV;

		// Animate Zoom
		while (percent < 1f) {
			percent += Time.deltaTime * speed;
			smoothPercent = firstPersonViewConfig.zoomCurve.Evaluate(percent);
			camera.fieldOfView = Mathf.Lerp(currentFov, targetFov, smoothPercent);
			yield return null;
		}
	}


	/*--- Run Methods ---*/

	public void beginRunFovAnimation(bool isBeginningRun, MonoBehaviour monoBehavior) {

		// Cancel Animations
		if (zoomRoutine != null) monoBehavior.StopCoroutine(zoomRoutine);
		if (runRoutine != null) monoBehavior.StopCoroutine(runRoutine);

		runRoutine = RunFovRoutine(isBeginningRun);
		monoBehavior.StartCoroutine(runRoutine);
	}

	IEnumerator RunFovRoutine(bool isBeginningRun) {

		// Setup Local Variables
		float percent = 0f;
		float smoothPercent = 0f;
		float duration = isBeginningRun ? firstPersonViewConfig.runTransitionDuration
									 : firstPersonViewConfig.runReturnTransitionDuration;
		float speed = 1f / duration;
		float currentFov = camera.fieldOfView;
		float targetFov = isBeginningRun ? firstPersonViewConfig.runFOV : firstPersonViewConfig.defaultFOV;
		isRunning = isBeginningRun;

		// Animate Run
		while (percent < 1f) {
			percent += Time.deltaTime * speed;
			smoothPercent = firstPersonViewConfig.runCurve.Evaluate(percent);
			camera.fieldOfView = Mathf.Lerp(currentFov, targetFov, smoothPercent);
			yield return null;
		}
	}
}
