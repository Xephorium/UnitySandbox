using UnityEngine;
using NaughtyAttributes;


public class InputHandler : MonoBehaviour {


    /*--- Variables ---*/

    [SerializeField] private LookInputState lookInputState = null;
    [SerializeField] private MoveInputState moveInputState = null;

    private InputDriver inputDriver;


    /*--- Lifecycle Methods---*/

    void Awake() {
        inputDriver = new InputDriver();
    }

    void Start() {
        lookInputState.resetInput();
        moveInputState.resetInput();

        setupLookCallbacks();
        setupMoveCallbacks();
    }

    void OnEnable() {
        inputDriver.Enable();
    }

    void OnDisable() {
        inputDriver.Disable();
    }

    void Update() {
        updateLookInputState();
        updateMoveInputState();
    }


    /*--- Private Methods ---*/

    private void setupLookCallbacks() {
        // inputDriver.FirstPersonCharacter.Zoom.started += _ => { lookInputState.isZoomClicked = true; };
        // inputDriver.FirstPersonCharacter.Zoom.canceled += _ => { lookInputState.isZoomReleased = true; };
    }

    private void setupMoveCallbacks() {
        inputDriver.FirstPersonCharacter.Run.started += _ => { moveInputState.isRunning = true; };
        inputDriver.FirstPersonCharacter.Run.canceled += _ => { moveInputState.isRunning = false; };
        
        // inputDriver.FirstPersonCharacter.Crouch.started += _ => { moveInputState.isCrouchClicked = true; };
        // inputDriver.FirstPersonCharacter.Crouch.canceled += _ => { moveInputState.isCrouchClicked = false; };
    }

    private void updateLookInputState() {
        Vector2 stickInput = inputDriver.FirstPersonCharacter.LookStick.ReadValue<Vector2>();
        Vector2 mouseInput = inputDriver.FirstPersonCharacter.LookMouse.ReadValue<Vector2>();

        if (stickInput != Vector2.zero) {

            // Use Stick Input
            lookInputState.inputVector.x = stickInput.x * 14.5f;
            lookInputState.inputVector.y = stickInput.y * 9.5f;

        } else {

            // Use Mouse Input
            lookInputState.inputVector.x = mouseInput.x;
            lookInputState.inputVector.y = mouseInput.y;
        }
    }

    private void updateMoveInputState() {
        Vector2 movementInput = inputDriver.FirstPersonCharacter.Move.ReadValue<Vector2>();
        moveInputState.inputVector.x = movementInput.x;
        moveInputState.inputVector.y = movementInput.y;

        moveInputState.isJumpClicked = inputDriver.FirstPersonCharacter.Jump.triggered;
    }
}