using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;
    public List<AchievementData> allAchievements; // Tüm ScriptableObject'leri buraya sürükle

    void Awake()
    {
        // Eğer zaten bir yönetici varsa (Önceki sahneden gelen)
        if (instance != null && instance != this)
        {
            // Bu yeni geleni hemen yok et! Kopyaya izin yok.
            Destroy(gameObject);
            return;
        }

        // Yoksa, patron benim
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- İLERLEME KAYDETME ---
    // Bu fonksiyonu oyunun içinden çağıracağız.
    // Örn: AddProgress("hard_master", 1);
    public void AddProgress(string achievementID, int amount)
    {
        AchievementData data = allAchievements.Find(x => x.id == achievementID);
        if (data == null) return;

        // 1. KONTROL: Eğer görev şartı varsa ve görev yapılmadıysa sayma!
        if (data.requiresMission)
        {
            // Görev bitince PlayerPrefs'e "MissionX_Done" gibi bir şey kaydediyoruz ya, onu kontrol ediyoruz.
            // Eğer görev bitmediyse (0 ise), işlem yapma.
            if (PlayerPrefs.GetInt(data.requiredMissionKey, 0) == 0)
            {
                Debug.Log($"[Achievement] {data.title} için önce ilgili görev yapılmalı!");
                return;
            }
        }

        // 2. İLERLEMEYİ ARTIR
        string currentKey = $"Ach_{achievementID}_Count";
        int currentCount = PlayerPrefs.GetInt(currentKey, 0);
        currentCount += amount;
        PlayerPrefs.SetInt(currentKey, currentCount);
        PlayerPrefs.Save();

        // 3. SEVİYE KONTROLÜ (Level Up var mı?)
        CheckUnlockStatus(data, currentCount);
    }

    // Sadece kontrol eder, UI güncellemesi için.
    public void CheckUnlockStatus(AchievementData data, int currentCount)
    {
        // Şimdilik sadece log atalım, UI kısmında bunu görselleştireceğiz.
        // Hangi seviyede olduğunu bulmak için:
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
            Debug.Log($"[Achievement] {data.title} şu an {data.tiers[currentTierIndex].tierName} seviyesinde! ({currentCount})");
        }
    }

    // UI'ın kullanacağı yardımcı fonksiyon: Şu anki ilerleme ve tier bilgisini getir
    public (int currentCount, int currentTierIndex) GetAchievementStatus(string id)
    {
        AchievementData data = allAchievements.Find(x => x.id == id);
        if (data == null) return (0, -1);

        int count = PlayerPrefs.GetInt($"Ach_{data.id}_Count", 0);

        int tierIndex = -1;
        for (int i = 0; i < data.tiers.Count; i++)
        {
            if (count >= data.tiers[i].targetCount) tierIndex = i;
        }

        // Eğer görev şartı varsa ve görev yapılmamışsa, kullanıcı 0 ilerlemede gözüksün
        if (data.requiresMission && PlayerPrefs.GetInt(data.requiredMissionKey, 0) == 0)
        {
            return (0, -1); // -1 demek "Kilitli/Sahip Değil" demek
        }

        // Eğer görev gerektirmiyorsa (Ceza gibi) ama henüz ilk hedefi (Örn: 1) tutturamadıysa yine kilitli
        if (!data.requiresMission && tierIndex == -1)
        {
            return (count, -1);
        }

        return (count, tierIndex);
    }
}