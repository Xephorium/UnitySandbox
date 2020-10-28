using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtility {


	/*--- 2D Vector ---*/

	public static float calculateVectorStrength(Vector2 input) {
		return (float) Math.Sqrt(Math.Pow(input.x, 2f) + Math.Pow(input.y, 2f));
	}
}
