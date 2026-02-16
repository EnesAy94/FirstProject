using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelButtonItem : MonoBehaviour
{
    public Button myButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;

    // --- BÖLÜMLER İÇİN ---
    public void Setup(ChapterData chapter, int highScore, System.Action onClickAction)
    {
        titleText.text = chapter.chapterName;

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

        myButton.interactable = true;
        GetComponent<Image>().color = Color.white;
    }

    // --- HİKAYELER İÇİN ---
    public void SetupStory(string storyTitle, int totalStoryScore, System.Action onClickAction)
    {
        titleText.text = storyTitle;
        scoreText.gameObject.SetActive(true);
        scoreText.text = $"Toplam Puan: {totalStoryScore}";

        if (totalStoryScore > 0) scoreText.color = Color.green;
        else scoreText.color = Color.gray;

        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());

        myButton.interactable = true;
        GetComponent<Image>().color = Color.white;
    }

    public void LockButton()
    {
        myButton.interactable = false;
        titleText.text += " 🔒";
        scoreText.gameObject.SetActive(false);
        GetComponent<Image>().color = Color.gray;
    }
}