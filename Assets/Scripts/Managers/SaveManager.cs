using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    // Aktif kullanılan veri kutumuz
    public PlayerData activeSave;

    // Kayıt dosyası adı (İlerde Bulut için ID olacak)
    private string saveFileName = "GameSaveData";

    void Awake()
    {
        // Singleton (Tekillik) Yapısı
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Oyun açılınca verileri yükle
        LoadGame();
    }

    // --- KAYDETME (SAVE) ---
    public void SaveGame()
    {
        // 1. Veriyi JSON formatına (Metne) çevir
        string json = JsonUtility.ToJson(activeSave);

        // 2. Diske yaz (PlayerPrefs içine tek bir string olarak)
        PlayerPrefs.SetString(saveFileName, json);
        PlayerPrefs.Save();

        Debug.Log("💾 Oyun Kaydedildi (Local): " + json);

        // NOT: İlerde buraya "Firebase.Database.Save(json)" gelecek.
    }

    // --- YÜKLEME (LOAD) ---
    public void LoadGame()
    {
        // Kayıt var mı?
        if (PlayerPrefs.HasKey(saveFileName))
        {
            string json = PlayerPrefs.GetString(saveFileName);

            // JSON'u tekrar Class'a çevir
            activeSave = JsonUtility.FromJson<PlayerData>(json);

            Debug.Log("📂 Oyun Yüklendi!");
        }
        else
        {
            // Kayıt yoksa yeni, boş bir kutu oluştur
            CreateNewSave();
        }
    }

    void CreateNewSave()
    {
        activeSave = new PlayerData();
        activeSave.totalScore = 0;
        activeSave.maxLevelReached = 1;

        // Listeleri başlat (Null hatası almamak için)
        activeSave.earnedAchievements = new List<string>();
        activeSave.completedMissions = new List<string>();
        activeSave.achievementProgress = new List<ProgressData>();

        SaveGame(); // İlk boş kaydı oluştur
    }

    // --- GÜNCELLEME KOMUTLARI (Diğer scriptler burayı kullanacak) ---

    // Puan Ekleme
    public void UpdateScore(int newScore)
    {
        activeSave.totalScore = newScore;
        SaveGame();
    }

    // Görev Bitirme
    public void CompleteMission(string missionID)
    {
        if (!activeSave.completedMissions.Contains(missionID))
        {
            activeSave.completedMissions.Add(missionID);
            SaveGame();
        }
    }

    // Görev bitmiş mi kontrolü
    public bool IsMissionCompleted(string missionID)
    {
        return activeSave.completedMissions.Contains(missionID);
    }

    // Başarım İlerlemesini Kaydet
    public void SetAchievementProgress(string id, int amount)
    {
        // Listede var mı diye bak
        int index = activeSave.achievementProgress.FindIndex(x => x.id == id);

        if (index != -1)
        {
            // Varsa güncelle
            ProgressData data = activeSave.achievementProgress[index];
            data.amount = amount;
            activeSave.achievementProgress[index] = data; // Struct olduğu için geri atıyoruz
        }
        else
        {
            // Yoksa yeni ekle
            ProgressData newData = new ProgressData { id = id, amount = amount };
            activeSave.achievementProgress.Add(newData);
        }
        SaveGame();
    }

    // Başarım İlerlemesini Getir (Load için)
    public int GetAchievementProgress(string id)
    {
        var data = activeSave.achievementProgress.Find(x => x.id == id);
        // Eğer data null değilse amount'u dön, yoksa 0 dön
        return (data.id != null) ? data.amount : 0;
    }

    // Soru çözüldüğünde bu fonksiyonu çağıracağız
    // GÜNCELLENMİŞ VERSİYON
    public void RegisterAnswer(bool isCorrect, bool isHardQuestion, bool isPenaltyQuestion)
    {
        // 1. STREAK (SERİ) HESABI (Aynı kalıyor)
        if (isCorrect)
        {
            activeSave.currentStreak++;
            if (activeSave.currentStreak > activeSave.maxStreak)
                activeSave.maxStreak = activeSave.currentStreak;
        }
        else
        {
            activeSave.currentStreak = 0;
        }

        // 2. KATEGORİYE GÖRE KAYIT (Burası Değişti)
        if (isPenaltyQuestion)
        {
            // Eğer Ceza alanındaysak buraya yaz
            if (isCorrect) activeSave.penaltyCorrectCount++;
            else activeSave.penaltyWrongCount++;
        }
        else if (isHardQuestion)
        {
            // Zor soruysa buraya
            if (isCorrect) activeSave.hardCorrectCount++;
            else activeSave.hardWrongCount++;
        }
        else
        {
            // Normal soruysa buraya
            if (isCorrect) activeSave.normalCorrectCount++;
            else activeSave.normalWrongCount++;
        }

        SaveGame();
    }
}