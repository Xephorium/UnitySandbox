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
    [SerializeField][ShowIf("NeverShow")] private bool isRunning;
    [SerializeField][ShowIf("NeverShow")] private bool isCrouching;
    [SerializeField][ShowIf("NeverShow")] private bool isAnimatingCrouch;

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
            checkIfMovingTowardWall();

            // Smoothing Calculations
            smoothMovementSpeed();
            smoothMovementDirection();

            // Movement Calculations
            calculateMovementDirection();
            calculateMovementSpeed();
            calculateFinalMovementVector();

            // Movement Updates
            updateLandState();
            updateCrouchState();
            updateRunState();
            handleHeadBob();
            handleCameraSway();

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
        isRunning = false;
        isCrouching = false;

        timeInAir = 0f;
        headBobManager.currentBaseHeight = baseCameraHeight;

        speedDifferenceWalkRun = firstPersonMovementConfig.runSpeed - firstPersonMovementConfig.walkSpeed;
    }


    /*--- Smoothing Methods ---*/

    protected virtual void smoothMovementSpeed() {
        smoothCurrentSpeed = Mathf.Lerp(smoothCurrentSpeed, currentSpeed, Time.deltaTime * firstPersonMovementConfig.smoothVelocitySpeed);

        if (isRunning) {
            float _walkRunPercent = Mathf.InverseLerp(firstPersonMovementConfig.walkSpeed, firstPersonMovementConfig.runSpeed, smoothCurrentSpeed);
            smoothFinalCurrentSpeed = firstPersonMovementConfig.runTransitionCurve.Evaluate(_walkRunPercent) * speedDifferenceWalkRun + firstPersonMovementConfig.walkSpeed;
        } else {
            smoothFinalCurrentSpeed = smoothCurrentSpeed;
        }
    }

    protected virtual void smoothMovementDirection() {
        smoothFinalMoveDirection = Vector3.Lerp(smoothFinalMoveDirection, finalMoveDirection, Time.deltaTime * firstPersonMovementConfig.smoothFinalDirectionSpeed);
        //Debug.DrawRay(transform.position, smoothFinalMoveDirection, Color.yellow);
    }


    /*--- Movement Calculation Methods ---*/

    protected virtual void checkIfTouchingGround() {
        Vector3 origin = transform.position + characterController.center;

        bool groundCheck = Physics.SphereCast(
            origin,
            firstPersonMovementConfig.raySphereRadius,
            Vector3.down,
            out raycastHit,
            finalRayLength,
            firstPersonMovementConfig.groundLayer
        );
        //Debug.DrawRay(origin, Vector3.down * (finalRayLength), Color.red);

        isTouchingGround = groundCheck;
    }

    /* Note: Performs three sphere casts from player's horizontal center in the forward
     *       direction to check for wall collisions. This approach is pretty hacky and
     *       could benefit from a single-check replacement when I figure out why
     *       Physics.CheckCapsule doesn't work the way I'd expect.
     */
    protected virtual void checkIfMovingTowardWall() {
        RaycastHit raycastHit;
        bool hitWall = false;

        if (moveInputState.hasInput && finalMoveDirection.sqrMagnitude > 0) {

        	// Check Ahead of Feet
        	hitWall = Physics.SphereCast(
                transform.position + new Vector3(0f, characterController.radius, 0f),
                characterController.radius,
                finalMoveDirection,
                out raycastHit,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : hitWall;

        	// Check Ahead of Core
        	hitWall = Physics.SphereCast(
                transform.position + characterController.center,
                characterController.radius,
                finalMoveDirection,
                out raycastHit,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : hitWall;

            // Check Ahead of Head
            hitWall = Physics.SphereCast(
                transform.position + new Vector3(0f, (isCrouching ? crouchHeight : baseHeight), 0f) - new Vector3(0f, characterController.radius, 0f),
                characterController.radius,
                finalMoveDirection,
                out raycastHit,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : hitWall;
    	}

    	isTouchingWall = hitWall;
    }

    protected virtual void calculateMovementDirection() {
        moveInputVector = moveInputState.inputVector;

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
        currentSpeed = isRunning ? firstPersonMovementConfig.runSpeed : firstPersonMovementConfig.walkSpeed;
        currentSpeed = isCrouching ? firstPersonMovementConfig.crouchSpeed : currentSpeed;
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
    	canStand();

        // Begin Crouch
        if (!isCrouching && moveInputState.isCrouchClicked && isTouchingGround) {
            moveInputState.isCrouchClicked = false;
            isCrouching = true;
            beginCrouchAnimation(true);

        // Return From Crouch
        } else if (isCrouching && moveInputState.isCrouchReleased && !canStand()) {
            moveInputState.isCrouchReleased = false;
            isCrouching = false;
            beginCrouchAnimation(false);
        }
    }

    protected virtual void beginCrouchAnimation(bool isBeginningCrouch) {
        
        // Cancel Movement Animations
        if (landRoutine != null) StopCoroutine(landRoutine);
        if (crouchRoutine != null) StopCoroutine(crouchRoutine);

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
        float desiredHeight = isBeginningCrouch ? crouchHeight : baseHeight;
        Vector3 desiredCenter = isBeginningCrouch ? crouchCenter : baseCenter;
        Vector3 cameraPosition = transformYaw.localPosition;
        float cameraCurrentHeight = cameraPosition.y;
        float cameraDesiredHeight = isBeginningCrouch ? crouchCameraHeight : baseCameraHeight;

        // Update HeadBob Height
        headBobManager.currentBaseHeight = isBeginningCrouch ? crouchCameraHeight : baseCameraHeight;

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

    protected virtual bool canStand() {

    	// Setup Local Variables
    	float verticalCastOffset = baseHeight / 2f; // Begin cast at 1/2 player height to avoid hitting floor
    	Vector3 raycastStart = transform.position + new Vector3(0f, verticalCastOffset, 0f);
    	float sphereRadius = characterController.radius;
    	Vector3 raycastDirection = Vector3.up;
        RaycastHit raycastInfo;
        float raycastLength = baseHeight - verticalCastOffset;

        // Check for Collisions at Player Height
        return Physics.SphereCast(
            raycastStart,
            sphereRadius,
            raycastDirection,
            out raycastInfo,
            raycastLength
        );
    }


    /*--- Landing Methods ---*/

    protected virtual void updateLandState() {
        if (!wasPreviouslyTouchingGround && isTouchingGround) {
            beginLandAnimation();
        }
    }

    protected virtual void beginLandAnimation() {
        if (landRoutine != null) StopCoroutine(landRoutine);

        landRoutine = LandRoutine();
        StartCoroutine(landRoutine);
    }

    protected virtual IEnumerator LandRoutine() {

    	// Setup Local Variables
        float percent = 0f;
        float landAmount = 0f;
        float speed = 1f / firstPersonMovementConfig.landDuration;
        Vector3 localPosition = transformYaw.localPosition;
        float initialLandHeight = localPosition.y;

        // Calculate Land Amount
        landAmount = timeInAir > firstPersonMovementConfig.landTimer ? firstPersonMovementConfig.highLandAmount : firstPersonMovementConfig.lowLandAmount;

        // Animate Landing
        while (percent < 1f) {
            percent += Time.deltaTime * speed;
            float desiredY = firstPersonMovementConfig.landCurve.Evaluate(percent) * landAmount;

            localPosition.y = initialLandHeight + desiredY;
            transformYaw.localPosition = localPosition;

            yield return null;
        }
    }


    /*--- Run Methods ---*/

    protected virtual bool canBeginRun() {
        return moveInputState.inputVector.y > firstPersonMovementConfig.runInputThreshold
               && isRunDirectionValid()
               && isTouchingGround
               && !isTouchingWall
               && !isCrouching;
    }

    protected virtual bool canContinueRun() {
        return moveInputState.inputVector.y > firstPersonMovementConfig.runInputThreshold
               && isRunDirectionValid()
               && !isTouchingWall
               && !isCrouching;
    }

    /* Note: Calculates the portion of movement facing the same direction as the player.
     *       This prevents running when the player is moving sideways or backwards. We find
     *       the amount of movement in the forward direction by taking the dot product of
     *       the forward direction and the move direction.
     */
    private bool isRunDirectionValid() {
        Vector3 moveDirection = smoothFinalMoveDirection != Vector3.zero
                                ? smoothFinalMoveDirection.normalized
                                : Vector3.zero;
        float moveDirectionFactor = Vector3.Dot(transform.forward, moveDirection);
        return moveDirectionFactor > firstPersonMovementConfig.runDirectionThreshold;
    }


    /*--- Movement Methods ---*/

    protected virtual void handleHeadBob() {
        if (moveInputState.hasInput && isTouchingGround  && !isTouchingWall) {
            if (!isAnimatingCrouch) { // we want to make our head bob only if we are moving and not during crouch routine
                headBobManager.updateHeadBob(isRunning, isCrouching, moveInputState.inputVector);
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

    protected virtual void updateRunState() {
        if (!isRunning && moveInputState.isRunClicked && canBeginRun()) {

            // Begin Running
            moveInputState.isRunClicked = false;
            isRunning = true;
            cameraController.beginRunFovAnimation(true);

        } else if (isRunning && (!canContinueRun() || moveInputState.isRunReleased)) {

            // End Running
            moveInputState.isRunClicked = false;
            moveInputState.isRunClicked = false;
            isRunning = false;
            cameraController.beginRunFovAnimation(false);
        }

        // TODO - Continue run in air even after player releases with momentum update.
    }

    protected virtual void handleCameraSway() {
        cameraController.updateCameraSway(moveInputVector, moveInputState.inputVector.x);
    }

    protected virtual void handleJump() {
        if (moveInputState.isJumpClicked && !isCrouching) {
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
