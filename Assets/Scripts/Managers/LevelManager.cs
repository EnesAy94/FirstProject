using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance; // Diğer scriptlerden ulaşmak için

    [Header("Mevcut Durum")]
    public ChapterData currentChapter; // Şu an oynanan bölüm verisi
    public List<MissionData> activeMissions; // Şu anki aktif görevler

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 1. Menüden gelen veriyi kontrol et
        if (GameSession.activeChapter != null)
        {
            currentChapter = GameSession.activeChapter;
            Debug.Log("📘 BÖLÜM YÜKLENDİ: " + currentChapter.chapterName);
            
            // 2. Bölümü Başlat
            StartChapter();
        }
        else
        {
            Debug.LogWarning("⚠️ Uyarı: Menüden bölüm seçilmedi! Test için varsayılan bir bölüm atayın.");
            // İstersen buraya test için elle bir ChapterData atayabilirsin.
        }
    }

    void StartChapter()
    {
        // Görev listesini sıfırla ve yenilerini ekle
        activeMissions = new List<MissionData>();

        foreach (MissionData mission in currentChapter.missions)
        {
            // Orijinal veriyi bozmamak için kopyasını oluşturuyoruz (Instantiate)
            // Böylece "3 tane topla" verisi azalırken orijinal dosya bozulmaz.
            MissionData missionCopy = Instantiate(mission);
            activeMissions.Add(missionCopy);
            
            Debug.Log("   🔸 Görev Eklendi: " + missionCopy.description);
        }

        // İleride buraya "UI'yı Güncelle" kodu gelecek (Adım 4)
    }
}