using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Veriler")]
    public List<StoryData> allStories; // Oyundaki tüm hikayeleri buraya atacağız (Story 1, Story 2...)
    public string gameSceneName = "GameScene";

    [Header("Paneller")]
    public GameObject rootPanel;         // En baştaki ana menü (Story, Multi, Settings)
    public GameObject storySelectPanel;  // Hikaye seçme ekranı
    public GameObject chapterSelectPanel;// Bölüm seçme ekranı

    [Header("Container & Prefabs")]
    public Transform storyListContainer;   // Hikaye butonlarının dizileceği yer
    public Transform chapterListContainer; // Bölüm butonlarının dizileceği yer
    public GameObject menuButtonPrefab;    // Standart buton tasarımı

    // Geri dönünce hangi paneli açacağını bilmek için
    public GameObject replayConfirmPanel;
    private ChapterData selectedChapterToReplay; // Hangi bölümü tekrar oynayacağız?

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

    // --- 1. ADIM: ANA MENÜ BUTONLARI ---
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
        Debug.Log("Profil sayfası henüz yapım aşamasında...");
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

    // --- 2. ADIM: HİKAYE SEÇİMİ ---
    void OpenStorySelection()
    {
        OpenPanel(storySelectPanel);
        ClearContainer(storyListContainer);

        foreach (StoryData story in allStories)
        {
            GameObject btnObj = Instantiate(menuButtonPrefab, storyListContainer);
            btnObj.transform.localScale = Vector3.one;

            // Scripti al
            LevelButtonItem buttonScript = btnObj.GetComponent<LevelButtonItem>();

            if (buttonScript != null)
            {
                // --- ORTALAMA PUAN HESAPLAMA ---
                float totalScore = 0;
                int totalChapters = story.chapters.Count; // Örn: 10 bölüm

                foreach (ChapterData chapter in story.chapters)
                {
                    // Her bölümün rekorunu çek, yoksa 0 gelir
                    int chapterScore = PlayerPrefs.GetInt($"HighScore_{chapter.chapterID}", 0);
                    totalScore += chapterScore;
                }

                // Ortalama Hesapla (Bölüm sayısı 0 değilse)
                int averageScore = 0;
                if (totalChapters > 0)
                {
                    // (int) diyerek tam sayıya yuvarlıyoruz (Örn: 9.5 -> 9)
                    // Mathf.RoundToInt kullanırsan 9.5 -> 10 olur. Tercih senin.
                    averageScore = Mathf.RoundToInt(totalScore / totalChapters);
                }
                // ---------------------------------

                // Butonu Kur (Yeni fonksiyonu kullanıyoruz)
                buttonScript.SetupStory(
                    story.storyTitle,
                    averageScore,
                    () => OpenChapterSelection(story)
                );
            }
        }
    }

    // --- 3. ADIM: BÖLÜM SEÇİMİ ---
    void OpenChapterSelection(StoryData selectedStory)
    {
        OpenPanel(chapterSelectPanel);
        ClearContainer(chapterListContainer);

        int unlockedLevelIndex = PlayerPrefs.GetInt("CompletedLevelIndex", 0);

        for (int i = 0; i < selectedStory.chapters.Count; i++)
        {
            ChapterData chapter = selectedStory.chapters[i];

            // Prefab'ı oluştur
            GameObject btnObj = Instantiate(menuButtonPrefab, chapterListContainer);
            btnObj.transform.localScale = Vector3.one;

            // 🔥 PROFESYONEL DOKUNUŞ BURADA 🔥
            // Objeyi aramak yerine direkt scriptine ulaşıyoruz.
            LevelButtonItem buttonScript = btnObj.GetComponent<LevelButtonItem>();

            if (buttonScript != null)
            {
                // Kilit kontrolü
                if (chapter.chapterID <= unlockedLevelIndex + 1)
                {
                    // Açık Bölüm
                    int highScore = PlayerPrefs.GetInt($"HighScore_{chapter.chapterID}", 0);
                    bool isCompleted = chapter.chapterID <= unlockedLevelIndex;

                    // Tıklanınca ne yapacağını belirliyoruz
                    System.Action clickAction = () =>
                    {
                        if (isCompleted) AskToReplay(chapter);
                        else StartLevelDirectly(chapter);
                    };

                    // Scriptin içindeki Setup fonksiyonunu çağırıyoruz
                    buttonScript.Setup(chapter, highScore, clickAction);
                }
                else
                {
                    // Kilitli Bölüm
                    buttonScript.Setup(chapter, 0, null); // Önce ismini yazsın
                    buttonScript.LockButton(); // Sonra kilitlesin
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
            // Panelin içindeki metni güncelleyebilirsin: "Bölüm 1'i tekrar oynamak istiyor musun?"
        }
        else
        {
            // Panel yoksa direkt başlat (Hata vermesin)
            StartLevelDirectly(chapter);
        }
    }

    // Paneldeki "EVET" butonu buna bağlanacak
    public void OnConfirmReplay()
    {
        if (selectedChapterToReplay != null)
        {
            StartLevelDirectly(selectedChapterToReplay);
        }
    }

    // Paneldeki "HAYIR" butonu buna bağlanacak
    public void OnCancelReplay()
    {
        if (replayConfirmPanel != null) replayConfirmPanel.SetActive(false);
    }

    // --- OYUNU BAŞLATMA ---
    void StartLevelDirectly(ChapterData chapter)
    {
        GameSession.activeChapter = chapter;
        SceneManager.LoadScene(gameSceneName);
    }

    // --- YARDIMCI: GERİ DÖN BUTONLARI ---
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
        PlayerPrefs.DeleteAll();
        Debug.Log("💥 TÜM İLERLEME SİLİNDİ! Oyun sıfırlandı.");
    }
}