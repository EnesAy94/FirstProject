using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Listeyi karıştırmak için lazım

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

    [Header("Joker Tanımları")]
    public List<JokerData> allJokerDefinitions;

    [Header("UI: HUD & Envanter")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI currentStreakHUDText;
    public Transform inventoryContainer;
    public GameObject jokerCardPrefab;   // Envanterdeki kart prefabı

    [Header("UI: Joker Kazanma (Seçim) Ekranı")]
    public GameObject jokerSelectionPanel;    // Kartların çıktığı büyük panel
    public Transform selectionContainer;      // Kartların dizileceği yer
    public GameObject pickCardPrefab;         // Masaya konacak kart prefabı (JokerCardPickUI olan)
    public Button takeButton;                 // "AL" butonu
    public GameObject takeButtonObj;          // Butonun objesi (Gizleyip açmak için)

    [Header("UI: Renk Seçim Paneli")]
    public GameObject colorSelectPanel;

    // O an masada seçilen kartın verisi
    private JokerData currentSelectedJoker;

    void Awake()
    {
        instance = this;
        // Envanteri sıfırla
        jokerInventory[JokerType.ColorMove] = 0;
        jokerInventory[JokerType.SecondChance] = 0;
        jokerInventory[JokerType.StreakRestore] = 0;

        if (jokerSelectionPanel) jokerSelectionPanel.SetActive(false);
    }

    void Update()
    {
        if (currentStreakHUDText != null && SaveManager.instance != null)
        {
            currentStreakHUDText.text = "Seri: " + SaveManager.instance.activeSave.currentStreak;
        }
    }

    // --- 1. JOKER KAZANMA SÜRECİNİ BAŞLAT ---
    // (Bunu Joker Kutusuna gelince LevelManager çağıracak)
    public void StartJokerSelection()
    {
        if (jokerSelectionPanel == null) return;

        jokerSelectionPanel.SetActive(true);
        takeButtonObj.SetActive(false);
        currentSelectedJoker = null;

        foreach (Transform child in selectionContainer) Destroy(child.gameObject);

        List<JokerData> shuffledList = allJokerDefinitions.OrderBy(x => Random.value).ToList();

        foreach (JokerData data in shuffledList)
        {
            GameObject cardObj = Instantiate(pickCardPrefab, selectionContainer); // Aynı prefabı kullanıyoruz
            JokerCardPickUI cardScript = cardObj.GetComponent<JokerCardPickUI>();

            // BURASI DEĞİŞTİ: SetupForSelection kullanıyoruz
            cardScript.SetupForSelection(data, OnCardRevealed);
        }
    }

    // --- 2. KART SEÇİLİNCE (Kart Scripti Burayı Çağırır) ---
    // --- 2. KART SEÇİLİNCE ---
    void OnCardRevealed(JokerData revealedData)
    {
        // Oyuncu bir karta tıkladı ve kart döndü.
        currentSelectedJoker = revealedData;

        // --- YENİ KISIM: DİĞER KARTLARI KİLİTLE ---
        // Selection Container içindeki tüm çocukları (kartları) gez
        foreach (Transform child in selectionContainer)
        {
            // Her kartın üzerindeki butonu bul ve kapat
            Button cardBtn = child.GetComponent<Button>();
            if (cardBtn != null)
            {
                cardBtn.interactable = false;
            }
        }
        // -----------------------------------------

        // "AL" butonunu göster ve hazırla
        takeButtonObj.SetActive(true);
        takeButton.onClick.RemoveAllListeners();
        takeButton.onClick.AddListener(TakeSelectedJoker);
    }

    // --- 3. "AL" BUTONUNA BASINCA ---
    void TakeSelectedJoker()
    {
        if (currentSelectedJoker == null) return;

        // A. Envantere Ekle
        if (!jokerInventory.ContainsKey(currentSelectedJoker.type))
            jokerInventory[currentSelectedJoker.type] = 0;

        jokerInventory[currentSelectedJoker.type]++;

        Debug.Log("🃏 JOKER ALINDI: " + currentSelectedJoker.jokerName);

        // B. Envanter UI'ını güncelle
        RefreshInventoryUI();

        // C. Seçim Panelini Kapat
        jokerSelectionPanel.SetActive(false);

        // D. ÖZEL DURUM: Eğer bu bir "Renk Jokeri" ise hemen kullanma panelini aç!
        if (currentSelectedJoker.type == JokerType.ColorMove)
        {
            UseColorJoker();
        }
    }

    // --- RENK JOKERİ KULLANIMI ---
    public void UseColorJoker()
    {
        // Envanterden düş
        if (jokerInventory[JokerType.ColorMove] > 0)
            jokerInventory[JokerType.ColorMove]--;

        RefreshInventoryUI();

        // Renk Panelini Aç
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

    // --- ENVANTER VE DİĞER FONKSİYONLAR (Aynen kaldı) ---
    public bool HasSecondChance()
    {
        return jokerInventory.ContainsKey(JokerType.SecondChance) && jokerInventory[JokerType.SecondChance] > 0;
    }

    public void ConsumeSecondChance()
    {
        if (HasSecondChance())
        {
            jokerInventory[JokerType.SecondChance]--;
            RefreshInventoryUI();
        }
    }

    public void UseJokerFromInventory(JokerType type)
    {
        if (type == JokerType.StreakRestore)
        {
            if (jokerInventory.ContainsKey(JokerType.StreakRestore) && jokerInventory[JokerType.StreakRestore] > 0)
            {
                jokerInventory[JokerType.StreakRestore]--;
                SaveManager.instance.RestoreLostStreak();
                RefreshInventoryUI();
            }
        }
    }

    public void RefreshInventoryUI()
    {
        if (inventoryContainer == null || jokerCardPrefab == null) return; // Not: Artık jokerCardPrefab = pickCardPrefab olabilir

        // Temizle
        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);

        // Diz
        foreach (var item in jokerInventory)
        {
            JokerType type = item.Key;
            int count = item.Value;

            if (count > 0)
            {
                JokerData data = allJokerDefinitions.Find(x => x.type == type);

                // Renk jokerini envanterde göstermek istiyor musun? Genelde hemen kullanılır.
                // Eğer istemiyorsan: && type != JokerType.ColorMove ekle
                if (data != null)
                {
                    // BURASI DEĞİŞTİ: Artık PickCardPrefab'ı veya aynısını kullanıyoruz
                    GameObject newCard = Instantiate(jokerCardPrefab, inventoryContainer);

                    // Yeni scripti alıyoruz
                    JokerCardPickUI cardScript = newCard.GetComponent<JokerCardPickUI>();

                    if (cardScript != null)
                    {
                        // Envanter modunda kuruyoruz
                        cardScript.SetupForInventory(data, count);
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
            if (isOpen) RefreshInventoryUI();
        }
    }
}