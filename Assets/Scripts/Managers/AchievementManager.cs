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

    // --- İLERLEME KAYDETME (ROBOT ENTEGRASYONLU) ---
    public void AddProgress(string achievementID, int amount)
    {
        AchievementData data = allAchievements.Find(x => x.id == achievementID);
        if (data == null) return;

        // 1. KONTROL: Görev yapıldı mı?
        if (data.requiresMission)
        {
            if (!SaveManager.instance.IsMissionCompleted(data.requiredMissionKey))
            {
                // Görev yapılmadıysa ilerleme ekleme
                return;
            }
        }

        // 2. MEVCUT (ESKİ) DURUMU AL
        int oldCount = SaveManager.instance.GetAchievementProgress(achievementID);

        // Eski seviyeyi hesapla
        int oldTierIndex = GetTierIndex(data, oldCount);

        // 3. YENİ DURUMU HESAPLA VE KAYDET
        int newCount = oldCount + amount;
        SaveManager.instance.SetAchievementProgress(achievementID, newCount);

        // Yeni seviyeyi hesapla
        int newTierIndex = GetTierIndex(data, newCount);

        // 4. KUTLAMA KONTROLÜ (Level Atladı mı?)
        // Eğer yeni seviye, eskisinden büyükse tebrik et
        if (newTierIndex > oldTierIndex)
        {
            UnlockAchievement(data, newTierIndex);
        }
    }

    // Yardımcı Fonksiyon: Verilen puana göre kaçıncı seviyede olduğunu bulur
    int GetTierIndex(AchievementData data, int count)
    {
        int tierIndex = -1;
        for (int i = 0; i < data.tiers.Count; i++)
        {
            if (count >= data.tiers[i].targetCount)
            {
                tierIndex = i;
            }
        }
        return tierIndex;
    }

    void UnlockAchievement(AchievementData data, int tierIndex)
    {
        string tierName = data.tiers[tierIndex].tierName;

        // --- ESKİSİ: RobotAssistant.instance.Celebrate... (SİLDİK) ---

        // --- YENİSİ: Üst Panelden Bildirim ---
        if (NotificationManager.instance != null)
        {
            // Örn: "BAŞARIM KAZANILDI! Zor Soruların Efendisi (Altın)"
            NotificationManager.instance.ShowNotification($"BAŞARIM KAZANILDI!\n{data.title} ({tierName})");
        }
    }

    // --- DURUM SORGULAMA ---
    public (int currentCount, int currentTierIndex) GetAchievementStatus(string id)
    {
        AchievementData data = allAchievements.Find(x => x.id == id);
        if (data == null) return (0, -1);

        int count = SaveManager.instance.GetAchievementProgress(data.id);

        // Görev kilidi kontrolü
        if (data.requiresMission && !SaveManager.instance.IsMissionCompleted(data.requiredMissionKey))
        {
            return (0, -1);
        }

        int tierIndex = GetTierIndex(data, count);
        return (count, tierIndex);
    }
}