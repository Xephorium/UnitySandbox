using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "FirstPersonState", menuName = "Data/FirstPersonCharacter/State", order = 3)]
public class FirstPersonState : ScriptableObject {


    /*--- Variables ---*/

    [ShowIf("NeverShow")] public bool isRunning;
    [ShowIf("NeverShow")] public bool isCrouching;
    [ShowIf("NeverShow")] public bool isAnimatingCrouch;
    [ShowIf("NeverShow")] public bool isTouchingWall;
    [ShowIf("NeverShow")] public bool isTouchingGround;
    [ShowIf("NeverShow")] public bool wasTouchingGround;
    [ShowIf("NeverShow")] public bool isGroundBeneath;
    
    [ShowIf("NeverShow")] public float groundAngle;
    [ShowIf("NeverShow")] public float timeInAir;


    /*--- Constructor ---*/

    public FirstPersonState() {
    	resetState();
    }


    /*--- Methods ---*/

    public void resetState() {
    	isRunning = false;
    	isCrouching = false;
    	isAnimatingCrouch = false;
    	isTouchingWall = false;
    	isTouchingGround = false;
    	wasTouchingGround = false;
    	isGroundBeneath = false;

    	timeInAir = 0f;
    }
}
