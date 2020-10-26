using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "HeadBobConfig", menuName = "HeadBobConfig", order = 3)]
public class HeadBobConfig : ScriptableObject {


    /*--- Variables ---*/

    [BoxGroup("Curves")] public AnimationCurve xCurve;
    [BoxGroup("Curves")] public AnimationCurve yCurve;

    [Space]
    [BoxGroup("Amplitude")] public float xAmplitude;
    [BoxGroup("Amplitude")] public float yAmplitude;

    [Space]
    [BoxGroup("Frequency")] public float xFrequency;
    [BoxGroup("Frequency")] public float yFrequency;

    [Space]
    [BoxGroup("Run Multipliers")] public float runAmplitudeMultiplier;
    [BoxGroup("Run Multipliers")] public float runFrequencyMultiplier;

    [Space]
    [BoxGroup("Crouch Multipliers")] public float crouchAmplitudeMultiplier;
    [BoxGroup("Crouch Multipliers")] public float crouchFrequencyMultiplier;

    [Space]
    [BoxGroup("Directional Frequency Multipliers")] [EnableIf("NotEnabled")] public float backwardsFrequencyMultiplier;
    [BoxGroup("Directional Frequency Multipliers")] [EnableIf("NotEnabled")] public float sidewaysFrequencyMultiplier;
}
