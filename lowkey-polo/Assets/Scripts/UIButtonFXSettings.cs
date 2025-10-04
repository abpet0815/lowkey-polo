using UnityEngine;

[CreateAssetMenu(fileName = "UIButtonFXSettings", menuName = "UI/UIButtonFX Settings")]
public class UIButtonFXSettings : ScriptableObject
{
    [Header("Animation")]
    public float hoverScale = 1.08f;
    public float pressScale = 0.94f;
    public float tweenDuration = 0.15f;

    [Header("Highlighting")]
    public Color highlightColor = new Color(1,1,1,0.15f);
    public Color pressedColor = new Color(1,1,1,0.28f);

    [Header("SFX")]
    public AudioClip hoverClip;
    public AudioClip pressClip;
}