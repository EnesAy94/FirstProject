using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ProfileUI : MonoBehaviour
{
    [Header("--- GİRİŞ ALANLARI (INPUTS) ---")]
    // Artık Text değil, InputField kullanıyoruz
    public TMP_InputField nameInput;
    public TMP_InputField surnameInput;
    public TMP_InputField nicknameInput;

    [Header("--- İSTATİSTİK YAZILARI ---")]
    public TextMeshProUGUI accuracyRateText;
    public TextMeshProUGUI hardSuccessText;
    public TextMeshProUGUI penaltyVisitsText;
    public TextMeshProUGUI longestStreakText;
    public TextMeshProUGUI totalPointText; // Toplam Puan

    [Header("--- BAŞARIM LİSTESİ ---")]
    public GameObject achievementItemPrefab;
    public Transform allContent;

    // Sıralama sınıfı (Aynı kalıyor)
    private class AchievementSortData
    {
        public AchievementData data;
        public int currentCount;
        public int tierIndex;
        public bool isUnlocked;
    }

    void OnEnable()
    {
        LoadProfileData(); // Verileri kutulara doldur
        UpdateStatTexts(); // İstatistikleri hesapla
        RefreshAchievements(); // Başarımları listele

        // Inputlar değiştiğinde kaydetmesi için dinleyiciler ekle
        AddInputListeners();
    }

    void OnDisable()
    {
        // Panel kapanırken son hali garanti olsun diye kaydedelim
        SaveCurrentInputs();
    }

    // --- PROFİL VERİLERİNİ YÜKLE VE DİNLE ---
    void LoadProfileData()
    {
        PlayerData data = SaveManager.instance.activeSave;

        // Kayıtlı isimleri kutulara yaz
        if (nameInput != null) nameInput.text = data.playerName;
        if (surnameInput != null) surnameInput.text = data.playerSurname;
        if (nicknameInput != null) nicknameInput.text = data.playerNickname;
    }

    void AddInputListeners()
    {
        // Kullanıcı yazmayı bitirince (Enter veya Tıkla Çık) otomatik kaydet
        // Her seferinde temizleyip ekliyoruz ki üst üste binmesin
        if (nameInput != null)
        {
            nameInput.onEndEdit.RemoveAllListeners();
            nameInput.onEndEdit.AddListener(delegate { SaveCurrentInputs(); });
        }
        if (surnameInput != null)
        {
            surnameInput.onEndEdit.RemoveAllListeners();
            surnameInput.onEndEdit.AddListener(delegate { SaveCurrentInputs(); });
        }
        if (nicknameInput != null)
        {
            nicknameInput.onEndEdit.RemoveAllListeners();
            nicknameInput.onEndEdit.AddListener(delegate { SaveCurrentInputs(); });
        }
    }

    // UI'daki yazıları SaveManager'a gönder
    public void SaveCurrentInputs()
    {
        string n = (nameInput != null) ? nameInput.text : "";
        string s = (surnameInput != null) ? surnameInput.text : "";
        string nick = (nicknameInput != null) ? nicknameInput.text : "";

        SaveManager.instance.SaveProfileInfo(n, s, nick);
    }

    // --- İSTATİSTİK YAZILARI (Aynı) ---
    void UpdateStatTexts()
    {
        PlayerData data = SaveManager.instance.activeSave;

        // İstatistik hesaplamaları (Aynı kalıyor...)
        int totalNormal = data.normalCorrectCount + data.normalWrongCount;
        float accuracy = (totalNormal > 0) ? ((float)data.normalCorrectCount / totalNormal) * 100f : 0;
        accuracyRateText.text = "%" + accuracy.ToString("F0");

        int totalHard = data.hardCorrectCount + data.hardWrongCount;
        float hardSuccess = (totalHard > 0) ? ((float)data.hardCorrectCount / totalHard) * 100f : 0;
        hardSuccessText.text = "%" + hardSuccess.ToString("F0");

        int totalPenalty = data.penaltyCorrectCount + data.penaltyWrongCount;
        float penaltySuccess = (totalPenalty > 0) ? ((float)data.penaltyCorrectCount / totalPenalty) * 100f : 0;
        penaltyVisitsText.text = "%" + penaltySuccess.ToString("F0");

        longestStreakText.text = data.maxStreak.ToString();

        // TOPLAM PUAN (Tüm bölümlerin toplamı)
        if (totalPointText != null)
            totalPointText.text = data.totalScore.ToString();
    }

    // --- BAŞARIM LİSTESİ (Aynı) ---
    public void RefreshAchievements()
    {
        foreach (Transform child in allContent) Destroy(child.gameObject);
        List<AchievementSortData> sortList = new List<AchievementSortData>();

        foreach (AchievementData ach in AchievementManager.instance.allAchievements)
        {
            var status = AchievementManager.instance.GetAchievementStatus(ach.id);
            AchievementSortData item = new AchievementSortData();
            item.data = ach;
            item.currentCount = status.currentCount;
            item.tierIndex = status.currentTierIndex;
            item.isUnlocked = (status.currentTierIndex != -1);
            sortList.Add(item);
        }

        var sortedList = sortList
            .OrderByDescending(x => x.isUnlocked)
            .ThenByDescending(x => x.tierIndex)
            .ToList();

        foreach (var item in sortedList)
        {
            GameObject newItem = Instantiate(achievementItemPrefab, allContent);
            newItem.GetComponent<AchievementItemUI>().Setup(item.data, item.currentCount, item.tierIndex);
        }
    }
}