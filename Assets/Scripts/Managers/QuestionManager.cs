using UnityEngine;
using System.Collections.Generic;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager instance;

    [Header("İsim Havuzları")]
    public List<string> rawAntikaIsimler;
    public List<string> rawTeknolojiIsimler;
    public List<string> rawKuyumcuIsimler;
    public List<string> rawMorFilmler;
    public List<string> rawMorKitaplar;

    private string unluler = "aeıioöuüAEIİOÖUÜ";

    void Awake()
    {
        instance = this;
    }

    public void SoruOlusturVeSor(TileType type)
    {
        string esyaDetayi = "";    // Gramofon (4 Adet)
        string soruKurali = "";    // "Ünlü - Ünsüz işlemini yap"
        string baslik = "";        // KATEGORİ ADI
        int dogruCevap = 0;

        switch (type)
        {
            case TileType.Red:
                GenerateAntikaSoru(out esyaDetayi, out soruKurali, out dogruCevap);
                baslik = "HURDACI DÜKKANI";
                break;

            case TileType.Blue:
                GenerateTeknolojiSoru(out esyaDetayi, out soruKurali, out dogruCevap);
                baslik = "TEKNOLOJİ MAĞAZASI";
                break;

            case TileType.Yellow:
                GenerateKuyumcuSoru(out esyaDetayi, out soruKurali, out dogruCevap);
                baslik = "KUYUMCU";
                break;

            case TileType.Purple:
                GenerateMorSoru(out esyaDetayi, out soruKurali, out dogruCevap);
                baslik = "KÜTÜPHANE & SİNEMA";
                break;

            case TileType.Green:
                GenerateYesilSoru(out esyaDetayi, out soruKurali, out dogruCevap);
                baslik = "GİZLİ MESAJ";
                break;
                
            case TileType.Empty:
                return;
        }

        Debug.Log($"Soru: {esyaDetayi} | Cevap: {dogruCevap}");
        
        // GÜNCELLENMİŞ ÇAĞRI: 'soruKurali' parametresini de ekledik
        AnswerManager.instance.PaneliAc(baslik, esyaDetayi, soruKurali, 1, dogruCevap, type);
    }

    // --- 1. ANTİKA (KIRMIZI) ---
    void GenerateAntikaSoru(out string metin, out string kural, out int cevap)
    {
        string isim = RastgeleSec(rawAntikaIsimler);
        int miktar = Random.Range(1, 10);

        int unlu = HarfSay(isim, true);
        int unsuz = HarfSay(isim, false);

        if (miktar % 2 == 0)
        {
            cevap = unlu - unsuz;
            kural = "Miktar ÇİFT sayı.\nSORU: (Ünlü Harf Sayısı) - (Ünsüz Harf Sayısı) kaçtır?";
        }
        else
        {
            cevap = unsuz - unlu;
            kural = "Miktar TEK sayı.\nSORU: (Ünsüz Harf Sayısı) - (Ünlü Harf Sayısı) kaçtır?";
        }

        metin = $"{isim}\n(Miktar: {miktar})";
    }

    // --- 2. TEKNOLOJİ (MAVİ) ---
    void GenerateTeknolojiSoru(out string metin, out string kural, out int cevap)
    {
        string isim = RastgeleSec(rawTeknolojiIsimler);
        int sol = Random.Range(100, 999);
        int sag = Random.Range(10, 99);
        
        string fiyatStr = $"{sol}.{sag}";
        int solTop = RakamlariTopla(sol.ToString());
        int sagTop = RakamlariTopla(sag.ToString());

        cevap = solTop - sagTop;
        
        metin = $"{isim}\n(Fiyat: {fiyatStr} TL)";
        kural = "SORU: (Noktanın Solundaki Rakamların Toplamı) - (Sağındakilerin Toplamı)";
    }

    // --- 3. KUYUMCU (SARI) ---
    void GenerateKuyumcuSoru(out string metin, out string kural, out int cevap)
    {
        string isim = RastgeleSec(rawKuyumcuIsimler);
        int adet = Random.Range(2, 6);
        int birimFiyat = Random.Range(10, 50) * 10; 

        int toplamFiyat = adet * birimFiyat;
        int rakamToplami = RakamlariTopla(toplamFiyat.ToString());

        cevap = rakamToplami - 4;
        
        metin = $"{isim}\n({adet} Adet x {birimFiyat} TL)";
        kural = $"Toplam Fiyat: {toplamFiyat} TL ediyor.\nSORU: (Toplam Fiyatın Rakamları Toplamı) - 4";
    }

    // --- 4. MOR (FİLM & KİTAP) ---
    void GenerateMorSoru(out string metin, out string kural, out int cevap)
    {
        string film = RastgeleSec(rawMorFilmler);
        string kitap = RastgeleSec(rawMorKitaplar);
        int filmYili = Random.Range(1950, 2024);
        int kitapYili = Random.Range(1800, 1950);

        int fSon = filmYili % 10;
        int kSon = kitapYili % 10;

        cevap = fSon - kSon;
        
        metin = $"Film: {film} ({filmYili})\nKitap: {kitap} ({kitapYili})";
        kural = "SORU: (Film Yılının Son Rakamı) - (Kitap Yılının Son Rakamı)";
    }

    // --- 5. YEŞİL (MESAJ) ---
    void GenerateYesilSoru(out string metin, out string kural, out int cevap)
    {
        int senaryo = Random.Range(0, 3);
        int s1 = Random.Range(10, 50);
        int s2 = Random.Range(10, 50);
        int s3 = Random.Range(1, 10);

        if (senaryo == 0) 
        {
            cevap = s1 - s2 + s3;
            metin = $"Baba saat kaç?\n({s1}) - ({s2}) + ({s3})";
            kural = "SORU: Mesajdaki matematik işlemini çöz.";
        }
        else if (senaryo == 1) 
        {
            cevap = (-1 * s1) + (-1 * s2);
            metin = $"Plaka Şifresi:\nTersi(+{s1}) + Tersi({s2})";
            kural = "SORU: Sayıların toplama işlemine göre tersini alıp topla.\n(Örn: 5'in tersi -5 tir)";
        }
        else 
        {
            cevap = s1 + s2 - s3;
            metin = $"Cadde No:\n({s1}) + (+{s2}) - ({s3})";
            kural = "SORU: Verilen sayıları sırasıyla topla ve çıkar.";
        }
    }

    // --- YARDIMCI ARAÇLAR ---
    string RastgeleSec(List<string> liste)
    {
        if (liste == null || liste.Count == 0) return "Bilinmeyen Öğe";
        return liste[Random.Range(0, liste.Count)];
    }

    int RakamlariTopla(string s)
    {
        int t = 0;
        foreach (char c in s) if (char.IsDigit(c)) t += int.Parse(c.ToString());
        return t;
    }

    int HarfSay(string s, bool unluMu)
    {
        int sayi = 0;
        foreach (char c in s)
        {
            if (char.IsLetter(c))
            {
                bool isVowel = unluler.Contains(c);
                if (unluMu == isVowel) sayi++;
            }
        }
        return sayi;
    }
}