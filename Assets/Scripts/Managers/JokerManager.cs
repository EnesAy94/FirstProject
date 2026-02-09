using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum JokerType
{
    ColorMove,     // 1. Renge Yürüme
    SecondChance,  // 2. İkinci Şans
    StreakRestore  // 3. Seri Kurtarma
}

public class JokerManager : MonoBehaviour
{
    public static JokerManager instance;

    [Header("Envanter Verisi")]
    public Dictionary<JokerType, int> jokerInventory = new Dictionary<JokerType, int>();

    [Header("Joker Tanımları (Buraya ScriptableObjectleri at)")]
    public List<JokerData> allJokerDefinitions;

    [Header("UI: HUD & Paneller")]
    public GameObject colorSelectPanel;
    public GameObject inventoryPanel;

    // 🔥 EKSİK OLAN KISIM EKLENDİ 🔥
    public TextMeshProUGUI currentStreakHUDText; // Ekrandaki "Seri: 5" yazısı

    [Header("UI: Dinamik Envanter")]
    public Transform inventoryContainer; // Kartların dizileceği kutu (Content)
    public GameObject jokerCardPrefab;   // Kartın tasarımı (Prefab)

    void Awake()
    {
        instance = this;
        // Envanteri sıfırla
        jokerInventory[JokerType.ColorMove] = 0;
        jokerInventory[JokerType.SecondChance] = 0;
        jokerInventory[JokerType.StreakRestore] = 0;
    }

    void Update()
    {
        // HUD Güncellemesi (SaveManager varsa)
        if (currentStreakHUDText != null && SaveManager.instance != null)
        {
            currentStreakHUDText.text = "Seri: " + SaveManager.instance.activeSave.currentStreak;
        }
    }

    // --- JOKER KAZANMA ---
    public void EarnRandomJoker()
    {
        JokerType earned = (JokerType)Random.Range(0, 3);

        // Envantere ekle
        if (!jokerInventory.ContainsKey(earned)) jokerInventory[earned] = 0;
        jokerInventory[earned]++;

        Debug.Log("🃏 JOKER KAZANILDI: " + earned);

        // Renk jokeri hemen kullanılır
        if (earned == JokerType.ColorMove)
        {
            UseColorJoker();
        }
        else
        {
            // Diğerleri için bildirim göster ve envanteri yenile
            RefreshInventoryUI(); // UI'ı güncelle

            // İsmini bulmak için listeye bakıyoruz
            string jName = "Joker";
            JokerData data = allJokerDefinitions.Find(x => x.type == earned);
            if (data != null) jName = data.jokerName;

            if (LevelManager.instance != null)
                LevelManager.instance.ShowNotification("TEBRİKLER!", jName + " kazandın!", () => { });
        }
    }

    // --- JOKER 1: RENK SEÇİMİ ---
    public void UseColorJoker()
    {
        if (jokerInventory[JokerType.ColorMove] > 0) jokerInventory[JokerType.ColorMove]--;
        RefreshInventoryUI(); // Sayı düştü, güncelle

        if (colorSelectPanel != null) colorSelectPanel.SetActive(true);
    }

    public void OnColorSelected(int colorIndex)
    {
        if (colorSelectPanel != null) colorSelectPanel.SetActive(false);
        TileType target = (TileType)colorIndex;

        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            GameManager.instance.player.GoToNearestColor(target);
        }
    }

    // --- JOKER 2: İKİNCİ ŞANS ---
    public bool HasSecondChance()
    {
        return jokerInventory.ContainsKey(JokerType.SecondChance) && jokerInventory[JokerType.SecondChance] > 0;
    }

    public void ConsumeSecondChance()
    {
        if (HasSecondChance())
        {
            jokerInventory[JokerType.SecondChance]--;
            RefreshInventoryUI(); // Ekranda da eksilsin
        }
    }

    // --- JOKER 3: ENVANTERDEN KULLANMA ---
    public void UseJokerFromInventory(JokerType type)
    {
        if (type == JokerType.StreakRestore)
        {
            if (jokerInventory.ContainsKey(JokerType.StreakRestore) && jokerInventory[JokerType.StreakRestore] > 0)
            {
                jokerInventory[JokerType.StreakRestore]--;

                SaveManager.instance.RestoreLostStreak();

                Debug.Log("🔥 Seri Kurtarıldı!");

                RefreshInventoryUI(); // Sadece sayıyı güncelle

                // inventoryPanel.SetActive(false); // <-- BU SATIRI SİL (veya yorum satırı yap)
                // Artık panel kapanmayacak, oyuncu çarpıya basıp kendi kapatır.
            }
        }
    }

    // --- DİNAMİK UI SİSTEMİ (PREFAB MANTIĞI) ---
    public void RefreshInventoryUI()
    {
        if (inventoryContainer == null || jokerCardPrefab == null) return;

        // 1. Önce eski kartları temizle
        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);

        // 2. Envanterdeki her joker türü için
        foreach (var item in jokerInventory)
        {
            JokerType type = item.Key;
            int count = item.Value;

            if (count > 0)
            {
                // Bu jokerin datasını (resmini, ismini) bul
                JokerData data = allJokerDefinitions.Find(x => x.type == type);

                if (data != null)
                {
                    // Eğer Renk jokeri değilse göster (Renk jokeri anında kullanılıyor demiştik)
                    // Ama istersen onu da gösterebilirsin.
                    if (type != JokerType.ColorMove)
                    {
                        GameObject newCard = Instantiate(jokerCardPrefab, inventoryContainer);
                        newCard.GetComponent<JokerItemUI>().Setup(data, count);
                    }
                }
            }
        }
    }

    public void ToggleInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            bool isOpen = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isOpen);

            if (isOpen) RefreshInventoryUI(); // Açılınca listeyi yenile
        }
    }
}