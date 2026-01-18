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
        public TileType type;      // Hangi Soru Türü? (Örn: Blue)
        public string achievementID; // Hangi Başarım ID'si? (Örn: "blue_master")
    }

    [Header("Başarım Ayarları")]
    // Editörden dolduracağımız liste bu:
    public List<AchievementLink> achievementLinks;

    [Header("UI Elemanları")]
    public GameObject answerPanel;
    public TextMeshProUGUI infoText;
    public TMP_InputField answerInput;
    public Whiteboard whiteboard;
    public GameObject questionPanel;

    [Header("Sonuç / Feedback UI")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackTitleText;
    public TextMeshProUGUI feedbackDescText;
    public Button feedbackContinueButton;

    private int currentCorrectAnswer;
    private TileType currentQuestionType;

    void Awake()
    {
        instance = this;
        answerPanel.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    public void PaneliAc(string baslik, string esyaDetayi, string soruKurali, int adet, int dogruCevap, TileType type)
    {
        answerPanel.SetActive(true);

        // DEĞİŞİKLİK 1: Soru paneli açılınca zarı kilitle (Arkada basılmasın)
        if (LevelManager.instance != null) LevelManager.instance.SetDiceInteractable(false);

        currentCorrectAnswer = dogruCevap;
        currentQuestionType = type;
        answerInput.text = "";
        if (whiteboard != null) whiteboard.ClearBoard();
        infoText.text = $"{baslik}\n\n{esyaDetayi}\n\n {soruKurali}";
    }

    public void CevabiKontrolEt()
    {
        if (string.IsNullOrEmpty(answerInput.text)) return;
        int oyuncuCevabi;
        if (!int.TryParse(answerInput.text, out oyuncuCevabi)) return;

        bool isCorrect = (oyuncuCevabi == currentCorrectAnswer);

        // Önce Soru Panellerini Kapat
        answerPanel.SetActive(false);
        if (questionPanel != null) questionPanel.SetActive(false);

        // Hangi Moddayız?
        if (LevelManager.instance != null && LevelManager.instance.isPenaltyActive)
        {
            // CEZA MODU (Özel Mantık)
            HandlePenaltyFeedback(isCorrect);
        }
        else
        {
            // NORMAL / ZOR MOD (Standart Mantık)
            HandleNormalFeedback(isCorrect);
        }
    }

    // --- NORMAL OYUN SONUCU ---
    void HandleNormalFeedback(bool isCorrect)
    {
        if (isCorrect)
        {
            // 1. ÖNCE GÖREVİ İLERLET VE KİLİDİ AÇTIR! (Yer değiştirdi)
            if (LevelManager.instance != null)
            {
                LevelManager.instance.CheckMissionProgress(currentQuestionType);
            }

            // 2. SONRA BAŞARIMI KONTROL ET (Artık kilit açık olduğu için işleyecek)
            foreach (AchievementLink link in achievementLinks)
            {
                if (link.type == currentQuestionType)
                {
                    AchievementManager.instance.AddProgress(link.achievementID, 1);
                    break;
                }
            }

            if (GameManager.instance != null) GameManager.instance.player.BonusMove(0);
        }
        else
        {
            if (LevelManager.instance != null) LevelManager.instance.DecreaseScore();
        }

        // İSTATİSTİK KAYDI:
        bool isHard = (currentQuestionType == TileType.Hard);
        bool isPenalty = false;
        if (LevelManager.instance != null)
        {
            isPenalty = LevelManager.instance.isPenaltyActive;
        }
        SaveManager.instance.RegisterAnswer(isCorrect, isHard, isPenalty);

        ShowFeedbackPanel(isCorrect, false);
    }

    // --- CEZA MODU SONUCU ---
    void HandlePenaltyFeedback(bool isCorrect)
    {
        LevelManager.instance.CheckPenaltyProgress(isCorrect);
        SaveManager.instance.RegisterAnswer(isCorrect, false, true);
        ShowFeedbackPanel(isCorrect, true);
    }

    // --- PANELİ GÖSTERME VE BUTON AYARLAMA ---
    void ShowFeedbackPanel(bool isCorrect, bool isPenaltyMode)
    {
        if (feedbackPanel == null) return;
        feedbackPanel.SetActive(true);

        // Başlık ve Renk
        if (isCorrect)
        {
            feedbackTitleText.text = "DOĞRU!";
            feedbackTitleText.color = Color.green;
        }
        else
        {
            feedbackTitleText.text = "YANLIŞ!";
            feedbackTitleText.color = Color.red;
        }

        feedbackContinueButton.onClick.RemoveAllListeners();

        // --- BUTON TIKLANINCA YAPILACAKLAR (SIRALI KONTROL) ---
        feedbackContinueButton.onClick.AddListener(() =>
        {
            // 1. Önce Feedback Panelini Kapat
            feedbackPanel.SetActive(false);

            // 2. KONTROL 1: Oyun Kaybedildi mi? (Puan bitti mi?)
            // En yüksek öncelik bunda. Puan bittiyse ne ceza modu kalır ne başka bir şey.
            if (LevelManager.instance.isFailurePending)
            {
                LevelManager.instance.OpenPendingLevelFailedPanel();
                return; // Fonksiyondan çık, başka işlem yapma
            }

            // 3. KONTROL 2: Oyun Kazanıldı mı?
            // Yanlış cevap verip puanı bitirmemiş ama bölümü bitirmiş olabilir mi?
            // (Nadir ama görev sistemi mantığına göre kontrol etmekte fayda var)
            if (LevelManager.instance.isCompletionPending)
            {
                LevelManager.instance.OpenPendingLevelCompletePanel();
                return; // Fonksiyondan çık
            }

            // 4. KONTROL 3: Oyun Devam Ediyor (Ceza veya Normal Mod)
            if (isPenaltyMode)
            {
                // Ceza Modu Mantığı
                int current = LevelManager.instance.penaltyCorrectCount;

                if (isCorrect && current >= 3)
                {
                    // Ceza Bitti
                    LevelManager.instance.ExitPenaltyZone();

                    // Çıkınca belki oyun bitmiştir (Son görevse)
                    // Tekrar kontrol et (AnswerManager içinde çağırmıştık ama garanti olsun)
                    if (LevelManager.instance.isCompletionPending)
                        LevelManager.instance.OpenPendingLevelCompletePanel();
                }
                else
                {
                    // Ceza Devam Ediyor -> Yeni Soru
                    QuestionManager.instance.AskRandomNormalQuestion();
                }
            }
            else
            {
                // Normal Mod -> Sadece devam et
                LevelManager.instance.SetDiceInteractable(true);
            }
        });

        // --- AÇIKLAMA METİNLERİ (Sadece Görsel) ---
        if (isPenaltyMode)
        {
            int current = LevelManager.instance.penaltyCorrectCount;
            int needed = 3 - current;

            if (isCorrect && current >= 3) feedbackDescText.text = "Özgürlüğüne kavuştun!";
            else feedbackDescText.text = isCorrect ? $"Harika! {needed} tane kaldı." : $"Bilemedin. Hala {needed} tane lazım.";
        }
        else
        {
            // Normal Mod Metinleri
            if (isCorrect) feedbackDescText.text = "Tebrikler, yola devam!";
            else
            {
                // Eğer puan bittiyse açıklama farklı olabilir
                if (LevelManager.instance.currentScore <= 0) feedbackDescText.text = "Eyvah! Puanın tükendi...";
                else feedbackDescText.text = "Olsun, bir dahakine dikkat et.\nPuanın düştü.";
            }
        }
    }
}