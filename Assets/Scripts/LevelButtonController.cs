using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelButtonItem : MonoBehaviour
{
    public Button myButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;

    // --- BÖLÜMLER İÇİN (CHAPTERS) ---
    // Not: highScore parametresi eski sistemden kalma olabilir, onu eziyoruz.
    public void Setup(ChapterData chapter, int highScore, System.Action onClickAction)
    {
        titleText.text = chapter.chapterName;

        // 1. KRİTİK HAMLE: Puanı dışarıdan bekleme, SaveManager'dan en tazesini çek!
        // (Eğer SaveManager henüz yoksa 0 kabul et)
        int realScore = 0;
        if (SaveManager.instance != null)
        {
            realScore = SaveManager.instance.GetLevelBestScore(chapter.chapterID);
        }

        // 2. GÖRSEL AYARLAMA
        if (realScore > 0)
        {
            scoreText.text = $"Puan: {realScore}";
            scoreText.gameObject.SetActive(true);
            scoreText.color = Color.yellow; // Puan varsa Sarı/Parlak
        }
        else
        {
            // Hiç oynanmamışsa
            scoreText.text = "Puan: -"; 
            scoreText.color = Color.gray;
        }

        // Kilit İkonu Mantığı (Opsiyonel):
        // Eğer önceki bölüm bitmemişse butonu kilitleyebilirsin.
        // Şimdilik sadece tıklama olayını bağlıyoruz.
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());
        
        // Butonu her ihtimale karşı aktif et (Kilitli değilse)
        myButton.interactable = true;
        GetComponent<Image>().color = Color.white;
    }

    // --- HİKAYELER İÇİN (STORY BUTTONS) ---
    // Not: Hikaye toplam puanını, bu butonu oluşturan scriptin (LevelSelectionManager) hesaplayıp göndermesi lazım.
    public void SetupStory(string storyTitle, int totalStoryScore, System.Action onClickAction)
    {
        // 1. Başlık
        titleText.text = storyTitle;

        // 2. Puan Gösterimi
        scoreText.gameObject.SetActive(true);
        scoreText.text = $"Toplam Puan: {totalStoryScore}";

        // 3. Renk Ayrımı
        if (totalStoryScore > 0)
        {
            scoreText.color = Color.green; // Puan varsa Yeşil
        }
        else
        {
            scoreText.color = Color.gray; // Yoksa Gri
        }

        // 4. Tıklama
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction.Invoke());
        
        // Görünüm sıfırla
        myButton.interactable = true;
        GetComponent<Image>().color = Color.white; 
    }

    public void LockButton()
    {
        myButton.interactable = false;
        titleText.text += " 🔒"; // Kilit ikonu ekledim
        scoreText.gameObject.SetActive(false);
        GetComponent<Image>().color = Color.gray; // Butonu gri yap
    }
}