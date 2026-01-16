using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public GameObject lockOverlay; // Siyah perde (Varsa)
    public Image backgroundImage;  // Arka plan rengi (Varsa)

    public void Setup(AchievementData data, int currentCount, int tierIndex)
    {
        titleText.text = data.title;

        // --- KİLİTLİ DURUM ---
        if (tierIndex == -1)
        {
            // İkonu gri yap
            iconImage.sprite = data.tiers[0].badgeIcon; // İlk seviye ikonunu göster ama gri olsun
            iconImage.color = Color.gray;

            // Yazılar
            int nextTarget = data.tiers[0].targetCount;
            descText.text = $"KİLİTLİ\nİlerleme: {currentCount}/{nextTarget}";
            descText.color = Color.gray;

            // Kilit perdesi
            if (lockOverlay != null) lockOverlay.SetActive(true);

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
                int nextTarget = data.tiers[tierIndex + 1].targetCount;
                descText.text = $"{currentTier.tierName}\nSonraki: {currentCount}/{nextTarget}";
            }
            else
            {
                descText.text = $"{currentTier.tierName} (TAMAMLANDI!)";
            }
            descText.color = Color.white; // Veya sarı/yeşil

            if (lockOverlay != null) lockOverlay.SetActive(false);

            // Arka planı canlı yap (Opsiyonel)
            if (backgroundImage != null) backgroundImage.color = new Color(0.1f, 0.3f, 0.1f, 1f); // Hafif yeşilimsi
        }
    }
}