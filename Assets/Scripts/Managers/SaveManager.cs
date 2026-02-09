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

        PlayerPrefs.SetString(saveFileName, json);
        PlayerPrefs.Save();

        Debug.Log("Oyun Kaydedildi (Local): " + json);

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

            Debug.Log("Oyun Yüklendi!");
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

    // --- 1. PROFİL BİLGİLERİNİ KAYDETME ---
    public void SaveProfileInfo(string name, string surname, string nickname)
    {
        activeSave.playerName = name;
        activeSave.playerSurname = surname;
        activeSave.playerNickname = nickname;
        SaveGame();
    }

    // --- 2. AKILLI PUAN SİSTEMİ (En Önemli Kısım) ---
    public void SubmitLevelScore(int chapterID, int matchScore)
    {
        // Önce listeyi kontrol et (Null ise başlat)
        if (activeSave.levelBestScores == null)
            activeSave.levelBestScores = new List<LevelScoreData>();

        // Bu bölüm daha önce oynanmış mı?
        int index = activeSave.levelBestScores.FindIndex(x => x.chapterID == chapterID);

        if (index != -1)
        {
            // EVET, daha önce oynanmış.
            int oldBest = activeSave.levelBestScores[index].bestScore;

            // Eğer yeni skor daha iyiyse güncelle
            if (matchScore > oldBest)
            {
                // Aradaki farkı genel toplama ekle
                int difference = matchScore - oldBest;
                activeSave.totalScore += difference;

                // Listeyi güncelle (Struct olduğu için geri atıyoruz)
                LevelScoreData data = activeSave.levelBestScores[index];
                data.bestScore = matchScore;
                activeSave.levelBestScores[index] = data;

                Debug.Log($"Yeni Bölüm Rekoru! Puan arttı: +{difference}");
            }
        }
        else
        {
            // HAYIR, bu bölüm ilk defa bitiriliyor.
            // Direkt toplama ekle
            activeSave.totalScore += matchScore;

            // Listeye kaydet
            LevelScoreData newData = new LevelScoreData { chapterID = chapterID, bestScore = matchScore };
            activeSave.levelBestScores.Add(newData);

            Debug.Log($"İlk Kez Bitti! Toplama Eklendi: {matchScore}");
        }

        SaveGame();
    }

    public int GetLevelBestScore(int chapterID)
    {
        if (activeSave.levelBestScores == null) return 0;

        // Listede bu ID'ye sahip bir kayıt var mı?
        var data = activeSave.levelBestScores.Find(x => x.chapterID == chapterID);

        // Varsa puanı döndür, yoksa 0 döndür
        return (data.chapterID != 0) ? data.bestScore : 0;
    }

    // UI İÇİN: Tüm bölümlerin toplam puanını hesapla (Garanti olsun diye)
    public int CalculateTotalScoreRealtime()
    {
        if (activeSave.levelBestScores == null) return 0;

        int total = 0;
        foreach (var levelData in activeSave.levelBestScores)
        {
            total += levelData.bestScore;
        }
        return total;
    }
    public int GetStoryAverageScore(List<ChapterData> storyChapters)
    {
        // Bölüm yoksa hata vermesin, 0 dönsün
        if (storyChapters == null || storyChapters.Count == 0) return 0;

        int totalScore = 0;

        // 1. Hikayenin içindeki tüm bölümleri tek tek gez
        foreach (var chapter in storyChapters)
        {
            // 2. Her bölümün en yüksek puanını SaveManager kasasından çek ve topla
            // (Oynanmamışsa zaten 0 gelir, oynanmışsa puanı gelir)
            totalScore += GetLevelBestScore(chapter.chapterID);
        }

        // 3. MATEMATİK: (Toplam Puan / Toplam Bölüm Sayısı)
        // Örnek: 300 / 10 = 30
        int average = totalScore / storyChapters.Count;

        return average;
    }

    [ContextMenu("Reset All Data")] // Inspector'dan sağ tıklayıp da çalıştırabilirsin
    public void ResetAllData()
    {
        // 1. Önce diski tamamen temizle
        PlayerPrefs.DeleteAll();

        // 2. Hafızadaki kutuyu (activeSave) sıfırla! (EN ÖNEMLİ KISIM)
        CreateNewSave();

        Debug.Log("HER ŞEY SIFIRLANDI! Hafıza ve Disk temizlendi.");

        // 3. Sahneyi yenile ki UI'daki puanlar ve isimler anında silinsin
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // --- GÖREV İLERLEMESİ (PARTIAL PROGRESS) ---

    // İlerlemeyi Kaydet (LevelManager çağıracak)
    public void SaveMissionProgress(int chapterID, int missionIndex, int progress)
    {
        // Listede var mı bak
        int index = activeSave.missionProgresses.FindIndex(x => x.chapterID == chapterID && x.missionIndex == missionIndex);

        if (index != -1)
        {
            // Varsa güncelle
            MissionProgressSave data = activeSave.missionProgresses[index];
            data.progress = progress;
            activeSave.missionProgresses[index] = data;
        }
        else
        {
            // Yoksa yeni ekle
            activeSave.missionProgresses.Add(new MissionProgressSave
            {
                chapterID = chapterID,
                missionIndex = missionIndex,
                progress = progress
            });
        }
        SaveGame();
    }

    // İlerlemeyi Getir (LevelManager açılışta çağıracak)
    public int GetMissionProgress(int chapterID, int missionIndex)
    {
        var data = activeSave.missionProgresses.Find(x => x.chapterID == chapterID && x.missionIndex == missionIndex);
        // Bulursa progress döner, bulamazsa 0 döner (struct varsayılanı)
        return (data.chapterID != 0) ? data.progress : 0;
    }

    // --- BÖLÜM KİLİT SİSTEMİ ---

    // En son açılan bölüm kaç?
    public int GetLastUnlockedLevel()
    {
        // Eğer veri 1'den küçükse (0 falansa) en az 1 döndür
        return (activeSave.lastUnlockedLevel > 0) ? activeSave.lastUnlockedLevel : 1;
    }

    // Yeni bölüm kilidi aç
    public void UnlockNextLevel(int completedLevelID)
    {
        // Eğer şu an bitirdiğim bölüm (Örn: 5), kayıtlı olandan (Örn: 4) büyükse güncelle
        // Ama eğer 1. bölümü tekrar oynuyorsam ve zaten 10. bölüm açıksa, 1 yapma!
        if (completedLevelID >= activeSave.lastUnlockedLevel)
        {
            activeSave.lastUnlockedLevel = completedLevelID + 1; // Bir sonrakini aç
            SaveGame();
        }
    }

    // --- ANA GÖREV TAKİBİ ---

    public bool IsMainMissionDone(int chapterID)
    {
        if (activeSave.completedMainChapters == null) return false;
        return activeSave.completedMainChapters.Contains(chapterID);
    }

    public void SetMainMissionDone(int chapterID)
    {
        if (activeSave.completedMainChapters == null)
            activeSave.completedMainChapters = new List<int>();

        if (!activeSave.completedMainChapters.Contains(chapterID))
        {
            activeSave.completedMainChapters.Add(chapterID);
            SaveGame();
        }
    }

    // --- JOKER SİSTEMİ İÇİN ---

    // Yanlış cevap verildiğinde, puan silinmeden önce bunu çağıracağız
    public void SaveLastStreakBeforeReset()
    {
        // Eğer kayda değer bir seri varsa (0 değilse) hafızaya al
        if (activeSave.currentStreak > 0)
        {
            activeSave.lastLostStreak = activeSave.currentStreak;
            SaveGame();
        }
    }

    // Joker 3 kullanıldığında bunu çağıracağız
    public void RestoreLostStreak()
    {
        if (activeSave.lastLostStreak > 0)
        {
            activeSave.currentStreak = activeSave.lastLostStreak;
            // Kullandık bitti, tekrar 0 yapabiliriz veya kalabilir. Şimdilik kalsın.
            SaveGame();
        }
    }
}