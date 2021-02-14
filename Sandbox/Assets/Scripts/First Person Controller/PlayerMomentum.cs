using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerMomentum {


	/*--- Variables ---*/

	private Vector3 direction; 



	/*--- Constructor ---*/

	public PlayerMomentum() {
		reset();
	}


	/*--- Public Methods ---*/

	public void reset() {
		direction = Vector3.zero;
	}

	public Vector3 getDirection() {
		return direction;
	}

	public void handleJump(Vector3 jumpVector) {
		direction = new Vector3(jumpVector.x, jumpVector.y, jumpVector.z);
	}

	public void updateMomentum(Vector3 desiredMoveDirection, FirstPersonMovementConfig config) {

		// TODO - Extract to Constants
        float aerialDriftMaxSpeed = 5f;
        float aerialDriftRateOfChange = 15f;
        float momentumFalloffConstant = 1.2f;

        float momentumChange = aerialDriftRateOfChange * Time.deltaTime;
        float momentumFalloff = desiredMoveDirection == Vector3.zero
                              ? 1f - (momentumFalloffConstant * Time.deltaTime)
                              : 1f;

        Vector3 desiredMomentum = direction + desiredMoveDirection * momentumChange;

        // Calculate X Drift
        float desiredComponentX = (desiredMoveDirection.normalized * aerialDriftMaxSpeed).x;
        float lowerBound = -Mathf.Abs(desiredComponentX) < direction.x ? -Mathf.Abs(desiredComponentX) : direction.x;
        float upperBound = Mathf.Abs(desiredComponentX) > direction.x ? Mathf.Abs(desiredComponentX) : direction.x;
        float newMomentumX = Mathf.Clamp(desiredMomentum.x, lowerBound, upperBound);

        // Calculate Z Drift
        float desiredComponentZ = (desiredMoveDirection.normalized * aerialDriftMaxSpeed).z;
        lowerBound = -Mathf.Abs(desiredComponentZ) < direction.z ? -Mathf.Abs(desiredComponentZ) : direction.z;
        upperBound = Mathf.Abs(desiredComponentZ) > direction.z ? Mathf.Abs(desiredComponentZ) : direction.z;
        float newMomentumZ = Mathf.Clamp(desiredMomentum.z, lowerBound, upperBound);

        Vector3 newMomentum = new Vector3(newMomentumX, 0f, newMomentumZ);
        newMomentum = Vector3.Lerp(
            direction,
            newMomentum,
            Time.deltaTime * config.smoothAerialDriftSpeed
        );

        direction = newMomentum * momentumFalloff;
	}

}
