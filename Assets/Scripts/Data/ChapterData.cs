using UnityEngine;
using System.Collections.Generic;

// --- YENİ EKLENEN YAPI (Mekan Hikayeleri) ---
// Bu kısmı ChapterData class'ının dışına (üstüne) koyabilirsin
[System.Serializable]
public struct LocationStoryInfo
{
    public TileType tileType;       // Hangi renk?
    public string locationName;     // Mekan İsmi
    public Sprite locationIcon;     // İkon

    [TextArea(3, 5)]
    public string introDescription; // Karta gelince çıkan yazı (Robot BURADA SUSACAK)

    [Header("Robot İpucu")]
    [TextArea(2, 4)]
    public string robotHint;        // YENİ: Soru Paneli açılınca Robotun söyleyeceği söz

    [Header("Sonuç Mesajları")]
    [TextArea(2, 4)]
    public string successMessage;
    [TextArea(2, 4)]
    public string failMessage;
}

// --- SENİN MEVCUT CHAPTER DATA CLASS'IN (GÜNCELLENMİŞ HALİ) ---
[CreateAssetMenu(fileName = "NewChapter", menuName = "StorySystem/Chapter")]
public class ChapterData : ScriptableObject
{
    [Header("Bölüm Kimliği")]
    public int chapterID;       // 1, 2, 3...
    public string chapterName;  // Örn: "Bölüm 1: İlk İpucu"
    public ChapterData nextChapter;
    [TextArea]
    public string introText;    // Bölüm başlarken çıkacak hikaye yazısı

    [Header("Soru Bankası")]
    public ChapterQuestionSet questionSet;

    [Header("Bu Bölümün Görevleri")]
    public List<MissionData> missions; // İçine görev dosyalarını sürükleyip bırakacağız

    [Header("Zorluk Ayarları")]
    public int startingScore = 100; // Başlangıç puanı (Genelde 100)
    public int penaltyPerWrongAnswer = 10; // Yanlış yapınca kaç puan düşsün?

    // --- YENİ EKLENEN LİSTE BURASI ---
    [Header("Mekan Hikayeleri")]
    public List<LocationStoryInfo> locationStories;

    // --- YENİ YARDIMCI FONKSİYON ---
    // Renge göre o mekanın bilgilerini bulup getirir
    public LocationStoryInfo GetStoryInfo(TileType type)
    {
        // Listede bu renge ait bir hikaye var mı diye arar
        return locationStories.Find(x => x.tileType == type);
    }
}