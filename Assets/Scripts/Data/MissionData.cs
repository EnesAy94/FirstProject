using UnityEngine;

// Görev Tipleri: Hangi tür eylem gerekiyor?
public enum MissionType
{
    SolveRed,
    SolveBlue,
    SolveYellow,
    SolvePurple,
    SolveOrange,
    SolveGreen,
    SolveAny,
    SolveHard,
    CompleteLapNoError,
    CompleteLapNoJoker,
    CompleteLevelNoJoker
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

    [Header("Başarım Kilidi (Varsa)")]
    public string unlockAchievementKey;

    [HideInInspector] public int currentProgress = 0;
}