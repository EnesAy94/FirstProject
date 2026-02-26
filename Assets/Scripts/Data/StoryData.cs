using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewStory", menuName = "StorySystem/Story")]
public class StoryData : ScriptableObject
{
    public string storyTitle; // Örn: "Matematik Dedektifi"
    public string storyDescription;
    public Sprite storyImage; // YENİ: Bu hikayenin genel kapak resmi
    public bool isComingSoon; // YENİ: Bu hikaye yakında eklenecekse true yap
    
    [Header("Bölüm Listesi")]
    public List<ChapterData> chapters; // Bölüm 1, Bölüm 2... sırayla buraya gelecek
}