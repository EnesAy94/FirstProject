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
    public bool isLevelFinished = false; // Oyun tamamen bitti mi?

    // Panel açılmayı bekliyor mu?
    public bool isCompletionPending = false;
    public bool isFailurePending = false;

    [Header("Tur Takibi")]
    public int wrongAnswersInCurrentLap = 0;

    [Header("Joker Takibi")]
    public bool isJokerUsedInCurrentLap = false; // Bu turda kullanıldı mı?
    public bool isJokerUsedInLevel = false;

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
        wrongAnswersInCurrentLap = 0;
        isJokerUsedInCurrentLap = false;
        isJokerUsedInLevel = false;

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

    // --- GÖREV KONTROL SİSTEMİ (GÜNCELLENDİ) ---
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
            case TileType.Orange: targetType = MissionType.SolveOrange; break;
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
                    if (RobotAssistant.instance != null)
                    {
                        string baslik = mission.isMainMission ? "ANA GÖREV" : "EK GÖREV";
                        RobotAssistant.instance.Say($"{baslik} TAMAMLANDI:\n{mission.description}", 4f);
                    }

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

            // --- PANEL AÇMA VE KAYIT MANTIĞI (KRİTİK GÜNCELLEME) ---

            // SENARYO 1: Ana Görevler İLK DEFA bitti mi?
            if (AreMainMissionsCompleted() && !hasCelebratedMainMissions)
            {
                Debug.Log("✅ ANA GÖREVLER TAMAMLANDI -> Puan Kaydediliyor...");
                hasCelebratedMainMissions = true;

                // Puanı Kaydet, Bileti Ver
                PrepareMainMissionCompletion();

                // Paneli açılmak üzere sıraya al
                isCompletionPending = true;
            }

            // SENARYO 2: Her şey (Yan Görevler dahil) bitti mi?
            else if (AreAllMissionsCompleted())
            {
                Debug.Log("✅ TÜM GÖREVLER (YAN DAHİL) BİTTİ -> Oyun Kilitleniyor...");

                if (!hasCelebratedMainMissions)
                {
                    hasCelebratedMainMissions = true;
                    PrepareMainMissionCompletion();
                }

                // Oyunu tamamen bitir (Zarları kilitle)
                isLevelFinished = true;
                SetDiceInteractable(false);
                if (UIManager.instance != null) UIManager.instance.SetRobotInteractable(false);

                // Paneli tekrar aç (Bu sefer "Devam Et" butonu çıkmayacak)
                isCompletionPending = true;
            }
        }
    }

    // --- ANA GÖREV BİTİNCE ÇALIŞACAK FONKSİYON (YENİ) ---
    void PrepareMainMissionCompletion()
    {
        Debug.Log("📢 BÖLÜM KAZANILDI! Veriler Kaydediliyor...");

        // 1. Temel İlerlemeler
        SaveManager.instance.SetMainMissionDone(currentChapter.chapterID);
        SaveManager.instance.UnlockNextLevel(currentChapter.chapterID);

        // 2. SKORU KAYDET (En önemli kısım burası!)
        SaveManager.instance.SubmitLevelScore(currentChapter.chapterID, currentScore);

        // 3. BİLET TRANSFERİ (Joker -> Cüzdan)
        if (JokerManager.instance != null &&
            JokerManager.instance.jokerInventory.ContainsKey(JokerType.WheelTicket))
        {
            int ticketCount = JokerManager.instance.jokerInventory[JokerType.WheelTicket];

            if (ticketCount > 0)
            {
                SaveManager.instance.AddTicketsToWallet(ticketCount);
                if (RobotAssistant.instance != null)
                    RobotAssistant.instance.Say($"Tebrikler! {ticketCount} adet Şans Bileti cüzdanına eklendi!", 4f);

                // Envanterden sil
                JokerManager.instance.jokerInventory[JokerType.WheelTicket] = 0;
            }
        }

        if (!isJokerUsedInLevel)
        {
            Debug.Log("🏆 BÖLÜM JOKERSİZ BİTTİ!");

            // a) Görevi İlerlet
            CheckLevelEndMissionProgress(MissionType.CompleteLevelNoJoker);

            // b) Başarımı Ver (Sadece bölüm bitince verilsin demiştin)
            // AchievementData ID'si: "no_joker_master" olsun
            if (AchievementManager.instance != null)
            {
                AchievementManager.instance.AddProgress("no_joker_master", 1);
            }
        }

        SaveAllProgress();
    }

    // --- YARDIMCI KONTROLLER ---
    public bool AreAllMissionsCompleted()
    {
        foreach (MissionData mission in activeMissions)
            if (mission.currentProgress < mission.targetAmount) return false;
        return true;
    }

    public bool AreMainMissionsCompleted()
    {
        foreach (MissionData mission in activeMissions)
        {
            if (mission.isMainMission && mission.currentProgress < mission.targetAmount)
                return false;
        }
        return true;
    }

    // --- BÖLÜM SONU PANELİNİ AÇ ---
    public void OpenLevelCompletePanelNow()
    {
        if (!isCompletionPending) return;

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            if (nextLevelButton != null)
                nextLevelButton.SetActive(currentChapter.nextChapter != null);

            // "Devam Et" butonu SADECE yan görevler duruyorsa görünür
            bool herSeyBittiMi = AreAllMissionsCompleted();

            if (keepPlayingButton != null)
            {
                keepPlayingButton.SetActive(!herSeyBittiMi);
            }
        }

        isCompletionPending = false;
    }

    // --- BUTON FONKSİYONLARI ---
    public void OnClick_KeepPlaying()
    {
        levelCompletePanel.SetActive(false);

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

    // --- CEZA & JOKER FONKSİYONLARI ---
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

        bool hasPrisonJoker = false;
        if (JokerManager.instance != null &&
            JokerManager.instance.jokerInventory.ContainsKey(JokerType.PrisonBreak) &&
            JokerManager.instance.jokerInventory[JokerType.PrisonBreak] > 0)
        {
            hasPrisonJoker = true;
        }

        ShowNotification(
            "CEZA ALANI!",
            "Buradan çıkmak için 3 soruyu doğru bilmelisin.\n\nYa da tünel kazıp kaçabilirsin!",
            () => { QuestionManager.instance.AskRandomNormalQuestion(); },
            hasPrisonJoker ? () =>
            {
                if (JokerConfirmationPanel.instance != null)
                {
                    JokerConfirmationPanel.instance.ShowPanel(
                        "FİRAR TÜNELİ",
                        "Riskli ama hızlı! Zor bir soru sorulacak.\nBilirsen ANINDA çıkarsın.\nDenemek istiyor musun?",
                        () => // EVET
                        {
                            notificationPanel.SetActive(false);
                            JokerManager.instance.jokerInventory[JokerType.PrisonBreak]--;
                            JokerManager.instance.RefreshInventoryUI();
                            LevelManager.instance.RegisterJokerUsage();
                            isPrisonJokerActive = true;
                            QuestionManager.instance.SoruOlusturVeSor(TileType.Hard);
                        },
                        () => { } // HAYIR
                    );
                }
            }
        : null);
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

        notificationButton.onClick.RemoveAllListeners();
        notificationButton.onClick.AddListener(() =>
        {
            notificationPanel.SetActive(false);
            onConfirm.Invoke();
        });

        if (notificationJokerButton != null)
        {
            if (onJokerAction != null)
            {
                notificationJokerButton.gameObject.SetActive(true);
                notificationJokerButton.onClick.RemoveAllListeners();
                notificationJokerButton.onClick.AddListener(() => { onJokerAction.Invoke(); });
            }
            else
            {
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

    public void IncreaseScore(int amount)
    {
        currentScore += amount;
        if (currentScore > currentChapter.startingScore)
            currentScore = currentChapter.startingScore;

        UpdateScoreUI();
        Debug.Log("Can Artırıldı! Yeni Can: " + currentScore);
    }

    public int GetCurrentPenalty()
    {
        if (currentChapter != null)
        {
            return currentChapter.penaltyPerWrongAnswer;
        }
        return 10;
    }

    public void RegisterWrongAnswer()
    {
        wrongAnswersInCurrentLap++;
        Debug.Log($"Bu turdaki hata sayısı: {wrongAnswersInCurrentLap}");
    }

    public void OnLapCompleted()
    {
        Debug.Log("🏁 TUR TAMAMLANDI!");

        // A) MEVCUT: Hatasız Tur Kontrolü
        if (wrongAnswersInCurrentLap == 0)
        {
            CheckLapMissionProgress(MissionType.CompleteLapNoError);
            if (AchievementManager.instance != null) AchievementManager.instance.AddProgress("flawless_lap", 1);
        }

        // B) YENİ: Jokersiz Tur Kontrolü
        if (!isJokerUsedInCurrentLap)
        {
            Debug.Log("✨ JOKERSİZ TUR! Görev kontrol ediliyor...");
            CheckLapMissionProgress(MissionType.CompleteLapNoJoker);
        }

        // Sıfırlamalar
        wrongAnswersInCurrentLap = 0;
        isJokerUsedInCurrentLap = false; // Yeni tur için temizle (Bölüm değişkenine dokunma!)
    }

    void CheckLapMissionProgress()
    {
        bool gorevGuncellendi = false;

        foreach (MissionData mission in activeMissions)
        {
            // Eğer görev "Kusursuz Tur" tipindeyse ve henüz bitmediyse
            if (mission.type == MissionType.CompleteLapNoError && mission.currentProgress < mission.targetAmount)
            {
                mission.currentProgress++;
                gorevGuncellendi = true;

                // Görev Tamamlandı mı?
                if (mission.currentProgress >= mission.targetAmount)
                {
                    if (RobotAssistant.instance != null)
                        RobotAssistant.instance.Say($"GÖREV TAMAMLANDI:\n{mission.description}", 4f);

                    // Ödül vs. kodları buraya (CheckMissionProgress'teki ile aynı mantık)
                }
            }
        }

        if (gorevGuncellendi)
        {
            if (OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
            SaveAllProgress();
            // Buraya CheckMissionProgress'teki "Tüm görevler bitti mi?" kontrollerini de ekleyebilirsin
        }
    }

    public void RegisterJokerUsage()
    {
        isJokerUsedInCurrentLap = true;
        isJokerUsedInLevel = true;
        Debug.Log("⚠️ Joker kullanıldı! 'Jokersiz' görevleri iptal.");
    }

    // LevelManager.cs içindeki bu fonksiyonu BUL ve DEĞİŞTİR:

    void CheckLapMissionProgress(MissionType typeToCheck)
    {
        bool gorevGuncellendi = false;

        foreach (MissionData mission in activeMissions)
        {
            // 1. Tip ve İlerleme Kontrolü
            if (mission.type == typeToCheck && mission.currentProgress < mission.targetAmount)
            {
                mission.currentProgress++;
                gorevGuncellendi = true;

                // 2. TAMAMLANDI MI? (Robotun Konuşması Gereken Yer Burası!)
                if (mission.currentProgress >= mission.targetAmount)
                {
                    Debug.Log("✅ Görev Bitti: " + mission.description);

                    // --- ROBOT KONUŞMA KODU EKLENDİ ---
                    if (RobotAssistant.instance != null)
                    {
                        string baslik = mission.isMainMission ? "ANA GÖREV" : "EK GÖREV";

                        // Normal Say yerine 'Celebrate' kullanıyoruz ki tebrik etsin (yeşil yazı)
                        RobotAssistant.instance.Celebrate($"{baslik} TAMAMLANDI:\n{mission.description}");
                    }

                    // --- BAŞARIM KİLİDİ VARSA AÇ ---
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
            SaveAllProgress();
            if (OnMissionsUpdated != null) OnMissionsUpdated.Invoke();

            // 3. EĞER BU SON GÖREVSE OYUNU BİTİRME KONTROLÜ
            // (Normal sorularda olan o "Bölüm Bitti" panelini açma kodu)
            if (AreAllMissionsCompleted())
            {
                // Panel açılma işlemleri (CheckMissionProgress'teki mantıkla aynı)
                if (!isCompletionPending)
                {
                    isLevelFinished = true;
                    isCompletionPending = true;
                    OpenLevelCompletePanelNow();
                }
            }
        }
    }

    void CheckLevelEndMissionProgress(MissionType typeToCheck)
    {
        // CheckLapMissionProgress ile aynı mantık, sadece ismi farklı karışmasın diye ayırdım.
        // İstersen tek fonksiyonda birleştirebilirsin.
        CheckLapMissionProgress(typeToCheck);
    }
}