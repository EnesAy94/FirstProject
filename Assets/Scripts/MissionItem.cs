using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionItem : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public TextMeshProUGUI descriptionText; // "3 Kırmızı Soru Çöz"
    public TextMeshProUGUI progressText;    // "0/3"
    public GameObject checkmarkObject;      // Tik işareti (Image objesi)

    private MissionData myMission;

    // Manager bu fonksiyonu çağırıp veriyi teslim edecek
    public void Setup(MissionData mission)
    {
        myMission = mission;
        RefreshUI();
    }

    // UI'yı veriye göre güncelle
    public void RefreshUI()
    {
        if (myMission == null) return;

        // 1. Açıklama Yazısı
        descriptionText.text = myMission.description;

        // 2. İlerleme Yazısı (Örn: 1/3)
        progressText.text = $"{myMission.currentProgress}/{myMission.targetAmount}";

        // 3. Görev Bitti mi? (Tik Kontrolü)
        bool isComplete = myMission.currentProgress >= myMission.targetAmount;

        if (isComplete)
        {
            checkmarkObject.SetActive(true); // Tik aç
            progressText.color = Color.green; // Yazıyı yeşil yap (Opsiyonel)
        }
        else
        {
            checkmarkObject.SetActive(false); // Tik gizli
            progressText.color = Color.white; // Yazı normal
        }
    }
}