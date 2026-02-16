using UnityEngine;

public class JokerActions : MonoBehaviour
{
    public static JokerActions instance;

    [Header("Joker Panelleri ve Referanslar")]
    public GameObject colorSelectPanel; // Renk jokeri paneli
    // Yeni jokerlerin panellerini buraya ekleyeceksin.
    // Örn: public GameObject freezeTimePanel;

    void Awake()
    {
        instance = this;
    }

    // --- TÜM JOKER ETKİLERİ BURADA YÖNETİLİR ---
    public void ExecuteJokerEffect(JokerType type)
    {
        switch (type)
        {
            case JokerType.ColorMove:
                OpenColorSelectPanel();
                break;

            case JokerType.StreakRestore:
                RestoreStreakAction();
                break;

            case JokerType.SecondChance:
                // Bu genelde pasif bir joker ama aktif etkisi varsa buraya yazılır
                Debug.Log("İkinci şans aktif edildi.");
                break;

            case JokerType.ScoreBoost:
                BoostScoreAction();
                break;

                // YENİ JOKERLER BURAYA GELECEK:
                // case JokerType.TimeFreeze:
                //     FreezeTimeAction();
                //     break;
        }
    }

    // --- 1. RENK JOKERİ MANTIĞI ---
    void OpenColorSelectPanel()
    {
        if (colorSelectPanel != null) colorSelectPanel.SetActive(true);
    }

    public void OnColorSelected(int colorIndex)
    {
        if (colorSelectPanel != null) colorSelectPanel.SetActive(false);

        // Hareketi başlat
        TileType target = (TileType)colorIndex;
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            GameManager.instance.player.GoToNearestColor(target);
        }
    }

    // --- 2. SERİ KURTARMA MANTIĞI ---
    void RestoreStreakAction()
    {
        // 1. Veriyi Kurtar
        SaveManager.instance.RestoreLostStreak();

        Debug.Log("🔥 Seri Kurtarıldı (Action Scriptinden)");

        // 2. UI'ı ANINDA GÜNCELLE (Bunu eklemezsen görünmez kalır)
        if (UIManager.instance != null && SaveManager.instance != null)
        {
            // Güncel seri değerini UIManager'a gönder, o da barı açsın
            UIManager.instance.UpdateStreak(SaveManager.instance.activeSave.currentStreak);
        }
    }

    // --- YENİ: PUAN ARTIRMA (YARIM CAN) ---
    void BoostScoreAction()
    {
        if (LevelManager.instance != null)
        {
            // 1. O anki bölümün cezasını öğren (LevelManager'a yeni eklediğimiz fonksiyon)
            int penalty = LevelManager.instance.GetCurrentPenalty();

            // 2. Yarısını hesapla (Örn: 25 ise 12 olur)
            int boostAmount = penalty / 2;

            // 0 olmasın, en az 1 can verelim
            if (boostAmount < 1) boostAmount = 1;

            // 3. Puanı artır (LevelManager'a yeni eklediğimiz fonksiyon)
            LevelManager.instance.IncreaseScore(boostAmount);

            // 4. Robot Konuşsun
            if (RobotAssistant.instance != null)
            {
                RobotAssistant.instance.Say("Canın tazelendi! (+" + boostAmount + ")", 3f);
            }
        }
    }
}