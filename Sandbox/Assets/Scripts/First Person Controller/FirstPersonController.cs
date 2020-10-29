using System.Collections;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {


    /*--- Variables ---*/

    [SerializeField] private MoveInputState moveInputState = null;
    [SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
    [SerializeField] private FirstPersonMovementConfig firstPersonMovementConfig = null;

    private CharacterController characterController;
    private Transform transformYaw;
    private Transform transformCamera;
    private HeadBobManager headBobManager;
    private CameraController cameraController;

    private RaycastHit raycastHit;
    private IEnumerator crouchRoutine;
    private IEnumerator landRoutine;

    [SerializeField][ShowIf("NeverShow")] private Vector2 moveInputVector;

    [SerializeField][ShowIf("NeverShow")] private Vector3 finalMoveDirection;
    [SerializeField][ShowIf("NeverShow")] private Vector3 smoothFinalMoveDirection;
    [SerializeField][ShowIf("NeverShow")] private Vector3 finalMoveVector;

    [SerializeField][ShowIf("NeverShow")] private float currentSpeed;
    [SerializeField][ShowIf("NeverShow")] private float smoothCurrentSpeed;
    [SerializeField][ShowIf("NeverShow")] private float smoothFinalCurrentSpeed;
    [SerializeField][ShowIf("NeverShow")] private float speedDifferenceWalkRun;

    [SerializeField][ShowIf("NeverShow")] private float finalRayLength;
    [SerializeField][ShowIf("NeverShow")] private bool isTouchingWall;
    [SerializeField][ShowIf("NeverShow")] private bool isTouchingGround;
    [SerializeField][ShowIf("NeverShow")] private bool wasPreviouslyTouchingGround;

    [SerializeField][ShowIf("NeverShow")] private float baseHeight;
    [SerializeField][ShowIf("NeverShow")] private float crouchHeight;
    [SerializeField][ShowIf("NeverShow")] private Vector3 baseCenter;
    [SerializeField][ShowIf("NeverShow")] private Vector3 crouchCenter;

    [SerializeField][ShowIf("NeverShow")] private float baseCameraHeight;
    [SerializeField][ShowIf("NeverShow")] private float crouchCameraHeight;
    [SerializeField][ShowIf("NeverShow")] private float heightDifferenceStandCrouch;
    [SerializeField][ShowIf("NeverShow")] private bool isAnimatingCrouch;
    [SerializeField][ShowIf("NeverShow")] private bool isAnimatingRun;

    [SerializeField][ShowIf("NeverShow")] private float timeInAir;


    /*--- Lifecycle Methods ---*/

    protected virtual void Start() {
        getComponents();
        initializeVariables();
    }

    protected virtual void Update() {
        if (transformYaw != null) rotateTowardsCamera();

        if (characterController) {

            // Contact Check
            checkIfTouchingGround();
            checkIfTouchingWall();

            // Smoothing Calculations
            smoothMovementInput();
            smoothMovementSpeed();
            smoothMovementDirection();

            // Movement Calculations
            calculateMovementDirection();
            calculateMovementSpeed();
            calculateFinalMovementVector();

            // Movement Updates
            updateCrouchState();
            handleHeadBob();
            handleRunFOV();
            handleCameraSway();
            handleLanding();

            applyGravity();
            applyMovement();

            wasPreviouslyTouchingGround = isTouchingGround;
        }
    }

    /*

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere((transform.position + characterController.center) - Vector3.up * finalRayLength, firstPersonMovementConfig.raySphereRadius);
        }

     */


    /*--- Setup Methods ---*/

    protected virtual void getComponents() {
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponentInChildren<CameraController>();
        transformYaw = cameraController.transform;
        transformCamera = GetComponentInChildren<Camera>().transform;
        headBobManager = new HeadBobManager(firstPersonViewConfig, firstPersonMovementConfig.moveBackwardsSpeedPercent, firstPersonMovementConfig.moveSideSpeedPercent);
    }

    protected virtual void initializeVariables() {
        // Calculate where our character center should be based on height and skin width
        characterController.center = new Vector3(0f, characterController.height / 2f + characterController.skinWidth, 0f);

        baseCenter = characterController.center;
        baseHeight = characterController.height;

        crouchHeight = baseHeight * firstPersonMovementConfig.crouchPercent;
        crouchCenter = (crouchHeight / 2f + characterController.skinWidth) * Vector3.up;

        heightDifferenceStandCrouch = baseHeight - crouchHeight;

        baseCameraHeight = transformYaw.localPosition.y;
        crouchCameraHeight = baseCameraHeight - heightDifferenceStandCrouch;

        // Sphere radius not included. If you want it to be included just decrease by sphere radius at the end of this equation
        finalRayLength = firstPersonMovementConfig.rayLength + characterController.center.y;

        isTouchingGround = true;
        wasPreviouslyTouchingGround = true;

        timeInAir = 0f;
        headBobManager.currentBaseHeight = baseCameraHeight;

        speedDifferenceWalkRun = firstPersonMovementConfig.runSpeed - firstPersonMovementConfig.walkSpeed;
    }


    /*--- Smoothing Methods ---*/

    protected virtual void smoothMovementInput() {
        moveInputVector = moveInputState.inputVector;
    }

    protected virtual void smoothMovementSpeed() {
        smoothCurrentSpeed = Mathf.Lerp(smoothCurrentSpeed, currentSpeed, Time.deltaTime * firstPersonMovementConfig.smoothVelocitySpeed);

        if (moveInputState.isRunning && canRun()) {
            float _walkRunPercent = Mathf.InverseLerp(firstPersonMovementConfig.walkSpeed, firstPersonMovementConfig.runSpeed, smoothCurrentSpeed);
            smoothFinalCurrentSpeed = firstPersonMovementConfig.runTransitionCurve.Evaluate(_walkRunPercent) * speedDifferenceWalkRun + firstPersonMovementConfig.walkSpeed;
        } else {
            smoothFinalCurrentSpeed = smoothCurrentSpeed;
        }
    }

    protected virtual void smoothMovementDirection() {

        smoothFinalMoveDirection = Vector3.Lerp(smoothFinalMoveDirection, finalMoveDirection, Time.deltaTime * firstPersonMovementConfig.smoothFinalDirectionSpeed);
        Debug.DrawRay(transform.position, smoothFinalMoveDirection, Color.yellow);
    }


    /*--- Movement Calculation Methods ---*/

    protected virtual void checkIfTouchingGround() {
        Vector3 origin = transform.position + characterController.center;

        bool hitGround = Physics.SphereCast(
            origin,
            firstPersonMovementConfig.raySphereRadius,
            Vector3.down,
            out raycastHit,
            finalRayLength,
            firstPersonMovementConfig.groundLayer
        );
        //Debug.DrawRay(origin, Vector3.down * (finalRayLength), Color.red);

        isTouchingGround = hitGround;
    }

    protected virtual void checkIfTouchingWall() { // TODO - Fix. Does not prevent head bob.

        Vector3 origin = transform.position + characterController.center;
        RaycastHit raycastHit;

        bool hitWall = false;

        if (moveInputState.hasInput && finalMoveDirection.sqrMagnitude > 0)
            hitWall = Physics.SphereCast(
                origin,
                firstPersonMovementConfig.rayObstacleSphereRadius,
                finalMoveDirection, out raycastHit,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            );
        //Debug.DrawRay(origin, finalMoveDirection * firstPersonMovementConfig.rayObstacleLength, Color.blue);

        isTouchingWall = hitWall;
    }

    protected virtual bool checkIfRoofDirectlyAbove() { // TODO - Fix. Does not accurately detect roof collision.
        Vector3 origin = transform.position;
        RaycastHit raycastHit;

        bool hitRoof = Physics.SphereCast(
            origin,
            firstPersonMovementConfig.raySphereRadius,
            Vector3.up,
            out raycastHit,
            baseHeight
        );

        return false; // hitRoof;
    }

    protected virtual bool canRun() {
        Vector3 normalizedDirection = Vector3.zero;

        if (smoothFinalMoveDirection != Vector3.zero)
            normalizedDirection = smoothFinalMoveDirection.normalized;

        float dotProduct = Vector3.Dot(transform.forward, normalizedDirection);
        return dotProduct >= firstPersonMovementConfig.canRunThreshold && !moveInputState.isCrouching ? true : false;
    }

    protected virtual void calculateMovementDirection() {
        Vector3 verticalDirection = transform.forward * moveInputVector.y;
        Vector3 horizontalDirection = transform.right * moveInputVector.x;

        Vector3 desiredDirection = verticalDirection + horizontalDirection;
        Vector3 flattenedDirection = flattenVectorOnSlopes(desiredDirection);

        finalMoveDirection = flattenedDirection;
    }

    protected virtual Vector3 flattenVectorOnSlopes(Vector3 vector) {
        if (isTouchingGround) vector = Vector3.ProjectOnPlane(vector, raycastHit.normal);

        return vector;
    }

    protected virtual void calculateMovementSpeed() {
        currentSpeed = moveInputState.isRunning && canRun() ? firstPersonMovementConfig.runSpeed : firstPersonMovementConfig.walkSpeed;
        currentSpeed = moveInputState.isCrouching ? firstPersonMovementConfig.crouchSpeed : currentSpeed;
        currentSpeed = !moveInputState.hasInput ? 0f : currentSpeed;
        currentSpeed = moveInputState.inputVector.y == -1 ? currentSpeed * firstPersonMovementConfig.moveBackwardsSpeedPercent : currentSpeed;
        currentSpeed = moveInputState.inputVector.x != 0 && moveInputState.inputVector.y ==  0 ? currentSpeed * firstPersonMovementConfig.moveSideSpeedPercent :  currentSpeed;
    }

    protected virtual void calculateFinalMovementVector() {
        float smoothInputVectorMagnitude = 1f;
        Vector3 finalVector = smoothFinalMoveDirection * smoothFinalCurrentSpeed * smoothInputVectorMagnitude;

        // We have to assign individually in order to make our character jump properly because before it was overwriting Y value and that's why it was jerky now we are adding to Y value and it's working
        finalMoveVector.x = finalVector.x;
        finalMoveVector.z = finalVector.z;

        if (characterController.isGrounded) // Thanks to this check we are not applying extra y velocity when in air so jump will be consistent
            finalMoveVector.y += finalVector.y; //so this makes our player go in forward dir using slope normal but when jumping this is making it go higher so this is weird
    }


    /*--- Crouch Methods ---*/

    protected virtual void updateCrouchState() {

        // Begin Crouch
        if (moveInputState.isCrouchClicked && !moveInputState.isCrouching && isTouchingGround) {
            moveInputState.isCrouchClicked = false;
            moveInputState.isCrouching = true;
            invokeCrouchRoutine(true);

        // Return From Crouch
        } else if (moveInputState.isCrouchReleased && moveInputState.isCrouching && !checkIfRoofDirectlyAbove()) {
            moveInputState.isCrouchReleased = false;
            moveInputState.isCrouching = false;
            invokeCrouchRoutine(false);
        }
    }

    protected virtual void invokeCrouchRoutine(bool isBeginningCrouch) {
        
        // Cancel Running Animations
        if (landRoutine != null)
            StopCoroutine(landRoutine);
        if (crouchRoutine != null)
            StopCoroutine(crouchRoutine);

        crouchRoutine = CrouchRoutine(isBeginningCrouch);
        StartCoroutine(crouchRoutine);
    }

    protected virtual IEnumerator CrouchRoutine(bool isBeginningCrouch) {
        isAnimatingCrouch = true;

        // Setup Local Variables
        float percent = 0f;
        float smoothPercent = 0f;
        float speed = 1f / firstPersonMovementConfig.crouchTransitionDuration;
        float currentHeight = characterController.height;
        Vector3 currentCenter = characterController.center;
        float desiredHeight = !isBeginningCrouch ? baseHeight : crouchHeight;
        Vector3 desiredCenter = !isBeginningCrouch ? baseCenter : crouchCenter;
        Vector3 cameraPosition = transformYaw.localPosition;
        float cameraCurrentHeight = cameraPosition.y;
        float cameraDesiredHeight = !isBeginningCrouch ? baseCameraHeight : crouchCameraHeight;

        // Update HeadBob Height
        headBobManager.currentBaseHeight = !isBeginningCrouch ? baseCameraHeight : crouchCameraHeight;

        // Animate Crouch
        while (percent < 1f) {
            percent += Time.deltaTime * speed;
            smoothPercent = firstPersonMovementConfig.crouchTransitionCurve.Evaluate(percent);

            characterController.height = Mathf.Lerp(currentHeight, desiredHeight, smoothPercent);
            characterController.center = Vector3.Lerp(currentCenter, desiredCenter, smoothPercent);

            cameraPosition.y = Mathf.Lerp(cameraCurrentHeight, cameraDesiredHeight, smoothPercent);
            transformYaw.localPosition = cameraPosition;

            yield return null;
        }

        isAnimatingCrouch = false;
    }


    /*--- Landing Methods ---*/

    protected virtual void handleLanding() {
        if (!wasPreviouslyTouchingGround && isTouchingGround) {
            invokeLandingRoutine();
        }
    }

    protected virtual void invokeLandingRoutine() {
        if (landRoutine != null)
            StopCoroutine(landRoutine);

        landRoutine = LandingRoutine();
        StartCoroutine(landRoutine);
    }

    protected virtual IEnumerator LandingRoutine() {
        float percent = 0f;
        float landAmount = 0f;

        float speed = 1f / firstPersonMovementConfig.landDuration;

        Vector3 localPosition = transformYaw.localPosition;
        float initialLandHeight = localPosition.y;

        landAmount = timeInAir > firstPersonMovementConfig.landTimer ? firstPersonMovementConfig.highLandAmount : firstPersonMovementConfig.lowLandAmount;

        while (percent < 1f) {
            percent += Time.deltaTime * speed;
            float desiredY = firstPersonMovementConfig.landCurve.Evaluate(percent) * landAmount;

            localPosition.y = initialLandHeight + desiredY;
            transformYaw.localPosition = localPosition;

            yield return null;
        }
    }


    /*--- Movement Methods ---*/

    protected virtual void handleHeadBob() {
        if (moveInputState.hasInput && isTouchingGround  && !isTouchingWall) {
            if (!isAnimatingCrouch) { // we want to make our head bob only if we are moving and not during crouch routine
                headBobManager.updateHeadBob(moveInputState.isRunning && canRun(), moveInputState.isCrouching, moveInputState.inputVector);
                transformYaw.localPosition = Vector3.Lerp(transformYaw.localPosition, (Vector3.up * headBobManager.currentBaseHeight) + headBobManager.currentPositionOffset, Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
            }
        } else { // if we are not moving or we are not grounded
            if (!headBobManager.isReset) {
                headBobManager.resetHeadBob();
            }

            if (!isAnimatingCrouch) // we want to reset our head bob only if we are standing still and not during crouch routine
                transformYaw.localPosition = Vector3.Lerp(transformYaw.localPosition, new Vector3(0f, headBobManager.currentBaseHeight, 0f), Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
        }

        //transformCamera.localPosition = Vector3.Lerp(transformCamera.localPosition,headBobManager.FinalOffset,Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
    }

    protected virtual void handleRunFOV() {
        if (moveInputState.hasInput && isTouchingGround  && !isTouchingWall) {
            if (moveInputState.isRunClicked && canRun()) {
                isAnimatingRun = true;
                cameraController.updateRunFov(false);
            }

            if (moveInputState.isRunning && canRun() && !isAnimatingRun ) {
                isAnimatingRun = true;
                cameraController.updateRunFov(false);
            }
        }

        if (moveInputState.isRunReleased || !moveInputState.hasInput || isTouchingWall) {
            if (isAnimatingRun) {
                isAnimatingRun = false;
                cameraController.updateRunFov(true);
            }
        }
    }

    protected virtual void handleCameraSway() {
        cameraController.updateCameraSway(moveInputVector, moveInputState.inputVector.x);
    }

    protected virtual void handleJump() {
        if (moveInputState.isJumpClicked && !moveInputState.isCrouching) {
            finalMoveVector.y += firstPersonMovementConfig.jumpSpeed /* currentSpeed */; // we are adding because ex. when we are going on slope we want to keep Y value not overwriting it
            //finalMoveVector.y = firstPersonMovementConfig.jumpSpeed /* currentSpeed */; // turns out that when adding to Y it is too much and it doesn't feel correct because jumping on slope is much faster and higher;

            wasPreviouslyTouchingGround = true;
            isTouchingGround = false;
        }
    }
    protected virtual void applyGravity() {
        if (characterController.isGrounded) { // if we would use our own isTouchingGround it would not work that good, this one is more precise
            timeInAir = 0f;
            finalMoveVector.y = -firstPersonMovementConfig.stickToGroundForce;

            handleJump();

        } else {
            timeInAir += Time.deltaTime;
            finalMoveVector += Physics.gravity * firstPersonMovementConfig.gravityMultiplier * Time.deltaTime;
        }
    }

    protected virtual void applyMovement() {
        characterController.Move(finalMoveVector * Time.deltaTime);
    }

    protected virtual void rotateTowardsCamera() {
        Quaternion currentRotation = transform.rotation;
        Quaternion desiredRotation = transformYaw.rotation;

        transform.rotation = Quaternion.Slerp(currentRotation, desiredRotation, Time.deltaTime * firstPersonMovementConfig.smoothRotateSpeed);
    }
}
