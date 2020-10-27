using System.Collections;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {


    /*--- Variables ---*/

    [Space, Header("Data")]
    [SerializeField] private MoveInputState moveInputState = null;
    [SerializeField] private FirstPersonViewConfig firstPersonViewConfig = null;
    [SerializeField] private FirstPersonMovementConfig firstPersonMovementConfig = null;

    private CharacterController m_characterController;
    private Transform m_yawTransform;
    private Transform m_camTransform;
    private HeadBobManager headBobManager;
    private CameraController m_cameraController;

    private RaycastHit m_hitInfo;
    private IEnumerator m_CrouchRoutine;
    private IEnumerator m_LandRoutine;

    [SerializeField] [ShowIf("NeverShow")] private Vector2 m_inputVector;
    [SerializeField] [ShowIf("NeverShow")] private Vector2 m_smoothInputVector;

    [SerializeField] [ShowIf("NeverShow")] private Vector3 m_finalMoveDir;
    [SerializeField] [ShowIf("NeverShow")] private Vector3 m_smoothFinalMoveDir;
    [SerializeField] [ShowIf("NeverShow")] private Vector3 m_finalMoveVector;

    [SerializeField] [ShowIf("NeverShow")] private float m_currentSpeed;
    [SerializeField] [ShowIf("NeverShow")] private float m_smoothCurrentSpeed;
    [SerializeField] [ShowIf("NeverShow")] private float m_finalSmoothCurrentSpeed;
    [SerializeField] [ShowIf("NeverShow")] private float m_walkRunSpeedDifference;

    [SerializeField] [ShowIf("NeverShow")] private float m_finalRayLength;
    [SerializeField] [ShowIf("NeverShow")] private bool m_hitWall;
    [SerializeField] [ShowIf("NeverShow")] private bool m_isGrounded;
    [SerializeField] [ShowIf("NeverShow")] private bool m_previouslyGrounded;

    [SerializeField] [ShowIf("NeverShow")] private float m_initHeight;
    [SerializeField] [ShowIf("NeverShow")] private float m_crouchHeight;
    [SerializeField] [ShowIf("NeverShow")] private Vector3 m_initCenter;
    [SerializeField] [ShowIf("NeverShow")] private Vector3 m_crouchCenter;

    [SerializeField] [ShowIf("NeverShow")] private float m_initCamHeight;
    [SerializeField] [ShowIf("NeverShow")] private float m_crouchCamHeight;
    [SerializeField] [ShowIf("NeverShow")] private float m_crouchStandHeightDifference;
    [SerializeField] [ShowIf("NeverShow")] private bool m_duringCrouchAnimation;
    [SerializeField] [ShowIf("NeverShow")] private bool m_duringRunAnimation;

    [SerializeField] [ShowIf("NeverShow")] private float m_inAirTimer;


    /*--- Lifecycle Methods ---*/

    protected virtual void Start() {
        GetComponents();
        InitVariables();
    }

    protected virtual void Update() {
        if(m_yawTransform != null)
            RotateTowardsCamera();

        if(m_characterController) {
            // Check if Grounded,Wall etc
            CheckIfGrounded();
            CheckIfWall();

            // Apply Smoothing
            SmoothInput();
            SmoothSpeed();
            SmoothDir();

            // Calculate Movement
            CalculateMovementDirection();
            CalculateSpeed();
            CalculateFinalMovement();

            // Handle Player Movement, Gravity, Jump, Crouch etc.
            HandleCrouch();
            HandleHeadBob();
            HandleRunFOV();
            HandleCameraSway();
            HandleLanding();

            ApplyGravity();
            ApplyMovement();

            m_previouslyGrounded = m_isGrounded;
        }
    }

    /*

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere((transform.position + m_characterController.center) - Vector3.up * m_finalRayLength, firstPersonMovementConfig.raySphereRadius);
        }

     */


    /*--- Setup Methods ---*/

    protected virtual void GetComponents() {
        m_characterController = GetComponent<CharacterController>();
        m_cameraController = GetComponentInChildren<CameraController>();
        m_yawTransform = m_cameraController.transform;
        m_camTransform = GetComponentInChildren<Camera>().transform;
        headBobManager = new HeadBobManager(firstPersonViewConfig, firstPersonMovementConfig.moveBackwardsSpeedPercent, firstPersonMovementConfig.moveSideSpeedPercent);
    }

    protected virtual void InitVariables() {
        // Calculate where our character center should be based on height and skin width
        m_characterController.center = new Vector3(0f, m_characterController.height / 2f + m_characterController.skinWidth, 0f);

        m_initCenter = m_characterController.center;
        m_initHeight = m_characterController.height;

        m_crouchHeight = m_initHeight * firstPersonMovementConfig.crouchPercent;
        m_crouchCenter = (m_crouchHeight / 2f + m_characterController.skinWidth) * Vector3.up;

        m_crouchStandHeightDifference = m_initHeight - m_crouchHeight;

        m_initCamHeight = m_yawTransform.localPosition.y;
        m_crouchCamHeight = m_initCamHeight - m_crouchStandHeightDifference;

        // Sphere radius not included. If you want it to be included just decrease by sphere radius at the end of this equation
        m_finalRayLength = firstPersonMovementConfig.rayLength + m_characterController.center.y;

        m_isGrounded = true;
        m_previouslyGrounded = true;

        m_inAirTimer = 0f;
        headBobManager.currentBaseHeight = m_initCamHeight;

        m_walkRunSpeedDifference = firstPersonMovementConfig.runSpeed - firstPersonMovementConfig.walkSpeed;
    }


    /*--- Smoothing Methods ---*/

    protected virtual void SmoothInput() {
        m_inputVector = moveInputState.inputVector.normalized;
        m_smoothInputVector = m_inputVector;     //Vector2.Lerp(m_smoothInputVector,m_inputVector,Time.deltaTime * firstPersonMovementConfig.smoothInputSpeed);
        //Debug.DrawRay(transform.position, new Vector3(m_smoothInputVector.x,0f,m_smoothInputVector.y), Color.green);
    }

    protected virtual void SmoothSpeed() {
        m_smoothCurrentSpeed = Mathf.Lerp(m_smoothCurrentSpeed, m_currentSpeed, Time.deltaTime * firstPersonMovementConfig.smoothVelocitySpeed);

        if(moveInputState.isRunning && CanRun()) {
            float _walkRunPercent = Mathf.InverseLerp(firstPersonMovementConfig.walkSpeed, firstPersonMovementConfig.runSpeed, m_smoothCurrentSpeed);
            m_finalSmoothCurrentSpeed = firstPersonMovementConfig.runTransitionCurve.Evaluate(_walkRunPercent) * m_walkRunSpeedDifference + firstPersonMovementConfig.walkSpeed;
        }
        else
        {
            m_finalSmoothCurrentSpeed = m_smoothCurrentSpeed;
        }
    }

    protected virtual void SmoothDir() {

        m_smoothFinalMoveDir = Vector3.Lerp(m_smoothFinalMoveDir, m_finalMoveDir, Time.deltaTime * firstPersonMovementConfig.smoothFinalDirectionSpeed);
        Debug.DrawRay(transform.position, m_smoothFinalMoveDir, Color.yellow);
    }


    /*--- Movement Calculation Methods ---*/

    protected virtual void CheckIfGrounded() {
        Vector3 _origin = transform.position + m_characterController.center;

        bool _hitGround = Physics.SphereCast(_origin, firstPersonMovementConfig.raySphereRadius, Vector3.down, out m_hitInfo, m_finalRayLength, firstPersonMovementConfig.groundLayer);
        Debug.DrawRay(_origin, Vector3.down * (m_finalRayLength), Color.red);

        m_isGrounded = _hitGround ? true : false;
    }

    protected virtual void CheckIfWall() {  // TODO - Fix. Does not prevent head bob.

        Vector3 _origin = transform.position + m_characterController.center;
        RaycastHit _wallInfo;

        bool _hitWall = false;

        if(moveInputState.hasInput && m_finalMoveDir.sqrMagnitude > 0)
            _hitWall = Physics.SphereCast(_origin, firstPersonMovementConfig.rayObstacleSphereRadius, m_finalMoveDir, out _wallInfo, firstPersonMovementConfig.rayObstacleLength, firstPersonMovementConfig.obstacleLayers);
        Debug.DrawRay(_origin, m_finalMoveDir * firstPersonMovementConfig.rayObstacleLength, Color.blue);

        m_hitWall = _hitWall ? true : false;
    }

    protected virtual bool CheckIfRoof() {  // TODO - Fix. Does not accurately detect roof collision.
        Vector3 _origin = transform.position;
        RaycastHit _roofInfo;

        bool _hitRoof = Physics.SphereCast(_origin, firstPersonMovementConfig.raySphereRadius, Vector3.up, out _roofInfo, m_initHeight);

        return false;     // _hitRoof;
    }

    protected virtual bool CanRun() {
        Vector3 _normalizedDir = Vector3.zero;

        if(m_smoothFinalMoveDir != Vector3.zero)
            _normalizedDir = m_smoothFinalMoveDir.normalized;

        float _dot = Vector3.Dot(transform.forward, _normalizedDir);
        return _dot >= firstPersonMovementConfig.canRunThreshold && !moveInputState.isCrouching ? true : false;
    }

    protected virtual void CalculateMovementDirection() {

        Vector3 _vDir = transform.forward * m_smoothInputVector.y;
        Vector3 _hDir = transform.right * m_smoothInputVector.x;

        Vector3 _desiredDir = _vDir + _hDir;
        Vector3 _flattenDir = FlattenVectorOnSlopes(_desiredDir);

        m_finalMoveDir = _flattenDir;
    }

    protected virtual Vector3 FlattenVectorOnSlopes(Vector3 _vectorToFlat) {
        if(m_isGrounded)
            _vectorToFlat = Vector3.ProjectOnPlane(_vectorToFlat, m_hitInfo.normal);

        return _vectorToFlat;
    }

    protected virtual void CalculateSpeed() {
        m_currentSpeed = moveInputState.isRunning && CanRun() ? firstPersonMovementConfig.runSpeed : firstPersonMovementConfig.walkSpeed;
        m_currentSpeed = moveInputState.isCrouching ? firstPersonMovementConfig.crouchSpeed : m_currentSpeed;
        m_currentSpeed = !moveInputState.hasInput ? 0f : m_currentSpeed;
        m_currentSpeed = moveInputState.inputVector.y == -1 ? m_currentSpeed * firstPersonMovementConfig.moveBackwardsSpeedPercent : m_currentSpeed;
        m_currentSpeed = moveInputState.inputVector.x != 0 && moveInputState.inputVector.y ==  0 ? m_currentSpeed * firstPersonMovementConfig.moveSideSpeedPercent :  m_currentSpeed;
    }

    protected virtual void CalculateFinalMovement() {
        float _smoothInputVectorMagnitude = 1f;
        Vector3 _finalVector = m_smoothFinalMoveDir * m_finalSmoothCurrentSpeed * _smoothInputVectorMagnitude;

        // We have to assign individually in order to make our character jump properly because before it was overwriting Y value and that's why it was jerky now we are adding to Y value and it's working
        m_finalMoveVector.x = _finalVector.x;
        m_finalMoveVector.z = _finalVector.z;

        if(m_characterController.isGrounded)     // Thanks to this check we are not applying extra y velocity when in air so jump will be consistent
            m_finalMoveVector.y += _finalVector.y;  //so this makes our player go in forward dir using slope normal but when jumping this is making it go higher so this is weird
    }


    /*--- Crouch Methods ---*/

    protected virtual void HandleCrouch() {
        if(moveInputState.isCrouchClicked && m_isGrounded)
            InvokeCrouchRoutine();
    }

    protected virtual void InvokeCrouchRoutine() {
        if(moveInputState.isCrouching)
            if(CheckIfRoof())
                return;

        if(m_LandRoutine != null)
            StopCoroutine(m_LandRoutine);

        if(m_CrouchRoutine != null)
            StopCoroutine(m_CrouchRoutine);

        m_CrouchRoutine = CrouchRoutine();
        StartCoroutine(m_CrouchRoutine);
    }

    protected virtual IEnumerator CrouchRoutine() {
        m_duringCrouchAnimation = true;

        float _percent = 0f;
        float _smoothPercent = 0f;
        float _speed = 1f / firstPersonMovementConfig.crouchTransitionDuration;

        float _currentHeight = m_characterController.height;
        Vector3 _currentCenter = m_characterController.center;

        float _desiredHeight = moveInputState.isCrouching ? m_initHeight : m_crouchHeight;
        Vector3 _desiredCenter = moveInputState.isCrouching ? m_initCenter : m_crouchCenter;

        Vector3 _camPos = m_yawTransform.localPosition;
        float _camCurrentHeight = _camPos.y;
        float _camDesiredHeight = moveInputState.isCrouching ? m_initCamHeight : m_crouchCamHeight;

        moveInputState.isCrouching = !moveInputState.isCrouching;
        headBobManager.currentBaseHeight = moveInputState.isCrouching ? m_crouchCamHeight : m_initCamHeight;

        while(_percent < 1f) {
            _percent += Time.deltaTime * _speed;
            _smoothPercent = firstPersonMovementConfig.crouchTransitionCurve.Evaluate(_percent);

            m_characterController.height = Mathf.Lerp(_currentHeight, _desiredHeight, _smoothPercent);
            m_characterController.center = Vector3.Lerp(_currentCenter, _desiredCenter, _smoothPercent);

            _camPos.y = Mathf.Lerp(_camCurrentHeight, _camDesiredHeight, _smoothPercent);
            m_yawTransform.localPosition = _camPos;

            yield return null;
        }

        m_duringCrouchAnimation = false;
    }


    /*--- Landing Methods ---*/

    protected virtual void HandleLanding() {
        if(!m_previouslyGrounded && m_isGrounded) {
            InvokeLandingRoutine();
        }
    }

    protected virtual void InvokeLandingRoutine() {
        if(m_LandRoutine != null)
            StopCoroutine(m_LandRoutine);

        m_LandRoutine = LandingRoutine();
        StartCoroutine(m_LandRoutine);
    }

    protected virtual IEnumerator LandingRoutine() {
        float _percent = 0f;
        float _landAmount = 0f;

        float _speed = 1f / firstPersonMovementConfig.landDuration;

        Vector3 _localPos = m_yawTransform.localPosition;
        float _initLandHeight = _localPos.y;

        _landAmount = m_inAirTimer > firstPersonMovementConfig.landTimer ? firstPersonMovementConfig.highLandAmount : firstPersonMovementConfig.lowLandAmount;

        while(_percent < 1f) {
            _percent += Time.deltaTime * _speed;
            float _desiredY = firstPersonMovementConfig.landCurve.Evaluate(_percent) * _landAmount;

            _localPos.y = _initLandHeight + _desiredY;
            m_yawTransform.localPosition = _localPos;

            yield return null;
        }
    }

    
    /*--- Movement Methods ---*/

    protected virtual void HandleHeadBob() {

        if(moveInputState.hasInput && m_isGrounded  && !m_hitWall) {
            if(!m_duringCrouchAnimation) { // we want to make our head bob only if we are moving and not during crouch routine
                headBobManager.updateHeadBob(moveInputState.isRunning && CanRun(), moveInputState.isCrouching, moveInputState.inputVector);
                m_yawTransform.localPosition = Vector3.Lerp(m_yawTransform.localPosition, (Vector3.up * headBobManager.currentBaseHeight) + headBobManager.currentPositionOffset, Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
            }
        }
        else     // if we are not moving or we are not grounded
        {
            if(!headBobManager.isReset) {
                headBobManager.resetHeadBob();
            }

            if(!m_duringCrouchAnimation) // we want to reset our head bob only if we are standing still and not during crouch routine
                m_yawTransform.localPosition = Vector3.Lerp(m_yawTransform.localPosition, new Vector3(0f, headBobManager.currentBaseHeight, 0f), Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
        }

        //m_camTransform.localPosition = Vector3.Lerp(m_camTransform.localPosition,headBobManager.FinalOffset,Time.deltaTime * firstPersonMovementConfig.smoothHeadBobSpeed);
    }

    protected virtual void HandleCameraSway() {
        m_cameraController.updateCameraSway(m_smoothInputVector, moveInputState.inputVector.x);
    }

    protected virtual void HandleRunFOV() {
        if(moveInputState.hasInput && m_isGrounded  && !m_hitWall) {
            if(moveInputState.isRunClicked && CanRun()) {
                m_duringRunAnimation = true;
                m_cameraController.updateRunFov(false);
            }

            if(moveInputState.isRunning && CanRun() && !m_duringRunAnimation ) {
                m_duringRunAnimation = true;
                m_cameraController.updateRunFov(false);
            }
        }

        if(moveInputState.isRunReleased || !moveInputState.hasInput || m_hitWall) {
            if(m_duringRunAnimation) {
                m_duringRunAnimation = false;
                m_cameraController.updateRunFov(true);
            }
        }
    }
    protected virtual void HandleJump() {
        if(moveInputState.isJumpClicked && !moveInputState.isCrouching) {
            m_finalMoveVector.y += firstPersonMovementConfig.jumpSpeed /* m_currentSpeed */; // we are adding because ex. when we are going on slope we want to keep Y value not overwriting it
            //m_finalMoveVector.y = firstPersonMovementConfig.jumpSpeed /* m_currentSpeed */; // turns out that when adding to Y it is too much and it doesn't feel correct because jumping on slope is much faster and higher;

            m_previouslyGrounded = true;
            m_isGrounded = false;
        }
    }
    protected virtual void ApplyGravity() {
        if(m_characterController.isGrounded) {   // if we would use our own m_isGrounded it would not work that good, this one is more precise
            m_inAirTimer = 0f;
            m_finalMoveVector.y = -firstPersonMovementConfig.stickToGroundForce;

            HandleJump();
        }
        else
        {
            m_inAirTimer += Time.deltaTime;
            m_finalMoveVector += Physics.gravity * firstPersonMovementConfig.gravityMultiplier * Time.deltaTime;
        }
    }

    protected virtual void ApplyMovement() {
        m_characterController.Move(m_finalMoveVector * Time.deltaTime);
    }

    protected virtual void RotateTowardsCamera() {
        Quaternion _currentRot = transform.rotation;
        Quaternion _desiredRot = m_yawTransform.rotation;

        transform.rotation = Quaternion.Slerp(_currentRot, _desiredRot, Time.deltaTime * firstPersonMovementConfig.smoothRotateSpeed);
    }
}
