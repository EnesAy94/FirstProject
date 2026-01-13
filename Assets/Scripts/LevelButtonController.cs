using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelButtonItem : MonoBehaviour
{
    public Button myButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;

    // --- MEVCUT: BÖLÜMLER İÇİN ---
    public void Setup(ChapterData chapter, int highScore, System.Action onClickAction)
    {
        titleText.text = chapter.chapterName;

        if (highScore > 0)
        {
            scoreText.text = $"Bölüm Puanı: {highScore}";
            scoreText.gameObject.SetActive(true);
            scoreText.color = Color.yellow;
        }
        else
        {
            scoreText.text = $"Bölüm Puanı: 0"; 

        }

        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());
    }

    // --- YENİ: HİKAYELER İÇİN (Story Buttons) ---
    public void SetupStory(string storyTitle, int averageScore, System.Action onClickAction)
    {
        // 1. Başlık
        titleText.text = storyTitle;

        // 2. Ortalama Puan Gösterimi
        // Hiç puan yoksa bile "0" gözüksün istiyorsan direkt yazdırıyoruz.
        scoreText.text = $"Hikaye Puanı: {averageScore}";
        scoreText.gameObject.SetActive(true);
        
        // Renk ayrımı: Eğer puan 0 ise gri, yüksekse yeşil/sarı olsun
        if (averageScore > 0) scoreText.color = Color.green;
        else scoreText.color = Color.gray;

        // 3. Tıklama
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());
        
        // Buton her zaman tıklanabilir olsun (Hikaye seçimi kilitli değilse)
        myButton.interactable = true;
        GetComponent<Image>().color = Color.white; 
    }

    public void LockButton()
    {
        myButton.interactable = false;
        titleText.text += " (Kilitli)";
        scoreText.gameObject.SetActive(false);
        GetComponent<Image>().color = Color.gray;
    }
}