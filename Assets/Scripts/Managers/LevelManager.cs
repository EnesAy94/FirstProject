using UnityEngine;
using System.Collections.Generic;
// 👇 BU SATIR EKSİK OLDUĞU İÇİN HATA VERİYORDU 👇
using UnityEngine.SceneManagement;
// 👆 SAHNE DEĞİŞTİRMEK İÇİN BU ŞART 👆

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public bool isLevelFinished = false;

    [Header("Mevcut Durum")]
    public ChapterData currentChapter;
    public List<MissionData> activeMissions;

    [Header("Level Sonu Ayarları")]
    public GameObject levelCompletePanel; // Panel
    public string mainMenuSceneName = "MainScene"; // Dönülecek Sahne Adı

    public System.Action OnMissionsUpdated;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        if (GameSession.activeChapter != null)
        {
            currentChapter = GameSession.activeChapter;
            StartChapter();
        }
        else
        {
            if (currentChapter != null) StartChapter();
        }
    }

    void StartChapter()
    {
        isLevelFinished = false;
        activeMissions = new List<MissionData>();
        foreach (MissionData mission in currentChapter.missions)
        {
            MissionData missionCopy = Instantiate(mission);
            activeMissions.Add(missionCopy);
        }
        if (OnMissionsUpdated != null) OnMissionsUpdated.Invoke();
    }

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
        }

        bool gorevGuncellendi = false;

        foreach (MissionData mission in activeMissions)
        {
            if (mission.currentProgress >= mission.targetAmount) continue;

            if (mission.type == targetType || mission.type == MissionType.SolveAny)
            {
                mission.currentProgress++;
                gorevGuncellendi = true;

                // Görev bittiyse log düşelim
                if (mission.currentProgress >= mission.targetAmount)
                {
                    Debug.Log($"✅ GÖREV TAMAMLANDI: {mission.description}");
                }
            }
        }

        if (gorevGuncellendi && OnMissionsUpdated != null)
        {
            OnMissionsUpdated.Invoke();
        }

        CheckLevelCompletion();
    }

    void CheckLevelCompletion()
    {
        bool allMainMissionsDone = true;

        foreach (MissionData mission in activeMissions)
        {
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

    void LevelCompleted()
    {
        isLevelFinished = true;
        // Kayıt İşlemi
        int savedLevel = PlayerPrefs.GetInt("CompletedLevelIndex", 0);
        if (currentChapter != null && currentChapter.chapterID >= savedLevel)
        {
            PlayerPrefs.SetInt("CompletedLevelIndex", currentChapter.chapterID);
            PlayerPrefs.Save();
        }

        // Paneli Aç
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
    }

    // ARTIK BU KOD HATA VERMEYECEK:
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}