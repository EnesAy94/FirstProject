using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement System/Achievement")]
public class AchievementData : ScriptableObject
{
    public string id; // Örn: "hard_master" (Kodda bunu kullanacağız)
    public string title; // Örn: "Zor Soruların Efendisi"
    public string description; // Örn: "Zor soruları çözerek ustalığını kanıtla."
    
    [Header("Gereksinimler")]
    public bool requiresMission; // Bu başarım bir yan göreve bağlı mı?
    public string requiredMissionKey; // Örn: "Mission_HardQuestion_Completed" (PlayerPrefs anahtarı)

    [Header("Seviyeler (Bronz -> Silver -> Gold)")]
    public List<AchievementTier> tiers; // Buraya 3 tane ekleyeceksin (Bronz, Gümüş, Altın)
}

[System.Serializable]
public struct AchievementTier
{
    public string tierName; // Bronz, Gümüş, Altın
    public int targetCount; // Kaç tane yapınca açılır? (Örn: 1, 10, 50)
    public Sprite badgeIcon; // O seviyenin ikonu
}