using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Sahne değişimi için şart
using TMPro; // Puan yazısı (Text) için şart

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Mevcut Durum")]
    public ChapterData currentChapter;          // Şu an oynanan bölüm verisi
    public List<MissionData> activeMissions;    // Aktif görev listesi
    public bool isLevelFinished = false;        // Oyun bitti mi? (Hareket engellemek için)

    [Header("Puan Sistemi")]
    public int currentScore;                    // Anlık puan
    public TextMeshProUGUI scoreText;           // Ekrandaki puan yazısı

    [Header("Paneller")]
    public GameObject levelCompletePanel;       // KAZANDIN Paneli
    public GameObject levelFailedPanel;         // KAYBETTİN Paneli
    public string mainMenuSceneName = "MainMenu"; // Ana menü sahnesinin adı

    // Görevler güncellenince UI'ya haber veren sistem
    public System.Action OnMissionsUpdated;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Başlangıçta panelleri gizle
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailedPanel != null) levelFailedPanel.SetActive(false);

        // Menüden seçilen bölümü al
        if (GameSession.activeChapter != null)
        {
            currentChapter = GameSession.activeChapter;
        }
        
        // Bölümü başlat
        if (currentChapter != null)
        {
            StartChapter();
        }
    }

    void StartChapter()
    {
        isLevelFinished = false;

        // 1. Puanı Başlat (ChapterData'dan gelen değere göre)
        currentScore = currentChapter.startingScore;
        UpdateScoreUI();

        // 2. Görevleri Kopyala (Orijinal dosya bozulmasın diye kopya oluşturuyoruz)
        activeMissions = new List<MissionData>();
        foreach (MissionData mission in currentChapter.missions)
        {
            MissionData missionCopy = Instantiate(mission);
            missionCopy.currentProgress = 0;
            activeMissions.Add(missionCopy);
        }

        // UI'yı güncelle (Görev Listesi)
        if(OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
    }

    // --- PUAN SİSTEMİ ---
    
    // Yanlış cevap verildiğinde çağrılır
    public void DecreaseScore()
    {
        if (isLevelFinished) return;

        // Bölüm ayarlarındaki ceza puanını düş
        int penalty = currentChapter.penaltyPerWrongAnswer;
        currentScore -= penalty;

        // Puan eksiye düşmesin
        if (currentScore < 0) currentScore = 0;

        UpdateScoreUI();
        Debug.Log($"⚠️ Yanlış Cevap! Puan düştü. Kalan: {currentScore}");

        // KAYBETME KONTROLÜ (0 puan)
        if (currentScore <= 0)
        {
            LevelFailed();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUAN: " + currentScore;
            
            // Puan azaldıysa (30 altı) kırmızı yap, yoksa beyaz
            if (currentScore <= 30) scoreText.color = Color.red;
            else scoreText.color = Color.black;
        }
    }

    // --- GÖREV SİSTEMİ ---

    public void CheckMissionProgress(TileType type)
    {
        if (isLevelFinished) return;

        // Hangi tür soru çözüldü?
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

        // Görev listesini tara
        foreach (MissionData mission in activeMissions)
        {
            // Zaten bitmişse atla
            if (mission.currentProgress >= mission.targetAmount) continue;

            // Tür eşleşiyorsa veya görev "Herhangi Bir Soru" ise
            if (mission.type == targetType || mission.type == MissionType.SolveAny)
            {
                mission.currentProgress++; 
                gorevGuncellendi = true;
                
                if (mission.currentProgress >= mission.targetAmount)
                {
                    Debug.Log($"✅ GÖREV TAMAMLANDI: {mission.description}");
                    // Burada 'Görev Tamamlandı' sesi çalabilirsin
                }
            }
        }

        // Eğer ilerleme olduysa UI'yı güncelle
        if (gorevGuncellendi && OnMissionsUpdated != null)
        {
            OnMissionsUpdated.Invoke();
        }

        // Bölüm bitti mi diye kontrol et
        CheckLevelCompletion();
    }

    void CheckLevelCompletion()
    {
        if (isLevelFinished) return;

        bool allMainMissionsDone = true;

        foreach (MissionData mission in activeMissions)
        {
            // Sadece 'Ana Görevler' bitince bölüm biter
            if (mission.isMainMission && mission.currentProgress < mission.targetAmount)
            {
                allMainMissionsDone = false;
                break;
            }
        }

        if (allMainMissionsDone)
        {
            Debug.Log("🏆 BÖLÜM TAMAMLANDI!");
            LevelCompleted();
        }
    }

    // --- OYUN SONU DURUMLARI ---

    // KAZANMA
    void LevelCompleted()
    {
        isLevelFinished = true; // Oyunu durdur
        
        // Kayıt İşlemi (Bir sonraki bölümü açmak için)
        int savedLevel = PlayerPrefs.GetInt("CompletedLevelIndex", 0);
        
        // Eğer şu anki bölüm ID'si kayıtlı olandan büyük veya eşitse kaydet
        if (currentChapter != null && currentChapter.chapterID >= savedLevel)
        {
            PlayerPrefs.SetInt("CompletedLevelIndex", currentChapter.chapterID); 
            PlayerPrefs.Save();
        }

        // Kazanma Panelini Aç
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
    }

    // KAYBETME
    void LevelFailed()
    {
        isLevelFinished = true; // Oyunu durdur
        Debug.Log("💀 OYUN BAŞARISIZ! Puan bitti.");

        // Kaybetme Panelini Aç
        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
        }
    }

    // --- BUTON FONKSİYONLARI ---

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RetryLevel()
    {
        // Şu anki sahneyi baştan yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}