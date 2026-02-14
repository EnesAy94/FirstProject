using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnswerManager : MonoBehaviour
{
    public static AnswerManager instance;

    [System.Serializable]
    public struct AchievementLink
    {
        public TileType type;
        public string achievementID;
    }

    [Header("Başarım Ayarları")]
    public List<AchievementLink> achievementLinks;

    [Header("UI Elemanları")]
    public GameObject answerPanel;
    public TextMeshProUGUI infoText; // Soru metni buraya yazılacak
    public TMP_InputField answerInput;
    public Whiteboard whiteboard;
    public GameObject questionPanel; // Eski panel varsa kapatacağız (Artık tek panel kullanıyoruz)
    public GameObject retryButton;   // Joker Butonu

    [Header("Sonuç / Feedback UI")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackTitleText;
    public TextMeshProUGUI feedbackDescText;
    public Button feedbackContinueButton;

    [Header("Soru Durumu (QuestionManager tarafından yönetilir)")]
    public bool isStoryPhase = false;       // Şu an hikayeli (ilk 5) soruda mıyız?
    public bool isFinalStoryQuestion = false; // Bu, o mekanın 5. ve son sorusu mu?

    [Header("Özel Mesaj Sistemi")]
    public string currentSuccessMsg = ""; // Mekandan gelen özel doğru mesajı
    public string currentFailMsg = "";    // Mekandan gelen özel yanlış mesajı

    // ŞU ANKİ DOĞRU CEVAP (QuestionManager burayı güncelleyecek)
    public string currentCorrectAnswer;
    private TileType currentQuestionType;

    void Awake()
    {
        instance = this;
        if (answerPanel != null) answerPanel.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    // --- YENİ SİSTEM: SORUYU AÇMA ---
    // QuestionManager burayı çağıracak
    // ESKİSİ: public void SetQuestion(string questionText, int correctAnswer, TileType type)
    // YENİSİ:
    public void SetQuestion(string questionText, string correctAnswer, TileType type)
    {
        currentCorrectAnswer = correctAnswer;
        currentQuestionType = type;

        if (answerPanel != null) answerPanel.SetActive(true);
        if (infoText != null) infoText.text = questionText;

        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.ActivateInputField();
        }

        if (whiteboard != null) whiteboard.ClearBoard();
        if (LevelManager.instance != null) LevelManager.instance.SetDiceInteractable(false);
    }

    // --- CEVAP KONTROLÜ ---
    public void CevabiKontrolEt()
    {
        if (answerInput == null || string.IsNullOrEmpty(answerInput.text)) return;

        // Kullanıcının yazdığını al, boşlukları sil ve küçük harfe çevir
        string pInput = answerInput.text.Trim().ToLower();
        bool isCorrect = false;

        // --- YENİ KURAL: TANIMSIZ KONTROLÜ ---
        if (currentCorrectAnswer == "tanımsız")
        {
            // Kullanıcı "tanimsiz" veya "tanımsız" yazmış olabilir, tolerans gösterelim
            string normalizedInput = pInput.Replace("ı", "i");
            if (normalizedInput == "tanimsiz")
            {
                isCorrect = true;
            }
            // Yanlış yazdıysa isCorrect 'false' olarak kalır ve ceza alır.
        }
        else
        {
            // --- NORMAL SAYISAL KONTROL ---
            // Eğer cevap sayıysa ama oyuncu yanlışlıkla harf girdiyse (örn: 'asd'), butona basmayı reddet.
            if (!int.TryParse(pInput, out int oyuncuCevabi)) return;

            if (int.TryParse(currentCorrectAnswer, out int gercekCevap))
            {
                isCorrect = (oyuncuCevabi == gercekCevap);
            }
        }

        // Soru panelini kapat
        if (answerPanel != null) answerPanel.SetActive(false);

        // Mod Kontrolü
        if (LevelManager.instance != null && LevelManager.instance.isPenaltyActive)
        {
            HandlePenaltyFeedback(isCorrect);
        }
        else
        {
            HandleNormalFeedback(isCorrect);
        }
    }

    // --- NORMAL GERİ BİLDİRİM (DÜZELTİLDİ) ---
    void HandleNormalFeedback(bool isCorrect)
    {
        // 1. Önce Joker Butonunu HER İHTİMALE KARŞI gizle.
        if (retryButton != null) retryButton.SetActive(false);

        if (isCorrect)
        {
            // --- DOĞRU CEVAP ---
            if (LevelManager.instance != null) LevelManager.instance.CheckMissionProgress(currentQuestionType);

            bool isHard = (currentQuestionType == TileType.Hard);
            bool isPenalty = (LevelManager.instance != null && LevelManager.instance.isPenaltyActive);
            SaveManager.instance.RegisterAnswer(true, isHard, isPenalty);

            if (GameManager.instance != null && GameManager.instance.player != null)
                GameManager.instance.player.BonusMove(0);

            ShowFeedbackPanel(true, false);
        }
        else
        {
            // --- YANLIŞ CEVAP ---
            SaveManager.instance.SaveLastStreakBeforeReset();

            // 2. Joker Kontrolü: Oyuncunun "İkinci Şans" jokeri var mı?
            bool hasJoker = false;
            if (JokerManager.instance != null)
                hasJoker = JokerManager.instance.HasSecondChance();

            ShowFeedbackPanel(false, false);

            // 3. Butonu SADECE joker varsa ve cevap yanlışsa göster
            if (retryButton != null) retryButton.SetActive(hasJoker);
        }
    }

    // --- CEZA MODU GERİ BİLDİRİM ---
    void HandlePenaltyFeedback(bool isCorrect)
    {
        if (LevelManager.instance != null) LevelManager.instance.CheckPenaltyProgress(isCorrect);
        SaveManager.instance.RegisterAnswer(isCorrect, false, true, false);
        ShowFeedbackPanel(isCorrect, true);
    }

    // --- PANEL GÖSTERME ---
    // ShowFeedbackPanel fonksiyonunu BU YENİ MANTIKLA değiştir:
    void ShowFeedbackPanel(bool isCorrect, bool isPenaltyMode)
    {
        if (feedbackPanel == null) return;
        feedbackPanel.SetActive(true);

        // --- 1. DOĞRU CEVAP MANTIĞI ---
        if (isCorrect)
        {
            feedbackTitleText.text = "DOĞRU!";
            feedbackTitleText.color = Color.green;

            // KURAL: Özel Başarı Mesajı SADECE 5. (Son) Hikaye Sorusunda Çıkar!
            if (isFinalStoryQuestion && !string.IsNullOrEmpty(currentSuccessMsg))
            {
                feedbackDescText.text = currentSuccessMsg; // Örn: "Gizli geçidi buldun!"
            }
            else if (isPenaltyMode)
            {
                int current = (LevelManager.instance != null) ? LevelManager.instance.penaltyCorrectCount : 0;
                int needed = 3 - current;
                if (current >= 3) feedbackDescText.text = "Özgürlüğüne kavuştun!";
                else feedbackDescText.text = $"Harika! {needed} tane kaldı.";
            }
            else
            {
                // İlk 4 hikaye sorusu veya Zor sorular için standart mesaj
                feedbackDescText.text = "Tebrikler, harika gidiyorsun!";
            }
        }
        // --- 2. YANLIŞ CEVAP MANTIĞI ---
        else
        {
            feedbackTitleText.text = "YANLIŞ!";
            feedbackTitleText.color = Color.red;

            // KURAL: Özel Yanlış Mesajı, TÜM Hikaye Sorularında (1-5) Çıkar.
            // Ama Zor (Hard) sorulara geçince artık standart mesaj çıkar.
            if (isStoryPhase && !string.IsNullOrEmpty(currentFailMsg))
            {
                feedbackDescText.text = currentFailMsg; // Örn: "Tüpler patladı!"
            }
            else if (isPenaltyMode)
            {
                int current = (LevelManager.instance != null) ? LevelManager.instance.penaltyCorrectCount : 0;
                int needed = 3 - current;
                feedbackDescText.text = $"Bilemedin. Hala {needed} tane lazım.";
            }
            else
            {
                // Zor sorularda veya özel mesaj yoksa standart uyarı
                if (LevelManager.instance != null && LevelManager.instance.currentScore <= 0)
                    feedbackDescText.text = "Eyvah! Puanın tükendi...";
                else
                    feedbackDescText.text = "Dikkatli ol, yanlış cevap.\nPuanın düştü.";
            }
        }

        // --- BUTON VE KAPANIŞ İŞLEMLERİ (Aynen Kalıyor) ---
        feedbackContinueButton.onClick.RemoveAllListeners();
        feedbackContinueButton.onClick.AddListener(() =>
        {
            if (feedbackTitleText.text.Contains("YANLIŞ"))
            {
                if (!isPenaltyMode && LevelManager.instance != null) LevelManager.instance.DecreaseScore();
            }

            feedbackPanel.SetActive(false);

            // Mesajları temizle
            currentSuccessMsg = "";
            currentFailMsg = "";

            // Durumları sıfırla (Güvenlik için)
            isStoryPhase = false;
            isFinalStoryQuestion = false;

            if (LevelManager.instance != null)
            {
                if (LevelManager.instance.isFailurePending) { LevelManager.instance.OpenPendingLevelFailedPanel(); return; }
                if (LevelManager.instance.isCompletionPending) { LevelManager.instance.OpenPendingLevelCompletePanel(); return; }

                if (isPenaltyMode)
                {
                    int current = LevelManager.instance.penaltyCorrectCount;
                    if (isCorrect && current >= 3) { LevelManager.instance.ExitPenaltyZone(); if (LevelManager.instance.isCompletionPending) LevelManager.instance.OpenPendingLevelCompletePanel(); }
                    else { QuestionManager.instance.AskRandomNormalQuestion(); }
                }
                else
                {
                    LevelManager.instance.SetDiceInteractable(true);
                }
            }
        });
    }

    // --- JOKER BUTONUNUN ÇALIŞTIRACAĞI FONKSİYON ---
    public void OnClick_UseSecondChanceJoker()
    {
        if (JokerManager.instance != null) JokerManager.instance.ConsumeSecondChance();

        feedbackPanel.SetActive(false);

        if (answerInput != null)
        {
            answerInput.text = "";
            answerPanel.SetActive(true);
            answerInput.ActivateInputField();
        }

        Debug.Log("🔁 Joker kullanıldı, soru tekrar soruluyor.");
    }

    // Mekandan gelen özel mesajları kaydeder
    public void SetCustomFeedbackMessages(string success, string fail)
    {
        currentSuccessMsg = success;
        currentFailMsg = fail;
        Debug.Log($"Özel Mesajlar Alındı: D-{success} / Y-{fail}");
    }
}