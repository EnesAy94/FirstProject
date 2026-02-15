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
}