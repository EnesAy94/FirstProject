using UnityEngine;
using DG.Tweening;

public class LogoBreathing : MonoBehaviour
{
    [Header("Breathing Settings")]
    [Tooltip("Target alpha to breathe down to (e.g. 0.5f)")]
    public float minAlpha = 0.5f;
    
    [Tooltip("Duration for a half-cycle in seconds")]
    public float halfCycleDuration = 2.0f;

    private CanvasGroup canvasGroup;
    private float originalAlpha;

    void Start()
    {
        // Try to get CanvasGroup, add it if not present
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalAlpha = canvasGroup.alpha;

        // Start breathing animation
        // Yoyo loops it back and forth continuously
        canvasGroup.DOFade(minAlpha, halfCycleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true); // Ignores Time.timeScale so it happens even when paused
    }

    void OnDestroy()
    {
        canvasGroup?.DOKill();
    }
}
