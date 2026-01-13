using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChapter", menuName = "StorySystem/Chapter")]
public class ChapterData : ScriptableObject
{
    [Header("Bölüm Kimliği")]
    public int chapterID;       // 1, 2, 3...
    public string chapterName;  // Örn: "Bölüm 1: İlk İpucu"
    public ChapterData nextChapter;
    [TextArea]
    public string introText;    // Bölüm başlarken çıkacak hikaye yazısı

    [Header("Bu Bölümün Görevleri")]
    public List<MissionData> missions; // İçine görev dosyalarını sürükleyip bırakacağız

    [Header("Zorluk Ayarları")]
    public int startingScore = 100; // Başlangıç puanı (Genelde 100)
    public int penaltyPerWrongAnswer = 10; // Yanlış yapınca kaç puan düşsün?
}