using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Listeyi karıştırmak için lazım

public enum JokerType
{
    ColorMove,
    SecondChance,
    StreakRestore,
    PrisonBreak,
    ScoreBoost
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
    public GameObject jokerCardPrefab;   // Envanterdeki kart prefabı (PickCardPrefab ile aynı olabilir)

    [Header("UI: Joker Kazanma (Seçim) Ekranı")]
    public GameObject jokerSelectionPanel;    // Kartların çıktığı büyük panel
    public Transform selectionContainer;      // Kartların dizileceği yer
    public GameObject pickCardPrefab;         // Masaya konacak kart prefabı (JokerCardPickUI olan)
    public Button takeButton;                 // "AL" butonu
    public GameObject takeButtonObj;          // Butonun objesi (Gizleyip açmak için)

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

        // Masayı temizle
        foreach (Transform child in selectionContainer) Destroy(child.gameObject);

        // Kartları karıştır
        List<JokerData> shuffledList = allJokerDefinitions.OrderBy(x => Random.value).ToList();

        // Kartları diz
        foreach (JokerData data in shuffledList)
        {
            GameObject cardObj = Instantiate(pickCardPrefab, selectionContainer);
            JokerCardPickUI cardScript = cardObj.GetComponent<JokerCardPickUI>();

            // Seçim modunda kur
            cardScript.SetupForSelection(data, OnCardRevealed);
        }
    }

    // --- 2. KART SEÇİLİNCE ---
    void OnCardRevealed(JokerData revealedData)
    {
        currentSelectedJoker = revealedData;

        // Diğer kartları kilitle
        foreach (Transform child in selectionContainer)
        {
            Button cardBtn = child.GetComponent<Button>();
            if (cardBtn != null) cardBtn.interactable = false;
        }

        // "AL" butonunu hazırla
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

        // D. Eğer Renk Jokeri ise hemen uygula
        if (currentSelectedJoker.type == JokerType.ColorMove)
        {
            UseColorJoker();
        }
    }

    // --- RENK JOKERİ KULLANIMI ---
    public void UseColorJoker()
    {
        if (jokerInventory.ContainsKey(JokerType.ColorMove) && jokerInventory[JokerType.ColorMove] > 0)
        {
            jokerInventory[JokerType.ColorMove]--;
            RefreshInventoryUI();

            // İŞLEMİ 'ACTIONS' SCRIPTİNE HAVALE ET
            if (JokerActions.instance != null)
            {
                JokerActions.instance.ExecuteJokerEffect(JokerType.ColorMove);
            }
        }
    }

    // --- ENVANTERDEN KULLANIM ---
    public void UseJokerFromInventory(JokerType type)
    {
        if (jokerInventory.ContainsKey(type) && jokerInventory[type] > 0)
        {
            jokerInventory[type]--;
            RefreshInventoryUI();

            // İŞLEMİ 'ACTIONS' SCRIPTİNE HAVALE ET
            if (JokerActions.instance != null)
            {
                JokerActions.instance.ExecuteJokerEffect(type);
            }
        }
    }

    // --- PASİF JOKERLER (Action gerekmez, sadece sayı düşer) ---
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

            // Eğer ikinci şansın özel bir efekti varsa Actions'a ekleyip buradan çağırabilirsin.
            // Şimdilik sadece sayı düşüyor.
        }
    }

    // --- UI GÜNCELLEME ---
    public void RefreshInventoryUI()
    {
        if (inventoryContainer == null || jokerCardPrefab == null) return;

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

                if (data != null)
                {
                    GameObject newCard = Instantiate(jokerCardPrefab, inventoryContainer);
                    JokerCardPickUI cardScript = newCard.GetComponent<JokerCardPickUI>();

                    if (cardScript != null)
                    {
                        // Envanter modunda kur
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