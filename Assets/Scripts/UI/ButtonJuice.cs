using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Selectable))]
public class ButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    public float punchScale = 0.9f;
    public float animationDuration = 0.15f;
    
    [Header("Color Settings (Optional)")]
    public bool changeColorOnPress = true;
    public Color pressedColorMultiplier = new Color(1.2f, 1.2f, 1.2f, 1f); // slightly brighter
    private Color originalColor;
    private Graphic targetGraphic;

    [Header("Haptics")]
    public bool useHaptics = true;

    private Vector3 originalScale;
    private Selectable selectable;

    void Start()
    {
        originalScale = transform.localScale;
        selectable = GetComponent<Selectable>();
        targetGraphic = selectable.targetGraphic;

        if (targetGraphic != null)
        {
            originalColor = targetGraphic.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!selectable.interactable) return;

        // Shrink button
        transform.DOKill();
        transform.DOScale(originalScale * punchScale, animationDuration).SetEase(Ease.OutQuad);

        // Change color to brighter
        if (changeColorOnPress && targetGraphic != null)
        {
            targetGraphic.DOKill();
            targetGraphic.DOColor(originalColor * pressedColorMultiplier, animationDuration);
        }

        // Haptic Feedback
        if (useHaptics)
        {
            TriggerLightHaptic();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!selectable.interactable) return;
        ReleaseAnimation();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Optional hover effect for PC/Web
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selectable.interactable) return;
        ReleaseAnimation();
    }

    private void ReleaseAnimation()
    {
        // Bounce back to original scale
        transform.DOKill();
        transform.DOScale(originalScale, animationDuration * 2f).SetEase(Ease.OutElastic);

        // Restore original color
        if (changeColorOnPress && targetGraphic != null)
        {
            targetGraphic.DOKill();
            targetGraphic.DOColor(originalColor, animationDuration);
        }
    }

    private void TriggerLightHaptic()
    {
        // For basic vibration on mobile platforms
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        // NOTE: For more advanced/subtle haptics (like iOS Impact Feedback), 
        // third-party plugins like NiceVibrations or CandyCoded.HapticFeedback are recommended.
#endif
    }
}
