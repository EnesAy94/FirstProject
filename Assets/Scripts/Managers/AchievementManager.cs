using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;
    public List<AchievementData> allAchievements; 

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- İLERLEME KAYDETME (GÜNCELLENDİ) ---
    public void AddProgress(string achievementID, int amount)
    {
        AchievementData data = allAchievements.Find(x => x.id == achievementID);
        if (data == null) return;

        // 1. KONTROL: Görev yapıldı mı? (SaveManager'a soruyoruz)
        if (data.requiresMission)
        {
            if (!SaveManager.instance.IsMissionCompleted(data.requiredMissionKey))
            {
                Debug.Log($"[Achievement] {data.title} için önce ilgili görev yapılmalı!");
                return;
            }
        }

        // 2. MEVCUT İLERLEMEYİ AL (SaveManager'dan)
        int currentCount = SaveManager.instance.GetAchievementProgress(achievementID);

        // 3. ARTIR VE KAYDET (SaveManager'a)
        currentCount += amount;
    
        SaveManager.instance.SetAchievementProgress(achievementID, currentCount);

        // 4. SEVİYE KONTROLÜ
        CheckUnlockStatus(data, currentCount);
    }

    public void CheckUnlockStatus(AchievementData data, int currentCount)
    {
        int currentTierIndex = -1;
        for (int i = 0; i < data.tiers.Count; i++)
        {
            if (currentCount >= data.tiers[i].targetCount)
            {
                currentTierIndex = i;
            }
        }

        if (currentTierIndex >= 0)
        {
            // İstersen burada UI bildirimi yapabilirsin
            // Debug.Log($"[Achievement] {data.title} seviye atladı!");
        }
    }

    // --- DURUM SORGULAMA (GÜNCELLENDİ) ---
    public (int currentCount, int currentTierIndex) GetAchievementStatus(string id)
    {
        AchievementData data = allAchievements.Find(x => x.id == id);
        if (data == null) return (0, -1);

        // 1. İLERLEMEYİ ÇEK (SaveManager'dan)
        // YENİSİ:
        int count = SaveManager.instance.GetAchievementProgress(data.id);

        int tierIndex = -1;
        for (int i = 0; i < data.tiers.Count; i++)
        {
            if (count >= data.tiers[i].targetCount) tierIndex = i;
        }

        // 2. GÖREV KİLİDİ KONTROLÜ (SaveManager'dan)
        // YENİSİ:
        if (data.requiresMission && !SaveManager.instance.IsMissionCompleted(data.requiredMissionKey))
        {
            return (0, -1); 
        }

        if (!data.requiresMission && tierIndex == -1)
        {
            return (count, -1);
        }

        return (count, tierIndex);
    }
}