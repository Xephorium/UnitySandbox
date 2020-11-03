using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "FirstPersonMovementConfig", menuName = "Data/FirstPersonCharacter/MovementConfig", order = 0)]
public class FirstPersonMovementConfig : ScriptableObject {

    [Foldout("Base Movement")] public float crouchSpeed = 4f;
    [Foldout("Base Movement")] public float walkSpeed = 7f;
    [Foldout("Base Movement")] public float runSpeed = 9f;
    [Foldout("Base Movement")] public float jumpSpeed = 12f;
    [Foldout("Base Movement")] public float moveBackwardsSpeedPercent = 1f;
    [Foldout("Base Movement")] public float moveSideSpeedPercent = 1f;
    [Foldout("Base Movement")] public float gravityMultiplier = 2.5f;

    [Foldout("Run Animation")] public float runDirectionThreshold = 0.7f;
    [Foldout("Run Animation")] public float runInputThreshold = 0.7f;
    [Foldout("Run Animation")] public AnimationCurve runTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Foldout("Crouch Animation")] public float crouchPercent = 0.7f;
    [Foldout("Crouch Animation")] public float crouchTransitionDuration = 0.2f;
    [Foldout("Crouch Animation")] public AnimationCurve crouchTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Foldout("Landing Animation")] public float lowLandAmount = 0f;
    [Foldout("Landing Animation")] public float highLandAmount = 0.17f;
    [Foldout("Landing Animation")] public float landTimer = 0.5f;
    [Foldout("Landing Animation")] public float landDuration = 0.3f;
    [Foldout("Landing Animation")] public AnimationCurve landCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Foldout("Ground Check")] public float stickToGroundForce = 1f;
    [Foldout("Ground Check")] public LayerMask groundLayer = ~0;
    [Foldout("Ground Check")] public float rayLength = 0.1f;
    [Foldout("Ground Check")] public float raySphereRadius = 0.1f;

    [Foldout("Wall Check")] public LayerMask obstacleLayers = ~0;
    [Foldout("Wall Check")] public float rayObstacleLength = 0.4f;
    [Foldout("Wall Check")] public float rayObstacleSphereRadius = 0.2f;

    [Foldout("Smoothing")] [Range(1f, 100f)]public float smoothRotateSpeed = 100f;
    [Foldout("Smoothing")] [Range(1f, 100f)]public float smoothInputSpeed = 1f;
    [Foldout("Smoothing")] [Range(1f, 100f)]public float smoothVelocitySpeed = 10f;
    [Foldout("Smoothing")] [Range(1f, 100f)]public float smoothFinalDirectionSpeed = 10f;
    [Foldout("Smoothing")] [Range(1f, 100f)]public float smoothHeadBobSpeed = 5f;
}
