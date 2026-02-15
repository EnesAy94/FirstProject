using UnityEngine;
using TMPro;
using DG.Tweening;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance;

    [Header("UI Bağlantıları")]
    public GameObject notificationPanel; // Panelin kendisi
    public TextMeshProUGUI notificationText; // İçindeki yazı
    public CanvasGroup canvasGroup; // Fade işlemi için

    private void Awake()
    {
        instance = this;
        if (notificationPanel) notificationPanel.SetActive(false);
        if (canvasGroup) canvasGroup.alpha = 0;
    }

    public void ShowNotification(string message)
    {
        // Varsa eski animasyonu durdur
        canvasGroup.DOKill();
        
        // İçeriği ayarla
        notificationPanel.SetActive(true);
        notificationText.text = message;
        canvasGroup.alpha = 0;

        // Animasyon: Belir -> Bekle -> Kaybol
        Sequence seq = DOTween.Sequence();
        
        // 1. 0.5 saniyede belir
        seq.Append(canvasGroup.DOFade(1, 0.5f));
        
        // 2. 4 saniye ekranda kal (Toplam 5 saniye gibi hissettirir)
        seq.AppendInterval(4f);
        
        // 3. 0.5 saniyede kaybol
        seq.Append(canvasGroup.DOFade(0, 0.5f));
        
        // 4. Panel objesini kapat
        seq.OnComplete(() => notificationPanel.SetActive(false));
    }
}