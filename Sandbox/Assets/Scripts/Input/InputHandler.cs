using UnityEngine;
using NaughtyAttributes;


public class InputHandler : MonoBehaviour {


    /*--- Variables ---*/

    [SerializeField] private LookInputState lookInputState = null;
    [SerializeField] private MoveInputState moveInputState = null;


    /*--- Lifecycle Methods---*/

    void Start() {
        lookInputState.resetInput();
        moveInputState.resetInput();
    }

    void Update() {
        updateLookInputState();
        updateMoveInputState();
    }


    /*--- Private Methods ---*/

    void updateLookInputState() {
        lookInputState.inputVector.x = Input.GetAxis("Mouse X");
        lookInputState.inputVector.y = Input.GetAxis("Mouse Y");

        lookInputState.isZoomClicked = Input.GetMouseButtonDown(1);
        lookInputState.isZoomReleased = Input.GetMouseButtonUp(1);
    }

    void updateMoveInputState() {
        moveInputState.inputVector.x = Input.GetAxisRaw("Horizontal");
        moveInputState.inputVector.y = Input.GetAxisRaw("Vertical");

        moveInputState.isRunClicked = Input.GetKeyDown(KeyCode.LeftShift);
        moveInputState.isRunReleased = Input.GetKeyUp(KeyCode.LeftShift);

        if(moveInputState.isRunClicked)
            moveInputState.isRunning = true;

        if(moveInputState.isRunReleased)
            moveInputState.isRunning = false;

        moveInputState.isJumpClicked = Input.GetKeyDown(KeyCode.Space);
        moveInputState.isCrouchClicked = Input.GetKeyDown(KeyCode.LeftControl);
    }
}