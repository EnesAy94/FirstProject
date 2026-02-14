using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Data;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager instance;

    void Awake()
    {
        instance = this;
    }

    // GÜNCELLEME 1: isPenalty parametresi eklendi (Varsayılan: false)
    public void SoruOlusturVeSor(TileType tileType, bool isPenalty = false)
    {
        if (LevelManager.instance == null || LevelManager.instance.currentChapter == null || LevelManager.instance.currentChapter.questionSet == null)
        {
            Debug.LogError("HATA: Soru seti eksik!");
            return;
        }

        ChapterQuestionSet set = LevelManager.instance.currentChapter.questionSet;
        int currentChapterID = LevelManager.instance.currentChapter.chapterID;

        // 1. Listeleri Belirle
        List<QuestionTemplate> storyList = null;
        List<QuestionTemplate> hardList = null;

        switch (tileType)
        {
            case TileType.Red: storyList = set.redTemplates; hardList = set.redHardTemplates; break;
            case TileType.Blue: storyList = set.blueTemplates; hardList = set.blueHardTemplates; break;
            case TileType.Yellow: storyList = set.yellowTemplates; hardList = set.yellowHardTemplates; break;
            case TileType.Purple: storyList = set.purpleTemplates; hardList = set.purpleHardTemplates; break;
            case TileType.Green: storyList = set.greenTemplates; hardList = set.greenHardTemplates; break;
            case TileType.Orange: storyList = set.orangeTemplates; hardList = set.orangeHardTemplates; break;
            case TileType.Hard: PickRandomFromList(set.hardTemplates, tileType, true); return;
        }

        UsedQuestionData data = GetUsedData(currentChapterID, tileType);
        QuestionTemplate selectedTemplate = null;

        // --- YENİ MANTIK: AnswerManager'a Durumu Bildir ---

        // Varsayılan olarak sıfırla
        if (AnswerManager.instance != null)
        {
            AnswerManager.instance.isStoryPhase = false;
            AnswerManager.instance.isFinalStoryQuestion = false;
        }

        // A) HİKAYELİ SORU MU?
        if (storyList != null && data.usedStoryIndices.Count < storyList.Count)
        {
            int index = GetRandomUnusedIndex(storyList.Count, data.usedStoryIndices);
            selectedTemplate = storyList[index];
            data.usedStoryIndices.Add(index);
            SaveManager.instance.SaveGame();

            // DURUM GÜNCELLEME:
            if (AnswerManager.instance != null)
            {
                AnswerManager.instance.isStoryPhase = true; // Evet, hikaye modundayız (Yanlış yaparsa özel mesaj çıkacak)

                // Eğer kullanılan soru sayısı, toplam soru sayısına eşitlendiyse bu SON sorudur.
                if (data.usedStoryIndices.Count == storyList.Count)
                {
                    AnswerManager.instance.isFinalStoryQuestion = true; // Evet, bu son soru (Doğru yaparsa özel mesaj çıkacak)
                }
            }
        }
        // B) ZOR (YEDEK) SORU MU?
        else if (hardList != null && data.usedHardIndices.Count < hardList.Count)
        {
            int index = GetRandomUnusedIndex(hardList.Count, data.usedHardIndices);
            selectedTemplate = hardList[index];
            data.usedHardIndices.Add(index);
            SaveManager.instance.SaveGame();

            // Burası StoryPhase DEĞİL. Standart mesajlar çıkacak.
        }
        // C) DÖNGÜ (HARD RESET)
        else
        {
            Debug.Log($"{tileType} döngüye girdi.");
            data.usedHardIndices.Clear();
            if (hardList != null && hardList.Count > 0)
            {
                int index = Random.Range(0, hardList.Count);
                selectedTemplate = hardList[index];
                data.usedHardIndices.Add(index);
                SaveManager.instance.SaveGame();
            }
        }

        if (selectedTemplate != null)
        {
            GenerateAndSendQuestion(selectedTemplate, tileType, isPenalty);
        }
        else
        {
            if (AnswerManager.instance != null)
                AnswerManager.instance.SetQuestion("Yedek Soru: 10+10=?", "20", tileType);
        }
    }

    void PickRandomFromList(List<QuestionTemplate> list, TileType type, bool isHardTile)
    {
        if (list != null && list.Count > 0)
        {
            int r = Random.Range(0, list.Count);
            // Sadece burası 'Hard' istatistiğine işleyecekse isHardTile parametresini kullanabiliriz
            // Ama sen mekan içindeki soruları normal saymak istedin, o yüzden burayı false yolluyoruz.
            // Ancak bu fonksiyon sadece "TileType.Hard" (Kuru Kafa) için çağrıldığı için type zaten Hard gidecek.
            GenerateAndSendQuestion(list[r], type, false);
        }
    }

    UsedQuestionData GetUsedData(int chapterID, TileType color)
    {
        PlayerData save = SaveManager.instance.activeSave;
        UsedQuestionData found = save.usedQuestions.Find(x => x.chapterID == chapterID && x.color == color);

        if (found == null)
        {
            found = new UsedQuestionData();
            found.chapterID = chapterID;
            found.color = color;
            save.usedQuestions.Add(found);
        }
        return found;
    }

    int GetRandomUnusedIndex(int totalCount, List<int> usedIndices)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < totalCount; i++)
        {
            if (!usedIndices.Contains(i)) available.Add(i);
        }

        if (available.Count == 0) return 0;
        return available[Random.Range(0, available.Count)];
    }

    // GÜNCELLEME 2: isPenalty parametresi ve Metin Mantığı
    // --- YENİ NESİL HESAPLAMA SİSTEMİ (Min Katı Korumalı ve EKOK Destekli) ---
    // --- YENİ NESİL HESAPLAMA SİSTEMİ (Sıfıra Bölme ve Tanımsız Korumalı) ---
    void GenerateAndSendQuestion(QuestionTemplate tmpl, TileType type, bool isPenalty)
    {
        int[] finalValues = new int[tmpl.variableRanges.Count];
        object[] formatArgs = new object[tmpl.variableRanges.Count];
        bool isUndefined = false; // Bu soru tanımsız mı?

        // 1. SAYILARI ÜRET (Senin yazdığın doğru mantık)
        for (int i = 0; i < tmpl.variableRanges.Count; i++)
        {
            int min = tmpl.variableRanges[i].x;
            int max = tmpl.variableRanges[i].y;

            if (tmpl.useMinMultiplierRule && (min != 0 || max != 0))
            {
                // X ve Y'den hangisi 0'a daha yakınsa onu çarpan yap
                int multiplier = Mathf.Min(Mathf.Abs(min), Mathf.Abs(max));
                if (multiplier == 0) multiplier = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));

                int steps = (max - min) / multiplier;
                if (steps < 0) steps = 0;

                finalValues[i] = min + (Random.Range(0, steps + 1) * multiplier);
            }
            else if (Mathf.Abs(min) >= 100)
            {
                int min10 = Mathf.CeilToInt(min / 10f);
                int max10 = Mathf.FloorToInt(max / 10f);
                if (min10 > max10) max10 = min10;
                finalValues[i] = Random.Range(min10, max10 + 1) * 10;
            }
            else
            {
                finalValues[i] = Random.Range(min, max + 1);
            }
        }

        // 2. AKILLI DÜZELTME & SIFIRA BÖLME KONTROLÜ (GÜNCELLENDİ)
        if (tmpl.divisionPairs != null && tmpl.divisionPairs.Count > 0)
        {
            Dictionary<int, int> combinedDivisors = new Dictionary<int, int>();

            foreach (Vector2Int pair in tmpl.divisionPairs)
            {
                int dividendIndex = pair.x;
                int divisorIndex = pair.y;

                if (dividendIndex < finalValues.Length && divisorIndex < finalValues.Length)
                {
                    int divisor = finalValues[divisorIndex];

                    // --- KRİTİK DÜZELTME BURADA ---
                    // Eğer bölen 0 ise; onu 1 yapmıyoruz! 'isUndefined' olarak işaretliyoruz.
                    if (divisor == 0)
                    {
                        isUndefined = true;
                        continue; // Döngüyü pas geç, bu sayıyı düzeltme listesine ekleme
                    }

                    // Zincirleme bölme için bölenleri birleştir
                    if (combinedDivisors.ContainsKey(dividendIndex))
                        combinedDivisors[dividendIndex] *= divisor;
                    else
                        combinedDivisors[dividendIndex] = divisor;
                }
            }

            // Eğer soru Tanımsız DEĞİLSE sayıları düzelt (Sıfırsa dokunma)
            if (!isUndefined)
            {
                foreach (var kvp in combinedDivisors)
                {
                    int dividendIndex = kvp.Key;
                    int totalDivisor = Mathf.Abs(kvp.Value);
                    int originalDividend = finalValues[dividendIndex];

                    // Tam bölünmüyorsa EKOK mantığıyla düzelt
                    if (originalDividend % totalDivisor != 0)
                    {
                        int step = 1;
                        int minVal = tmpl.variableRanges[dividendIndex].x;

                        if (tmpl.useMinMultiplierRule && minVal != 0) step = Mathf.Abs(minVal);
                        else if (Mathf.Abs(minVal) >= 100) step = 10;

                        int targetMultiple = LCM(step, totalDivisor);
                        int cleanDividend = (originalDividend / targetMultiple) * targetMultiple;

                        if (cleanDividend == 0) cleanDividend = targetMultiple;
                        finalValues[dividendIndex] = cleanDividend;
                    }
                }
            }
        }

        // 3. Object dizisine aktar
        for (int i = 0; i < finalValues.Length; i++) formatArgs[i] = finalValues[i];

        // 4. Metin ve Formül Hazırla
        string rawFormula = string.Format(tmpl.formula, formatArgs);
        string cleanFormula = rawFormula.Replace("\"", "").Replace("'", "").Trim();
        string finalQuestionText;

        if (isPenalty) finalQuestionText = $"İŞLEMİ ÇÖZ:\n\n{cleanFormula} = ?";
        else finalQuestionText = string.Format(tmpl.questionText, formatArgs);

        // 5. HESAPLA
        string calculatedAnswerStr = "";

        if (isUndefined)
        {
            // SIFIRA BÖLME VARSA CEVAP DİREKT TANIMSIZ
            calculatedAnswerStr = "tanımsız";
            Debug.Log($"✅ Soru Hazır (TANIMSIZ): {cleanFormula} = tanımsız");
        }
        else
        {
            try
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                var resultObj = dt.Compute(cleanFormula, "");

                int intResult = 0;
                if (resultObj is int) intResult = (int)resultObj;
                else if (resultObj is double) intResult = (int)(double)resultObj;
                else if (resultObj is float) intResult = (int)(float)resultObj;
                else if (resultObj is decimal) intResult = (int)(decimal)resultObj;
                else intResult = System.Convert.ToInt32(resultObj);

                calculatedAnswerStr = intResult.ToString();
                Debug.Log($"✅ Soru Hazır: {cleanFormula} = {calculatedAnswerStr}");
            }
            catch (System.Exception e)
            {
                // DataTable içinde görünmez bir sıfıra bölme olursa yakala
                if (e is System.DivideByZeroException || e.Message.ToLower().Contains("zero"))
                {
                    calculatedAnswerStr = "tanımsız";
                }
                else
                {
                    Debug.LogError($"Hesaplama Hatası: {e.Message}");
                    calculatedAnswerStr = "0";
                    finalQuestionText += " (Hata)";
                }
            }
        }

        // 6. Gönder
        if (AnswerManager.instance != null)
        {
            AnswerManager.instance.SetQuestion(finalQuestionText, calculatedAnswerStr, type);
        }
    }

    // --- MATEMATİK YARDIMCILARI (EBOB ve EKOK) ---
    // En Büyük Ortak Bölen
    int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    // En Küçük Ortak Kat
    int LCM(int a, int b)
    {
        if (a == 0 || b == 0) return 0;
        return (a / GCD(a, b)) * b;
    }

    // GÜNCELLEME 3: Burası Ceza Köşesi olduğu için 'true' gönderiyoruz
    public void AskRandomNormalQuestion()
    {
        TileType[] validTypes = {
            TileType.Red, TileType.Blue, TileType.Green,
            TileType.Yellow, TileType.Purple, TileType.Orange
        };

        int randIndex = Random.Range(0, validTypes.Length);
        TileType selectedType = validTypes[randIndex];

        // DİKKAT: Buradaki 'true', "Bu bir ceza sorusudur, hikaye yazma!" demek.
        SoruOlusturVeSor(selectedType, true);
    }
}