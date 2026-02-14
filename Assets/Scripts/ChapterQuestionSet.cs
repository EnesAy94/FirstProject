using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChapterQuestions", menuName = "Quiz System/Chapter Question Set")]
public class ChapterQuestionSet : ScriptableObject
{
    public string chapterName;

    // --- MEVCUT HİKAYELİ SORULAR (İlk 5 Sefer) ---
    [Header("🟥 Kırmızı - Hikayeli (Normal)")]
    public List<QuestionTemplate> redTemplates;
    [Header("🟥 Kırmızı - Zor (Yedek)")]
    public List<QuestionTemplate> redHardTemplates; // YENİ

    [Header("🟦 Mavi - Hikayeli (Normal)")]
    public List<QuestionTemplate> blueTemplates;
    [Header("🟦 Mavi - Zor (Yedek)")]
    public List<QuestionTemplate> blueHardTemplates; // YENİ

    [Header("🟨 Sarı - Hikayeli (Normal)")]
    public List<QuestionTemplate> yellowTemplates;
    [Header("🟨 Sarı - Zor (Yedek)")]
    public List<QuestionTemplate> yellowHardTemplates; // YENİ

    [Header("🟪 Mor - Hikayeli (Normal)")]
    public List<QuestionTemplate> purpleTemplates;
    [Header("🟪 Mor - Zor (Yedek)")]
    public List<QuestionTemplate> purpleHardTemplates; // YENİ

    [Header("🟩 Yeşil - Hikayeli (Normal)")]
    public List<QuestionTemplate> greenTemplates;
    [Header("🟩 Yeşil - Zor (Yedek)")]
    public List<QuestionTemplate> greenHardTemplates; // YENİ

    [Header("🟧 Turuncu - Hikayeli (Normal)")]
    public List<QuestionTemplate> orangeTemplates;
    [Header("🟧 Turuncu - Zor (Yedek)")]
    public List<QuestionTemplate> orangeHardTemplates; // YENİ

    [Header("💀 Riskli Alan (Gerçek Zor Sorular)")]
    public List<QuestionTemplate> hardTemplates;
}