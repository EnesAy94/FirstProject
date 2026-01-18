using UnityEngine;
using TMPro; // TextMeshPro yazılarını kullanmak için şart
using System.Collections.Generic;
using System.Linq; // Sıralama (OrderBy) için şart

public class ProfileUI : MonoBehaviour
{
    [Header("--- İSTATİSTİK YAZILARI (STATS) ---")]
    public TextMeshProUGUI accuracyRateText;    // Normal Doğruluk %
    public TextMeshProUGUI hardSuccessText;     // Zor Soru %
    public TextMeshProUGUI penaltyVisitsText;   // Ceza Sayısı
    public TextMeshProUGUI longestStreakText;   // En Uzun Seri
    public TextMeshProUGUI playerNameText;      // İsim (Şimdilik boş kalabilir)
    public TextMeshProUGUI totalPointText;      // Toplam Puan

    [Header("--- BAŞARIM LİSTESİ (ACHIEVEMENTS) ---")]
    public GameObject achievementItemPrefab; // Küçük başarım kutucuğu
    public Transform allContent;             // Scroll View içindeki Content objesi

    // Sıralama için yardımcı sınıf
    private class AchievementSortData
    {
        public AchievementData data;
        public int currentCount;
        public int tierIndex;
        public bool isUnlocked;
    }

    // Panel açıldığı an çalışır
    void OnEnable()
    {
        // 1. İstatistikleri Güncelle
        UpdateStatTexts();

        // 2. Başarım Listesini Yenile
        RefreshAchievements();
    }

    // --- BÖLÜM 1: İSTATİSTİK YAZILARI ---
    void UpdateStatTexts()
    {
        // SaveManager'dan verileri çekiyoruz
        PlayerData data = SaveManager.instance.activeSave;

        // A. Normal Sorular Başarı Oranı
        int totalNormal = data.normalCorrectCount + data.normalWrongCount;
        float accuracy = 0;
        if (totalNormal > 0)
        {
            accuracy = ((float)data.normalCorrectCount / totalNormal) * 100f;
        }
        accuracyRateText.text = "%" + accuracy.ToString("F0");

        // B. Zor Sorular Başarı Oranı
        int totalHard = data.hardCorrectCount + data.hardWrongCount;
        float hardSuccess = 0;
        if (totalHard > 0)
        {
            hardSuccess = ((float)data.hardCorrectCount / totalHard) * 100f;
        }
        hardSuccessText.text = "%" + hardSuccess.ToString("F0");

        // C. Diğer Veriler
        int totalPenalty = data.penaltyCorrectCount + data.penaltyWrongCount;
        float penaltySuccess = 0;

        if (totalPenalty > 0)
        {
            penaltySuccess = ((float)data.penaltyCorrectCount / totalPenalty) * 100f;
        }

        // Ekrana yazdır (Örn: "%75")
        penaltyVisitsText.text = "%" + penaltySuccess.ToString("F0");

        // D. Streak
        longestStreakText.text = data.maxStreak.ToString();

        if (totalPointText != null)
            totalPointText.text = data.totalScore.ToString();
    }

    // --- BÖLÜM 2: BAŞARIM LİSTESİ ---
    public void RefreshAchievements()
    {
        // Önce temizlik: Content altındaki eski kutuları sil
        foreach (Transform child in allContent) Destroy(child.gameObject);

        // AchievementManager'dan verileri alıp geçici listeye koy
        List<AchievementSortData> sortList = new List<AchievementSortData>();

        foreach (AchievementData ach in AchievementManager.instance.allAchievements)
        {
            var status = AchievementManager.instance.GetAchievementStatus(ach.id);

            AchievementSortData item = new AchievementSortData();
            item.data = ach;
            item.currentCount = status.currentCount;
            item.tierIndex = status.currentTierIndex;
            item.isUnlocked = (status.currentTierIndex != -1);

            sortList.Add(item);
        }

        // Sıralama Yap: Önce kazanılanlar, sonra kazanılanlar içinde rütbesi yüksek olanlar
        var sortedList = sortList
            .OrderByDescending(x => x.isUnlocked)
            .ThenByDescending(x => x.tierIndex)
            .ToList();

        // Listeyi Ekrana Bas
        foreach (var item in sortedList)
        {
            GameObject newItem = Instantiate(achievementItemPrefab, allContent);
            newItem.GetComponent<AchievementItemUI>().Setup(item.data, item.currentCount, item.tierIndex);
        }
    }
}