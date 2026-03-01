using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [Header("Dil Ayarı")]
    [Tooltip("Bu yazının LocalizationManager'daki arama anahtarı (Örn: 'menu_story')")]
    public string localizationKey;

    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        // Sahne açıldığında anında çeviri yap (Eğer LocalizationManager varsa)
        if (LocalizationManager.instance != null)
        {
            UpdateText();
            // Dil değiştiği an bu fonksiyona da haber gelmesini sağla (Abone (Subscribe) ol)
            LocalizationManager.instance.OnLanguageChanged += UpdateText;
        }
        else
        {
            Debug.LogWarning($"LocalizationManager sahnede bulunamadı. '{gameObject.name}' çevirilemeyecek.");
        }
    }

    void OnDestroy()
    {
        // Obje silinirken abonelikten çık (Hafıza sızıntısını ve hataları önler)
        if (LocalizationManager.instance != null)
        {
            LocalizationManager.instance.OnLanguageChanged -= UpdateText;
        }
    }

    // Ana fonksyion: O anki dile göre yazıyı değiştir
    private void UpdateText()
    {
        if (string.IsNullOrEmpty(localizationKey)) return;

        string translatedText = LocalizationManager.instance.GetText(localizationKey);
        
        // Eğer bu obje içinde ekstra statik bir yazı varsa (Örn: DİKKAT: [ÇEVİRİ]), onu ezebilir. 
        // Şimdilik direkt çeviriyi atıyoruz.
        textComponent.text = translatedText;
    }
}
