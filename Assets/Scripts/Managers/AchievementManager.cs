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

    // --- İLERLEME KAYDETME (GÜNCELLENMİŞ VE DEBUG'LI) ---
    public void AddProgress(string achievementID, int amount)
    {
        AchievementData data = allAchievements.Find(x => x.id == achievementID);
        if (data == null)
        {
            Debug.LogWarning($"[Achievement] ID bulunamadı: {achievementID}");
            return;
        }

        // 1. KONTROL: Görev yapıldı mı?
        if (data.requiresMission)
        {
            if (!SaveManager.instance.IsMissionCompleted(data.requiredMissionKey))
            {
                Debug.Log($"[Achievement] {achievementID} için görev henüz tamamlanmadı: {data.requiredMissionKey}");
                return;
            }
        }

        // 2. MEVCUT (ESKİ) DURUMU AL
        int oldCount = SaveManager.instance.GetAchievementProgress(achievementID);
        int oldTierIndex = GetTierIndex(data, oldCount);

        // 3. YENİ DURUMU HESAPLA VE KAYDET
        int newCount = oldCount + amount;
        SaveManager.instance.SetAchievementProgress(achievementID, newCount);

        Debug.Log($"[Achievement] {achievementID} ilerleme güncellendi: {oldCount} → {newCount}");

        // 4. YENİ SEVİYEYİ HESAPLA
        int newTierIndex = GetTierIndex(data, newCount);

        // 5. KUTLAMA KONTROLÜ (Level Atladı mı?)
        if (newTierIndex > oldTierIndex)
        {
            string oldTierName = oldTierIndex >= 0 ? data.tiers[oldTierIndex].tierName : "Kilitli";
            string newTierName = data.tiers[newTierIndex].tierName;
            Debug.Log($"[Achievement] ⭐ SEVİYE YÜKSELDİ! {achievementID}: {oldTierName} → {newTierName}");
            UnlockAchievement(data, newTierIndex);
        }
        else
        {
            Debug.Log($"[Achievement] Henüz seviye atlamadı. Mevcut tier: {(newTierIndex >= 0 ? data.tiers[newTierIndex].tierName : "Kilitli")} ({newCount}/{(newTierIndex < data.tiers.Count - 1 ? data.tiers[newTierIndex + 1].targetCount : "MAX")})");
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