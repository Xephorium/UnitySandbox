using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "FirstPersonViewConfig", menuName = "Data/FirstPersonCharacter/ViewConfig", order = 0)]
public class FirstPersonViewConfig : ScriptableObject {

	[Foldout("General")] [Label("FOV")] [Range(50f, 120f)]public float defaultFOV = 80f;
	[Foldout("General")] public Vector2 lookSensitivity = new Vector2(130f, 130f);
	[Foldout("General")] [Label("Vertical Angle Clamp")] [MinMaxSlider(-90f,90f)] public Vector2 verticalAngleClamp = new Vector2(-90f, 90f);

    [Foldout("Run")] [Range(60f, 150f)] public float runFOV = 90f;
    [Foldout("Run")] public AnimationCurve runCurve = new AnimationCurve();
    [Foldout("Run")] public float runTransitionDuration = 0f;
    [Foldout("Run")] public float runReturnTransitionDuration = 0f;

    [Foldout("Zoom")] [Range(20f, 60f)] public float zoomFOV = 55f;
    [Foldout("Zoom")] public AnimationCurve zoomCurve = new AnimationCurve();
    [Foldout("Zoom")] public float zoomTransitionDuration = 0f;

	[Header("Curves")]
    [Foldout("HeadBob")] public AnimationCurve xCurve;
    [Foldout("HeadBob")] public AnimationCurve yCurve;
    [Header("Amplitude")]
    [Foldout("HeadBob")] public float xAmplitude = 0.05f;
    [Foldout("HeadBob")] public float yAmplitude = 0.04f;
    [Header("Frequency")]
    [Foldout("HeadBob")] public float xFrequency = 1.42f;
    [Foldout("HeadBob")] public float yFrequency = 2.84f;
    [Header("Run Multipliers")]
    [Foldout("HeadBob")] public float runAmplitudeMultiplier = 2f;
    [Foldout("HeadBob")] public float runFrequencyMultiplier = 1.05f;
    [Header("Crouch Multipliers")] 
    [Foldout("HeadBob")] public float crouchAmplitudeMultiplier = 0.2f;
    [Foldout("HeadBob")] public float crouchFrequencyMultiplier = 1.0f;
    //[ShowIf("NeverShow")] [Header("Directional Frequency Multipliers (Synced to Speed Multipliers)")]
    [Foldout("HeadBob")] [ShowIf("NeverShow")] public float backwardsFrequencyMultiplier;
    [Foldout("HeadBob")] [ShowIf("NeverShow")] public float sidewaysFrequencyMultiplier;

    [Foldout("Sway")] public float swayAmount = 0.14f;
    [Foldout("Sway")] public float swaySpeed = 5f;
    [Foldout("Sway")] public float returnSpeed = 5f;
    [Foldout("Sway")] public float changeDirectionMultiplier = 4f;
    [Foldout("Sway")] public AnimationCurve swayCurve = new AnimationCurve();

}
