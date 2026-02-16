using UnityEngine;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    // Aktif kullanılan veri kutumuz (PlayerData.cs dosyasından geliyor)
    public PlayerData activeSave;

    // Kayıt dosyası adı
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
        string json = JsonUtility.ToJson(activeSave);
        PlayerPrefs.SetString(saveFileName, json);
        PlayerPrefs.Save();
        // Debug.Log("Oyun Kaydedildi."); 
    }

    // --- YÜKLEME (LOAD) ---
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(saveFileName))
        {
            string json = PlayerPrefs.GetString(saveFileName);
            activeSave = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("Oyun Yüklendi!");
        }
        else
        {
            CreateNewSave();
        }
    }

    void CreateNewSave()
    {
        activeSave = new PlayerData();
        activeSave.totalScore = 0;
        activeSave.maxLevelReached = 1;
        activeSave.lastUnlockedLevel = 1;
        activeSave.globalTicketCount = 0; // Biletleri sıfırla

        // Listeleri başlat (Null hatası almamak için)
        activeSave.earnedAchievements = new List<string>();
        activeSave.completedMissions = new List<string>();
        activeSave.achievementProgress = new List<ProgressData>();
        activeSave.levelBestScores = new List<LevelScoreData>();
        activeSave.missionProgresses = new List<MissionProgressSave>();
        activeSave.completedMainChapters = new List<int>();
        activeSave.usedQuestions = new List<UsedQuestionData>();

        SaveGame(); // İlk boş kaydı oluştur
    }

    // ========================================================================
    // --- YENİ EKLENEN: CÜZDAN & BİLET SİSTEMİ ---
    // ========================================================================

    public void AddTicketsToWallet(int amount)
    {
        if (activeSave != null)
        {
            // PlayerData dosyanın içinde 'globalTicketCount' olduğundan emin ol!
            activeSave.globalTicketCount += amount;
            SaveGame();
            Debug.Log($"🎟️ Cüzdana {amount} bilet eklendi! Toplam: {activeSave.globalTicketCount}");
        }
    }

    public int GetTicketCount()
    {
        return activeSave != null ? activeSave.globalTicketCount : 0;
    }

    // ========================================================================

    // --- GÜNCELLEME KOMUTLARI ---

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
        int index = activeSave.achievementProgress.FindIndex(x => x.id == id);

        if (index != -1)
        {
            ProgressData data = activeSave.achievementProgress[index];
            data.amount = amount;
            activeSave.achievementProgress[index] = data;
        }
        else
        {
            activeSave.achievementProgress.Add(new ProgressData { id = id, amount = amount });
        }
        SaveGame();
    }

    // Başarım İlerlemesini Getir
    public int GetAchievementProgress(string id)
    {
        var data = activeSave.achievementProgress.Find(x => x.id == id);
        return (data.id != null) ? data.amount : 0;
    }

    // Cevap Kaydı (Streak ve İstatistik)
    public void RegisterAnswer(bool isCorrect, bool isHardQuestion, bool isPenaltyQuestion, bool updateStreak = true)
    {
        // 1. STREAK (SERİ) HESABI
        if (updateStreak)
        {
            if (isCorrect)
            {
                activeSave.currentStreak++;
                if (activeSave.currentStreak > activeSave.maxStreak)
                {
                    activeSave.maxStreak = activeSave.currentStreak;
                }
            }
            else
            {
                activeSave.currentStreak = 0;
            }
        }

        // 2. İSTATİSTİK KAYDI
        if (isPenaltyQuestion)
        {
            if (isCorrect) activeSave.penaltyCorrectCount++;
            else activeSave.penaltyWrongCount++;
        }
        else if (isHardQuestion)
        {
            if (isCorrect) activeSave.hardCorrectCount++;
            else activeSave.hardWrongCount++;
        }
        else
        {
            if (isCorrect) activeSave.normalCorrectCount++;
            else activeSave.normalWrongCount++;
        }

        SaveGame();
    }

    // --- PROFİL BİLGİLERİ ---
    public void SaveProfileInfo(string name, string surname, string nickname)
    {
        activeSave.playerName = name;
        activeSave.playerSurname = surname;
        activeSave.playerNickname = nickname;
        SaveGame();
    }

    // --- AKILLI BÖLÜM PUANI (HighScore) ---
    public void SubmitLevelScore(int chapterID, int matchScore)
    {
        if (activeSave.levelBestScores == null)
            activeSave.levelBestScores = new List<LevelScoreData>();

        int index = activeSave.levelBestScores.FindIndex(x => x.chapterID == chapterID);

        if (index != -1)
        {
            int oldBest = activeSave.levelBestScores[index].bestScore;
            if (matchScore > oldBest)
            {
                int difference = matchScore - oldBest;
                activeSave.totalScore += difference;

                LevelScoreData data = activeSave.levelBestScores[index];
                data.bestScore = matchScore;
                activeSave.levelBestScores[index] = data;
            }
        }
        else
        {
            activeSave.totalScore += matchScore;
            LevelScoreData newData = new LevelScoreData { chapterID = chapterID, bestScore = matchScore };
            activeSave.levelBestScores.Add(newData);
        }
        SaveGame();
    }

    public int GetLevelBestScore(int chapterID)
    {
        if (activeSave.levelBestScores == null) return 0;
        var data = activeSave.levelBestScores.Find(x => x.chapterID == chapterID);
        return (data.chapterID != 0) ? data.bestScore : 0;
    }

    // --- GÖREV İLERLEMESİ (Partial Progress) ---
    public void SaveMissionProgress(int chapterID, int missionIndex, int progress)
    {
        int index = activeSave.missionProgresses.FindIndex(x => x.chapterID == chapterID && x.missionIndex == missionIndex);

        if (index != -1)
        {
            MissionProgressSave data = activeSave.missionProgresses[index];
            data.progress = progress;
            activeSave.missionProgresses[index] = data;
        }
        else
        {
            activeSave.missionProgresses.Add(new MissionProgressSave
            {
                chapterID = chapterID,
                missionIndex = missionIndex,
                progress = progress
            });
        }
        SaveGame();
    }

    public int GetMissionProgress(int chapterID, int missionIndex)
    {
        var data = activeSave.missionProgresses.Find(x => x.chapterID == chapterID && x.missionIndex == missionIndex);
        return (data.chapterID != 0) ? data.progress : 0;
    }

    // --- BÖLÜM KİLİT SİSTEMİ ---
    public int GetLastUnlockedLevel()
    {
        return (activeSave.lastUnlockedLevel > 0) ? activeSave.lastUnlockedLevel : 1;
    }

    public void UnlockNextLevel(int completedLevelID)
    {
        if (completedLevelID >= activeSave.lastUnlockedLevel)
        {
            activeSave.lastUnlockedLevel = completedLevelID + 1;
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

    // --- JOKER SERİ KURTARMA ---
    public void SaveLastStreakBeforeReset()
    {
        if (activeSave.currentStreak > 0)
        {
            activeSave.lastLostStreak = activeSave.currentStreak;
            SaveGame();
        }
    }

    public void RestoreLostStreak()
    {
        if (activeSave.lastLostStreak > 0)
        {
            activeSave.currentStreak = activeSave.lastLostStreak;
            SaveGame();
        }
    }

    // --- SIFIRLAMA ---
    [ContextMenu("Reset All Data")]
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        CreateNewSave();
        Debug.Log("HER ŞEY SIFIRLANDI!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public int GetStoryAverageScore(List<ChapterData> storyChapters)
    {
        // Bölüm listesi boşsa 0 döndür
        if (storyChapters == null || storyChapters.Count == 0) return 0;

        int totalScore = 0;

        // 1. Hikayenin içindeki tüm bölümleri gez ve puanları topla
        foreach (var chapter in storyChapters)
        {
            // Oynanmamışsa zaten 0 gelir, oynanmışsa puanı gelir
            int score = GetLevelBestScore(chapter.chapterID);
            totalScore += score;
        }

        return totalScore / storyChapters.Count;
    }
}