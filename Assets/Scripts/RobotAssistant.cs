using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RobotAssistant : MonoBehaviour
{
    public static RobotAssistant instance;

    [Header("UI Bağlantıları")]
    public GameObject speechBubbleObj;
    public TextMeshProUGUI bubbleText;
    public CanvasGroup bubbleCanvasGroup;

    [Header("Ayarlar")]
    public float typeSpeed = 0.02f;

    private struct MessageData
    {
        public string text;
        public float duration;
        public bool isDismissible; // Tıklayınca geçilir mi?
        public bool isHint;        // BU BİR İPUCU MU? (Yeni Koruma)
    }

    private Queue<MessageData> messageQueue = new Queue<MessageData>();

    private bool isSpeaking = false;
    private bool isTyping = false;
    private bool isTextCompleted = false;

    // Şu anki mesajın özellikleri
    private bool currentMessageDismissible = true;
    private bool currentMessageIsHint = false; // Şu an ekrandaki yazı ipucu mu?

    private string lastContextHint = "";

    private Tween currentTypewriter;
    private Tween currentFade;
    private Tween currentDelayedCall;
    private float lastInteractionTime;

    void Awake()
    {
        instance = this;
        if (speechBubbleObj) speechBubbleObj.SetActive(false);
        if (bubbleCanvasGroup) bubbleCanvasGroup.alpha = 0;
    }

    void Update()
    {
        if (isSpeaking)
        {
            if (IsPointerOverUI())
            {
                lastInteractionTime = Time.time;
                return;
            }

            if (Time.time - lastInteractionTime < 0.2f) return;

            if (WasClickedOrTouched())
            {
                HandleClick();
            }
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return true;
        }
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.touchId)) return true;
            }
        }
        return false;
    }

    private bool WasClickedOrTouched()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        return false;
    }

    void HandleClick()
    {
        if (isTyping)
        {
            if (currentTypewriter != null && currentTypewriter.IsActive()) currentTypewriter.Complete();
            return;
        }

        if (isTextCompleted)
        {
            // Eğer mesaj kapatılamaz türdeyse (İpucu gibi) tıklamayı yoksay
            if (!currentMessageDismissible) return;

            if (currentDelayedCall != null) currentDelayedCall.Kill();
            ProcessNextMessage();
        }
    }

    // --- KONUŞMA SİSTEMİ (GÜNCELLENDİ: isHint Eklendi) ---
    public void Say(string text, float duration = 4f, bool canCloseByClick = true, bool isHintMessage = false)
    {
        messageQueue.Enqueue(new MessageData
        {
            text = text,
            duration = duration,
            isDismissible = canCloseByClick,
            isHint = isHintMessage // Mesajın türünü kaydet
        });

        if (!isSpeaking) ProcessNextMessage();
    }

    private void ProcessNextMessage()
    {
        if (messageQueue.Count == 0) { FinishSpeaking(); return; }

        isSpeaking = true;
        isTyping = true;
        isTextCompleted = false;

        MessageData data = messageQueue.Dequeue();

        // Şu anki mesajın özelliklerini hafızaya al
        currentMessageDismissible = data.isDismissible;
        currentMessageIsHint = data.isHint;

        if (currentTypewriter != null) currentTypewriter.Kill();
        if (currentFade != null) currentFade.Kill();
        if (currentDelayedCall != null) currentDelayedCall.Kill();

        speechBubbleObj.SetActive(true);
        bubbleText.text = "";
        bubbleCanvasGroup.alpha = 1;

        float animDuration = data.text.Length * typeSpeed;

        currentTypewriter = DOTween.To(() => bubbleText.text, x => bubbleText.text = x, data.text, animDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isTyping = false;
                isTextCompleted = true;
                if (data.duration > 0)
                {
                    currentDelayedCall = DOVirtual.DelayedCall(data.duration, () => { ProcessNextMessage(); });
                }
            });
    }

    private void FinishSpeaking()
    {
        isSpeaking = false; isTyping = false; isTextCompleted = false;

        // Eğer ekranda kalan mesaj kapatılamaz bir ipucuysa kapatma
        if (!currentMessageDismissible) return;

        CloseBubble();
    }

    public void CloseBubble()
    {
        if (messageQueue.Count > 0) { ProcessNextMessage(); return; }
        if (!speechBubbleObj.activeSelf) return;

        currentFade = bubbleCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
        {
            speechBubbleObj.SetActive(false);
            isSpeaking = false;
        });
    }

    public bool IsBusy() { return isSpeaking || messageQueue.Count > 0; }

    // --- TEMİZLİK FONKSİYONU (AKILLI KORUMA EKLENDİ) ---
    public void ClearHintMemory()
    {
        lastContextHint = "";

        // KRİTİK NOKTA:
        // Eğer şu an ekranda bir şey yazıyorsa VE bu bir İpucu DEĞİLSE (yani Tebrikse),
        // ONU ÖLDÜRME! Bırak konuşsun.
        if (isSpeaking && !currentMessageIsHint)
        {
            // Robot şu an önemli bir şey (Tebrik) söylüyor, elleme.
            return;
        }

        // Eğer buraya geldiysek, ya ipucu veriyordur ya da boştadır. Temizle.
        if (currentTypewriter != null) currentTypewriter.Kill();
        if (currentDelayedCall != null) currentDelayedCall.Kill();

        isSpeaking = false;
        isTyping = false;
        isTextCompleted = false;
        bubbleText.text = "";

        // Sırada mesaj varsa ona geç, yoksa kapat
        if (messageQueue.Count > 0) ProcessNextMessage();
        else CloseBubble();
    }

    // --- SENARYOLAR ---

    public void ShowLocationHint(string hint)
    {
        messageQueue.Clear();
        lastContextHint = hint;
        // Bu bir İPUCUDUR (isHint = true)
        Say($"{hint}", 0, false, true);
    }

    public void RestoreLastHint()
    {
        if (messageQueue.Count == 0 && !string.IsNullOrEmpty(lastContextHint) && AnswerManager.instance.answerPanel.activeSelf)
        {
            messageQueue.Clear();
            // Bu bir İPUCUDUR (isHint = true)
            Say($"{lastContextHint}", 0, false, true);
        }
        else if (messageQueue.Count == 0)
        {
            CloseBubble();
        }
    }

    public void AskForMenu()
    {
        messageQueue.Clear();
        // Menü sorusu da teknik olarak ipucu gibi davranır (Kalıcıdır)
        Say("Hangi paneli açmak istersin?", 0, false, true);
    }

    public void Celebrate(string message)
    {
        // Bu bir KUTLAMADIR (isHint = false) -> KORUMALI MESAJ
        Say($"<color=#00FF00>TEBRİKLER!</color>\n{message}", 4f, true, false);
    }

    public void ShowError(string message)
    {
        Say($"<color=red>DİKKAT:</color>\n{message}", 3f, true, false);
    }
}