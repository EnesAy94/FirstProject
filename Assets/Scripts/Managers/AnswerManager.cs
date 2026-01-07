using UnityEngine;
using TMPro;

public class AnswerManager : MonoBehaviour
{
    public static AnswerManager instance; // Her yerden ulaşabilmek için

    [Header("UI Elemanları")]
    public GameObject answerPanel;      // Panelin kendisi
    public TextMeshProUGUI infoText;    // Üstteki bilgi yazısı
    public TMP_InputField answerInput;  // Cevap kutusu
    public Whiteboard whiteboard;       // Çizim tahtası (Temizlemek için)

    public GameObject questionPanel;
    public PlayerMovement player;
    private int currentCorrectAnswer;   // Doğru cevap hafızada burada tutulacak

    void Awake()
    {
        instance = this;
        answerPanel.SetActive(false); // Başlangıçta gizle
    }

    // QuestionManager burayı çağıracak
    // Parametreleri güncelledik
    public void PaneliAc(string esyaAdi, string miktarStr, int adet, int dogruCevap)
    {
        answerPanel.SetActive(true);
        currentCorrectAnswer = dogruCevap;

        answerInput.text = "";
        whiteboard.ClearBoard();

        string detayMetni = "";

        // Eğer adet 1'den büyükse (Demek ki Sarı soru)
        if (adet > 1)
        {
            detayMetni = $"SEÇİLEN: {esyaAdi}  FİYAT: {miktarStr} TL  ADET: {adet}";
        }
        else // Kırmızı ve Mavi
        {
            detayMetni = $"SEÇİLEN: {esyaAdi}  SAYI: {miktarStr}";
        }

        infoText.text = detayMetni + "\nİşlemi yap ve cevabı yaz.";
    }

    // "Kontrol Et" butonuna basınca bu çalışacak
    public void CevabiKontrolEt()
    {
        if (string.IsNullOrEmpty(answerInput.text)) return;

        int oyuncuCevabi = int.Parse(answerInput.text);

        if (oyuncuCevabi == currentCorrectAnswer)
        {
            Debug.Log("✅ DOĞRU BİLDİNİZ! +2 ADIM İLERLE");

            // ÖDÜL: Piyonu 2 adım ileri at (Birazdan Player koduna bunu ekleyeceğiz)
            player.BonusMove(2);
        }
        else
        {
            Debug.Log("❌ YANLIŞ CEVAP!");

            // CEZA: Piyonu 1 adım geri al
            player.BonusMove(-1);
        }

        // --- TEMİZLİK ZAMANI ---
        // 1. Cevap panelini kapat
        answerPanel.SetActive(false);
        // 2. Arkadaki soru panelini kapat (QuestionManager'ın paneli)
        questionPanel.SetActive(false);

        GameManager.instance.SwitchTurn();
    }
}