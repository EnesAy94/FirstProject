using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChapterQuestions", menuName = "Quiz System/Chapter Question Set")]
public class ChapterQuestionSet : ScriptableObject
{
    public string chapterName;

    [Header("🟥 Kırmızı Bölge Şablonları")]
    public List<QuestionTemplate> redTemplates;

    [Header("🟦 Mavi Bölge Şablonları")]
    public List<QuestionTemplate> blueTemplates;

    [Header("🟨 Sarı Bölge Şablonları")]
    public List<QuestionTemplate> yellowTemplates;

    [Header("🟪 Mor Bölge Şablonları")]
    public List<QuestionTemplate> purpleTemplates;

    [Header("🟩 Yeşil Bölge Şablonları")]
    public List<QuestionTemplate> greenTemplates;

    [Header("🟧 Turuncu Bölge Şablonları")]
    public List<QuestionTemplate> orangeTemplates; // Yeni Renk

    [Header("💀 Zor Sorular")]
    public List<QuestionTemplate> hardTemplates;
}