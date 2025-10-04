using UnityEngine;

[CreateAssetMenu(fileName = "UIFloatWaveSettings", menuName = "UI/UIFloatWave Settings")]
public class UIFloatWaveSettings : ScriptableObject
{
    [Header("Float Animation")]
    public float floatHeight = 20f;
    public float floatDuration = 2.0f;

    [Header("Wave Animation")]
    public float waveWidth = 32f;
    public float waveDuration = 1.8f;

    [Header("Rotation (Wobble)")]
    public float waveAngle = 8f;
    public float angleDuration = 1.2f;
}