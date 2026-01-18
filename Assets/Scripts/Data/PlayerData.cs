using System.Collections.Generic;

[System.Serializable] // Bu satır çok önemli! Verinin kaydedilebilir olduğunu söyler.
public class PlayerData
{
    // Temel Veriler
    public int totalScore;
    public int maxLevelReached;

    // 1. Genel Başarı (Zor olmayanlar)
    public int normalCorrectCount; // Doğru normal soru
    public int normalWrongCount;   // Yanlış normal soru

    // 2. Zor Soru Başarısı
    public int hardCorrectCount;
    public int hardWrongCount;

    // 3. Ceza Alanı
    public int penaltyCorrectCount; // Cezada kaç doğru yaptı?
    public int penaltyWrongCount;   // Cezada kaç yanlış yaptı?

    // 4. Streak (Seri) Bilgisi
    public int currentStreak; // Şu anki seri (Hata yapana kadar artar)
    public int maxStreak;     // En yüksek rekor seri

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