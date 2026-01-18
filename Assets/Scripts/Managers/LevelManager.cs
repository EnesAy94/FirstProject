using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Başarım Ayarları")]
    public string penaltyExitAchievementID;

    [Header("Mevcut Durum")]
    public ChapterData currentChapter;
    public List<MissionData> activeMissions;
    public bool isLevelFinished = false;

    // Panel açılmayı bekliyor mu?
    public bool isCompletionPending = false;
    public bool isFailurePending = false;

    private bool hasCelebratedMainMissions = false;
    private bool areMainMissionsDoneInitially = false;

    [Header("Puan Sistemi")]
    public int currentScore;
    public TextMeshProUGUI scoreText;

    [Header("Bölüm Sonu Panelleri & Butonlar")]
    public GameObject levelCompletePanel;
    public GameObject levelFailedPanel;
    public GameObject nextLevelButton;
    public GameObject keepPlayingButton;

    [Header("Bilgilendirme (Info) Paneli")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationTitle;
    public TextMeshProUGUI notificationDesc;
    public Button notificationButton;

    [Header("Ceza Köşesi Durumu")]
    public bool isPenaltyActive = false;
    public int penaltyCorrectCount = 0;
    public Button diceButton;

    public string mainMenuSceneName = "MainMenu";

    // UI güncellemesi için Action
    public System.Action OnMissionsUpdated;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailedPanel != null) levelFailedPanel.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);

        if (GameSession.activeChapter != null) currentChapter = GameSession.activeChapter;

        if (currentChapter != null) StartChapter();
    }

    // --- BÖLÜM BAŞLATMA ---
    void StartChapter()
    {
        isLevelFinished = false;
        hasCelebratedMainMissions = false;
        isCompletionPending = false;

        currentScore = currentChapter.startingScore;
        UpdateScoreUI();

        activeMissions = new List<MissionData>();

        // --- DÜZELTME 1: PlayerPrefs -> SaveManager ---
        // Eskiden: PlayerPrefs.GetInt($"Chapter_{currentChapter.chapterID}_MainDone", 0) == 1;
        // Şimdi:
        areMainMissionsDoneInitially = SaveManager.instance.IsMainMissionDone(currentChapter.chapterID);
        // ----------------------------------------------

        for (int i = 0; i < currentChapter.missions.Count; i++)
        {
            MissionData originalMission = currentChapter.missions[i];
            MissionData missionCopy = Instantiate(originalMission);

            if (missionCopy.isMainMission)
            {
                missionCopy.currentProgress = 0;
            }
            else
            {
                // Yan görevler SaveManager'dan geliyor (Zaten yapmıştık)
                missionCopy.currentProgress = SaveManager.instance.GetMissionProgress(currentChapter.chapterID, i);
            }
            activeMissions.Add(missionCopy);
        }

        if (OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
    }

    // --- PUAN SİSTEMİ ---
    public void DecreaseScore()
    {
        if (isLevelFinished) return;

        int penalty = currentChapter.penaltyPerWrongAnswer;
        currentScore -= penalty;

        if (currentScore < 0) currentScore = 0;

        UpdateScoreUI();

        if (currentScore <= 0)
        {
            isLevelFinished = true;
            isFailurePending = true;
            Debug.Log("Puan bitti! Başarısızlık paneli sıraya alındı.");
        }
    }

    public void OpenPendingLevelFailedPanel()
    {
        if (!isFailurePending) return;

        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
        }

        isFailurePending = false;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUAN: " + currentScore;
            scoreText.color = (currentScore <= 30) ? Color.red : Color.white;
        }
    }

    // --- GÖREV KONTROL SİSTEMİ ---
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
            case TileType.Hard: targetType = MissionType.SolveHard; break;
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

                    if (!string.IsNullOrEmpty(mission.unlockAchievementKey))
                    {
                        SaveManager.instance.CompleteMission(mission.unlockAchievementKey);
                        Debug.Log($"Kilit Açıldı: {mission.unlockAchievementKey}");
                    }
                }
            }
        }

        if (gorevGuncellendi)
        {
            if (OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
            SaveAllProgress();

            if (hasCelebratedMainMissions && yanGorevBitti)
            {
                if (AreAllMissionsCompleted())
                {
                    Debug.Log("TEBRİKLER! HER ŞEY BİTTİ (Sıraya Alınıyor...)");
                    PrepareLevelCompletion();
                }
            }
        }

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
            Debug.Log("ANA GÖREVLER BİTTİ (Sıraya Alınıyor...)");
            hasCelebratedMainMissions = true;
            PrepareLevelCompletion();
        }
    }

    // --- BÖLÜM BİTİRME (SIRAYA ALMA & AÇMA) ---

    void PrepareLevelCompletion()
    {
        isCompletionPending = true;

        // --- DÜZELTME 2: MainDone Kaydı ---
        // Eskiden: PlayerPrefs.SetInt($"Chapter_{currentChapter.chapterID}_MainDone", 1);
        // Şimdi:
        SaveManager.instance.SetMainMissionDone(currentChapter.chapterID);

        // --- DÜZELTME 3: Bölüm Kilidi Açma ---
        // Eskiden: PlayerPrefs.GetInt("CompletedLevelIndex")... SetInt...
        // Şimdi:
        SaveManager.instance.UnlockNextLevel(currentChapter.chapterID);

        // Yüksek skor kaydı (Zaten yapmıştık)
        SaveManager.instance.SubmitLevelScore(currentChapter.chapterID, currentScore);

        SaveAllProgress();
    }

    public void OpenPendingLevelCompletePanel()
    {
        if (!isCompletionPending) return;

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            if (nextLevelButton != null)
                nextLevelButton.SetActive(currentChapter.nextChapter != null);

            bool herSeyBittiMi = AreAllMissionsCompleted();
            if (keepPlayingButton != null)
                keepPlayingButton.SetActive(!herSeyBittiMi);
        }

        isCompletionPending = false;
    }

    void LevelFailed()
    {
        isLevelFinished = true;
        if (levelFailedPanel != null) levelFailedPanel.SetActive(true);
    }

    // --- CEZA KÖŞESİ (PENALTY ZONE) ---
    public void EnterPenaltyZone()
    {
        Debug.Log(" CEZA KÖŞESİNE GİRİLDİ! Zar Kilitlendi.");
        isPenaltyActive = true;
        penaltyCorrectCount = 0;
        SetDiceInteractable(false);

        ShowNotification(
            "CEZA ALANI! ",
            "Bu alandan çıkmak için 3 soruyu doğru cevaplaman gerekiyor.\nHazır mısın?",
            () =>
            {
                QuestionManager.instance.AskRandomNormalQuestion();
            }
        );
    }

    public void CheckPenaltyProgress(bool isCorrect)
    {
        if (isCorrect) penaltyCorrectCount++;
    }

    public void ExitPenaltyZone()
    {
        Debug.Log("TEBRİKLER! Ceza Köşesinden Çıktın.");

        if (!string.IsNullOrEmpty(penaltyExitAchievementID))
        {
            AchievementManager.instance.AddProgress(penaltyExitAchievementID, 1);
        }

        isPenaltyActive = false;
        penaltyCorrectCount = 0;
        SetDiceInteractable(true);
    }

    // --- ZOR SORU KÖŞESİ (HARD ZONE) ---
    public void EnterHardZone()
    {
        SetDiceInteractable(false);

        ShowNotification(
            "RİSKLİ BÖLGE!",
            "Çok zor bir soruyla karşılaşacaksın.\nBilirsen ödül büyük, bilemezsen puanın düşer!",
            () =>
            {
                QuestionManager.instance.SoruOlusturVeSor(TileType.Hard);
            }
        );
    }

    // --- BİLGİLENDİRME PANELİ ---
    public void ShowNotification(string title, string desc, System.Action onConfirm)
    {
        if (notificationPanel == null) return;

        notificationPanel.SetActive(true);
        notificationTitle.text = title;
        notificationDesc.text = desc;

        notificationButton.onClick.RemoveAllListeners();
        notificationButton.onClick.AddListener(() =>
        {
            notificationPanel.SetActive(false);
            onConfirm.Invoke();
        });
    }

    // --- YARDIMCI VE BUTON FONKSİYONLARI ---

    public void SetDiceInteractable(bool state)
    {
        if (diceButton != null)
        {
            diceButton.interactable = state;
        }
    }

    bool AreAllMissionsCompleted()
    {
        foreach (MissionData mission in activeMissions)
        {
            if (mission.currentProgress < mission.targetAmount) return false;
        }
        return true;
    }

    void SaveAllProgress()
    {
        for (int i = 0; i < activeMissions.Count; i++)
        {
            SaveManager.instance.SaveMissionProgress(currentChapter.chapterID, i, activeMissions[i].currentProgress);
        }
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
        SetDiceInteractable(true);
        isCompletionPending = false;
    }

    public void OnClick_ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}