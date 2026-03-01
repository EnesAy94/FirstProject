using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewStory", menuName = "StorySystem/Story")]
public class StoryData : ScriptableObject
{
    [Header("Türkçe Metinler")]
    public string storyTitle; // Örn: "Matematik Dedektifi"
    public string storyDescription;

    [Header("İngilizce Metinler")]
    public string storyTitleEN; // Örn: "Math Detective"
    public string storyDescriptionEN;
    public Sprite storyImage; // YENİ: Bu hikayenin genel kapak resmi
    public bool isComingSoon; // YENİ: Bu hikaye yakında eklenecekse true yap
    
    [Header("Bölüm Listesi")]
    public List<ChapterData> chapters; // Bölüm 1, Bölüm 2... sırayla buraya gelecek
}