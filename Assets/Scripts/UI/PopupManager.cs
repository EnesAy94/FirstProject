using UnityEngine;
using TMPro;
using DG.Tweening;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    [Header("UI Elemanları")]
    public CanvasGroup popupCanvasGroup; // Yazının ve arka planının bulunduğu ana grup
    public TextMeshProUGUI popupText;
    public RectTransform popupRect;

    [Header("Ayarlar")]
    public float fadeDuration = 0.3f;
    public float waitDuration = 2f;
    public float moveDistance = 50f;

    private bool isShowing = false;
    private Vector2 originalPosition;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Başlangıçta görünmez yap
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.gameObject.SetActive(false);
        }

        if (popupRect != null)
        {
            originalPosition = popupRect.anchoredPosition;
        }
    }

    /// <summary>
    /// Ekranda geçici bir uyarı metni gösterir. Spam korumalıdır.
    /// </summary>
    /// <param name="localizationKey">Örn: "popup_coming_soon"</param>
    public void ShowPopup(string localizationKey)
    {
        if (isShowing || popupCanvasGroup == null || popupText == null) return;

        isShowing = true;
        popupCanvasGroup.gameObject.SetActive(true);

        // Dil yöneticisinden çeviriyi al, yoksa direkt yazıyı bas
        string message = LocalizationManager.instance != null 
            ? LocalizationManager.instance.GetText(localizationKey) 
            : localizationKey;

        popupText.text = message;

        // Animasyon Sıfırlamaları
        popupCanvasGroup.DOKill();
        popupRect.DOKill();

        popupCanvasGroup.alpha = 0f;
        popupRect.anchoredPosition = originalPosition - new Vector2(0, moveDistance); // Biraz aşağıdan başla

        // Giriş Animasyonu (Yukarı kayarak belirme)
        Sequence seq = DOTween.Sequence();
        seq.Append(popupCanvasGroup.DOFade(1f, fadeDuration));
        seq.Join(popupRect.DOAnchorPos(originalPosition, fadeDuration).SetEase(Ease.OutBack));
        
        // Bekleme
        seq.AppendInterval(waitDuration);

        // Çıkış Animasyonu (Yukarı doğru kaybolma)
        seq.Append(popupCanvasGroup.DOFade(0f, fadeDuration));
        seq.Join(popupRect.DOAnchorPos(originalPosition + new Vector2(0, moveDistance), fadeDuration).SetEase(Ease.InBack));

        seq.OnComplete(() => 
        {
            isShowing = false;
            popupCanvasGroup.gameObject.SetActive(false);
        });
    }
}
