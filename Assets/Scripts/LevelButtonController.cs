using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelButtonItem : MonoBehaviour
{
    public Button myButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;
    public Image iconImage; // YENİ: Butonun üzerindeki resim (Story veya Chapter görseli)
    public GameObject selectVisual; // YENİ: "SELECT" yazısı/resmi (kilitliyken veya yakında açılacakken gizlenecek)
    public TextMeshProUGUI btnSelectText; // YENİ: Select yazısı objesi ("PLAY" vs "SELECT" değiştirmek için)
    public TextMeshProUGUI scoreLabelText; // YENİ: "BÖLÜM PUANI" vs "DOSYA PUANI" yazacak statik metin

    // --- BÖLÜMLER İÇİN ---
    public void Setup(ChapterData chapter, int highScore, System.Action onClickAction)
    {
        string currentLang = LocalizationManager.instance != null ? LocalizationManager.instance.currentLanguage : "tr";
        
        if (currentLang.ToLower() == "en" && !string.IsNullOrEmpty(chapter.chapterNameEN))
        {
            titleText.text = chapter.chapterNameEN;
        }
        else
        {
            titleText.text = chapter.chapterName;
        }

        // Görsel Atama (Eğer ChapterData içine resim koyulmuşsa)
        if (iconImage != null)
        {
            if (chapter.chapterImage != null)
            {
                iconImage.sprite = chapter.chapterImage;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                // Resim yoksa kapat veya varsayılan bırak
                iconImage.gameObject.SetActive(false); 
            }
        }

        int realScore = 0;
        if (SaveManager.instance != null)
        {
            realScore = SaveManager.instance.GetLevelBestScore(chapter.chapterID);

            // --- DEBUG KODU (Bunu konsoldan takip et) ---
            Debug.Log($"Bölüm: {chapter.chapterName} (ID: {chapter.chapterID}) | Kayıtlı Puan: {realScore}");
        }
        else
        {
            Debug.LogError("HATA: SaveManager bulunamadı!");
        }

        // GÖRSEL AYARLAMA
        if (realScore > 0)
        {
            scoreText.gameObject.SetActive(true); // Görünür yap
            scoreText.color = Color.yellow;
            scoreText.text = realScore.ToString(); // Yalnızca sayıyı yazdırıyoruz! ("Puan:" yazısı ayrı Text olacak).
        }
        else
        {
            scoreText.text = "-";
            scoreText.color = Color.gray;
        }

        // Tıklama olayları
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());

        if (selectVisual != null) selectVisual.SetActive(true);
        myButton.interactable = true;
        
        // Bölümlerde Text "OYNA / PLAY" yazmalı
        if (btnSelectText != null) 
        {
            string locPlay = LocalizationManager.instance != null ? LocalizationManager.instance.GetText("chapter_card_play") : "OYNA";
            btnSelectText.text = locPlay;
        }

        // Puan etiketi "BÖLÜM PUANI / CHAPTER SCORE"
        if (scoreLabelText != null)
        {
            scoreLabelText.gameObject.SetActive(true);
            string locLabel = LocalizationManager.instance != null ? LocalizationManager.instance.GetText("chapter_card_score") : "BÖLÜM PUANI";
            scoreLabelText.text = locLabel;
        }
        
        GetComponent<Image>().color = Color.white;
    }

    // --- HİKAYELER İÇİN ---
    public void SetupStory(StoryData story, int totalStoryScore, System.Action onClickAction)
    {
        string currentLang = LocalizationManager.instance != null ? LocalizationManager.instance.currentLanguage : "tr";
        
        if (currentLang.ToLower() == "en" && !string.IsNullOrEmpty(story.storyTitleEN))
        {
            titleText.text = story.storyTitleEN;
        }
        else
        {
            titleText.text = story.storyTitle;
        }
        scoreText.gameObject.SetActive(true);
        
        // Puanı çıplak sayı olarak yazdırıyoruz.
        scoreText.text = totalStoryScore.ToString();

        if (iconImage != null)
        {
            if (story.storyImage != null)
            {
                iconImage.sprite = story.storyImage;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false); 
            }
        }

        if (totalStoryScore > 0) scoreText.color = Color.green;
        else scoreText.color = Color.gray;

        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());

        if (selectVisual != null) selectVisual.SetActive(true);
        myButton.interactable = true;

        // Hikayelerde Text "SEÇ / SELECT" yazmalı
        if (btnSelectText != null) 
        {
            string locSelect = LocalizationManager.instance != null ? LocalizationManager.instance.GetText("chapter_card_select") : "SEÇ";
            btnSelectText.text = locSelect;
        }

        // Puan etiketi "DOSYA PUANI / FILE SCORE"
        if (scoreLabelText != null)
        {
            scoreLabelText.gameObject.SetActive(true);
            string locLabel = LocalizationManager.instance != null ? LocalizationManager.instance.GetText("story_card_score") : "DOSYA PUANI";
            scoreLabelText.text = locLabel;
        }

        GetComponent<Image>().color = Color.white;
    }

    public void LockButton(Sprite lockSprite = null)
    {
        if (selectVisual != null) selectVisual.SetActive(false);
        myButton.interactable = false;
        scoreText.gameObject.SetActive(false);
        if (scoreLabelText != null) scoreLabelText.gameObject.SetActive(false);
        GetComponent<Image>().color = Color.gray;

        if (iconImage != null && lockSprite != null)
        {
            iconImage.sprite = lockSprite;
            iconImage.gameObject.SetActive(true);
        }
    }

    public void SetComingSoon()
    {
        if (selectVisual != null) selectVisual.SetActive(false);
        myButton.interactable = false;
        scoreText.gameObject.SetActive(false);
        if (scoreLabelText != null) scoreLabelText.gameObject.SetActive(false);
        GetComponent<Image>().color = Color.gray;
    }
}