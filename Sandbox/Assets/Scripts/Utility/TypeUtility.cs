using System.Collections;
using System.Collections.Generic;
using System;

public static class TypeUtility {
    
	public static float getValidFloat(float value) {
		return hasValue(value) ? value : 0f;
	}

    public static bool hasValue(float value) {
    	return !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
