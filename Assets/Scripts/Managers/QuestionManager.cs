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

            case TileType.Hard:
                GenerateHardSoru(out esyaDetayi, out soruKurali, out dogruCevap);
                baslik = "⚠️ RİSKLİ BÖLGE ⚠️"; // Başlık dikkat çekici olsun
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
        }
        else
        {
            cevap = unsuz - unlu;
        }

        metin = $"{isim} (Miktar: {miktar})";
        kural = "Sayı Tek ise : (Ünsüz Harf Sayısı) - (Ünlü Harf Sayısı) kaçtır?\nSayı Çift ise : (Ünlü Harf Sayısı) - (Ünsüz Harf Sayısı) kaçtır?";
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

        metin = $"{isim} ({adet} Adet x {birimFiyat} TL)";
        kural = $"Toplam Fiyat: (Adet x birim fiyat) TL ediyor.\nSORU: (Toplam Fiyatın Rakamları Toplamı) - 4";
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
            kural = "SORU: Sayıların toplama işlemine göre tersini alıp topla.";
        }
        else
        {
            cevap = s1 + s2 - s3;
            metin = $"Cadde No:\n({s1}) + (+{s2}) - ({s3})";
            kural = "SORU: Verilen sayıları sırasıyla topla ve çıkar.";
        }
    }

    // --- 6. ZOR SORULAR (HARD) --- 
    void GenerateHardSoru(out string metin, out string kural, out int cevap)
    {
        // 3 farklı zor senaryo hazırladım. Rastgele birini seçecek.
        int senaryo = Random.Range(0, 3);

        if (senaryo == 0)
        {
            // SENARYO 1: Üslü Sayılar ve Negatiflik Tuzağı
            // Örn: (-3)^2 + (-2)^3 = ?
            // (-3)^2 = 9 eder, (-2)^3 = -8 eder. Sonuç 1.

            int taban1 = Random.Range(2, 6) * -1; // -2 ile -5 arası negatif
            int us1 = 2; // Çift kuvvet (Sonuç pozitif çıkar)

            int taban2 = Random.Range(2, 5) * -1; // Negatif
            int us2 = 3; // Tek kuvvet (Sonuç negatif çıkar)

            // İşlemi yap
            int sonuc1 = (int)Mathf.Pow(taban1, us1); // Pozitif olur
            int sonuc2 = (int)Mathf.Pow(taban2, us2); // Negatif olur

            cevap = sonuc1 + sonuc2;

            metin = $"Zorlu Denklem:\n({taban1})^{us1} + ({taban2})^{us2}";
            kural = "DİKKAT: Negatif sayıların parantez kuvvetlerine dikkat et!\nÇift kuvvet pozitife, tek kuvvet negatife döner.";
        }
        else if (senaryo == 1)
        {
            // SENARYO 2: Mutlak Değer ve İşlem Önceliği
            // Örn: |-8| - |+3| x (-2) = ?
            // 8 - (3 x -2) -> 8 - (-6) -> 8 + 6 = 14

            int s1 = Random.Range(5, 10) * -1; // -5 ile -10 arası
            int s2 = Random.Range(2, 6);       // Pozitif
            int s3 = Random.Range(2, 5) * -1;  // Negatif çarpan

            // İşlem: Mutlak(s1) - (Mutlak(s2) * s3)
            int mutlakS1 = Mathf.Abs(s1);
            int mutlakS2 = Mathf.Abs(s2);

            cevap = mutlakS1 - (mutlakS2 * s3);

            metin = $"Mutlak Tuzak:\n|{s1}| - |+{s2}| x ({s3})";
            kural = "DİKKAT: Önce mutlak değerleri çıkar, sonra ÇARPMA işlemini yap, en son çıkarma işlemini yap.";
        }
        else
        {
            // SENARYO 3: Çoklu İşlem (Parantez ve Bölme)
            // Örn: (-20 / 4) - (-3 x 2)
            // (-5) - (-6) -> -5 + 6 = 1

            int bolunen = Random.Range(2, 10) * 4; // 4'e bölünebilen sayı olsun
            int bolen = 4;
            int carpan1 = Random.Range(2, 6) * -1;
            int carpan2 = Random.Range(2, 5);

            // (-Bolunen / Bolen) - (Carpan1 * Carpan2)
            int kisim1 = (-bolunen) / bolen;
            int kisim2 = carpan1 * carpan2;

            cevap = kisim1 - kisim2;

            metin = $"Karmaşık İşlem:\n(-{bolunen} : {bolen}) - ({carpan1} x {carpan2})";
            kural = "DİKKAT: Önce parantez içlerini yap. Eksileri çıkarırken işaret değiştirmeyi unutma! (- - yan yana gelirse + olur)";
        }
    }

    // --- CEZA KÖŞESİ İÇİN GÜNCELLENMİŞ FONKSİYON ---
    public void AskRandomNormalQuestion()
    {
        // 1. Sadece soru sorulabilecek geçerli türleri bir listeye koyuyoruz.
        // Böylece Empty, Start, Hard veya Penalty gelme ihtimali SIFIR oluyor.
        TileType[] validTypes = {
            TileType.Red,
            TileType.Blue,
            TileType.Green,
            TileType.Yellow,
            TileType.Purple
        };

        // 2. Bu listenin uzunluğuna göre rastgele bir index seçiyoruz
        int randIndex = Random.Range(0, validTypes.Length);

        // 3. Seçilen türü alıyoruz
        TileType selectedType = validTypes[randIndex];

        Debug.Log("Ceza Köşesi: Rastgele Soru Türü Seçildi -> " + selectedType);

        // 4. Soruyu soruyoruz
        SoruOlusturVeSor(selectedType);
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