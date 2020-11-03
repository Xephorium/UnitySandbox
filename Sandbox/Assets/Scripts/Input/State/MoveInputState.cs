using UnityEngine;
using NaughtyAttributes;


[CreateAssetMenu(fileName = "MoveInputState", menuName = "Data/Input/MoveState", order = 1)]
public class MoveInputState : ScriptableObject {


    /*--- Variables ---*/

    [ShowIf("NeverShow")] public Vector2 inputVector;
    [ShowIf("NeverShow")] public bool isRunning;
    [ShowIf("NeverShow")] public bool isCrouching;
    [ShowIf("NeverShow")] public bool isCrouchClicked;
    [ShowIf("NeverShow")] public bool isCrouchReleased;
    [ShowIf("NeverShow")] public bool isJumpClicked;
    [ShowIf("NeverShow")] public bool isRunClicked;
    [ShowIf("NeverShow")] public bool isRunReleased;


    /*--- Methods ---*/

    public bool hasInput => inputVector != Vector2.zero;

    public void resetInput() {
        inputVector = Vector2.zero;
        isRunning = false;
        isCrouching = false;
        isCrouchClicked = false;
        isJumpClicked = false;
        isRunClicked = false;
        isRunReleased = false;
    }
}
