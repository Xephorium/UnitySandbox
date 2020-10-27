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

	private IEnumerator fovZoomRoutine;
	private IEnumerator fovRunRoutine;


	/*--- Methods ---*/

	public void initialize(Camera cam, LookInputState state, FirstPersonViewConfig config) {
		camera = cam;
		lookInputState = state;
		firstPersonViewConfig = config;

		camera.fieldOfView = firstPersonViewConfig.defaultFOV; // TODO - Move
	}

	public void updateZoomFov(MonoBehaviour monoBehavior) {
		if (isRunning) {
			lookInputState.isZooming = !lookInputState.isZooming;
			isZooming = lookInputState.isZooming;
			return;
		}

		if (fovRunRoutine != null) monoBehavior.StopCoroutine(fovRunRoutine);
		if (fovZoomRoutine != null) monoBehavior.StopCoroutine(fovZoomRoutine);

		fovZoomRoutine = FovZoomRoutine();
		monoBehavior.StartCoroutine(fovZoomRoutine);
	}

	public void updateRunFov(bool isReturningToWalk, MonoBehaviour monoBehavior) {
		if (fovZoomRoutine != null) monoBehavior.StopCoroutine(fovZoomRoutine);
		if (fovRunRoutine != null) monoBehavior.StopCoroutine(fovRunRoutine);

		fovRunRoutine = FovRunRoutine(isReturningToWalk);
		monoBehavior.StartCoroutine(fovRunRoutine);
	}


	/*--- Coroutines ---*/

	IEnumerator FovZoomRoutine() {
		float percent = 0f;
		float smoothPercent = 0f;

		float speed = 1f / firstPersonViewConfig.zoomTransitionDuration;

		float currentFov = camera.fieldOfView;
		float targetFov = lookInputState.isZooming ? firstPersonViewConfig.defaultFOV : firstPersonViewConfig.zoomFOV;

		lookInputState.isZooming = !lookInputState.isZooming;
		isZooming = lookInputState.isZooming;

		while (percent < 1f) {
			percent += Time.deltaTime * speed;
			smoothPercent = firstPersonViewConfig.zoomCurve.Evaluate(percent);
			camera.fieldOfView = Mathf.Lerp(currentFov, targetFov, smoothPercent);
			yield return null;
		}
	}

	IEnumerator FovRunRoutine(bool isReturningToWalk) {
		float percent = 0f;
		float smoothPercent = 0f;

		float duration = isReturningToWalk ?
		                 firstPersonViewConfig.runReturnTransitionDuration : firstPersonViewConfig.runTransitionDuration;
		float speed = 1f / duration;

		float currentFov = camera.fieldOfView;
		float targetFov = isReturningToWalk ? firstPersonViewConfig.defaultFOV : firstPersonViewConfig.runFOV;

		isRunning = !isReturningToWalk;

		while (percent < 1f) {
			percent += Time.deltaTime * speed;
			smoothPercent = firstPersonViewConfig.runCurve.Evaluate(percent);
			camera.fieldOfView = Mathf.Lerp(currentFov, targetFov, smoothPercent);
			yield return null;
		}
	}
}
