using UnityEngine;
using NaughtyAttributes;


[CreateAssetMenu(fileName = "LookInputState", menuName = "Data/Input/LookState", order = 0)]
public class LookInputState : ScriptableObject {


	/*--- Variables---*/

	[ShowIf("NeverShow")] public Vector2 inputVector;
	[ShowIf("NeverShow")] public bool isZooming;
	[ShowIf("NeverShow")] public bool isZoomClicked;
	[ShowIf("NeverShow")] public bool isZoomReleased;


	/*--- Methods ---*/

	public void resetInput() {
		inputVector = Vector2.zero;
		isZooming = false;
		isZoomClicked = false;
		isZoomReleased = false;
	}
}
