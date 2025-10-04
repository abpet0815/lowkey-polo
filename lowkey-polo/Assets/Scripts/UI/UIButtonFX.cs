using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class UIButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Settings")]
    [SerializeField] UIButtonFXSettings settings;
    [Header("SFX")]
    [SerializeField] AudioSource audioSource;

    Vector3 defaultScale;
    Image targetImage;
    Color normalColor;

    void Awake() {
        defaultScale = transform.localScale;
        targetImage = GetComponent<Image>();
        if(targetImage) normalColor = targetImage.color;
        if(audioSource == null && (settings.hoverClip || settings.pressClip))
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        transform.DOScale(defaultScale * settings.hoverScale, settings.tweenDuration).SetEase(Ease.OutBack);
        PlaySFX(settings.hoverClip);
    }
    public void OnPointerExit(PointerEventData eventData) {
        transform.DOScale(defaultScale, settings.tweenDuration).SetEase(Ease.InOutQuad);
    }
    public void OnPointerDown(PointerEventData eventData) {
        transform.DOScale(defaultScale * settings.pressScale, settings.tweenDuration/2).SetEase(Ease.InQuart)
            .OnComplete(()=>transform.DOScale(defaultScale * settings.hoverScale, settings.tweenDuration/2).SetEase(Ease.OutBack));
        PlaySFX(settings.pressClip);
    }
    void PlaySFX(AudioClip clip) {
        if(clip && audioSource) audioSource.PlayOneShot(clip);
    }
}
