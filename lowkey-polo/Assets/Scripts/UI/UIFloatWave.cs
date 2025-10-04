using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIFloatWave : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] UIFloatWaveSettings settings;

    RectTransform rect;
    Vector2 basePos;
    Quaternion baseRot;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        basePos = rect.anchoredPosition;
        baseRot = rect.localRotation;
    }

    void OnEnable()
    {
        rect.DOAnchorPosY(basePos.y + settings.floatHeight, settings.floatDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        rect.DOAnchorPosX(basePos.x + settings.waveWidth, settings.waveDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        rect.DOLocalRotate(new Vector3(0, 0, settings.waveAngle), settings.angleDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    void OnDisable()
    {
        DOTween.Kill(rect);
    }
}
