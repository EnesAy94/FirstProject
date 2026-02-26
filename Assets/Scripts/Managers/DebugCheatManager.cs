using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugCheatManager : MonoBehaviour
{
    // Bu scripti tıklandığında hileyi açmasını istediğiniz bir objeye (buton gibi) atayabilirsiniz.
    // Projeniz "New Input System" kullandığı için klavye algılamasını iptal ettim, sadece butona tıklayınca çalışır.


    // Butonla (Örn: logoya tıklayarak) tetiklemek isterseniz bu fonksiyonu Button'un OnClick olayına bağlayın.
    public void OnClick_ActivateCheats()
    {
        ApplyAllCheats();
    }

    private void ApplyAllCheats()
    {
        if (SaveManager.instance == null)
        {
            Debug.LogWarning("[CheatManager] SaveManager bulunamadı!");
            return;
        }

        Debug.Log("🚀 --- HİLE SİSTEMİ AKTİF EDİLDİ --- 🚀");

        // 1. Tüm Bölümleri / Hikayeleri Aç
        // SaveManager içerisinde son açılan seviye mantığı var, onu çok yüksek bir sayıya çekiyoruz.
        SaveManager.instance.activeSave.lastUnlockedLevel = 999;
        
        // Ana bölümleri (Chapterları) tamamlanmış saysın
        for (int i = 1; i <= 20; i++) // Şimdilik 20'ye kadar
        {
            SaveManager.instance.SetMainMissionDone(i);
        }
        
        // 2. Profil İstatistiklerini Şişir
        SaveManager.instance.UpdateScore(Random.Range(10000, 50000));
        SaveManager.instance.AddTicketsToWallet(500); // 500 Bilet ver
        
        SaveManager.instance.activeSave.normalCorrectCount = Random.Range(50, 150);
        SaveManager.instance.activeSave.normalWrongCount = Random.Range(5, 20);
        
        SaveManager.instance.activeSave.hardCorrectCount = Random.Range(20, 80);
        SaveManager.instance.activeSave.hardWrongCount = Random.Range(10, 30);

        SaveManager.instance.activeSave.penaltyCorrectCount = Random.Range(10, 40);
        SaveManager.instance.activeSave.penaltyWrongCount = Random.Range(0, 5);

        SaveManager.instance.activeSave.maxStreak = Random.Range(15, 45);
        SaveManager.instance.activeSave.currentStreak = Random.Range(0, SaveManager.instance.activeSave.maxStreak);

        // 3. Başarımları Rastgele Ver
        if (AchievementManager.instance != null)
        {
            foreach (var ach in AchievementManager.instance.allAchievements)
            {
                // Rastgele bir tutar ekle (0 ile o başarımı fullleyecek bir sayı arasında)
                if (ach.tiers.Count > 0)
                {
                    int maxPossibleTarget = ach.tiers[ach.tiers.Count - 1].targetCount;
                    int randomProgress = Random.Range(0, maxPossibleTarget + 5); // +5 fazla bile verebilir (tamamlanması için)
                    
                    // Önceki izi silip net sayı vermek için SaveManager'ı direkt uyaralım
                    SaveManager.instance.SetAchievementProgress(ach.id, randomProgress);
                }
            }
        }
        else
        {
            Debug.LogWarning("[CheatManager] AchievementManager sahnede yok, başarımlar hilelenmedi.");
        }

        // Değişiklikleri Kaydet
        SaveManager.instance.SaveGame();

        // 4. Arayüzü Güncellemek Yerine Sahneyi Yeniden Yükle (En Temiz Çözüm)
        Debug.Log("✅ Tüm başarımlar, leveller ve istatistikler rastgele hilelendi! Sahne yenileniyor...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
