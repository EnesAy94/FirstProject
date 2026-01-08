using UnityEngine;

// Görev Tipleri: Hangi tür eylem gerekiyor?
public enum MissionType
{
    SolveRed,    // Hurdacı (Kırmızı) sorusu çöz
    SolveBlue,   // Teknoloji (Mavi) sorusu çöz
    SolveYellow, // Kuyumcu (Sarı) sorusu çöz
    SolvePurple, // Kitap/Film (Mor) sorusu çöz
    SolveGreen,  // Mesaj (Yeşil) sorusu çöz
    SolveAny     // Herhangi bir renk fark etmez
}

[CreateAssetMenu(fileName = "NewMission", menuName = "StorySystem/Mission")]
public class MissionData : ScriptableObject
{
    [Header("Görev Bilgileri")]
    public string description; // Örn: "3 tane antika eşya topla"
    public bool isMainMission; // Tikliyse ANA GÖREV, değilse YAN GÖREV
    
    [Header("Hedef Ayarları")]
    public MissionType type;   // Görevin türü ne?
    public int targetAmount;   // Kaç tane yapılması lazım? (Örn: 3)
    
    [Header("Görsel")]
    public Sprite icon; // Görev listesinde gözükecek küçük ikon

    [HideInInspector] public int currentProgress = 0;
}