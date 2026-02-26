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

    // --- BÖLÜMLER İÇİN ---
    public void Setup(ChapterData chapter, int highScore, System.Action onClickAction)
    {
        titleText.text = chapter.chapterName;

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
            scoreText.text = $"Puan: {realScore}";
            scoreText.gameObject.SetActive(true); // Görünür yap
            scoreText.color = Color.yellow;
        }
        else
        {
            scoreText.text = "Puan: -";
            scoreText.color = Color.gray;
        }

        // Tıklama olayları
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());

        if (selectVisual != null) selectVisual.SetActive(true);
        myButton.interactable = true;
        GetComponent<Image>().color = Color.white;
    }

    // --- HİKAYELER İÇİN ---
    public void SetupStory(string storyTitle, int totalStoryScore, Sprite storySprite, System.Action onClickAction)
    {
        titleText.text = storyTitle;
        scoreText.gameObject.SetActive(true);
        scoreText.text = $"Toplam Puan: {totalStoryScore}";

        if (iconImage != null)
        {
            if (storySprite != null)
            {
                iconImage.sprite = storySprite;
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
        GetComponent<Image>().color = Color.white;
    }

    public void LockButton(Sprite lockSprite = null)
    {
        if (selectVisual != null) selectVisual.SetActive(false);
        myButton.interactable = false;
        scoreText.gameObject.SetActive(false);
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
        GetComponent<Image>().color = Color.gray;
    }
}