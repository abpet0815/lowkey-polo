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
    [Tooltip("Drag your hover ParticleSystem prefab here, e.g. CFXR3 effect")]
    [SerializeField] private ParticleSystem hoverVFXPrefab;
    [Tooltip("Drag your match ParticleSystem here if needed")]
    [SerializeField] private ParticleSystem matchEffect;

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

    public CardData cardData { get; private set; }
    public CardState CurrentState { get; private set; }

    private bool _isFlipping;
    private Tween _currentTween;
    private Coroutine _autoFlipCoroutine;
    private ParticleSystem _currentHoverVFXInstance;
    private GameManager _gameManager;

    void Awake()
    {
        if (cardButton != null)
            cardButton.onClick.AddListener(OnCardClicked);
        CurrentState = CardState.FaceDown;
    }

    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
        cardImage.sprite = cardData.cardBackSprite;
        cardImage.color = Color.white;
        CurrentState = CardState.FaceDown;
        transform.localScale = Vector3.one * baseScale;
        cardButton.interactable = true;
        _isFlipping = false;
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

        if (matchEffect != null) matchEffect.Play();

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(matchGrowScale, matchGrowDuration).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(baseScale, matchShrinkDuration).SetEase(Ease.InBack));
        seq.OnComplete(() => ShakeScreen());
    }

    public void OnWrongMatch()
    {
        CancelAutoFlip();
        StartCoroutine(WrongMatchFlipBackRoutine());
    }

    private IEnumerator WrongMatchFlipBackRoutine()
    {
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
        _isFlipping = false;
        CancelAutoFlip();
    }

    public void OnPoolReturn()
    {
        gameObject.SetActive(false);
        CancelAutoFlip();
    }

    // Hover VFX implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentState == CardState.FaceDown && !_isFlipping)
        {
            transform.DOScale(Vector3.one * hoverScale, 0.17f).SetEase(Ease.OutBack);
            if (hoverVFXPrefab != null && _currentHoverVFXInstance == null)
            {
                _currentHoverVFXInstance = Instantiate(hoverVFXPrefab, transform.position, Quaternion.identity);
                _currentHoverVFXInstance.transform.SetParent(transform);
                _currentHoverVFXInstance.Play();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CurrentState == CardState.FaceDown && !_isFlipping)
        {
            transform.DOScale(Vector3.one * baseScale, 0.17f).SetEase(Ease.InBack);
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
