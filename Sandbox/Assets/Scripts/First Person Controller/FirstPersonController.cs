using System.Collections;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {


    /*--- Variables ---*/

    [SerializeField] private MoveInputState moveInputState = null;
    [SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
    [SerializeField] private FirstPersonMovementConfig firstPersonMovementConfig = null;
    [SerializeField] private FirstPersonState firstPersonState = null;

    private CharacterController characterController;
    private Transform transformYaw;
    private Transform transformCamera;
    private HeadBobManager headBobManager;
    private CameraController cameraController;

    private RaycastHit groundRaycastInfo;

    private IEnumerator crouchRoutine;
    private IEnumerator landRoutine;

    [SerializeField][ShowIf("NeverShow")] private Vector2 moveInputVector;

    [SerializeField][ShowIf("NeverShow")] private Vector3 finalMoveDirection;
    [SerializeField][ShowIf("NeverShow")] private Vector3 smoothFinalMoveDirection;
    [SerializeField][ShowIf("NeverShow")] private Vector3 finalMoveVector;

    [SerializeField][ShowIf("NeverShow")] private float currentSpeed;
    [SerializeField][ShowIf("NeverShow")] private float smoothCurrentSpeed;
    [SerializeField][ShowIf("NeverShow")] private float smoothFinalCurrentSpeed;

    [SerializeField][ShowIf("NeverShow")] private float baseHeight;
    [SerializeField][ShowIf("NeverShow")] private float crouchHeight;
    [SerializeField][ShowIf("NeverShow")] private Vector3 baseCenter;
    [SerializeField][ShowIf("NeverShow")] private Vector3 crouchCenter;
    [SerializeField][ShowIf("NeverShow")] private float baseCameraHeight;
    [SerializeField][ShowIf("NeverShow")] private float crouchCameraHeight;


    /*--- Lifecycle Methods ---*/

    protected virtual void Start() {
        getComponents();
        initializeVariables();
    }

    protected virtual void Update() {
        if (transformYaw != null) rotateTowardsCamera();

        if (characterController) {

            // Contact Check
            updateGroundCheck();
            updateWallCheck();

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
            updateHeadBob();
            updateCameraSway();

            applyGravity();
            applyMovement();

            firstPersonState.wasTouchingGround = firstPersonState.isTouchingGround;
        }
    }


    /*--- Setup Methods ---*/

    protected virtual void getComponents() {
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponentInChildren<CameraController>();
        transformYaw = cameraController.transform;
        transformCamera = GetComponentInChildren<Camera>().transform;
        headBobManager = new HeadBobManager(
        	firstPersonViewConfig,
        	firstPersonMovementConfig.moveBackwardsSpeedPercent,
        	firstPersonMovementConfig.moveSideSpeedPercent
        );
    }

    protected virtual void initializeVariables() {
        // Calculate where our character center should be based on height and skin width
        characterController.center = new Vector3(0f, characterController.height / 2f + characterController.skinWidth, 0f);

        baseCenter = characterController.center;
        baseHeight = characterController.height;

        crouchHeight = baseHeight * firstPersonMovementConfig.crouchPercent;
        crouchCenter = (crouchHeight / 2f + characterController.skinWidth) * Vector3.up;

        baseCameraHeight = transformYaw.localPosition.y;
        crouchCameraHeight = baseCameraHeight - (baseHeight - crouchHeight);

        firstPersonState.isTouchingGround = true;
        firstPersonState.wasTouchingGround = true;
        firstPersonState.isRunning = false;
        firstPersonState.isCrouching = false;

        firstPersonState.timeInAir = 0f;
        headBobManager.currentBaseHeight = baseCameraHeight;
    }


    /*--- Contact Check Methods ---*/

    protected virtual void updateGroundCheck() {

    	// Setup Local Variables
		Vector3 raycastStart = transform.position + characterController.center;
		float sphereRadius = firstPersonMovementConfig.raySphereRadius;
		Vector3 raycastDirection = Vector3.down;
		float raycastLength = characterController.center.y + firstPersonMovementConfig.rayLength;

		// Check for Ground
        firstPersonState.isTouchingGround = Physics.SphereCast(
            raycastStart,
            sphereRadius,
            raycastDirection,
            out groundRaycastInfo,
            raycastLength,
            firstPersonMovementConfig.groundLayer
        );
    }

    /* Note: Performs three sphere casts from player's horizontal center in the forward
     *       direction to check for wall collisions. This approach is pretty hacky and
     *       could benefit from a single-check replacement when I figure out why
     *       Physics.CheckCapsule doesn't work the way I'd expect.
     */
    protected virtual void updateWallCheck() {
        RaycastHit raycastInfo;
        bool wallCheck = false;

        if (moveInputState.hasInput && finalMoveDirection.sqrMagnitude > 0) {

        	// Check Ahead of Feet
        	wallCheck = Physics.SphereCast(
                transform.position + new Vector3(0f, characterController.radius, 0f),
                characterController.radius,
                finalMoveDirection,
                out raycastInfo,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : wallCheck;

        	// Check Ahead of Core
        	wallCheck = Physics.SphereCast(
                transform.position + characterController.center,
                characterController.radius,
                finalMoveDirection,
                out raycastInfo,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : wallCheck;

            // Check Ahead of Head
            wallCheck = Physics.SphereCast(
                transform.position + new Vector3(0f, (firstPersonState.isCrouching ? crouchHeight : baseHeight), 0f) - new Vector3(0f, characterController.radius, 0f),
                characterController.radius,
                finalMoveDirection,
                out raycastInfo,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : wallCheck;
    	}

    	firstPersonState.isTouchingWall = wallCheck;
    }


    /*--- Smoothing Methods ---*/

    protected virtual void smoothMovementSpeed() {
    	float speedDifferenceWalkRun = firstPersonMovementConfig.runSpeed - firstPersonMovementConfig.walkSpeed;
        smoothCurrentSpeed = Mathf.Lerp(
        	smoothCurrentSpeed,
        	currentSpeed,
        	firstPersonMovementConfig.smoothVelocitySpeed * Time.deltaTime
        );

        if (firstPersonState.isRunning) {
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

    protected virtual void calculateMovementDirection() {
        moveInputVector = moveInputState.inputVector;

        Vector3 verticalDirection = transform.forward * moveInputVector.y;
        Vector3 horizontalDirection = transform.right * moveInputVector.x;

        Vector3 desiredDirection = verticalDirection + horizontalDirection;
        Vector3 flattenedDirection = flattenVectorOnSlopes(desiredDirection);

        finalMoveDirection = flattenedDirection;
    }

    protected virtual Vector3 flattenVectorOnSlopes(Vector3 vector) {
        if (firstPersonState.isTouchingGround) vector = Vector3.ProjectOnPlane(vector, groundRaycastInfo.normal);

        return vector;
    }

    protected virtual void calculateMovementSpeed() {
        currentSpeed = firstPersonState.isRunning ? firstPersonMovementConfig.runSpeed : firstPersonMovementConfig.walkSpeed;
        currentSpeed = firstPersonState.isCrouching ? firstPersonMovementConfig.crouchSpeed : currentSpeed;
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


    /*--- Land Methods ---*/

    protected virtual void updateLandState() {
        if (!firstPersonState.wasTouchingGround && firstPersonState.isTouchingGround) {
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
        landAmount = firstPersonState.timeInAir > firstPersonMovementConfig.landTimer ? firstPersonMovementConfig.highLandAmount : firstPersonMovementConfig.lowLandAmount;

        // Animate Landing
        while (percent < 1f) {
            percent += Time.deltaTime * speed;
            float desiredY = firstPersonMovementConfig.landCurve.Evaluate(percent) * landAmount;

            localPosition.y = initialLandHeight + desiredY;
            transformYaw.localPosition = localPosition;

            yield return null;
        }
    }


    /*--- Crouch Methods ---*/

    protected virtual void updateCrouchState() {
    	canStand();

        // Begin Crouch
        if (!firstPersonState.isCrouching && moveInputState.isCrouchClicked && firstPersonState.isTouchingGround) {
            moveInputState.isCrouchClicked = false;
            firstPersonState.isCrouching = true;
            beginCrouchAnimation(true);

        // Return From Crouch
        } else if (firstPersonState.isCrouching && moveInputState.isCrouchReleased && !canStand()) {
            moveInputState.isCrouchReleased = false;
            firstPersonState.isCrouching = false;
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
        firstPersonState.isAnimatingCrouch = true;

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

        firstPersonState.isAnimatingCrouch = false;
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


    /*--- Run Methods ---*/

    protected virtual void updateRunState() {
        if (!firstPersonState.isRunning && moveInputState.isRunClicked && canBeginRun()) {

            // Begin Running
            moveInputState.isRunClicked = false;
            firstPersonState.isRunning = true;
            cameraController.beginRunFovAnimation(true);

        } else if (firstPersonState.isRunning && (!canContinueRun() || moveInputState.isRunReleased)) {

            // End Running
            moveInputState.isRunClicked = false;
            moveInputState.isRunClicked = false;
            firstPersonState.isRunning = false;
            cameraController.beginRunFovAnimation(false);
        }

        // TODO - Continue run in air even after player releases with momentum update.
    }

    protected virtual bool canBeginRun() {
        return moveInputState.inputVector.y > firstPersonMovementConfig.runInputThreshold
               && isRunDirectionValid()
               && firstPersonState.isTouchingGround
               && !firstPersonState.isTouchingWall
               && !firstPersonState.isCrouching;
    }

    protected virtual bool canContinueRun() {
        return moveInputState.inputVector.y > firstPersonMovementConfig.runInputThreshold
               && isRunDirectionValid()
               && !firstPersonState.isTouchingWall
               && !firstPersonState.isCrouching;
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


    /*--- Head Bob & Sway Methods ---*/

    protected virtual void updateHeadBob() {
        if (moveInputState.hasInput && firstPersonState.isTouchingGround  && !firstPersonState.isTouchingWall) {
            if (!firstPersonState.isAnimatingCrouch) { // we want to make our head bob only if we are moving and not during crouch routine
                headBobManager.updateHeadBob(firstPersonState.isRunning, firstPersonState.isCrouching, moveInputState.inputVector);
                transformYaw.localPosition = Vector3.Lerp(transformYaw.localPosition, (Vector3.up * headBobManager.currentBaseHeight) + headBobManager.currentPositionOffset, Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
            }
        } else { // if we are not moving or we are not grounded
            if (!headBobManager.isReset) {
                headBobManager.resetHeadBob();
            }

            if (!firstPersonState.isAnimatingCrouch) // we want to reset our head bob only if we are standing still and not during crouch routine
                transformYaw.localPosition = Vector3.Lerp(transformYaw.localPosition, new Vector3(0f, headBobManager.currentBaseHeight, 0f), Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
        }

        //transformCamera.localPosition = Vector3.Lerp(transformCamera.localPosition,headBobManager.FinalOffset,Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
    }

    protected virtual void updateCameraSway() {
        cameraController.updateCameraSway(moveInputVector, moveInputState.inputVector.x);
    }


    /*--- Movement Methods ---*/

    protected virtual void handleJump() {
        if (moveInputState.isJumpClicked && !firstPersonState.isCrouching) {
            finalMoveVector.y += firstPersonMovementConfig.jumpSpeed /* currentSpeed */; // we are adding because ex. when we are going on slope we want to keep Y value not overwriting it
            //finalMoveVector.y = firstPersonMovementConfig.jumpSpeed /* currentSpeed */; // turns out that when adding to Y it is too much and it doesn't feel correct because jumping on slope is much faster and higher;

            firstPersonState.wasTouchingGround = true;
            firstPersonState.isTouchingGround = false;
        }
    }
    protected virtual void applyGravity() {
        if (characterController.isGrounded) { // if we would use our own firstPersonState.isTouchingGround it would not work that good, this one is more precise
            firstPersonState.timeInAir = 0f;
            finalMoveVector.y = -firstPersonMovementConfig.stickToGroundForce;

            handleJump();

        } else {
            firstPersonState.timeInAir += Time.deltaTime;
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
