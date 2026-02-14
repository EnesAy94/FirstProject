using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LocationInfoPanel : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Image locationIconImage;
    public TextMeshProUGUI locationNameText;
    public TextMeshProUGUI descriptionText;
    public Button continueButton;
    public GameObject panelObj; // Panelin kendisi (Aç/Kapa için)

    private Action onContinueCallback; // Devam butonuna basınca ne olacak?

    void Start()
    {
        // Butona tıklanınca yapılacaklar
        continueButton.onClick.AddListener(() =>
        {
            panelObj.SetActive(false); // Paneli kapat
            onContinueCallback?.Invoke(); // Sıradaki işleme (Soru sormaya) geç
        });
    }

    public void ShowLocationCard(LocationStoryInfo info, Action onContinue)
    {
        // 1. Bilgileri UI'ya doldur
        if (locationNameText) locationNameText.text = info.locationName;
        if (descriptionText) descriptionText.text = info.introDescription;

        if (info.locationIcon != null && locationIconImage != null)
        {
            locationIconImage.sprite = info.locationIcon;
            locationIconImage.gameObject.SetActive(true);
        }
        else if (locationIconImage != null)
        {
            locationIconImage.gameObject.SetActive(false);
        }

        // 2. Devam edilince ne olacağını kaydet
        onContinueCallback = onContinue;

        // 3. Paneli Aç
        panelObj.SetActive(true);
    }
}