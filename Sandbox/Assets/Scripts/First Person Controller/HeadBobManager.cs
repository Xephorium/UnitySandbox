using UnityEngine;


public class HeadBobManager {


    /*--- Variables ---*/

    public FirstPersonViewConfig firstPersonViewConfig;

    private float animationProgressX;
    private float animationProgressY;

    public Vector3 currentPositionOffset;
    public float currentBaseHeight = 0f;  // Changes When Crouched
    public bool isReset;


    /*--- Constructor ---*/

    public HeadBobManager(FirstPersonViewConfig config, float backwardsSpeedMultiplier, float sidewaysSpeedMultiplier) {
        firstPersonViewConfig = config;

        animationProgressX = 0f;
        animationProgressY = 0f;

        currentPositionOffset = Vector3.zero;
        isReset = true;

        firstPersonViewConfig.backwardsFrequencyMultiplier = backwardsSpeedMultiplier;
        firstPersonViewConfig.sidewaysFrequencyMultiplier = sidewaysSpeedMultiplier;
    }


    /*--- Methods ---*/

    public void updateHeadBob(bool isRunning, bool isCrouching, Vector2 moveInputVector) {
        isReset = false;

        float speedMultiplier;
        float amplitudeMultiplier;
        float frequencyMultiplier;
        float additionalMultiplier; // when moving backwards or to sides

        speedMultiplier = VectorUtility.calculateVectorStrength(moveInputVector);

        amplitudeMultiplier = isRunning ? firstPersonViewConfig.runAmplitudeMultiplier : 1f;
        amplitudeMultiplier = isCrouching ? firstPersonViewConfig.crouchAmplitudeMultiplier : amplitudeMultiplier;

        frequencyMultiplier = isRunning ? firstPersonViewConfig.runFrequencyMultiplier : 1f;
        frequencyMultiplier = isCrouching ? firstPersonViewConfig.crouchFrequencyMultiplier : frequencyMultiplier;
        frequencyMultiplier *= (speedMultiplier / 1f);

        additionalMultiplier = moveInputVector.y == -1 ? firstPersonViewConfig.backwardsFrequencyMultiplier : 1f;
        additionalMultiplier = moveInputVector.x != 0 & moveInputVector.y == 0 ? firstPersonViewConfig.sidewaysFrequencyMultiplier : additionalMultiplier;
        additionalMultiplier *= speedMultiplier;

        animationProgressX += Time.deltaTime * firstPersonViewConfig.xFrequency * frequencyMultiplier;
        animationProgressY += Time.deltaTime * firstPersonViewConfig.yFrequency * frequencyMultiplier;

        float finalFrequencyX = firstPersonViewConfig.xCurve.Evaluate(animationProgressX);
        float finalFrequencyY = firstPersonViewConfig.yCurve.Evaluate(animationProgressY);

        currentPositionOffset.x = finalFrequencyX * firstPersonViewConfig.xAmplitude * amplitudeMultiplier * additionalMultiplier;
        currentPositionOffset.y = finalFrequencyY * firstPersonViewConfig.yAmplitude * amplitudeMultiplier * additionalMultiplier;
    }

    public void resetHeadBob() {
        animationProgressX = 0f;
        animationProgressY = 0f;

        currentPositionOffset = Vector3.zero;
        isReset = true;
    }
}
