using System.Collections;
using UnityEngine;
using NaughtyAttributes;

/* TODO:
 *   - Identify & fix weird canRun() behavior after crouch.
 *   - Fix slope running.
 *   - Smooth abrupt direction changes on land.
 *   - Fix edge stickyness.
 */

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {


    /*--- Variables ---*/

	// Data Assets
    [SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
    [SerializeField] private FirstPersonMovementConfig firstPersonMovementConfig = null;
    [SerializeField] private MoveInputState moveInputState = null;
    [SerializeField] private FirstPersonState firstPersonState = null;

    // Components & Helper Classes
    private CharacterController characterController;
    private Transform transformCameraHolder;
    private Transform transformCamera;
    private HeadBobManager headBobManager;
    private CameraController cameraController;
    private PlayerMomentum playerMomentum;

    // Raycast & Routine Objects
    private RaycastHit groundRaycastInfo;
    private IEnumerator crouchRoutine;
    private IEnumerator landRoutine;

    // Player Height Fields
    [SerializeField][ShowIf("NeverShow")] private float baseHeight;
    [SerializeField][ShowIf("NeverShow")] private float crouchHeight;
    [SerializeField][ShowIf("NeverShow")] private Vector3 baseCenter;
    [SerializeField][ShowIf("NeverShow")] private Vector3 crouchCenter;
    [SerializeField][ShowIf("NeverShow")] private float baseCameraHeight;
    [SerializeField][ShowIf("NeverShow")] private float crouchCameraHeight;

    // Player Movement Fields
    [SerializeField][ShowIf("NeverShow")] private float initialStepOffset;
    [SerializeField][ShowIf("NeverShow")] private float desiredMoveSpeed;
    [SerializeField][ShowIf("NeverShow")] private float smoothedMoveSpeed;
    [SerializeField][ShowIf("NeverShow")] private float finalMoveSpeed;
    [SerializeField][ShowIf("NeverShow")] private Vector3 desiredMoveDirection;
    [SerializeField][ShowIf("NeverShow")] private Vector3 smoothedMoveDirection;
    [SerializeField][ShowIf("NeverShow")] private Vector3 finalMoveDirection;


    /*--- Lifecycle Methods ---*/

    protected virtual void Start() {
        getComponents();
        initializeVariables();
    }

    protected virtual void Update() {

        if (characterController) {

            // Update Contact Checks
            updateGroundCheck();
            updateWallCheck();

            // Update Player Rig Components
            updateComponents();

            // Update Animation States
            updateLandAnimation();
            updateCrouchAnimation();
            updateRunAnimation();
            updateHeadBobAnimation();
            updateSwayAnimation();

            // Read Inputs
            calculateInputMoveSpeed();
            calculateInputMoveDirection();

            // Perform Movement Calculations
            calculateFinalMoveSpeed();
            calculateFinalMoveDirection();
            calculateVerticalMovement();

            // Move Player
            // TODO - Input vs Momentum
            if (true) movePlayer(finalMoveDirection * Time.deltaTime);
            //else movePlayer(finalMoveDirection * Time.deltaTime);
        }
    }


    /*--- Setup Methods ---*/

    protected virtual void getComponents() {
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponentInChildren<CameraController>();
        transformCameraHolder = cameraController.transform;
        transformCamera = GetComponentInChildren<Camera>().transform;
        headBobManager = new HeadBobManager(
        	firstPersonViewConfig,
        	firstPersonMovementConfig.moveBackwardsSpeedPercent,
        	firstPersonMovementConfig.moveSideSpeedPercent
        );
        playerMomentum = new PlayerMomentum();
    }

    protected virtual void initializeVariables() {
        moveInputState.resetInput();
        firstPersonState.resetState();

       	baseHeight = characterController.height;
        crouchHeight = baseHeight * firstPersonMovementConfig.crouchPercent;
        baseCenter = new Vector3(0f, characterController.height / 2f + characterController.skinWidth, 0f);
        crouchCenter = (crouchHeight / 2f + characterController.skinWidth) * Vector3.up;
        baseCameraHeight = transformCameraHolder.localPosition.y;
        crouchCameraHeight = baseCameraHeight - (baseHeight - crouchHeight);

        initialStepOffset = characterController.stepOffset;
        characterController.center = baseCenter;
        headBobManager.currentBaseHeight = baseCameraHeight;
    }


    /*--- Contact Check Methods ---*/

    protected virtual void updateGroundCheck() {
        firstPersonState.wasTouchingGround = firstPersonState.isTouchingGround;
        firstPersonState.isTouchingGround = characterController.isGrounded;
        firstPersonState.isGroundBeneath = checkForGroundBeneath();
        firstPersonState.groundAngle = getGroundAngle();

        // Set Move Direction to Momentum Vector
        // TODO - Move to somewhere more sensible
        if (!firstPersonState.wasTouchingGround && firstPersonState.isTouchingGround) {
            smoothedMoveDirection = smoothedMoveDirection * Vector3.Dot(
                playerMomentum.getDirection().normalized * smoothedMoveDirection.magnitude,
                smoothedMoveDirection
            );
        }
    }

    /* Note: This method is distinct from characterController.isGrounded in that
     *       it detects floors a small distance beneath the player. This is very
     *       useful when determining whether to adjust the player's movement
     *       direction on a slope or to treat them as falling.
     */
    protected virtual bool checkForGroundBeneath() {

        // Setup Local Variables
        Vector3 raycastStart = transform.position + characterController.center;
        float sphereRadius = .33f; // TODO - Set To Player Width
        Vector3 raycastDirection = Vector3.down;
        float raycastLength = characterController.center.y + firstPersonMovementConfig.rayLength;

        // Check for Ground
        return Physics.SphereCast(
            raycastStart,
            sphereRadius,
            raycastDirection,
            out groundRaycastInfo,
            raycastLength
        );
    }

    /* Note: This method is distinct from checkForGroundBeneath in that it performs 
     *       a SphereCast of great length to register hits arbitrarily far beneath
     *       the player when standing on a sloped surface. The normal of this hit
     *       is then used to calculate and return the ground angle.
     */
    protected virtual float getGroundAngle() {

        // Setup Local Variables
        Vector3 raycastStart = transform.position + characterController.center;
        float sphereRadius = .33f; // TODO - Set To Player Width
        Vector3 raycastDirection = Vector3.down;
        float raycastLength = characterController.center.y + 2f; // TODO - Extract to Config Value.

        // Perform Ground Sphere Cast
        Physics.SphereCast(
            raycastStart,
            sphereRadius,
            raycastDirection,
            out groundRaycastInfo,
            raycastLength
        );

        return (firstPersonState.isTouchingGround ? Vector3.Angle(groundRaycastInfo.normal, Vector3.up) : 0f);
    }

    /* Note: Performs three sphere casts from player's horizontal center in the forward
     *       direction to check for wall collisions. This approach is pretty hacky and
     *       could benefit from a single-check replacement when I figure out why
     *       Physics.CheckCapsule doesn't work the way I'd expect.
     */
    protected virtual void updateWallCheck() {
        RaycastHit raycastInfo;
        bool wallCheck = false;

        if (moveInputState.hasInput && desiredMoveDirection.sqrMagnitude > 0) {

        	// Check Ahead of Feet
        	wallCheck = Physics.SphereCast(
                transform.position + new Vector3(0f, characterController.radius, 0f),
                characterController.radius,
                desiredMoveDirection,
                out raycastInfo,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : wallCheck;

        	// Check Ahead of Core
        	wallCheck = Physics.SphereCast(
                transform.position + characterController.center,
                characterController.radius,
                desiredMoveDirection,
                out raycastInfo,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : wallCheck;

            // Check Ahead of Head
            wallCheck = Physics.SphereCast(
                transform.position + new Vector3(0f, (firstPersonState.isCrouching ? crouchHeight : baseHeight), 0f) - new Vector3(0f, characterController.radius, 0f),
                characterController.radius,
                desiredMoveDirection,
                out raycastInfo,
                firstPersonMovementConfig.rayObstacleLength,
                firstPersonMovementConfig.obstacleLayers
            ) ? true : wallCheck;
    	}

    	firstPersonState.isTouchingWall = wallCheck;
    }


    /*--- Component Update Method ---*/

    protected virtual void updateComponents() {
        if (transformCameraHolder != null) transform.rotation = transformCameraHolder.rotation;

        // Note: This block is an easy fix for a bug in Unity's Character Controller that causes
        //       the player's vertical position to stutter when they jump toward a wall that's
        //       just shorter than their jump height. You don't want to know how long it took me
        //       to track this little monster down.
        if (firstPersonState.isTouchingGround) {
            characterController.slopeLimit = firstPersonMovementConfig.slideAngle;
            characterController.stepOffset = initialStepOffset;
        } else {
            characterController.slopeLimit = 90f;
            characterController.stepOffset = 0f;
        }

    }


    /*--- Movement Calculation Methods ---*/

    protected virtual void calculateInputMoveSpeed() {
        desiredMoveSpeed = firstPersonState.isRunning ? firstPersonMovementConfig.runSpeed : firstPersonMovementConfig.walkSpeed;
        desiredMoveSpeed = firstPersonState.isCrouching ? firstPersonMovementConfig.crouchSpeed : desiredMoveSpeed;
        desiredMoveSpeed = !moveInputState.hasInput ? 0f : desiredMoveSpeed;
    }

    protected virtual void calculateInputMoveDirection() {
        Vector3 verticalDirection = transform.forward * moveInputState.inputVector.y;
        Vector3 horizontalDirection = transform.right * moveInputState.inputVector.x;

        Vector3 desiredDirection = verticalDirection + horizontalDirection;
        //desiredDirection = flattenVectorOnSlopes(desiredDirection);

        desiredMoveDirection = desiredDirection;
    }

    // // Note: Far as I can tell, this block just breaks player movement on steep
    // //       angles & edges. Gonna comment it out for now and plan to remove it
    // //       during the final movement cleanup if I don't find its use.
    // protected virtual Vector3 flattenVectorOnSlopes(Vector3 vector) {
    //     if (firstPersonState.isGroundBeneath) // && firstPersonState.groundAngle > 45f)
    //         vector = Vector3.ProjectOnPlane(vector, groundRaycastInfo.normal);
    //     return vector;
    // }

    protected virtual void calculateFinalMoveSpeed() {

        // Update Smoothed Speed
        smoothedMoveSpeed = Mathf.Lerp(
            smoothedMoveSpeed,
            desiredMoveSpeed,
            firstPersonMovementConfig.smoothVelocitySpeed * Time.deltaTime
        );

        // Calculate Final Speed
        if (firstPersonState.isRunning) {
            float speedDifferenceWalkRun = firstPersonMovementConfig.runSpeed - firstPersonMovementConfig.walkSpeed;
            float walkRunPercent = Mathf.InverseLerp(
                firstPersonMovementConfig.walkSpeed,
                firstPersonMovementConfig.runSpeed,
                smoothedMoveSpeed
            );

            finalMoveSpeed = firstPersonMovementConfig.runTransitionCurve.Evaluate(walkRunPercent) * speedDifferenceWalkRun + firstPersonMovementConfig.walkSpeed;
        
        } else {

            finalMoveSpeed = smoothedMoveSpeed;
        }
    }

    protected virtual void calculateFinalMoveDirection() {

        // Update Smoothed Direction
        smoothedMoveDirection = Vector3.Lerp(
            smoothedMoveDirection,
            desiredMoveDirection,
            Time.deltaTime * firstPersonMovementConfig.smoothFinalDirectionSpeed
        );
        Debug.DrawRay(transform.position, desiredMoveDirection, Color.yellow);

        playerMomentum.updateMomentum(desiredMoveDirection, firstPersonMovementConfig);

        // Calculate Final Direction
        Vector3 finalVector = (firstPersonState.isTouchingGround || firstPersonState.isGroundBeneath
            ? smoothedMoveDirection * finalMoveSpeed
            : playerMomentum.getDirection());

        // Assign Axis Movements
        finalMoveDirection.x = finalVector.x;
        finalMoveDirection.z = finalVector.z;
        // Note: Below prevents extra y-velocity being added while in air.
        if (firstPersonState.isTouchingGround) finalMoveDirection.y += finalVector.y;
    }

    protected virtual void calculateVerticalMovement() {
        if (firstPersonState.isTouchingGround) {

            // Update State Variables
            firstPersonState.timeInAir = 0f;
            playerMomentum.reset();
            finalMoveDirection.y = -firstPersonMovementConfig.stickToGroundForce;

            // Handle Jump
            if (moveInputState.isJumpClicked && !firstPersonState.isCrouching
                    && firstPersonState.groundAngle < firstPersonMovementConfig.slideAngle) {
                finalMoveDirection.y += firstPersonMovementConfig.jumpSpeed;
                firstPersonState.isTouchingGround = false;
                playerMomentum.handleJump(finalMoveDirection);
            }

        } else {

            // Set Momentum Direction
            if (firstPersonState.wasTouchingGround) {
                playerMomentum.handleJump(finalMoveDirection);
            }

            // Apply Gravity
            firstPersonState.timeInAir += Time.deltaTime;
            finalMoveDirection += Physics.gravity * firstPersonMovementConfig.gravityMultiplier * Time.deltaTime;
        }
    }

    protected virtual void movePlayer(Vector3 movement) {
        characterController.Move(movement);
    }


    /*--- Land Methods ---*/

    protected virtual void updateLandAnimation() {
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
        Vector3 localPosition = transformCameraHolder.localPosition;
        float initialLandHeight = localPosition.y;

        // Calculate Land Amount
        landAmount = firstPersonState.timeInAir > firstPersonMovementConfig.landTimer ? firstPersonMovementConfig.highLandAmount : firstPersonMovementConfig.lowLandAmount;

        // Animate Landing
        while (percent < 1f) {
            percent += Time.deltaTime * speed;
            float desiredY = firstPersonMovementConfig.landCurve.Evaluate(percent) * landAmount;

            localPosition.y = initialLandHeight + desiredY;
            transformCameraHolder.localPosition = localPosition;

            yield return null;
        }
    }


    /*--- Crouch Methods ---*/

    protected virtual void updateCrouchAnimation() {
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
        Vector3 cameraPosition = transformCameraHolder.localPosition;
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
            transformCameraHolder.localPosition = cameraPosition;

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

    protected virtual void updateRunAnimation() {
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
        Vector3 moveDirection = smoothedMoveDirection != Vector3.zero
                                ? smoothedMoveDirection.normalized
                                : Vector3.zero;
        float moveDirectionFactor = Vector3.Dot(transform.forward, moveDirection);
        return moveDirectionFactor > firstPersonMovementConfig.runDirectionThreshold;
    }


    /*--- Head Bob & Sway Methods ---*/

    protected virtual void updateHeadBobAnimation() {
        if (moveInputState.hasInput && firstPersonState.isTouchingGround  && !firstPersonState.isTouchingWall) {
            if (!firstPersonState.isAnimatingCrouch) { // we want to make our head bob only if we are moving and not during crouch routine
                headBobManager.updateHeadBob(firstPersonState.isRunning, firstPersonState.isCrouching, moveInputState.inputVector);
                transformCameraHolder.localPosition = Vector3.Lerp(transformCameraHolder.localPosition, (Vector3.up * headBobManager.currentBaseHeight) + headBobManager.currentPositionOffset, Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
            }
        } else { // if we are not moving or we are not grounded
            if (!headBobManager.isReset) {
                headBobManager.resetHeadBob();
            }

            if (!firstPersonState.isAnimatingCrouch) // we want to reset our head bob only if we are standing still and not during crouch routine
                transformCameraHolder.localPosition = Vector3.Lerp(transformCameraHolder.localPosition, new Vector3(0f, headBobManager.currentBaseHeight, 0f), Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
        }

        //transformCamera.localPosition = Vector3.Lerp(transformCamera.localPosition,headBobManager.FinalOffset,Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
    }

    protected virtual void updateSwayAnimation() {
        cameraController.updateCameraSway(moveInputState.inputVector, moveInputState.inputVector.x);
    }
}
