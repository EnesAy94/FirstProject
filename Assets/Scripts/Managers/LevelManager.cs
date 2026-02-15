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
    public bool isLevelFinished = false; // Oyun tamamen bitti mi? (Puan 0 veya Tüm görevler bitti)

    // Panel açılmayı bekliyor mu?
    public bool isCompletionPending = false;
    public bool isFailurePending = false;

    private bool hasCelebratedMainMissions = false; // Ana görev kutlaması yapıldı mı?

    [Header("Harita ve UI")]
    public LocationInfoPanel locationCardPanel;
    public Route mapRoute;

    [Header("Puan Sistemi")]
    public int currentScore;
    public TextMeshProUGUI scoreText;

    [Header("Bölüm Sonu Panelleri & Butonlar")]
    public GameObject levelCompletePanel;
    public GameObject levelFailedPanel;
    public GameObject nextLevelButton;
    public GameObject keepPlayingButton; // "Devam Et" butonu

    [Header("Bilgilendirme (Info) Paneli")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationTitle;
    public TextMeshProUGUI notificationDesc;
    public Button notificationButton;
    public Button notificationJokerButton;

    [Header("Ceza Köşesi Durumu")]
    public bool isPenaltyActive = false;
    public int penaltyCorrectCount = 0;
    public Button diceButton;

    [Header("Hapishane Joker Durumu")]
    public bool isPrisonJokerActive = false;

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
        UpdateBoardVisuals();
    }

    // --- BÖLÜM BAŞLATMA ---
    void StartChapter()
    {
        isLevelFinished = false;
        hasCelebratedMainMissions = false;
        isCompletionPending = false;

        currentScore = currentChapter.startingScore;

        if (UIManager.instance != null)
        {
            UIManager.instance.InitializeHealthBar(currentChapter.startingScore, currentScore);

            if (SaveManager.instance != null)
            {
                int savedStreak = SaveManager.instance.activeSave.currentStreak;
                UIManager.instance.UpdateStreak(savedStreak);
            }
        }

        UpdateScoreUI();

        activeMissions = new List<MissionData>();

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
                missionCopy.currentProgress = SaveManager.instance.GetMissionProgress(currentChapter.chapterID, i);
            }
            activeMissions.Add(missionCopy);
        }

        if (OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
    }

    // --- PUAN SİSTEMİ (DÜZELTİLDİ) ---
    public void DecreaseScore()
    {
        // Eğer oyun zaten bitmişse (Kaybetme veya Kazanma), puan düşmesin.
        if (isLevelFinished) return;

        int penalty = currentChapter.penaltyPerWrongAnswer;
        currentScore -= penalty;

        if (currentScore < 0) currentScore = 0;

        UpdateScoreUI();

        if (currentScore <= 0)
        {
            isLevelFinished = true; // Oyun bitti (Kaybettin)
            isFailurePending = true; // Panel açılmayı bekliyor
            Debug.Log("Puan bitti! Başarısızlık paneli sıraya alındı.");
        }
    }

    public void OpenPendingLevelFailedPanel()
    {
        if (!isFailurePending) return;
        if (levelFailedPanel != null) levelFailedPanel.SetActive(true);
        isFailurePending = false;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUAN: " + currentScore;
            scoreText.color = (currentScore <= 30) ? Color.red : Color.white;
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealthBar(currentScore, currentChapter.startingScore);
        }
    }

    // --- GÖREV KONTROL SİSTEMİ ---
    public void CheckMissionProgress(TileType type)
    {
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

        foreach (MissionData mission in activeMissions)
        {
            if (mission.currentProgress >= mission.targetAmount) continue;

            if (mission.type == MissionType.SolveAny || mission.type == targetType)
            {
                mission.currentProgress++;
                gorevGuncellendi = true;

                // Görev Tamamlandı
                if (mission.currentProgress >= mission.targetAmount)
                {
                    // 1. Robot Konuşması
                    if (RobotAssistant.instance != null)
                    {
                        string baslik = mission.isMainMission ? "ANA GÖREV" : "EK GÖREV";
                        RobotAssistant.instance.Say($"{baslik} TAMAMLANDI:\n{mission.description}", 4f);
                    }

                    // 2. Başarım (Notification)
                    if (!string.IsNullOrEmpty(mission.unlockAchievementKey))
                    {
                        SaveManager.instance.CompleteMission(mission.unlockAchievementKey);
                        if (AchievementManager.instance != null)
                        {
                            AchievementManager.instance.AddProgress(mission.unlockAchievementKey, 1);
                        }
                    }
                }
            }
        }

        if (gorevGuncellendi)
        {
            if (OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
            SaveAllProgress();

            // --- PANEL AÇMA MANTIĞI (GÜNCELLENDİ) ---

            // 1. Durum: Tüm görevler (Ana + Yan) bitti mi?
            if (AreAllMissionsCompleted())
            {
                Debug.Log("HER ŞEY BİTTİ -> Panel Bekliyor");
                PrepareLevelCompletionData();
                isCompletionPending = true; // AnswerManager panel kapatınca bunu görecek
            }
            // 2. Durum: Sadece Ana Görevler yeni mi bitti? (Daha önce kutlanmadıysa)
            else if (AreMainMissionsCompleted() && !hasCelebratedMainMissions)
            {
                Debug.Log("ANA GÖREVLER BİTTİ -> Panel Bekliyor");
                hasCelebratedMainMissions = true; // Bir daha açılmasın

                // Ana görev bitince de verileri kaydedelim ve kilidi açalım
                SaveManager.instance.SetMainMissionDone(currentChapter.chapterID);
                SaveManager.instance.UnlockNextLevel(currentChapter.chapterID);
                SaveAllProgress();

                isCompletionPending = true; // AnswerManager panel kapatınca bunu görecek
            }
        }
    }

    // --- YARDIMCI KONTROLLER ---

    // Tüm görevler (Ana + Yan) bitti mi?
    public bool AreAllMissionsCompleted()
    {
        foreach (MissionData mission in activeMissions)
            if (mission.currentProgress < mission.targetAmount) return false;
        return true;
    }

    // Sadece Ana Görevler bitti mi?
    public bool AreMainMissionsCompleted()
    {
        foreach (MissionData mission in activeMissions)
        {
            if (mission.isMainMission && mission.currentProgress < mission.targetAmount)
                return false;
        }
        return true;
    }

    // --- BÖLÜM BİTİŞ VERİLERİNİ HAZIRLA ---
    void PrepareLevelCompletionData()
    {
        SaveManager.instance.SetMainMissionDone(currentChapter.chapterID);
        SaveManager.instance.UnlockNextLevel(currentChapter.chapterID);
        SaveManager.instance.SubmitLevelScore(currentChapter.chapterID, currentScore);
        SaveAllProgress();

        // Eğer HER ŞEY bittiyse (Yan görev kalmadıysa) oyunu bitiriyoruz
        if (AreAllMissionsCompleted())
        {
            isLevelFinished = true; // Artık puan düşmez, zar atılmaz
            SetDiceInteractable(false);
            if (UIManager.instance != null) UIManager.instance.SetRobotInteractable(false);
        }
    }

    // --- BÖLÜM SONU PANELİNİ AÇ (AnswerManager Çağırır) ---
    public void OpenLevelCompletePanelNow()
    {
        // Eğer açılmayı bekleyen bir durum yoksa açma
        if (!isCompletionPending) return;

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            // Sonraki bölüm butonu her zaman görünür (Eğer varsa)
            if (nextLevelButton != null)
                nextLevelButton.SetActive(currentChapter.nextChapter != null);

            // "Devam Et" butonu SADECE yan görevler duruyorsa görünür
            bool herSeyBittiMi = AreAllMissionsCompleted();

            if (keepPlayingButton != null)
            {
                // Eğer her şey bittiyse -> Devam butonu YOK
                // Eğer yan görev kaldıysa -> Devam butonu VAR
                keepPlayingButton.SetActive(!herSeyBittiMi);
            }
        }

        isCompletionPending = false; // Bekleme bitti
    }

    // --- BUTON FONKSİYONLARI ---

    // "Devam Et" butonuna basınca çalışır
    public void OnClick_KeepPlaying()
    {
        levelCompletePanel.SetActive(false);

        // Oyunu "bitmiş" modundan çıkar, devam etsin
        isLevelFinished = false;
        SetDiceInteractable(true);
        if (UIManager.instance != null) UIManager.instance.SetRobotInteractable(true);

        isCompletionPending = false;
    }

    public void OnClick_NextLevel()
    {
        if (currentChapter.nextChapter != null)
        {
            GameSession.activeChapter = currentChapter.nextChapter;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void OnClick_ExitGame() { Time.timeScale = 1f; SceneManager.LoadScene(mainMenuSceneName); }
    public void ReturnToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(mainMenuSceneName); }
    public void RetryLevel() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

    // --- CEZA & DİĞER FONKSİYONLAR ---
    void LevelFailed()
    {
        isLevelFinished = true;
        if (levelFailedPanel != null) levelFailedPanel.SetActive(true);
    }

    public void EnterPenaltyZone()
    {
        isPenaltyActive = true;
        penaltyCorrectCount = 0;
        isPrisonJokerActive = false;
        SetDiceInteractable(false);

        // Joker var mı kontrol et
        bool hasPrisonJoker = false;
        if (JokerManager.instance != null &&
            JokerManager.instance.jokerInventory.ContainsKey(JokerType.PrisonBreak) &&
            JokerManager.instance.jokerInventory[JokerType.PrisonBreak] > 0)
        {
            hasPrisonJoker = true;
        }

        // INFO PANELİ AÇ
        ShowNotification(
            "CEZA ALANI!",
            "Buradan çıkmak için 3 soruyu doğru bilmelisin.\n\nYa da tünel kazıp kaçabilirsin!",
            // A) Normal Devam Butonu (Joker kullanmazsa)
            () =>
            {
                QuestionManager.instance.AskRandomNormalQuestion();
            },
            // B) Joker Butonu (Varsa çalışır)
            hasPrisonJoker ? () =>
            {
                // Joker butonuna basınca ONAY PANELI açılır
                if (JokerConfirmationPanel.instance != null)
                {
                    JokerConfirmationPanel.instance.ShowPanel(
                        "FİRAR TÜNELİ",
                        "Riskli ama hızlı! Zor bir soru sorulacak.\nBilirsen ANINDA çıkarsın.\nDenemek istiyor musun?",
                        () => // EVET dedi
                        {
                            // Info Paneli de kapat
                            notificationPanel.SetActive(false);

                            // Jokeri Harca
                            JokerManager.instance.jokerInventory[JokerType.PrisonBreak]--;
                            JokerManager.instance.RefreshInventoryUI();

                            // Modu Aç ve Zor Soru Sor
                            isPrisonJokerActive = true;
                            QuestionManager.instance.SoruOlusturVeSor(TileType.Hard);
                        },
                        () => // HAYIR dedi
                        {
                            // Hiçbir şey yapma, sadece onay paneli kapanır.
                            // Oyuncu Info Panel'e geri döner.
                        }
                    );
                }
            }
        : null // Joker yoksa null gider
        );
    }

    public void CheckPenaltyProgress(bool isCorrect)
    {
        if (isCorrect) penaltyCorrectCount++;
    }

    public void ExitPenaltyZone()
    {
        if (!string.IsNullOrEmpty(penaltyExitAchievementID)) AchievementManager.instance.AddProgress(penaltyExitAchievementID, 1);
        isPenaltyActive = false;
        penaltyCorrectCount = 0;
        SetDiceInteractable(true);
    }

    public void EnterHardZone()
    {
        SetDiceInteractable(false);
        ShowNotification("RİSKLİ BÖLGE!", "Zor soru geliyor!", () => { QuestionManager.instance.SoruOlusturVeSor(TileType.Hard); });
    }

    public void ShowNotification(string title, string desc, System.Action onConfirm, System.Action onJokerAction = null)
    {
        if (notificationPanel == null) return;

        notificationPanel.SetActive(true);
        notificationTitle.text = title;
        notificationDesc.text = desc;

        // 1. Normal Buton (Devam)
        notificationButton.onClick.RemoveAllListeners();
        notificationButton.onClick.AddListener(() =>
        {
            notificationPanel.SetActive(false);
            onConfirm.Invoke();
        });

        // 2. Joker Butonu Ayarı
        if (notificationJokerButton != null)
        {
            if (onJokerAction != null)
            {
                // Joker var, butonu göster ve işlev ata
                notificationJokerButton.gameObject.SetActive(true);
                notificationJokerButton.onClick.RemoveAllListeners();
                notificationJokerButton.onClick.AddListener(() =>
                {
                    // Info paneli kapatmıyoruz! Onay paneli üstüne açılacak.
                    // Eğer onay panelinden "Evet" gelirse o zaman kapatacağız (yukarıdaki kodda yazdık).
                    onJokerAction.Invoke();
                });
            }
            else
            {
                // Joker yok veya bu panel başka bir şey için açıldı -> Butonu gizle
                notificationJokerButton.gameObject.SetActive(false);
            }
        }
    }

    public void SetDiceInteractable(bool state)
    {
        if (diceButton != null) diceButton.interactable = state;
    }

    void SaveAllProgress()
    {
        for (int i = 0; i < activeMissions.Count; i++)
            SaveManager.instance.SaveMissionProgress(currentChapter.chapterID, i, activeMissions[i].currentProgress);
    }

    void UpdateBoardVisuals()
    {
        if (currentChapter == null || mapRoute == null) return;
        foreach (Transform child in mapRoute.childNodes)
        {
            Tile tileScript = child.GetComponent<Tile>();
            if (tileScript != null)
            {
                LocationStoryInfo info = currentChapter.GetStoryInfo(tileScript.type);
                if (!string.IsNullOrEmpty(info.locationName)) tileScript.UpdateVisuals(info.locationIcon, info.locationName);
            }
        }
    }
}