using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elemanları - Ses")]
    [Tooltip("Sesi seviyesini ayarlayan Slider (0 ile 1 arası olmalı)")]
    public Slider volumeSlider;
    [Tooltip("Sesi tamamen kapatmak/açmak için buton")]
    public Button soundButton;
    
    [Tooltip("Ses ikonunun bulunduğu Image bileşeni")]
    public Image soundIconImage;
    [Tooltip("Ses açıkken gösterilecek İkon (Sprite)")]
    public Sprite soundOnSprite;
    [Tooltip("Ses kapalıyken gösterilecek İkon (Sprite)")]
    public Sprite soundOffSprite;

    [Tooltip("Türkçe Dili Toggle (Seçeneği)")]
    public Toggle langTRToggle;
    [Tooltip("Türkçe Butonunun İçindeki Tik İşareti (Toggle'ın Checkmark objesi)")]
    public GameObject langTRCheckmark;

    [Tooltip("İngilizce Dili Toggle (Seçeneği)")]
    public Toggle langENToggle;
    [Tooltip("İngilizce Butonunun İçindeki Tik İşareti (Toggle'ın Checkmark objesi)")]
    public GameObject langENCheckmark;
    
    [Tooltip("Google Play Button (Placeholder)")]
    public Button googleLoginButton;

    // İç Değişkenler
    private float currentVolume = 1f;
    private float lastUnmutedVolume = 1f; // Sesi kapattığımızda önceki ses seviyesini hatırlamak için

    void Start()
    {
        LoadSettings();
        UpdateUI();

        // Buton Dinleyicileri (Listeners)
        if (soundButton != null) soundButton.onClick.AddListener(ToggleMute);
        
        // Slider değiştiğinde dinamik olarak sesi ayarla
        if (volumeSlider != null) 
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        // Toggle Dinleyicileri (Seçim kutusuna tıklandığında)
        if (langTRToggle != null) 
        {
            langTRToggle.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) ChangeLanguage("TR"); 
            });
        }
        
        if (langENToggle != null) 
        {
            langENToggle.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) ChangeLanguage("EN"); 
            });
        }

        if (googleLoginButton != null) googleLoginButton.onClick.AddListener(OnClick_GoogleLogin);
    }

    // --- YÜKLEME VE GÜNCELLEME ---

    private void LoadSettings()
    {
        // Kaydedilmiş sesi yükle (Varsayılan 1f)
        currentVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        
        // Eğer ses 0'sa son açık seviyeyi hafızada 1 tut, yoksa kendisini tut
        lastUnmutedVolume = currentVolume > 0f ? currentVolume : 1f;

        // Slider değerini koddan ayarla (Bu otomatik olarak OnVolumeSliderChanged fırlatır ama 
        // biz yine de manuel Apply çağırıyoruz ki garanti olsun)
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
        }

        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        // Ana sesi direkt 0 ile 1 arasına ayarlar
        AudioListener.volume = currentVolume;
    }

    private void UpdateUI()
    {
        // 1. SES İKONUNU DEĞİŞTİR (Sprite Swap)
        if (soundIconImage != null)
        {
            if (currentVolume <= 0f && soundOffSprite != null)
            {
                soundIconImage.sprite = soundOffSprite;
            }
            else if (currentVolume > 0f && soundOnSprite != null)
            {
                soundIconImage.sprite = soundOnSprite;
            }
        }

        // 2. DİL TOGGLE'LARI VE TİKLER (CHECKMARKS)
        if (LocalizationManager.instance != null)
        {
            string currentLang = LocalizationManager.instance.currentLanguage;
            
            // Koddan toggle'ların durumunu işaretle 
            // Döngüye girmemesi için SetIsOnWithoutNotify kullanıyoruz
            if (langTRToggle != null) langTRToggle.SetIsOnWithoutNotify(currentLang == "TR");
            if (langENToggle != null) langENToggle.SetIsOnWithoutNotify(currentLang == "EN");

            // Eğer Toggle'ların kendi hazır "Checkmark" resimlerini kullanmıyorsanız (El ile objeyi atıyorsanız):
            if (langTRCheckmark != null) langTRCheckmark.SetActive(currentLang == "TR");
            if (langENCheckmark != null) langENCheckmark.SetActive(currentLang == "EN");
        }
    }

    // --- BUTON VE SLIDER İŞLEMLERİ ---

    public void OnVolumeSliderChanged(float value)
    {
        currentVolume = value;

        // Eğer sesi sıfırdan büyük bir sayıya çektiysek onu hafızaya al ki 
        // Mute butonuna basınca geri getirebilelim.
        if (currentVolume > 0f)
        {
            lastUnmutedVolume = currentVolume;
        }

        PlayerPrefs.SetFloat("GameVolume", currentVolume);
        PlayerPrefs.Save();

        ApplyAudioSettings();
        
        // Çarpı ikonunu güncellemek için
        UpdateUI();
    }

    public void ToggleMute()
    {
        // Eğer ses zaten 0'sa (Sessizdeyse), eski sese geri dön
        if (currentVolume <= 0f)
        {
            currentVolume = lastUnmutedVolume;
        }
        else // Ses açıksa komple kapat (0 yap)
        {
            currentVolume = 0f;
        }

        // Slideri kodla güncelle. Slider, kendi OnValueChanged eventini otomatik 
        // çağıracağı için kaydetme ve UpdateUI işlerini kendi halledecektir.
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
        }
        else
        {
            // Slider objesi eklenmemişse manuel kaydet
            PlayerPrefs.SetFloat("GameVolume", currentVolume);
            PlayerPrefs.Save();
            ApplyAudioSettings();
            UpdateUI();
        }
        
        Debug.Log("Sesi Sustur/Aç Butonuna Basıldı, Ses: " + currentVolume);
    }

    public void ChangeLanguage(string targetLang)
    {
        if (LocalizationManager.instance != null)
        {
            // Sadece eğer dil HARBİDEN değişiyorsa işlem yap ki sonsuz döngü olmasın.
            if (LocalizationManager.instance.currentLanguage != targetLang)
            {
                LocalizationManager.instance.SetLanguage(targetLang);
                UpdateUI();
                Debug.Log("Dil Değiştirildi: " + targetLang);
            }
        }
    }

    public void OnClick_GoogleLogin()
    {
        // Şu anlık sadece görsel hazırlığı olduğu için oyuncuyu uyaracağız.
        Debug.LogWarning("Google Play Services bağlantısı test sürümünde olduğu için henüz aktif değildir.");
    }
}
