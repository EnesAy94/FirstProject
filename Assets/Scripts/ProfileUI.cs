using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Sıralama (OrderBy) için gerekli

public class ProfileUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject achievementItemPrefab;
    public Transform allContent; // TEK BİR CONTENT ALANI

    // Sıralama yapabilmek için geçici bir sınıf
    private class AchievementSortData
    {
        public AchievementData data;
        public int currentCount;
        public int tierIndex;
        public bool isUnlocked; // TierIndex > -1 ise true
    }

    void OnEnable()
    {
        RefreshAchievements();
    }

    public void RefreshAchievements()
    {
        // 1. Önce eski listeyi temizle
        foreach (Transform child in allContent) Destroy(child.gameObject);

        // 2. Geçici bir liste oluştur ve verileri doldur
        List<AchievementSortData> sortList = new List<AchievementSortData>();

        foreach (AchievementData ach in AchievementManager.instance.allAchievements)
        {
            var status = AchievementManager.instance.GetAchievementStatus(ach.id);

            AchievementSortData item = new AchievementSortData();
            item.data = ach;
            item.currentCount = status.currentCount;
            item.tierIndex = status.currentTierIndex;

            // Eğer Tier -1 değilse bu başarım açılmıştır (Kazanılmıştır)
            item.isUnlocked = (status.currentTierIndex != -1);

            sortList.Add(item);
        }

        // 3. LİSTEYİ SIRALA (SORTING)
        // Mantık: Önce 'isUnlocked' olanlar gelsin (Descending: True > False), 
        // Sonra kendi aralarında ID'ye veya isme göre sıralansın (Düzenli dursun diye)

        var sortedList = sortList
            .OrderByDescending(x => x.isUnlocked) // True (Kazanılanlar) en başa
            .ThenByDescending(x => x.tierIndex)   // Kazanılanlar arasında da Gold > Silver > Bronz olsun
            .ToList();

        // 4. SIRALANMIŞ LİSTEYİ EKRANA BAS
        foreach (var item in sortedList)
        {
            GameObject newItem = Instantiate(achievementItemPrefab, allContent);

            // Setup fonksiyonunu çağır
            newItem.GetComponent<AchievementItemUI>().Setup(item.data, item.currentCount, item.tierIndex);
        }
    }
}