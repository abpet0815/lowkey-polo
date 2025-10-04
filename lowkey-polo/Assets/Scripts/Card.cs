using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;

public class Card : MonoBehaviour, IPoolable, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;
    [SerializeField] private ParticleSystem hoverVFXPrefab;
    [SerializeField] private ParticleSystem matchEffect;
    [SerializeField] private ParticleSystem mismatchEffect;

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
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color matchColor = Color.green;
    [SerializeField] private Color mismatchColor = Color.red;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float mismatchAnimDuration = 0.3f;

    public CardData cardData { get; private set; }
    public CardState CurrentState { get; private set; }

    private bool _isFlipping;
    private bool _isAnimating;
    private Tween _currentTween;
    private Coroutine _autoFlipCoroutine;
    private ParticleSystem _currentHoverVFXInstance;
    private GameManager _gameManager;
    private Color _originalColor;
    private Vector3 _originalPosition;

    void Awake()
    {
        if (cardButton != null)
            cardButton.onClick.AddListener(OnCardClicked);
        CurrentState = CardState.FaceDown;
        _originalColor = cardImage != null ? cardImage.color : Color.white;
        _originalPosition = transform.localPosition;
    }

    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
        cardImage.sprite = cardData.cardBackSprite;
        cardImage.color = _originalColor;
        CurrentState = CardState.FaceDown;
        transform.localScale = Vector3.one * baseScale;
        transform.localPosition = _originalPosition;
        cardButton.interactable = true;
        _isFlipping = false;
        _isAnimating = false;
        CancelAutoFlip();
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
        _currentTween?.Kill();

        yield return transform.DOScaleX(0f, flipDuration / 2f).SetEase(Ease.InOutSine).WaitForCompletion();

        cardImage.sprite = flipToFront ? cardData.cardFaceSprite : cardData.cardBackSprite;
        CurrentState = flipToFront ? CardState.FaceUp : CardState.FaceDown;

        if (flipToFront)
        {
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
        
        // Play match effect with animation and particles
        StartCoroutine(MatchEffectRoutine());
        ShakeScreen();
    }

    private IEnumerator MatchEffectRoutine()
    {
        _isAnimating = true;
        
        // Play particle effect
        if (matchEffect != null) matchEffect.Play();
        
        // Color and scale animation
        if (cardImage != null) cardImage.color = matchColor;
        
        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(matchGrowScale, matchGrowDuration).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(baseScale, matchShrinkDuration).SetEase(Ease.InBack));
        yield return seq.WaitForCompletion();
        
        if (cardImage != null) cardImage.color = _originalColor;
        _isAnimating = false;
    }

    public void OnWrongMatch()
    {
        CancelAutoFlip();
        StartCoroutine(WrongMatchRoutine());
    }

    private IEnumerator WrongMatchRoutine()
    {
        _isAnimating = true;
        
        // Play mismatch particle effect
        if (mismatchEffect != null) mismatchEffect.Play();
        
        // Flash red color
        if (cardImage != null) cardImage.color = mismatchColor;
        
        // Shake animation
        int shakeCount = 6;
        float interval = mismatchAnimDuration / shakeCount;
        
        for (int i = 0; i < shakeCount; i++)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity), 0f);
            transform.localPosition = _originalPosition + shakeOffset;
            yield return new WaitForSeconds(interval);
        }
        
        transform.localPosition = _originalPosition;
        
        // Fade back to original color
        float elapsed = 0f;
        float fadeDuration = 0.2f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (cardImage != null)
                cardImage.color = Color.Lerp(mismatchColor, _originalColor, elapsed / fadeDuration);
            yield return null;
        }
        
        if (cardImage != null) cardImage.color = _originalColor;
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
    }

    public void OnPoolReturn()
    {
        gameObject.SetActive(false);
        CancelAutoFlip();
        if (_currentHoverVFXInstance != null)
        {
            Destroy(_currentHoverVFXInstance.gameObject);
            _currentHoverVFXInstance = null;
        }
    }

    // Hover effects with particles
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentState == CardState.FaceDown && !_isFlipping && !_isAnimating)
        {
            transform.DOScale(Vector3.one * hoverScale, 0.17f).SetEase(Ease.OutBack);
            if (cardImage != null) cardImage.color = Color.Lerp(_originalColor, hoverColor, 0.3f);
            
            if (hoverVFXPrefab != null && _currentHoverVFXInstance == null)
            {
                _currentHoverVFXInstance = Instantiate(hoverVFXPrefab, transform.position, Quaternion.identity, transform);
                _currentHoverVFXInstance.Play();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CurrentState == CardState.FaceDown && !_isFlipping && !_isAnimating)
        {
            transform.DOScale(Vector3.one * baseScale, 0.17f).SetEase(Ease.InBack);
            if (cardImage != null) cardImage.color = _originalColor;
            
            if (_currentHoverVFXInstance != null)
            {
                _currentHoverVFXInstance.Stop();
                Destroy(_currentHoverVFXInstance.gameObject, 1f);
                _currentHoverVFXInstance = null;
            }
        }
    }

    public static void ShakeScreen(float duration = 0.16f, float strength = 15f)
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.transform != null)
        {
            mainCam.transform.DOShakePosition(duration, strength, 14, 90, false, true);
        }
    }
}
