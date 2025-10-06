using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIFloatWave : MonoBehaviour
{
    [SerializeField] UIFloatWaveSettings settings;

    RectTransform rect;
    Vector2 basePos;
    Tween tY;
    Tween tX;
    Tween tR;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        basePos = rect.anchoredPosition;
    }

    void OnEnable()
    {
        tY = rect.DOAnchorPosY(basePos.y + settings.floatHeight, settings.floatDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        tX = rect.DOAnchorPosX(basePos.x + settings.waveWidth, settings.waveDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        tR = rect.DOLocalRotate(new Vector3(0, 0, settings.waveAngle), settings.angleDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    void OnDisable()
    {
        if (tY != null && tY.IsActive()) tY.Kill();
        if (tX != null && tX.IsActive()) tX.Kill();
        if (tR != null && tR.IsActive()) tR.Kill();
    }
}
