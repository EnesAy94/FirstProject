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
        List<QuestionTemplate> targetList = null;

        switch (tileType)
        {
            case TileType.Red: targetList = set.redTemplates; break;
            case TileType.Blue: targetList = set.blueTemplates; break;
            case TileType.Yellow: targetList = set.yellowTemplates; break;
            case TileType.Purple: targetList = set.purpleTemplates; break;
            case TileType.Green: targetList = set.greenTemplates; break;
            case TileType.Orange: targetList = set.orangeTemplates; break;
            case TileType.Hard: targetList = set.hardTemplates; break;
        }

        if (targetList != null && targetList.Count > 0)
        {
            int randomIndex = Random.Range(0, targetList.Count);
            QuestionTemplate template = targetList[randomIndex];

            // Parametreyi buraya da iletiyoruz
            GenerateAndSendQuestion(template, tileType, isPenalty);
        }
        else
        {
            // Yedek soru
            if (AnswerManager.instance != null)
                AnswerManager.instance.SetQuestion("Yedek Soru: 5 + 5 = ?", 10, tileType);
        }
    }

    // GÜNCELLEME 2: isPenalty parametresi ve Metin Mantığı
    void GenerateAndSendQuestion(QuestionTemplate tmpl, TileType type, bool isPenalty)
    {
        // 1. Sayıları Oluştur
        List<int> generatedValues = new List<int>();
        object[] formatArgs = new object[tmpl.variableRanges.Count];

        for (int i = 0; i < tmpl.variableRanges.Count; i++)
        {
            int min = tmpl.variableRanges[i].x;
            int max = tmpl.variableRanges[i].y;
            int val = Random.Range(min, max + 1);

            generatedValues.Add(val);
            formatArgs[i] = val;
        }

        // 2. Formülü Hazırla (Hesaplama ve Gösterim için)
        string rawFormula = string.Format(tmpl.formula, formatArgs);
        string cleanFormula = rawFormula.Replace("\"", "").Replace("'", "").Trim();

        // --- KRİTİK DEĞİŞİKLİK BURADA ---
        string finalQuestionText;

        if (isPenalty)
        {
            // Eğer CEZA ise: Hikayeyi çöpe at, sadece formülü göster!
            // Örn: "45 + 8 + 7 = ?"
            finalQuestionText = $"İŞLEMİ ÇÖZ:\n\n{cleanFormula} = ?";
        }
        else
        {
            // Eğer NORMAL ise: Hikayeli metni kullan
            // Örn: "Ali'nin 45 elması var..."
            finalQuestionText = string.Format(tmpl.questionText, formatArgs);
        }
        // ---------------------------------

        int calculatedAnswer = 0;

        try
        {
            DataTable dt = new DataTable();
            var resultObj = dt.Compute(cleanFormula, "");

            if (resultObj is int) calculatedAnswer = (int)resultObj;
            else calculatedAnswer = System.Convert.ToInt32(resultObj);

            Debug.Log($"✅ Soru ({isPenalty}): {cleanFormula} = {calculatedAnswer}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Formül Hatası: {e.Message}");
            calculatedAnswer = 0;
            finalQuestionText = "Hatalı Soru (Cevap 0)";
        }

        // 3. Gönder
        if (AnswerManager.instance != null)
        {
            AnswerManager.instance.SetQuestion(finalQuestionText, calculatedAnswer, type);
        }
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