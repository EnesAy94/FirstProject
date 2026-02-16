using System.Collections.Generic;

[System.Serializable] // Bu satır çok önemli! Verinin kaydedilebilir olduğunu söyler.
public class PlayerData
{
    // --- TEMEL VERİLER ---
    public int totalScore;
    public int maxLevelReached;

    // --- YENİ EKLENEN: CÜZDAN / EKONOMİ ---
    // Şans Çarkı için kazanılan biletler burada tutulacak.
    public int globalTicketCount = 0;
    // --------------------------------------

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
    public int lastLostStreak = 0; // Jokerle kurtarılacak seri

    // --- PROFİL BİLGİLERİ ---
    public string playerName = "";
    public string playerSurname = "";
    public string playerNickname = "";

    // --- OYUN AYARLARI ---
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    // --- İLERLEME LİSTELERİ ---
    public int lastUnlockedLevel = 1; // Varsayılan 1. bölüm açık

    // Hangi bölümden kaç puan aldı?
    public List<LevelScoreData> levelBestScores = new List<LevelScoreData>();

    // Görev ilerlemeleri
    public List<MissionProgressSave> missionProgresses = new List<MissionProgressSave>();

    // Çözülen soruların kaydı (Tekrar sormamak için)
    public List<UsedQuestionData> usedQuestions = new List<UsedQuestionData>();

    // Kazanılan başarımlar (Achievement ID'leri)
    public List<string> earnedAchievements = new List<string>();

    // Tamamlanan ana bölümler
    public List<int> completedMainChapters = new List<int>();

    // Tamamlanan görevler
    public List<string> completedMissions = new List<string>();

    // Başarım ilerlemeleri (Örn: "hard_master" -> 5 tane çözdü)
    public List<ProgressData> achievementProgress = new List<ProgressData>();
}

// --- YARDIMCI YAPILAR (STRUCTS) ---

[System.Serializable]
public struct ProgressData
{
    public string id;
    public int amount;
}

[System.Serializable]
public struct LevelScoreData
{
    public int chapterID;
    public int bestScore;
}

// Görev ilerlemesini tutan küçük yapı
[System.Serializable]
public struct MissionProgressSave
{
    public int chapterID;
    public int missionIndex; // O bölümdeki kaçıncı görev?
    public int progress;     // Kaç tanesini yaptı?
}

[System.Serializable]
public class UsedQuestionData
{
    public int chapterID;
    public TileType color;
    public List<int> usedStoryIndices = new List<int>(); // Çözülen hikayeli sorular
    public List<int> usedHardIndices = new List<int>();  // Çözülen zor sorular
}