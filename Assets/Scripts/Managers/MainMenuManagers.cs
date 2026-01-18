using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Veriler")]
    public List<StoryData> allStories;
    public string gameSceneName = "GameScene";

    [Header("Paneller")]
    public GameObject rootPanel;
    public GameObject storySelectPanel;
    public GameObject chapterSelectPanel;
    public GameObject profilePanel;

    [Header("Container & Prefabs")]
    public Transform storyListContainer;
    public Transform chapterListContainer;
    public GameObject menuButtonPrefab;

    public GameObject replayConfirmPanel;
    private ChapterData selectedChapterToReplay;

    void Start()
    {
        OpenPanel(rootPanel);
        if (replayConfirmPanel != null) replayConfirmPanel.SetActive(false);
    }

    // --- PANEL YÖNETİMİ ---
    void OpenPanel(GameObject panelToOpen)
    {
        rootPanel.SetActive(false);
        storySelectPanel.SetActive(false);
        chapterSelectPanel.SetActive(false);
        if (replayConfirmPanel != null) replayConfirmPanel.SetActive(false);

        panelToOpen.SetActive(true);
    }

    public void OnClick_StoryMode()
    {
        OpenStorySelection();
    }

    public void OnClick_Multiplayer()
    {
        Debug.Log("Çok Oyunculu modu henüz yapım aşamasında...");
    }

    public void OnClick_Profile()
    {
        if (rootPanel != null) rootPanel.SetActive(false);

        if (profilePanel != null)
        {
            profilePanel.SetActive(true);
            // ProfileUI OnEnable ile verileri otomatik çekecek
        }
    }

    public void OnClick_CloseProfile()
    {
        if (profilePanel != null) profilePanel.SetActive(false);
        if (rootPanel != null) rootPanel.SetActive(true);
    }

    public void OnClick_Settings()
    {
        Debug.Log("Ayarlar sayfası henüz yapım aşamasında...");
    }

    public void OnClick_Quit()
    {
        Application.Quit();
        Debug.Log("Oyundan Çıkıldı.");
    }

    // --- 2. ADIM: HİKAYE SEÇİMİ (GÜNCELLENDİ) ---
    void OpenStorySelection()
    {
        OpenPanel(storySelectPanel);
        ClearContainer(storyListContainer);

        foreach (StoryData story in allStories)
        {
            GameObject btnObj = Instantiate(menuButtonPrefab, storyListContainer);
            btnObj.transform.localScale = Vector3.one;

            LevelButtonItem buttonScript = btnObj.GetComponent<LevelButtonItem>();

            if (buttonScript != null)
            {
                // --- DEĞİŞİKLİK BURADA ---
                // Eskiden burada 10 satır kodla ortalama hesaplıyorduk.
                // Şimdi tek satırda SaveManager'a soruyoruz:
                int averageScore = 0;

                if (SaveManager.instance != null)
                {
                    averageScore = SaveManager.instance.GetStoryAverageScore(story.chapters);
                }

                // Butonu Kur
                buttonScript.SetupStory(
                    story.storyTitle,
                    averageScore,
                    () => OpenChapterSelection(story)
                );
            }
        }
    }

    // --- 3. ADIM: BÖLÜM SEÇİMİ (GÜNCELLENDİ) ---
    // --- 3. ADIM: BÖLÜM SEÇİMİ (SaveManager ile Tam Uyumlu) ---
    void OpenChapterSelection(StoryData selectedStory)
    {
        OpenPanel(chapterSelectPanel);
        ClearContainer(chapterListContainer);

        // YENİ KOD: SaveManager'a soruyoruz "En son hangi seviye açık?"
        int maxUnlockedLevel = 1;
        if (SaveManager.instance != null)
        {
            maxUnlockedLevel = SaveManager.instance.GetLastUnlockedLevel();
        }

        for (int i = 0; i < selectedStory.chapters.Count; i++)
        {
            ChapterData chapter = selectedStory.chapters[i];

            GameObject btnObj = Instantiate(menuButtonPrefab, chapterListContainer);
            btnObj.transform.localScale = Vector3.one;

            LevelButtonItem buttonScript = btnObj.GetComponent<LevelButtonItem>();

            if (buttonScript != null)
            {
                // KİLİT MANTIĞI:
                // Bölüm ID'si, açık olan son seviyeden küçük veya eşitse o bölüm açıktır.
                // Örn: Son açık 2 ise -> 1. Bölüm (Açık), 2. Bölüm (Açık), 3. Bölüm (Kilitli)
                if (chapter.chapterID <= maxUnlockedLevel)
                {
                    // PUAN ÇEKME
                    int highScore = 0;
                    if (SaveManager.instance != null)
                    {
                        highScore = SaveManager.instance.GetLevelBestScore(chapter.chapterID);
                    }

                    // TAMAMLANDI MI?
                    // Eğer bölümün ana görevleri bitmişse "Tamamlandı" sayılır.
                    // (Replay sormak için bunu kullanıyoruz)
                    bool isCompleted = false;
                    if (SaveManager.instance != null)
                    {
                        isCompleted = SaveManager.instance.IsMainMissionDone(chapter.chapterID);
                    }

                    // TIKLAMA OLAYI
                    System.Action clickAction = () =>
                    {
                        if (isCompleted) AskToReplay(chapter);
                        else StartLevelDirectly(chapter);
                    };

                    buttonScript.Setup(chapter, highScore, clickAction);
                }
                else
                {
                    // KİLİTLİ BÖLÜM
                    buttonScript.Setup(chapter, 0, null);
                    buttonScript.LockButton();
                }
            }
        }
    }

    // --- REPLAY SİSTEMİ ---
    void AskToReplay(ChapterData chapter)
    {
        selectedChapterToReplay = chapter;
        if (replayConfirmPanel != null)
        {
            replayConfirmPanel.SetActive(true);
        }
        else
        {
            StartLevelDirectly(chapter);
        }
    }

    public void OnConfirmReplay()
    {
        if (selectedChapterToReplay != null)
        {
            StartLevelDirectly(selectedChapterToReplay);
        }
    }

    public void OnCancelReplay()
    {
        if (replayConfirmPanel != null) replayConfirmPanel.SetActive(false);
    }

    void StartLevelDirectly(ChapterData chapter)
    {
        GameSession.activeChapter = chapter;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClick_BackToRoot()
    {
        OpenPanel(rootPanel);
    }

    public void OnClick_BackToStories()
    {
        OpenStorySelection();
    }

    void ClearContainer(Transform container)
    {
        foreach (Transform child in container) Destroy(child.gameObject);
    }
    [ContextMenu("Tüm Kayıtları Sil")]
    public void DeleteAllSaveData()
    {
        // Şimdi: SaveManager'a emrediyoruz
        if (SaveManager.instance != null)
        {
            SaveManager.instance.ResetAllData();
        }
        else
        {
            // Eğer sahnede SaveManager yoksa (nadir durum) manuel sil
            PlayerPrefs.DeleteAll();
            Debug.Log("SaveManager bulunamadı, sadece PlayerPrefs silindi.");
        }
    }
}