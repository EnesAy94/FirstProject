using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager instance;

    [Header("UI Panelleri")]
    public GameObject questionPanel;
    public GameObject greenQuestionPanel;

    [Header("Yeşil Panel UI")]
    public GameObject[] greenChatObjects;
    public TextMeshProUGUI[] greenChatTexts;

    [Header("Soru UI Elemanları")]
    public TextMeshProUGUI categoryTitle;
    public TextMeshProUGUI questionText;
    public ItemButton[] answerButtons;

    [Header("HAM VERİLER (10+ İsim Girilecek)")]
    public List<string> rawAntikaIsimler;
    public List<string> rawTeknolojiIsimler;
    public List<string> rawKuyumcuIsimler;
    public List<string> rawMorFilmler;
    public List<string> rawMorKitaplar;

    // --- OYUNCUYA ÖZEL ENVANTERLER (Her oyuncunun 5'er tane butonu olacak) ---
    // P1'in Listeleri
    private List<GameItem> p1Antika = new List<GameItem>();
    private List<GameItem> p1Teknoloji = new List<GameItem>();
    private List<GameItem> p1Kuyumcu = new List<GameItem>();
    private List<GameItem> p1Filmler = new List<GameItem>();
    private List<GameItem> p1Kitaplar = new List<GameItem>();

    // P2'nin Listeleri
    private List<GameItem> p2Antika = new List<GameItem>();
    private List<GameItem> p2Teknoloji = new List<GameItem>();
    private List<GameItem> p2Kuyumcu = new List<GameItem>();
    private List<GameItem> p2Filmler = new List<GameItem>();
    private List<GameItem> p2Kitaplar = new List<GameItem>();

    // Yeşil Senaryolar (Her oyuncu için 3 senaryo üretilir)
    private List<GreenScenarioData> p1Yesil = new List<GreenScenarioData>();
    private List<GreenScenarioData> p2Yesil = new List<GreenScenarioData>();

    // Genel Değişkenler
    private int dogruCevap;
    private string unluler = "aeıioöuüAEIİOÖUÜ";
    private string currentMode = "";
    private string yesilIslemMetni = "";

    // Mor Detaylar
    private bool isMorSecondStage = false;
    private GameItem secilenFilmItem;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializeGameData();
    }

    // --- VERİ OLUŞTURMA MERKEZİ ---
    void InitializeGameData()
    {
        // P1 ve P2 için Havuzdan Rastgele 5'er tane çek ve değerlerini üret
        p1Antika = GenerateRandomList(rawAntikaIsimler, "Antika");
        p2Antika = GenerateRandomList(rawAntikaIsimler, "Antika");

        p1Teknoloji = GenerateRandomList(rawTeknolojiIsimler, "Teknoloji");
        p2Teknoloji = GenerateRandomList(rawTeknolojiIsimler, "Teknoloji");

        p1Kuyumcu = GenerateRandomList(rawKuyumcuIsimler, "Kuyumcu");
        p2Kuyumcu = GenerateRandomList(rawKuyumcuIsimler, "Kuyumcu");

        p1Filmler = GenerateRandomList(rawMorFilmler, "Mor");
        p2Filmler = GenerateRandomList(rawMorFilmler, "Mor");

        p1Kitaplar = GenerateRandomList(rawMorKitaplar, "Mor");
        p2Kitaplar = GenerateRandomList(rawMorKitaplar, "Mor");

        // Yeşil Senaryoları Üret (Her oyuncuya 3 tane)
        GenerateGreenScenarios(p1Yesil);
        GenerateGreenScenarios(p2Yesil);

        Debug.Log("🎲 OYUN BAŞLADI: Her oyuncuya farklı liste ve değerler atandı!");
    }

    // --- YARDIMCI: RASTGELE LİSTE OLUŞTURUCU ---
    List<GameItem> GenerateRandomList(List<string> sourceList, string mode)
    {
        List<GameItem> newList = new List<GameItem>();

        // 1. Ana listeyi kopyala ve karıştır (Shuffle)
        List<string> tempPool = new List<string>(sourceList);
        ShuffleStringList(tempPool);

        // 2. İlk 5 tanesini seç (veya liste kısaysa hepsi)
        int count = Mathf.Min(tempPool.Count, 5); // Buton sayısı kadar (5)

        for (int i = 0; i < count; i++)
        {
            string name = tempPool[i];
            string val = "";
            int qty = 1;

            // Değerleri Üret
            if (mode == "Antika")
            {
                val = Random.Range(1, 10).ToString();
            }
            else if (mode == "Teknoloji")
            {
                int sol = Random.Range(10, 999);
                int sag = Random.Range(100, 999);
                val = sol + "." + sag;
            }
            else if (mode == "Kuyumcu")
            {
                qty = Random.Range(1, 11);
                int fiyat = 0;
                if (name.Contains("Dolar") || name.Contains("Euro") || name.Contains("Sterlin"))
                    fiyat = Random.Range(30, 41);
                else
                    fiyat = Random.Range(10, 101) * 100;
                val = fiyat.ToString();
            }
            else if (mode == "Mor")
            { // Film veya Kitap
                val = Random.Range(1900, 2026).ToString();
            }

            newList.Add(new GameItem(name, val, qty));
        }
        return newList;
    }

    void GenerateGreenScenarios(List<GreenScenarioData> targetList)
    {
        for (int i = 0; i < 3; i++)
        {
            GreenScenarioData d = new GreenScenarioData();
            d.s1 = Random.Range(-59, 99); d.s2 = Random.Range(-59, 99);
            d.s3 = Random.Range(-59, 99); d.s4 = Random.Range(-59, 99);
            targetList.Add(d);
        }
    }

    // --- SIRA KİMDEYSE ONUN LİSTESİNİ GETİREN FONKSİYON ---
    List<GameItem> GetCurrentPlayerList(string type)
    {
        int pIndex = GameManager.instance.currentPlayerIndex;

        if (type == "Antika") return (pIndex == 0) ? p1Antika : p2Antika;
        if (type == "Teknoloji") return (pIndex == 0) ? p1Teknoloji : p2Teknoloji;
        if (type == "Kuyumcu") return (pIndex == 0) ? p1Kuyumcu : p2Kuyumcu;
        if (type == "Filmler") return (pIndex == 0) ? p1Filmler : p2Filmler;
        if (type == "Kitaplar") return (pIndex == 0) ? p1Kitaplar : p2Kitaplar;

        return null;
    }

    // --- HAZIRLIK FONKSİYONLARI ---
    public void AntikaSorusunuHazirla()
    {
        currentMode = "Antika";
        PrepareUI("KENT ANTİKA", "Hırsız çaldığı eşyaları antikacıya getirdi. Birini seç.\nMiktarı ÇİFT ise: (Ünlü - Ünsüz)\nMiktarı TEK ise: (Ünsüz - Ünlü)", GetCurrentPlayerList("Antika"));
    }
    public void MaviSorusunuHazirla()
    {
        currentMode = "Teknoloji";
        PrepareUI("TEKNOLOJİ MAĞAZASI", "Hırsız kredi kartıyla alışveriş yapıyor. Birini seç.\nFiyatın (Noktanın solu top) - (Noktanın sağı top).", GetCurrentPlayerList("Teknoloji"));
    }
    public void SariSorusunuHazirla()
    {
        currentMode = "Kuyumcu";
        PrepareUI("KUYUMCU", "Hırsız mücevher bozduruyor. Birini seç.\n(Adet x Miktar) -> Rakam Toplamı -> (-4) ile topla.", GetCurrentPlayerList("Kuyumcu"));
    }
    public void MorSorusunuHazirla()
    {
        currentMode = "Mor";
        isMorSecondStage = false;
        PrepareUI("SİNEMA ODASI", "Hırsızın izlediği filmlerden birini seç.", GetCurrentPlayerList("Filmler"));
    }

    // --- UI DOLDURMA ---
    void PrepareUI(string baslik, string hikaye, List<GameItem> veriListesi)
    {
        questionPanel.SetActive(true);
        greenQuestionPanel.SetActive(false);
        categoryTitle.text = baslik;
        questionText.text = hikaye;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i >= veriListesi.Count)
            {
                answerButtons[i].gameObject.SetActive(false);
                continue;
            }

            answerButtons[i].gameObject.SetActive(true);
            GameItem item = veriListesi[i];

            // Listeyi zaten oyuncuya özel çektik, o yüzden direkt item.isUsed kullanabiliriz.
            answerButtons[i].ButonuAyarla(item.itemName, item.quantity, item.valueStr, item.isUsed);
        }
    }

    // --- YEŞİL SORU ---
    public void YesilSorusunuHazirla()
    {
        currentMode = "Yesil";
        questionPanel.SetActive(false);
        greenQuestionPanel.SetActive(true);
        foreach (var chat in greenChatObjects) chat.SetActive(false);

        // Rastgele bir senaryo seç
        int index = Random.Range(0, 3);
        greenChatObjects[index].SetActive(true);

        HazirlaYesilSenaryo(index);
    }

    void HazirlaYesilSenaryo(int index)
    {
        int pIndex = GameManager.instance.currentPlayerIndex;
        // Oyuncuya özel listeyi al
        List<GreenScenarioData> currentGreenList = (pIndex == 0) ? p1Yesil : p2Yesil;
        GreenScenarioData data = currentGreenList[index];

        string mesajMetni = "";

        // --- SENARYO 1: SAAT KAÇ ---
        if (index == 0)
        {
            int s1 = (data.s1 % 40) - 10;
            int s2 = (data.s2 % 40) - 10;
            int s3 = (data.s3 % 10) - 5;

            dogruCevap = s1 - s2 + s3;
            yesilIslemMetni = $"({s1}) - ({s2}) + ({s3})";

            // Tam Metin
            mesajMetni = $"Neredesin oğlum?\n\n" +
                         $"Geliyorum baba köşedeyim.\n\n" +
                         $"Saat kaç saat\n\n" +
                         $"Baba saat {yesilIslemMetni} işleminin sonucu kadar\n\n" +
                         $"Çabuk eve gel!";
        }
        // --- SENARYO 2: ARABA PLAKA ---
        else if (index == 1)
        {
            int s1 = Mathf.Abs(data.s1 % 90) + 10; // Pozitif olsun
            int s2 = -1 * (Mathf.Abs(data.s2 % 90) + 10); // Negatif olsun

            dogruCevap = (-1 * s1) + (-1 * s2);
            yesilIslemMetni = $"Tersi(+{s1}) + Tersi({s2})";

            // Tam Metin
            mesajMetni = $"Kardeşim selam bana acil bir araba bulabilir misin?\n\n" +
                         $"Ayarlarız abi hayırdır?\n\n" +
                         $"Bir yere kadar gidip gelicem.\n\n" +
                         $"Tamamdır abi araç geliyor.\nPlakası: (+{s1}) in toplama işlemine göre tersi ile ({s2}) ün toplama işlemine göre tersinin toplamıdır.\n\n" +
                         $"Tamamdır. O yöne doğru ilerliyorum.";
        }
        // --- SENARYO 3: ADRES / BULVAR ---
        else if (index == 2)
        {
            int s1 = (data.s1 % 20);
            int s2 = Mathf.Abs(data.s2 % 20);
            int s3 = (data.s3 % 20);
            int s4 = (data.s4 % 20);

            dogruCevap = s1 + s2 - s3 + s4;
            yesilIslemMetni = $"({s1}) + (+{s2}) - ({s3}) + ({s4})";

            // Tam Metin
            mesajMetni = $"Selam gençler\n\n" +
                         $"Selam agacım\n\n" +
                         $"Bana acil kuyumcu adresi söyleyebilir misiniz? İşim düştüde\n\n" +
                         $"Gazi bulvarından ilerle agacım.\nCadde numaralarının yani {yesilIslemMetni} işleminin sonucu kadar ilerle.\n\n" +
                         $"Tamamdır.";
        }

        greenChatTexts[index].text = mesajMetni;
    }

    public void YesilSecimYapildi()
    {
        greenQuestionPanel.SetActive(false);
        AnswerManager.instance.PaneliAc("GİZLİ MESAJ", yesilIslemMetni, 1, dogruCevap);
    }

    // --- CEVAP HESAPLAMA VE İŞARETLEME ---
    public void CevabiHesapla(string secilenEsya, string degerStr, int adet)
    {
        // 1. Önce aktif listeyi bulup öğeyi işaretlemeliyiz
        GameItem itemToMark = FindAndMarkItem(secilenEsya);

        // 2. MOR FİLM SEÇİMİ
        if (currentMode == "Mor" && !isMorSecondStage)
        {
            secilenFilmItem = itemToMark; // Filmi sakla
            isMorSecondStage = true;
            // Şimdi Kitapları Göster
            PrepareUI("KÜTÜPHANE ODASI", "Hırsızın okuduğu kitaplardan birini seç.\n(Film - Kitap) İşlemi.", GetCurrentPlayerList("Kitaplar"));
            return;
        }

        // 3. Hesaplamalar
        if (currentMode == "Antika")
        {
            int miktar = int.Parse(degerStr);
            int unlu = HarfSay(secilenEsya, true);
            int unsuz = HarfSay(secilenEsya, false);
            dogruCevap = (miktar % 2 == 0) ? (unlu - unsuz) : (unsuz - unlu);
        }
        else if (currentMode == "Teknoloji")
        {
            string[] parcalar = degerStr.Split('.');
            int sol = RakamlariTopla(parcalar[0]);
            int sag = RakamlariTopla(parcalar[1]);
            dogruCevap = sol - sag;
        }
        else if (currentMode == "Kuyumcu")
        {
            int fiyat = int.Parse(degerStr);
            int carpim = fiyat * adet;
            dogruCevap = RakamlariTopla(carpim.ToString()) - 4;
        }
        else if (currentMode == "Mor") // Kitap Hesaplaması
        {
            // Mor ise zaten secilenFilmItem oyuncunun kendi listesinden geldi
            int filmYili = int.Parse(secilenFilmItem.valueStr);
            int kitapYili = int.Parse(degerStr);

            int filmSon = -1 * (filmYili % 10);
            int kitapSon = -1 * (kitapYili % 10);
            dogruCevap = filmSon - kitapSon;

            secilenEsya = $"{secilenFilmItem.itemName} ({filmYili})\nVE\n{secilenEsya} ({kitapYili})";
            degerStr = "Şifre Çözüldü";
        }

        AnswerManager.instance.PaneliAc(secilenEsya, degerStr, adet, dogruCevap);
    }

    // --- YARDIMCI FONKSİYONLAR ---

    // Doğru listeden elemanı bulup işaretleyen fonksiyon
    GameItem FindAndMarkItem(string name)
    {
        List<GameItem> list = null;
        if (currentMode == "Antika") list = GetCurrentPlayerList("Antika");
        else if (currentMode == "Teknoloji") list = GetCurrentPlayerList("Teknoloji");
        else if (currentMode == "Kuyumcu") list = GetCurrentPlayerList("Kuyumcu");
        else if (currentMode == "Mor") list = isMorSecondStage ? GetCurrentPlayerList("Kitaplar") : GetCurrentPlayerList("Filmler");

        if (list != null)
        {
            foreach (var item in list)
            {
                if (item.itemName == name)
                {
                    item.isUsed = true; // SADECE O OYUNCUNUN LİSTESİNDEKİ İŞARETLENİR
                    return item;
                }
            }
        }
        return null;
    }

    void ShuffleStringList(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string temp = list[i];
            int r = Random.Range(i, list.Count);
            list[i] = list[r];
            list[r] = temp;
        }
    }

    // Matematik Fonksiyonları (Aynı)
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

// SINIFLAR (QuestionManager'ın dışına veya en altına)
[System.Serializable]
public class GameItem
{
    public string itemName;
    public string valueStr;
    public int quantity;
    public bool isUsed; // Sadece o liste için geçerli

    public GameItem(string name, string val, int qty)
    {
        itemName = name;
        valueStr = val;
        quantity = qty;
        isUsed = false;
    }
}

[System.Serializable]
public class GreenScenarioData
{
    public int s1, s2, s3, s4;
}