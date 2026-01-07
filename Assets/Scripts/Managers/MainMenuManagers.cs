using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Veriler")]
    public StoryData storyData; // Oluşturduğun Hikaye dosyası buraya
    public string gameSceneName = "GameScene"; // Oyun sahnenin adı neyse buraya yaz

    [Header("UI Elemanları")]
    public Transform buttonContainer; // Butonların dizileceği kutu (Content)
    public GameObject chapterButtonPrefab; // Çoğaltacağımız buton örneği

    void Start()
    {
        CreateChapterButtons();
    }

    void CreateChapterButtons()
    {
        // Kaydedilmiş en son biten bölümü çek (Hiç oynamadıysa 0 gelir)
        // Örn: 1. Bölümü bitirdiyse 'unlockedLevel' 1 olur, yani index 1 (Bölüm 2) açılır.
        int unlockedLevelIndex = PlayerPrefs.GetInt("CompletedLevelIndex", 0);

        // Her bir bölüm için döngü kur
        for (int i = 0; i < storyData.chapters.Count; i++)
        {
            ChapterData chapter = storyData.chapters[i];
            
            // Butonu oluştur
            GameObject btnObj = Instantiate(chapterButtonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Butonun adını ayarla
            btnText.text = chapter.chapterName;

            // Kilit Kontrolü
            if (i <= unlockedLevelIndex)
            {
                // KİLİT AÇIK: Tıklanabilir
                btn.interactable = true;
                
                // Tıklanınca ne olsun?
                btn.onClick.AddListener(() => OnChapterClicked(chapter));
            }
            else
            {
                // KİLİTLİ: Tıklanamaz ve rengi soluk
                btn.interactable = false;
                btnText.text += " (Kilitli)";
                btnObj.GetComponent<Image>().color = Color.gray;
            }
        }
    }

    void OnChapterClicked(ChapterData chapter)
    {
        // 1. Seçilen bölümü köprüye (Static scripte) kaydet
        GameSession.activeChapter = chapter;

        // 2. Oyun sahnesini yükle
        SceneManager.LoadScene(gameSceneName);
    }
}