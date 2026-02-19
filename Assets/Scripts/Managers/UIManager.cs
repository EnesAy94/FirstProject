using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HUD")]
    public Slider healthBarSlider;
    public Image healthBarFillImage;
    public TextMeshProUGUI healthText;
    public GameObject streakBadgeObj;
    public TextMeshProUGUI streakText;

    [Header("Robot ve Menü")]
    public GameObject menuContainer;
    public Button btnRobot;
    public Button btnInventory;
    public Button btnMissions;

    // Boşluğa tıklamayı yakalayan görünmez buton
    public Button menuOverlayButton;

    private bool isMenuOpen = false;

    [Header("Paneller")]
    public GameObject inventoryPanel;
    public GameObject missionsPanel;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Başlangıçta gizle
        if (menuContainer) menuContainer.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (missionsPanel) missionsPanel.SetActive(false);
        if (menuOverlayButton) menuOverlayButton.gameObject.SetActive(false);

        // Buton Bağlantıları
        if (btnRobot) btnRobot.onClick.AddListener(ToggleRobotMenu);
        if (btnInventory) btnInventory.onClick.AddListener(ToggleInventory);
        if (btnMissions) btnMissions.onClick.AddListener(ToggleMissions);

        // --- DEĞİŞİKLİK BURADA: Overlay artık Akıllı Yönetici ---
        if (menuOverlayButton) menuOverlayButton.onClick.AddListener(OnOverlayClicked);

        // Başlangıç verilerini yükle
        if (SaveManager.instance != null && SaveManager.instance.activeSave != null)
        {
            UpdateStreak(SaveManager.instance.activeSave.currentStreak);
        }
        else
        {
            if (streakBadgeObj) streakBadgeObj.SetActive(false);
        }
    }

    // --- 1. AKILLI OVERLAY SİSTEMİ (YENİ FONKSİYON) ---
    // Boşluğa tıklayınca ne olacağına burası karar veriyor
    public void OnOverlayClicked()
    {
        // 1. Önce: Açık bir panel varsa onu kapat
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            ToggleInventory(); // Sadece envanteri kapatır, menü kalır
            return; // İşlem bitti, aşağı inme
        }

        if (missionsPanel != null && missionsPanel.activeSelf)
        {
            ToggleMissions(); // Sadece görevleri kapatır, menü kalır
            return; // İşlem bitti, aşağı inme
        }

        // 2. Eğer açık panel yoksa: Menüyü tamamen kapat
        CloseRobotMenuAndRestore();
    }

    // --- 2. ROBOT MENÜSÜ YÖNETİMİ ---

    // --- 1. ROBOT MENÜSÜ YÖNETİMİ (DÜZELTİLDİ) ---
    public void ToggleRobotMenu()
    {
        isMenuOpen = !isMenuOpen;
        UpdateMenuVisuals();

        // Eğer menüyü kapatıyorsak:
        if (!isMenuOpen)
        {
            // 1. Açık olan panelleri zorla kapat
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (missionsPanel) missionsPanel.SetActive(false);

            // 2. Robot eski ipucuna dönsün
            if (RobotAssistant.instance != null) RobotAssistant.instance.RestoreLastHint();
        }
    }

    // UIManager.cs içindeki CloseRobotMenuAndRestore fonksiyonunu BUL ve GÜNCELLE:

    public void CloseRobotMenuAndRestore()
    {
        isMenuOpen = false;

        // Panelleri kapat
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (missionsPanel) missionsPanel.SetActive(false);

        UpdateMenuVisuals();

        // --- KRİTİK DEĞİŞİKLİK BURADA ---
        if (RobotAssistant.instance != null)
        {
            // 1. Önce robotu tamamen sustur (Ekranda "Bu joker kullanılmaz" vs. kalmasın)
            RobotAssistant.instance.ForceStopSpeaking();

            // 2. (İsteğe Bağlı) Eğer eski ipucunun geri gelmesini istersen bunu açabilirsin.
            // Ama "Yazı da kapansın" dediğin için şimdilik kapalı tutuyorum.
            // RobotAssistant.instance.RestoreLastHint(); 
        }
    }

    private void UpdateMenuVisuals()
    {
        if (menuContainer) menuContainer.SetActive(isMenuOpen);

        // Menü açıksa Overlay (Perde) her zaman açık olmalı
        if (menuOverlayButton) menuOverlayButton.gameObject.SetActive(isMenuOpen);

        if (isMenuOpen)
        {
            // Menü açıldıysa sorusunu sorsun
            if (RobotAssistant.instance != null) RobotAssistant.instance.AskForMenu();
        }
    }

    // UIManager.cs içindeki ToggleInventory fonksiyonu:

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        bool willBeActive = !inventoryPanel.activeSelf;

        if (missionsPanel) missionsPanel.SetActive(false);
        inventoryPanel.SetActive(willBeActive);

        if (willBeActive)
        {
            // Panel açıldı, Robot sussun
            if (RobotAssistant.instance != null) RobotAssistant.instance.CloseBubble();
        }
        else
        {
            // --- DEĞİŞİKLİK ---
            // Panel KAPANDI. Eğer menü de kapalıysa Robotu ANINDA sustur.
            // (Eskiden burada soru sormaya çalışıyordu, o yüzden takılıyordu)
            if (!isMenuOpen && RobotAssistant.instance != null)
            {
                RobotAssistant.instance.ForceStopSpeaking();
            }
            // Eğer menü arkada hala açıksa (ki overlay sistemiyle zor ama) menü sorusunu sorabilir.
            else if (isMenuOpen && RobotAssistant.instance != null)
            {
                RobotAssistant.instance.AskForMenu();
            }
        }
    }

    public void ToggleMissions()
    {
        if (missionsPanel == null) return;

        bool willBeActive = !missionsPanel.activeSelf;

        if (inventoryPanel) inventoryPanel.SetActive(false);
        missionsPanel.SetActive(willBeActive);

        if (willBeActive)
        {
            if (RobotAssistant.instance != null) RobotAssistant.instance.CloseBubble();
        }
        else if (isMenuOpen)
        {
            if (RobotAssistant.instance != null) RobotAssistant.instance.AskForMenu();
        }
    }

    // ... (Can Barı ve Streak kodları AYNEN KALIYOR) ...
    public void InitializeHealthBar(int max, int cur)
    {
        if (healthBarSlider)
        {
            healthBarSlider.maxValue = max; healthBarSlider.value = cur; UpdateHealthVisuals(cur, max);
        }
    }
    public void UpdateHealthBar(int cur, int max) { if (healthBarSlider) { healthBarSlider.value = cur; UpdateHealthVisuals(cur, max); } }
    private void UpdateHealthVisuals(int cur, int max)
    {
        if (healthBarFillImage)
        {
            float r = (float)cur / max; if (r > 0.5f) healthBarFillImage.color = Color.green; else if (r > 0.2f) healthBarFillImage.color = Color.yellow; else healthBarFillImage.color = Color.red;
        }
        if (healthText) healthText.text = cur.ToString();
    }
    public void UpdateStreak(int streakCount)
    {
        if (streakBadgeObj)
        {
            if (streakCount > 1) { streakBadgeObj.SetActive(true); if (streakText) streakText.text = "x" + streakCount; }
            else streakBadgeObj.SetActive(false);
        }
    }

    // --- ROBOT KİLİT SİSTEMİ (YENİ) ---
    // Soru paneli açılınca Robotu kilitleyeceğiz.
    public void SetRobotInteractable(bool isInteractable)
    {
        if (btnRobot != null)
        {
            btnRobot.interactable = isInteractable; // Tıklanabilirliği aç/kapat
        }

        // Eğer kilitliyorsak (false), açık olan menüleri de zorla kapatmalıyız
        if (!isInteractable)
        {
            // Menüyü ve panelleri kapat, Robotu susturma (İpucu kalacak)
            isMenuOpen = false;
            if (menuContainer) menuContainer.SetActive(false);
            if (menuOverlayButton) menuOverlayButton.gameObject.SetActive(false);
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (missionsPanel) missionsPanel.SetActive(false);
        }
    }
}