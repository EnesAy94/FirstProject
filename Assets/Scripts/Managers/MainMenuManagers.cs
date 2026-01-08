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
    private GameObject currentActivePanel;

    void Start()
    {
        // Başlangıçta Ana Menüyü aç
        OpenPanel(rootPanel);
    }

    // --- PANEL YÖNETİMİ ---
    void OpenPanel(GameObject panelToOpen)
    {
        // Tüm panelleri kapat
        rootPanel.SetActive(false);
        storySelectPanel.SetActive(false);
        chapterSelectPanel.SetActive(false);

        // İsteneni aç
        panelToOpen.SetActive(true);
        currentActivePanel = panelToOpen;
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
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = story.storyTitle; // Örn: "Matematik Dedektifi"

            Button btn = btnObj.GetComponent<Button>();

            // Butona basınca o hikayenin bölümlerini açsın
            btn.onClick.AddListener(() => OpenChapterSelection(story));
        }
    }

    // --- 3. ADIM: BÖLÜM SEÇİMİ ---
    void OpenChapterSelection(StoryData selectedStory)
    {
        OpenPanel(chapterSelectPanel);
        ClearContainer(chapterListContainer);

        // Başlığa hangi hikayede olduğumuzu yazdırabiliriz (Opsiyonel)
        // Debug.Log("Seçilen Hikaye: " + selectedStory.storyTitle);

        // Kayıtlı ilerlemeyi çek
        int unlockedLevelIndex = PlayerPrefs.GetInt("CompletedLevelIndex", 0);

        for (int i = 0; i < selectedStory.chapters.Count; i++)
        {
            ChapterData chapter = selectedStory.chapters[i];
            GameObject btnObj = Instantiate(menuButtonPrefab, chapterListContainer);

            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = chapter.chapterName;

            Button btn = btnObj.GetComponent<Button>();

            // Kilit Sistemi (Basit Hali - Sadece ID'ye bakar)
            // Eğer her hikayenin kilidi ayrı olsun istersen PlayerPrefs ismini özelleştirmemiz gerekir.
            // Şimdilik genel ilerleme kullanıyoruz.
            if (chapter.chapterID <= unlockedLevelIndex + 1) // +1 tolerans veya mantığına göre düzenle
            {
                btn.interactable = true;
                btn.onClick.AddListener(() => StartLevel(chapter));
            }
            else
            {
                btn.interactable = false;
                txt.text += " (Kilitli)";
                btnObj.GetComponent<Image>().color = Color.gray;
            }
        }
    }

    // --- OYUNU BAŞLATMA ---
    void StartLevel(ChapterData chapter)
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