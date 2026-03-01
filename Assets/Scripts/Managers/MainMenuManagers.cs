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
    public GameObject settingsPanel;

    [Header("Container & Prefabs")]
    public Transform storyListContainer;
    public Transform chapterListContainer;
    public GameObject menuButtonPrefab;
    public Sprite lockedLevelSprite; // YENİ: Kilitli bölümlerin ikon resmi

    [Header("Para Birimi UI")]
    public TextMeshProUGUI ticketCountText;

    public GameObject replayConfirmPanel;
    private ChapterData selectedChapterToReplay;
    private GameObject currentActivePanel;

    void Start()
    {
        OpenPanel(rootPanel);
        if (replayConfirmPanel != null) replayConfirmPanel.SetActive(false);
        UpdateTicketUI();
    }

    // --- PANEL YÖNETİMİ ---
    public enum TransitionType { Normal, Skip, Glitch }

    void OpenPanel(GameObject panelToOpen, TransitionType transition = TransitionType.Normal)
    {
        if (currentActivePanel == panelToOpen) return;

        if (currentActivePanel != null)
        {
            var animator = currentActivePanel.GetComponent<PanelCRTAnimator>();
            
            if (transition == TransitionType.Glitch)
            {
                // Glitch efekti: Eski panel kapatılmaz, kapanış animasyonu da girmez. 
                // Yeni panele "Glitch" efekti ile geçildiğini varsayıyoruz. 
                // Aslında Story -> Chapter durumunda ikisi de aynı yerde durduğu için
                // eskiyi anında kapatıp yeniyi Glitch ile açmak en iyisi.
                if (animator != null) animator.KillTweens();
                currentActivePanel.SetActive(false);
            }
            else if (animator != null && transition != TransitionType.Skip)
            {
                // Normal kapanış
                animator.ClosePanel();
            }
            else
            {
                // Skip veya Animator yok
                if (animator != null) animator.KillTweens(); 
                currentActivePanel.SetActive(false);
            }
        }
        else
        {
            // İlk açılış
            if (rootPanel != null) rootPanel.SetActive(false);
            if (storySelectPanel != null) storySelectPanel.SetActive(false);
            if (chapterSelectPanel != null) chapterSelectPanel.SetActive(false);
            if (profilePanel != null) profilePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (replayConfirmPanel != null) replayConfirmPanel.SetActive(false);
        }

        currentActivePanel = panelToOpen;
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
            var newAnimator = panelToOpen.GetComponent<PanelCRTAnimator>();
            
            if (newAnimator != null)
            {
                if (transition == TransitionType.Skip)
                {
                    newAnimator.SetOpenedInstantly();
                }
                else if (transition == TransitionType.Glitch)
                {
                    newAnimator.PlayGlitchTransition();
                }
                else
                {
                    newAnimator.PlayOpenAnimation(); // Normal Açılış
                }
            }
        }
    }

    public void OnClick_StoryMode()
    {
        OpenStorySelection();
    }

    public void OnClick_Multiplayer()
    {
        if (PopupManager.instance != null)
        {
            PopupManager.instance.ShowPopup("popup_coming_soon");
        }
    }

    public void UpdateTicketUI()
    {
        if (ticketCountText != null && SaveManager.instance != null)
        {
            // SaveManager'dan sayıyı çek
            int amount = SaveManager.instance.GetTicketCount();

            // Ekrana yaz (Örn: "5" veya "x5" şeklinde)
            ticketCountText.text = amount.ToString();
        }
    }

    public void OnClick_Profile()
    {
        if (profilePanel != null) OpenPanel(profilePanel);
    }

    public void OnClick_CloseProfile()
    {
        if (rootPanel != null) OpenPanel(rootPanel);
    }

    public void OnClick_Settings()
    {
        if (settingsPanel != null) OpenPanel(settingsPanel);
    }

    public void OnClick_CloseSettings()
    {
        if (rootPanel != null) OpenPanel(rootPanel);
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
                    story,
                    averageScore,
                    () => OpenChapterSelection(story)
                );

                if (story.isComingSoon)
                {
                    buttonScript.SetComingSoon();
                }
            }
        }

        // HİKAYELER YÜKLENDİ - SWIPE AYARLARINI HESAPLA!
        UI.StorySwipeController storySwipe = storySelectPanel.GetComponentInChildren<UI.StorySwipeController>(true);
        if (storySwipe != null)
        {
            storySwipe.Initialize();
        }
    }

    // --- 3. ADIM: BÖLÜM SEÇİMİ (GÜNCELLENDİ) ---
    // --- 3. ADIM: BÖLÜM SEÇİMİ (SaveManager ile Tam Uyumlu) ---
    void OpenChapterSelection(StoryData selectedStory)
    {
        // Hikayeden bölüme geçerken Glitch animasyonu kullan
        OpenPanel(chapterSelectPanel, TransitionType.Glitch);
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
                    buttonScript.LockButton(lockedLevelSprite);
                }
            }
        }

        // TIKLANABİLİR BÖLÜMLER YÜKLENDİ - SWIPE AYARLARINI HESAPLA!
        UI.StorySwipeController chapterSwipe = chapterSelectPanel.GetComponentInChildren<UI.StorySwipeController>(true);
        if (chapterSwipe != null)
        {
            chapterSwipe.Initialize();
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
        // Chapter'dan Story'e geri dönerken de Glitch efekti kullan
        OpenPanel(storySelectPanel, TransitionType.Glitch);
    }

    void ClearContainer(Transform container)
    {
        // Unity'de foreach içindeyken SetParent veya Destroy(Immediate) yaparsan
        // liste o an çöker ve bazı elemanlar atlanır. 
        // Kesin çözüm: Listeyi tersten dönüp temizlemektir.
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            child.SetParent(null); 
            Destroy(child.gameObject);
        }
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