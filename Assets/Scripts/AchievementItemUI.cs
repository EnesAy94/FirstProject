using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statusText;  // YENİ: "DURUM: KİLİTLİ" yazacak alan
    public TextMeshProUGUI progressText; // YENİ: "İLERLEME: 3/9" yazacak alan
    public Image backgroundImage;  // Arka plan rengi (Varsa)
    public Slider progressSlider; // YENİ: Resimdeki gibi altta dolan yeşil bar için Slider bileşeni
    
    [Header("Görseller")]
    public Sprite lockedBadgeIcon; // YENİ: Başarım kilitliyken görünecek kilit ikonu

    public void Setup(AchievementData data, int currentCount, int tierIndex)
    {
        titleText.text = data.title;

        // --- KİLİTLİ DURUM ---
        if (tierIndex == -1)
        {
            // İkonu kilit ikonu yap
            if (lockedBadgeIcon != null)
            {
                iconImage.sprite = lockedBadgeIcon;
                iconImage.color = Color.gray; // Veya orijinal renginde kalması için Color.white yapabilirsiniz
            }

            // Yazılar (Durum ve İlerleme)
            int nextTarget = data.tiers[0].targetCount;
            
            if (statusText != null) 
            {
                statusText.text = "DURUM: <color=#FF5555>KİLİTLİ</color>"; // Kırmızımsı KİLİTLİ yazısı
            }

            if (progressText != null)
            {
                progressText.text = $"İLERLEME: {currentCount}/{nextTarget}";
            }

            // Eğer slider varsa oranını ayarla
            if (progressSlider != null)
            {
                progressSlider.maxValue = 1f; // 0 ile 1 arasına sabitledik (Matematiksel oran yapıyoruz)
                progressSlider.value = (float)currentCount / nextTarget;
            }

            // Arka planı biraz karart (Opsiyonel)
            if (backgroundImage != null) backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }
        // --- AÇIK (KAZANILMIŞ) DURUM ---
        else
        {
            AchievementTier currentTier = data.tiers[tierIndex];

            // İkonu canlı yap
            iconImage.sprite = currentTier.badgeIcon;
            iconImage.color = Color.white;

            // Yazılar
            if (tierIndex < data.tiers.Count - 1)
            {
                // Bir sonraki aşama var, o yüzden "AÇIK" diyeceğiz ama bitmemiş.
                int nextTarget = data.tiers[tierIndex + 1].targetCount;
                if (statusText != null) statusText.text = "DURUM: <color=#55FF55>DEVAM EDİYOR</color>";
                if (progressText != null) progressText.text = $"İLERLEME: {currentCount}/{nextTarget}";
                
                if (progressSlider != null)
                {
                    progressSlider.maxValue = 1f;
                    progressSlider.value = (float)currentCount / nextTarget;
                }
            }
            else
            {
                // Tamamen bitmiş
                if (statusText != null) statusText.text = "DURUM: <color=#55FF55>TAMAMLANDI</color>";
                if (progressText != null) progressText.text = $"İLERLEME: MAKSİMUM";
                
                if (progressSlider != null)
                {
                    progressSlider.maxValue = 1f;
                    progressSlider.value = 1f;
                }
            }

            // Arka planı canlı yap (Opsiyonel)
            if (backgroundImage != null) backgroundImage.color = new Color(0.1f, 0.3f, 0.1f, 1f); // Hafif yeşilimsi
        }
    }
}