using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Mevcut Durum")]
    public ChapterData currentChapter;
    public List<MissionData> activeMissions;
    
    public bool isLevelFinished = false; 

    private bool hasCelebratedMainMissions = false; 
    private bool areMainMissionsDoneInitially = false; 

    [Header("Puan Sistemi")]
    public int currentScore;
    public TextMeshProUGUI scoreText;

    [Header("Paneller & Butonlar")]
    public GameObject levelCompletePanel;       
    public GameObject levelFailedPanel;         
    
    public GameObject nextLevelButton;  
    public GameObject keepPlayingButton; 

    public string mainMenuSceneName = "MainMenu";
    public System.Action OnMissionsUpdated;

    void Awake() { instance = this; }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailedPanel != null) levelFailedPanel.SetActive(false);

        if (GameSession.activeChapter != null) currentChapter = GameSession.activeChapter;
        if (currentChapter != null) StartChapter();
    }

    void StartChapter()
    {
        isLevelFinished = false;
        hasCelebratedMainMissions = false; 

        currentScore = currentChapter.startingScore;
        UpdateScoreUI();

        activeMissions = new List<MissionData>();

        string mainDoneKey = $"Chapter_{currentChapter.chapterID}_MainDone";
        areMainMissionsDoneInitially = PlayerPrefs.GetInt(mainDoneKey, 0) == 1;

        for (int i = 0; i < currentChapter.missions.Count; i++)
        {
            MissionData originalMission = currentChapter.missions[i];
            MissionData missionCopy = Instantiate(originalMission);

            if (missionCopy.isMainMission)
            {
                missionCopy.currentProgress = 0; // Ana görevler hep sıfırlanır
            }
            else
            {
                // Yan görevler hafızadan gelir
                string missionKey = $"Chapter_{currentChapter.chapterID}_Mission_{i}_Progress";
                missionCopy.currentProgress = PlayerPrefs.GetInt(missionKey, 0);
            }

            activeMissions.Add(missionCopy);
        }

        if(OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
    }

    public void DecreaseScore()
    {
        if (isLevelFinished) return;
        int penalty = currentChapter.penaltyPerWrongAnswer;
        currentScore -= penalty;
        if (currentScore < 0) currentScore = 0;
        UpdateScoreUI();
        if (currentScore <= 0) LevelFailed();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUAN: " + currentScore;
            scoreText.color = (currentScore <= 30) ? Color.red : Color.white;
        }
    }

    public void CheckMissionProgress(TileType type)
    {
        if (isLevelFinished) return;

        MissionType targetType = MissionType.SolveAny;
        switch (type)
        {
            case TileType.Red: targetType = MissionType.SolveRed; break;
            case TileType.Blue: targetType = MissionType.SolveBlue; break;
            case TileType.Yellow: targetType = MissionType.SolveYellow; break;
            case TileType.Purple: targetType = MissionType.SolvePurple; break;
            case TileType.Green: targetType = MissionType.SolveGreen; break;
        }

        bool gorevGuncellendi = false;
        bool yanGorevBitti = false;

        foreach (MissionData mission in activeMissions)
        {
            if (mission.currentProgress >= mission.targetAmount) continue;

            if (mission.type == targetType || mission.type == MissionType.SolveAny)
            {
                mission.currentProgress++; 
                gorevGuncellendi = true;

                if (!mission.isMainMission && mission.currentProgress >= mission.targetAmount)
                {
                    yanGorevBitti = true;
                }
            }
        }

        if (gorevGuncellendi)
        {
            if(OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
            SaveAllProgress();

            // Eğer "Ana Görevler" daha önceden (veya önceki turda) bitmişse
            // VE şimdi "Yan Görev" de bitmişse -> O zaman kontrol et.
            // (Eğer ana görev şu an bittiyse aşağıdaki CheckMainMissionsCompletion zaten paneli açacak)
            if (hasCelebratedMainMissions && yanGorevBitti)
            {
                // Eğer her şey bittiyse paneli aç
                if (AreAllMissionsCompleted())
                {
                    Debug.Log("🌟 TEBRİKLER! BÖLÜMDEKİ HER ŞEY BİTTİ.");
                    LevelCompleted(); 
                }
            }
        }

        // Ana görevler bitti mi kontrolü
        CheckMainMissionsCompletion();
    }

    void CheckMainMissionsCompletion()
    {
        if (hasCelebratedMainMissions) return; 

        bool allMainDone = true;
        foreach (MissionData mission in activeMissions)
        {
            if (mission.isMainMission && mission.currentProgress < mission.targetAmount)
            {
                allMainDone = false;
                break;
            }
        }

        if (allMainDone)
        {
            Debug.Log("🏆 ANA GÖREVLER BİTTİ!");
            hasCelebratedMainMissions = true; 
            LevelCompleted(); // Parametre göndermiyoruz artık!
        }
    }

    // ARTIK PARAMETRE YOK: Fonksiyon kendisi hesaplıyor
    void LevelCompleted()
    {
        // 1. Önce kayıt işlemlerini yap
        PlayerPrefs.SetInt($"Chapter_{currentChapter.chapterID}_MainDone", 1);
        int savedLevel = PlayerPrefs.GetInt("CompletedLevelIndex", 0);
        if (currentChapter.chapterID >= savedLevel)
        {
            PlayerPrefs.SetInt("CompletedLevelIndex", currentChapter.chapterID); 
        }

        string scoreKey = $"HighScore_{currentChapter.chapterID}";
        if (currentScore > PlayerPrefs.GetInt(scoreKey, 0))
        {
            PlayerPrefs.SetInt(scoreKey, currentScore);
        }

        SaveAllProgress();

        // 2. Paneli Aç
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            // 3. BUTON AYARLARI
            if (nextLevelButton != null)
            {
                nextLevelButton.SetActive(currentChapter.nextChapter != null);
            }

            // 🔥 KRİTİK DÜZELTME BURASI 🔥
            // Dışarıdan gelen veriye güvenmek yerine, şu anki durumu kendimiz hesaplıyoruz.
            // "Şu an gerçekten her şey (ana+yan) bitti mi?"
            bool herSeyBittiMi = AreAllMissionsCompleted();

            // Eğer her şey bittiyse (TRUE) -> Buton GİZLENİR (Active: FALSE)
            // Eğer bitmeyen varsa (FALSE) -> Buton GÖZÜKÜR (Active: TRUE)
            if (keepPlayingButton != null)
            {
                keepPlayingButton.SetActive(!herSeyBittiMi);
            }
        }
    }

    // Yardımcı: Her şey (Yan görevler dahil) bitti mi?
    bool AreAllMissionsCompleted()
    {
        foreach (MissionData mission in activeMissions)
        {
            // Eğer tek bir görev bile hedefe ulaşmadıysa -> FALSE
            if (mission.currentProgress < mission.targetAmount)
            {
                return false; 
            }
        }
        return true; // Hepsi tamsa -> TRUE
    }

    void LevelFailed()
    {
        isLevelFinished = true;
        if (levelFailedPanel != null) levelFailedPanel.SetActive(true);
    }

    void SaveAllProgress()
    {
        for (int i = 0; i < activeMissions.Count; i++)
        {
            string missionKey = $"Chapter_{currentChapter.chapterID}_Mission_{i}_Progress";
            PlayerPrefs.SetInt(missionKey, activeMissions[i].currentProgress);
        }
        PlayerPrefs.Save();
    }

    public void OnClick_NextLevel()
    {
        if (currentChapter.nextChapter != null)
        {
            GameSession.activeChapter = currentChapter.nextChapter;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void OnClick_KeepPlaying()
    {
        levelCompletePanel.SetActive(false);
    }

    public void OnClick_ExitGame() { ReturnToMainMenu(); }
    public void ReturnToMainMenu() { SceneManager.LoadScene(mainMenuSceneName); }
    public void RetryLevel() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
}