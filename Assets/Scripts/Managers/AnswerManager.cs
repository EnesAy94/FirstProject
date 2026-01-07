using UnityEngine;
using TMPro;

public class AnswerManager : MonoBehaviour
{
    public static AnswerManager instance;

    [Header("UI Elemanları")]
    public GameObject answerPanel;
    public TextMeshProUGUI infoText;
    public TMP_InputField answerInput;
    public Whiteboard whiteboard;

    // Soru panelini kapatmak için referans (QuestionManager'ın içindeki panel)
    // Eğer QuestionManager'da panel public ise direkt oradan kapatabiliriz ama şimdilik kalsın.
    public GameObject questionPanel;

    private int currentCorrectAnswer;
    private TileType currentQuestionType; // Hangi renk soruyu çözüyoruz? (Görev için lazım)

    void Awake()
    {
        instance = this;
        answerPanel.SetActive(false);
    }

    // QuestionManager burayı çağıracak
    // DİKKAT: Buraya 'TileType' parametresi ekledik!
    // Parametreye 'string soruKurali' ekledik
    public void PaneliAc(string baslik, string esyaDetayi, string soruKurali, int adet, int dogruCevap, TileType type)
    {
        answerPanel.SetActive(true);

        currentCorrectAnswer = dogruCevap;
        currentQuestionType = type;

        answerInput.text = "";
        if (whiteboard != null) whiteboard.ClearBoard();

        // ARTIK KURALI DA YAZDIRIYORUZ:
        // Örn: SEÇİLEN: Gramofon
        //      SORU: Miktar Çift olduğu için (Ünlü - Ünsüz) işlemini yap.
        infoText.text = $"{baslik}\n\n{esyaDetayi}\n\n {soruKurali}";
    }

    public void CevabiKontrolEt()
    {
        if (string.IsNullOrEmpty(answerInput.text)) return;

        int oyuncuCevabi;
        bool isNumeric = int.TryParse(answerInput.text, out oyuncuCevabi);

        if (!isNumeric) return; // Sayı girilmemişse işlem yapma

        if (oyuncuCevabi == currentCorrectAnswer)
        {
            Debug.Log("✅ DOĞRU CEVAP!");

            // 1. Oyuncuyu Ödüllendir (Bonus Hareket)
            if (GameManager.instance != null && GameManager.instance.player != null)
            {
                GameManager.instance.player.BonusMove(2);
            }

            // 2. GÖREV SİSTEMİNE HABER VER (4. Adımda burayı LevelManager'a bağlayacağız)
            // Örn: LevelManager.instance.GorevIlerlet(currentQuestionType);
            Debug.Log($"📜 Görev Tetiklendi: {currentQuestionType} sorusu çözüldü.");
        }
        else
        {
            Debug.Log("❌ YANLIŞ CEVAP!");

            // Ceza: Geri git
            if (GameManager.instance != null && GameManager.instance.player != null)
            {
                GameManager.instance.player.BonusMove(-1);
            }
        }

        // --- KAPANIŞ ---
        answerPanel.SetActive(false);
        if (questionPanel != null) questionPanel.SetActive(false);

        // 'SwitchTurn' kalktı. Oyun akışına devam ediyor.
        // Eğer zar sistemi varsa burada "Zar Butonunu Aç" diyebiliriz ileride.
    }
}