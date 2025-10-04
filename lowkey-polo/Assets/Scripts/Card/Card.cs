using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Coffee.UIEffects;
using System.Collections;

public class Card : MonoBehaviour, IPoolable, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;
    [SerializeField] private UIEffect uiEffect;

    [Header("Animation Settings")]
    [SerializeField] private float flipDuration = 0.4f;
    [SerializeField] private float wrongMatchDelay = 0.6f;
    [SerializeField] private float autoFlipDelay = 1.0f;
    [SerializeField] private float hoverScale = 1.10f;
    [SerializeField] private float baseScale = 1f;
    [SerializeField] private float slapScale = 1.12f;
    [SerializeField] private float slapDuration = 0.18f;
    [SerializeField] private float slapReturnDuration = 0.13f;
    [SerializeField] private float matchGrowScale = 1.16f;
    [SerializeField] private float matchGrowDuration = 0.18f;
    [SerializeField] private float matchShrinkDuration = 0.18f;

    [Header("Effect Settings")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float mismatchAnimDuration = 0.3f;

    [Header("Effect Presets")]
    [SerializeField] private UIEffectPreset hoverPreset;
    [SerializeField] private UIEffectPreset matchPreset;
    [SerializeField] private UIEffectPreset mismatchPreset;

    public CardData cardData { get; private set; }
    public CardState CurrentState { get; private set; }

    private bool _isFlipping;
    private bool _isAnimating;
    private Coroutine _autoFlipCoroutine;
    private GameManager _gameManager;
    private Vector3 _originalPosition;
    private Tween _effectTween;
    private UIEffectPreset _currentAppliedPreset;

    void Awake()
    {
        if (cardButton != null)
            cardButton.onClick.AddListener(OnCardClicked);
        CurrentState = CardState.FaceDown;
        _originalPosition = transform.localPosition;
        ResetEffects();
    }

    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
        cardImage.sprite = cardData.cardBackSprite;
        CurrentState = CardState.FaceDown;
        transform.localScale = Vector3.one * baseScale;
        transform.localPosition = _originalPosition;
        cardButton.interactable = true;
        _isFlipping = false;
        _isAnimating = false;
        CancelAutoFlip();
        ResetEffects();
    }

    private void OnCardClicked()
    {
        if (_isFlipping || CurrentState != CardState.FaceDown || _gameManager == null || !_gameManager.CanFlipCard(this)) return;
        FlipUp();
        GameEvents.CardFlipped(this);
    }

    public void FlipUp()
    {
        if (_isFlipping || CurrentState != CardState.FaceDown) return;
        CancelAutoFlip();
        StartCoroutine(DOTweenFlipRoutine(true));
        _autoFlipCoroutine = StartCoroutine(AutoFlipBackRoutine());
    }

    public void FlipDown()
    {
        if (_isFlipping || CurrentState != CardState.FaceUp) return;
        CancelAutoFlip();
        StartCoroutine(DOTweenFlipRoutine(false));
    }

    public void ForceFlipToBack()
    {
        CancelAutoFlip();
        if (_isFlipping) return;
        StartCoroutine(DOTweenFlipRoutine(false));
    }

    private IEnumerator DOTweenFlipRoutine(bool flipToFront)
    {
        _isFlipping = true;
        cardButton.interactable = false;

        yield return transform.DOScaleX(0f, flipDuration / 2f).SetEase(Ease.InOutSine).WaitForCompletion();

        cardImage.sprite = flipToFront ? cardData.cardFaceSprite : cardData.cardBackSprite;
        CurrentState = flipToFront ? CardState.FaceUp : CardState.FaceDown;

        if (flipToFront)
        {
            yield return transform.DOScaleX(1f, flipDuration / 2f).SetEase(Ease.InOutSine).WaitForCompletion();
            yield return transform.DOScale(new Vector3(slapScale, slapScale, 1f), slapDuration).SetEase(Ease.OutBounce).WaitForCompletion();
            yield return transform.DOScale(Vector3.one * baseScale, slapReturnDuration).SetEase(Ease.InOutSine).WaitForCompletion();
        }
        else
        {
            yield return transform.DOScale(Vector3.one * baseScale, flipDuration / 2f).SetEase(Ease.InOutSine).WaitForCompletion();
        }

        cardButton.interactable = CurrentState == CardState.FaceDown;
        _isFlipping = false;
    }

    public void OnMatchFound()
    {
        CurrentState = CardState.Matched;
        cardButton.interactable = false;
        CancelAutoFlip();
        StartCoroutine(MatchEffectRoutine());
        ShakeScreen();
    }

    private IEnumerator MatchEffectRoutine()
    {
        _isAnimating = true;
        ApplyPresetWithBlend(matchPreset, 0f, 1f, matchGrowDuration + matchShrinkDuration);
        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(matchGrowScale, matchGrowDuration).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(baseScale, matchShrinkDuration).SetEase(Ease.InBack));
        yield return seq.WaitForCompletion();
        _isAnimating = false;
        ApplyPresetWithBlend(matchPreset, 1f, 0f, 0.2f, true);
    }

    public void OnWrongMatch()
    {
        CancelAutoFlip();
        StartCoroutine(WrongMatchRoutine());
    }

    private IEnumerator WrongMatchRoutine()
    {
        _isAnimating = true;
        ApplyPresetWithBlend(mismatchPreset, 0f, 1f, mismatchAnimDuration * 0.5f);
        int shakeCount = 6;
        float interval = mismatchAnimDuration / shakeCount;
        for (int i = 0; i < shakeCount; i++)
        {
            Vector3 shakeOffset = new Vector3(Random.Range(-shakeIntensity, shakeIntensity), Random.Range(-shakeIntensity, shakeIntensity), 0f);
            transform.localPosition = _originalPosition + shakeOffset;
            yield return new WaitForSeconds(interval);
        }
        transform.localPosition = _originalPosition;
        ApplyPresetWithBlend(mismatchPreset, 1f, 0f, mismatchAnimDuration * 0.5f, true);
        _isAnimating = false;
        yield return new WaitForSeconds(wrongMatchDelay);
        FlipDown();
    }

    private IEnumerator AutoFlipBackRoutine()
    {
        yield return new WaitForSeconds(autoFlipDelay);
        if (_gameManager != null && CurrentState == CardState.FaceUp)
            _gameManager.CheckSingleCardAutoFlip(this);
        _autoFlipCoroutine = null;
    }

    public void CancelAutoFlip()
    {
        if (_autoFlipCoroutine != null)
        {
            StopCoroutine(_autoFlipCoroutine);
            _autoFlipCoroutine = null;
        }
    }

    public void OnPoolGet()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.one * baseScale;
        transform.localPosition = _originalPosition;
        _isFlipping = false;
        _isAnimating = false;
        CancelAutoFlip();
        ResetEffects();
    }

    public void OnPoolReturn()
    {
        gameObject.SetActive(false);
        CancelAutoFlip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentState == CardState.FaceDown && !_isFlipping && !_isAnimating)
        {
            transform.DOScale(Vector3.one * hoverScale, 0.17f).SetEase(Ease.OutBack);
            StartHoverEffect();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHoverEffect();
        if (CurrentState == CardState.FaceDown && !_isFlipping && !_isAnimating)
        {
            transform.DOScale(Vector3.one * baseScale, 0.17f).SetEase(Ease.InBack);
        }
    }

    private void StartHoverEffect()
    {
        ApplyPresetWithBlend(hoverPreset, 0f, 1f, 0.17f);
    }

    private void StopHoverEffect()
    {
        ApplyPresetWithBlend(hoverPreset, 1f, 0f, 0.17f, true);
    }

    private void ApplyPresetWithBlend(UIEffectPreset preset, float from, float to, float duration, bool disableOnComplete = false)
    {
        if (uiEffect == null || preset == null) return;
        if (_effectTween != null && _effectTween.IsActive())
            _effectTween.Kill();
        _currentAppliedPreset = preset;
        uiEffect.LoadPreset(preset);
        uiEffect.enabled = true;
        uiEffect.transitionRate = from;
        _effectTween = DOTween.To(() => uiEffect.transitionRate, x => uiEffect.transitionRate = x, to, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                if (disableOnComplete && Mathf.Approximately(uiEffect.transitionRate, 0f))
                {
                    uiEffect.enabled = false;
                }
            });
    }

    private void ResetEffects()
    {
        if (_effectTween != null && _effectTween.IsActive())
        {
            _effectTween.Kill();
            _effectTween = null;
        }
        _currentAppliedPreset = null;
        if (uiEffect == null) return;
        uiEffect.enabled = false;
        uiEffect.transitionRate = 0f;
    }

    public static void ShakeScreen(float duration = 0.16f, float strength = 15f)
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.transform != null)
            mainCam.transform.DOShakePosition(duration, strength, 14, 90, false, true);
    }
}
