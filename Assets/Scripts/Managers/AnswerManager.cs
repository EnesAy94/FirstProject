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
    public string currentRobotHint = "";
    private string currentQuestionText;

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
        currentQuestionText = questionText;

        if (answerPanel != null) answerPanel.SetActive(true);
        if (infoText != null) infoText.text = questionText;

        // Input alanını temizle ve odaklan
        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.ActivateInputField();
        }

        if (whiteboard != null) whiteboard.ClearBoard();

        // Zarı kilitle (LevelManager üzerinden)
        if (LevelManager.instance != null) LevelManager.instance.SetDiceInteractable(false);

        // Robot Butonunu Pasif Yap (Answer Panel açıkken menü açılmasın)
        if (UIManager.instance != null)
        {
            UIManager.instance.SetRobotInteractable(false);
        }

        // --- ROBOT İPUCU MANTIĞI (GÜNCELLENDİ) ---
        string finalHint = "";

        // 1. DURUM: Hapishane (Ceza) Modu -> İpucu YOK
        if (LevelManager.instance != null && LevelManager.instance.isPenaltyActive)
        {
            finalHint = ""; // Robot sussun
        }
        // 2. DURUM: Zor Soru (Hard) -> Sabit Uyarı Mesajı
        else if (type == TileType.Hard)
        {
            finalHint = "⚠️ DİKKAT: Bu bir ZOR SORU!\nYanlış yaparsan yüksek puan kaybedersin. İyice düşün!";
        }
        // 3. DURUM: Normal Hikaye Sorusu -> Mekana Özel İpucu
        else
        {
            // PlayerMovement'tan gelen, o mekana özel ipucunu kullan
            finalHint = currentRobotHint;
        }

        // Karar verilen ipucunu Robota söylet
        if (RobotAssistant.instance != null && !string.IsNullOrEmpty(finalHint))
        {
            RobotAssistant.instance.ShowLocationHint(finalHint);
        }
        // ------------------------------------------
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

    // --- NORMAL GERİ BİLDİRİM (GÜNCELLENMİŞ - BAŞARIM SİSTEMİ EKLENDİ) ---
    void HandleNormalFeedback(bool isCorrect)
    {
        // 1. Joker Butonunu Gizle
        if (retryButton != null) retryButton.SetActive(false);

        if (RobotAssistant.instance != null)
        {
            RobotAssistant.instance.ClearHintMemory();
        }

        // Zorluk ve Ceza Durumunu Belirle
        bool isHard = (currentQuestionType == TileType.Hard);
        bool isPenalty = (LevelManager.instance != null && LevelManager.instance.isPenaltyActive);

        if (isCorrect)
        {
            // --- DOĞRU CEVAP ---

            // 1. Görev İlerlemesini Kontrol Et
            if (LevelManager.instance != null)
                LevelManager.instance.CheckMissionProgress(currentQuestionType);

            // 2. Veritabanına "DOĞRU" olarak kaydet
            SaveManager.instance.RegisterAnswer(true, isHard, isPenalty);

            // ✨ 3. BAŞARIM SİSTEMİNE BİLDİR (YENİ EKLENEN KISIM) ✨
            CheckAndUpdateAchievement(currentQuestionType);

            // 4. UI'ı Güncelle (Streak)
            if (UIManager.instance != null && SaveManager.instance != null)
            {
                UIManager.instance.UpdateStreak(SaveManager.instance.activeSave.currentStreak);
            }

            if (GameManager.instance != null && GameManager.instance.player != null)
                GameManager.instance.player.BonusMove(0);

            // İpucu hafızasını temizle (Doğru bildi, artık ipucuya gerek yok)
            if (RobotAssistant.instance != null)
            {
                RobotAssistant.instance.ClearHintMemory();
            }

            ShowFeedbackPanel(true, false);
        }
        else
        {
            // --- YANLIŞ CEVAP ---

            // 1. Önce eski seriyi hafızaya al
            SaveManager.instance.SaveLastStreakBeforeReset();

            // 2. Veritabanına "YANLIŞ" olarak kaydet (Seri SIFIRLANIR)
            SaveManager.instance.RegisterAnswer(false, isHard, isPenalty);

            // 3. UI'ı Güncelle (Sıfırla)
            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateStreak(0);
            }

            if (LevelManager.instance != null)
            {
                LevelManager.instance.RegisterWrongAnswer();
            }

            bool hasJoker = false;
            if (JokerManager.instance != null)
                hasJoker = JokerManager.instance.HasSecondChance();

            // Yanlış cevapta da ipucu temizlenmeli (Yeni soru gelecek)
            if (RobotAssistant.instance != null)
            {
                RobotAssistant.instance.ClearHintMemory();
            }

            ShowFeedbackPanel(false, false);

            if (retryButton != null) retryButton.SetActive(hasJoker);
        }
    }

    // ✨ YENİ FONKSİYON: BAŞARIM KONTROLÜ VE GÜNCELLEME ✨
    void CheckAndUpdateAchievement(TileType solvedType)
    {
        // AchievementManager yoksa çık
        if (AchievementManager.instance == null)
        {
            Debug.LogWarning("[AnswerManager] AchievementManager bulunamadı!");
            return;
        }

        // achievementLinks listesi boşsa çık
        if (achievementLinks == null || achievementLinks.Count == 0)
        {
            Debug.LogWarning("[AnswerManager] achievementLinks listesi boş! Inspector'da ayarlanmalı.");
            return;
        }

        // Bu soru tipine bağlı bir başarım var mı?
        foreach (AchievementLink link in achievementLinks)
        {
            if (link.type == solvedType)
            {
                // Başarımı bulduk! İlerlet
                Debug.Log($"[AnswerManager] 🎯 {solvedType} sorusu çözüldü → {link.achievementID} başarımına +1 ekleniyor");
                AchievementManager.instance.AddProgress(link.achievementID, 1);
                return; // Bir tane bulduk, yeter
            }
        }

        // Eğer hiç eşleşme yoksa (Opsiyonel log)
        // Debug.Log($"[AnswerManager] {solvedType} için başarım tanımlanmamış.");
    }

    // --- CEZA MODU GERİ BİLDİRİM ---
    void HandlePenaltyFeedback(bool isCorrect)
    {
        if (LevelManager.instance != null) LevelManager.instance.CheckPenaltyProgress(isCorrect);
        SaveManager.instance.RegisterAnswer(isCorrect, false, true, false);
        ShowFeedbackPanel(isCorrect, true);
    }

    // --- PANEL GÖSTERME ---
    void ShowFeedbackPanel(bool isCorrect, bool isPenaltyMode)
    {
        if (feedbackPanel == null) return;
        feedbackPanel.SetActive(true);

        // --- 1. METİN AYARLARI ---
        if (isCorrect)
        {
            feedbackTitleText.text = "DOĞRU!";
            feedbackTitleText.color = Color.green;

            if (isPenaltyMode)
            {
                // Firar Tüneli ile bildiyse
                if (LevelManager.instance != null && LevelManager.instance.isPrisonJokerActive)
                {
                    feedbackDescText.text = "MÜKEMMEL! Risk aldın ve kazandın.\nÖzgürsün!";
                }
                else
                {
                    int current = (LevelManager.instance != null) ? LevelManager.instance.penaltyCorrectCount : 0;
                    int needed = 3 - current;
                    if (current >= 3) feedbackDescText.text = "Özgürlüğüne kavuştun!";
                    else feedbackDescText.text = $"Harika! {needed} tane kaldı.";
                }
            }
            else if (isFinalStoryQuestion && !string.IsNullOrEmpty(currentSuccessMsg))
            {
                feedbackDescText.text = currentSuccessMsg;
            }
            else
            {
                feedbackDescText.text = "Tebrikler, harika gidiyorsun!";
            }
        }
        else // YANLIŞ CEVAP KISMI
        {
            feedbackTitleText.text = "YANLIŞ!";
            feedbackTitleText.color = Color.red;

            if (isPenaltyMode)
            {
                // Hapishanede standart uyarı
                feedbackDescText.text = "Yanlış çözdün.\nGelecek soruyu daha dikkatli çöz.";
            }
            // --- DÜZELTME BURADA ---
            // Hikaye metni varsa VE soru tipi 'Hard' DEĞİLSE göster.
            else if (isStoryPhase && !string.IsNullOrEmpty(currentFailMsg) && currentQuestionType != TileType.Hard)
            {
                feedbackDescText.text = currentFailMsg;
            }
            else
            {
                // Zor sorularda veya hikayesiz sorularda burası çalışır
                if (LevelManager.instance != null && LevelManager.instance.currentScore <= 0)
                    feedbackDescText.text = "Eyvah! Puanın tükendi...";
                else
                    feedbackDescText.text = "Dikkatli ol, yanlış cevap.\nPuanın düşecek.";
            }
        }
        // --- 2. JOKER (RETRY) BUTONU KONTROLÜ ---
        if (!isCorrect && JokerManager.instance != null && JokerManager.instance.HasSecondChance())
        {
            if (retryButton != null)
            {
                retryButton.SetActive(true);

                Button btn = retryButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();

                    btn.onClick.AddListener(() =>
                    {
                        // SADECE ONAY PANELİNİ AÇ
                        if (JokerConfirmationPanel.instance != null)
                        {
                            JokerConfirmationPanel.instance.ShowPanel(
                                "İKİNCİ ŞANS",
                                "Jokerini kullanıp soruyu tekrar denemek istiyor musun?",
                        () =>
                        {
                            Debug.Log("2. Şans Jokeri Kullanıldı! Aynı soru tekrar soruluyor.");

                            // 1. Jokeri Harca
                            JokerManager.instance.ConsumeSecondChance();

                            // 2. Paneli Kapat
                            feedbackPanel.SetActive(false);

                            // 3. AYNI SORUYU TEKRAR AÇ
                            SetQuestion(currentQuestionText, currentCorrectAnswer, currentQuestionType);
                        },
                                () => // --- HAYIR'A BASARSA BURASI ÇALIŞIR ---
                                {
                                    // Hiçbir şey yapma (Soru açma kodu burada YOK)
                                }
                            );
                        }
                    });
                }
            }
        }
        else
        {
            // Joker yoksa veya Doğru bildiyse butonu gizle
            if (retryButton != null) retryButton.SetActive(false);
        }

        // --- 3. DEVAM ET BUTONU ---
        feedbackContinueButton.onClick.RemoveAllListeners();
        feedbackContinueButton.onClick.AddListener(() =>
        {
            // Puan Düşme (Yanlışsa ve Ceza Modu değilse)
            if (!isCorrect && !isPenaltyMode)
            {
                if (LevelManager.instance != null) LevelManager.instance.DecreaseScore();
            }

            feedbackPanel.SetActive(false);

            // Robot ve UI Temizliği
            if (UIManager.instance != null) UIManager.instance.SetRobotInteractable(true);
            if (RobotAssistant.instance != null) RobotAssistant.instance.ClearHintMemory();

            if (LevelManager.instance != null)
            {
                // Oyun Bitti mi?
                if (LevelManager.instance.isFailurePending) { LevelManager.instance.OpenPendingLevelFailedPanel(); return; }
                if (LevelManager.instance.isCompletionPending) { LevelManager.instance.OpenLevelCompletePanelNow(); return; }

                // Oyun Devam Ediyor
                if (isPenaltyMode)
                {
                    // A) Firar Tüneli Modu (Riskli)
                    if (LevelManager.instance.isPrisonJokerActive)
                    {
                        if (isCorrect)
                        {
                            // KAZANDI -> ÇIK
                            LevelManager.instance.isPrisonJokerActive = false;
                            LevelManager.instance.ExitPenaltyZone();
                        }
                        else
                        {
                            // KAYBETTİ -> RİSK BİTTİ, NORMAL CEZA BAŞLAR
                            LevelManager.instance.isPrisonJokerActive = false;
                            QuestionManager.instance.AskRandomNormalQuestion();
                        }
                    }
                    // B) Normal Ceza Modu
                    else
                    {
                        int current = LevelManager.instance.penaltyCorrectCount;
                        if (isCorrect && current >= 3) LevelManager.instance.ExitPenaltyZone();
                        else QuestionManager.instance.AskRandomNormalQuestion();
                    }
                }
                else
                {
                    // Normal oyun
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

    public void SetRobotHint(string hint)
    {
        currentRobotHint = hint;
    }
}