using UnityEngine;
using System.Collections.Generic;
using System;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager instance;

    // Herhangi bir yazının dili değiştiğinde uyarılmasını sağlayacak olay (Event)
    public event Action OnLanguageChanged;

    [Header("Ayarlar")]
    public string currentLanguage = "TR"; // "TR" veya "EN"

    // Türkçe Sözlük
    private Dictionary<string, string> dictTR = new Dictionary<string, string>()
    {
        {"menu_story", "HİKAYE DOSYALARI"},
        {"menu_multiplayer", "ARKADAŞLARLA OYNA"},
        {"menu_profile", "AJAN KİMLİĞİ"},
        {"menu_settings", "AYARLAR"},
        {"profile_title", "AJAN KİMLİĞİ"},
        {"settings_title", "SİSTEM AYARLARI"},
        {"settings_sound", "SES"},
        {"settings_language", "DİL"},
        {"settings_google", "HESAP BAĞLA"},
        {"settings_english", "İNGİLİZCE"},
        {"settings_turkish", "TÜRKÇE"},
        {"coming_soon", "YAKINDA"},
        {"chapter_completed", "BÖLÜM TAMAMLANDI"},
        {"achievement_locked_status", "DURUM: KİLİTLİ"},
        {"achievement_progress", "İLERLEME:"},
        {"general_back", "ÇIKIŞ"},
        // Profil Paneli Kelimeleri
        {"profile_header", "PROFİL"},
        {"profile_name", "İSİM :"},
        {"profile_surname", "SOYİSİM :"},
        {"profile_nickname", "TAKMA İSİM :"},
        {"profile_point", "PUAN :"},
        {"profile_value", "Yazınız..."},
        
        {"stats_menu", "İSTATİSTİKLER"},
        {"stat_accuracy", "DOĞRULUK ORANI:"},
        {"stat_hard", "ZOR SORU BAŞARISI:"},
        {"stat_penalty", "CEZA BAŞARISI:"},
        {"stat_streak", "EN UZUN DOĞRU SERİSİ:"},
        
        {"achievement_header", "BAŞARIMLAR"},
        {"achievement_ongoing", "DURUM: DEVAM EDİYOR"},
        {"achievement_completed", "DURUM: TAMAMLANDI"},
        {"achievement_max", "İLERLEME: MAKSİMUM"},

        {"popup_coming_soon", "YAKINDA EKLENECEK!"},

        // SelectStory and ChapterSelect Panel (TR)
        {"selectstory_title", "BİR DAVA DOSYASI SEÇ"},
        {"chapterselect_title", "BİR BÖLÜM SEÇ"},
        {"chapterselect_back", "GERİ"},

        //Story and Chapter Card
        {"story_card_score", "DOSYA PUANI"},
        {"chapter_card_score", "BÖLÜM PUANI"},
        {"chapter_card_select", "SEÇ"},
        {"chapter_card_play", "OYNA"},
        
        // İleride buraya oyun içi kelimeleri de (Örn: "Doğru", "Yanlış") ekleyeceğiz.
    };

    // İngilizce Sözlük
    private Dictionary<string, string> dictEN = new Dictionary<string, string>()
    {
        {"menu_story", "STORY CASES"},
        {"menu_multiplayer", "PLAY WITH FRIENDS"},
        {"menu_profile", "AGENT ID"},
        {"menu_settings", "SETTINGS"},
        {"profile_title", "AGENT ID"},
        {"settings_title", "SYSTEM SETTINGS"},
        {"settings_sound", "AUDIO"},
        {"settings_language", "LANGUAGE"},
        {"settings_google", "LINK ACCOUNT"},
        {"settings_english", "ENGLISH"},
        {"settings_turkish", "TURKISH"},
        {"coming_soon", "COMING SOON"},
        {"chapter_completed", "CHAPTER CLEARED"},
        {"achievement_locked_status", "STATUS: LOCKED"},
        {"achievement_progress", "PROGRESS:"},
        {"general_back", "EXIT"},

        // Profil Paneli Kelimeleri (EN)
        {"profile_header", "AGENT ID"},
        {"profile_name", "NAME :"},
        {"profile_surname", "SURNAME :"},
        {"profile_nickname", "NICKNAME :"},
        {"profile_point", "POINT :"},
        {"profile_value", "Enter text..."},
        
        {"stats_menu", "STATS MENU"},
        {"stat_accuracy", "ACCURACY RATE:"},
        {"stat_hard", "HARD QUESTION SUCCESS RATE:"},
        {"stat_penalty", "PENALTY SUCCESS RATE:"},
        {"stat_streak", "LONGEST CORRECT STREAK:"},
        
        {"achievement_header", "ACHIEVEMENTS"},
        {"achievement_ongoing", "STATUS: IN PROGRESS"},
        {"achievement_completed", "STATUS: COMPLETED"},
        {"achievement_max", "PROGRESS: MAXIMUM"},

        {"popup_coming_soon", "COMING SOON!"},

        // SelectStory and ChapterSelect Panel (EN)
        {"selectstory_title", "CHOOSE A CASE FILE"},
        {"chapterselect_title", "CHOOSE A CHAPTER"},
        {"chapterselect_back", "BACK"},

        //Story and Chapter Card
        {"story_card_score", "FILE SCORE"},
        {"chapter_card_score", "CHAPTER SCORE"},
        {"chapter_card_select", "SELECT"},
        {"chapter_card_play", "PLAY"},
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değişse de silinmesin
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Kaydedilmiş dili (PlayerPrefs) yükle. Yoksa varsayılan TR olsun.
        currentLanguage = PlayerPrefs.GetString("GameLanguage", "TR");
    }

    // Dili değiştirir ve oyundaki tüm yazılara "Güncellenin" diye haber verir.
    public void SetLanguage(string langCode)
    {
        if (langCode != "TR" && langCode != "EN") return;

        currentLanguage = langCode;
        PlayerPrefs.SetString("GameLanguage", currentLanguage);
        PlayerPrefs.Save();

        // Olay (Event) fırlatılarak bu olaya abone olan (LocalizedText) scriptlerinin çalışması sağlanır
        OnLanguageChanged?.Invoke();
    }

    // İstenilen anahtar kelimenin (Key) o anki dildeki karşılığını verir.
    public string GetText(string key)
    {
        Dictionary<string, string> targetDict = (currentLanguage == "TR") ? dictTR : dictEN;

        if (targetDict.ContainsKey(key))
        {
            return targetDict[key];
        }
        else
        {
            // Eğer çeviri bulunamazsa kırmızı renkli bir uyarı metni gösterilir (Geliştirici bulabilsin diye)
            return $"<color=red>!{key}!</color>";
        }
    }
}
