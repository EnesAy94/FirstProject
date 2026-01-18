using System.Collections.Generic;

[System.Serializable] // Bu satır çok önemli! Verinin kaydedilebilir olduğunu söyler.
public class PlayerData
{
    // Temel Veriler
    public int totalScore;
    public int maxLevelReached;
    
    // Oyun Ayarları (Ses vs.)
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    // Listeler (Firebase bunları çok sever)
    // Hangi başarımları kazandı? ID'leri tutuyoruz.
    public List<string> earnedAchievements = new List<string>();

    // Hangi görevler bitti? ID'leri tutuyoruz.
    public List<string> completedMissions = new List<string>();

    // Başarım ilerlemeleri (Örn: "hard_master" -> 5 tane çözdü)
    // Dictionary Unity'de direkt serileşmez, basit bir struct listesi yapalım:
    public List<ProgressData> achievementProgress = new List<ProgressData>();
}

[System.Serializable]
public struct ProgressData
{
    public string id;
    public int amount;
}