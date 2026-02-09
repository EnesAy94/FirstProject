using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Data;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager instance;

    // Hesplanan cevabı AnswerManager'a göndermek için geçici değişken
    private int calculatedAnswer;

    void Awake()
    {
        instance = this;
    }

    public void SoruOlusturVeSor(TileType tileType)
    {
        // 1. Bölüm Verisi Kontrolü
        if (LevelManager.instance == null || LevelManager.instance.currentChapter == null || LevelManager.instance.currentChapter.questionSet == null)
        {
            Debug.LogError("HATA: LevelManager veya ChapterQuestionSet eksik! Lütfen ChapterData'ya soru seti atayın.");
            return;
        }

        ChapterQuestionSet set = LevelManager.instance.currentChapter.questionSet;
        List<QuestionTemplate> targetList = null;

        // 2. Renge Göre Listeyi Seç
        switch (tileType)
        {
            case TileType.Red: targetList = set.redTemplates; break;
            case TileType.Blue: targetList = set.blueTemplates; break;
            case TileType.Yellow: targetList = set.yellowTemplates; break;
            case TileType.Purple: targetList = set.purpleTemplates; break;
            case TileType.Green: targetList = set.greenTemplates; break;
            case TileType.Orange: targetList = set.orangeTemplates; break; // Yeni Renk
            case TileType.Hard: targetList = set.hardTemplates; break;
        }

        // 3. Listeden Rastgele Şablon Seç
        if (targetList != null && targetList.Count > 0)
        {
            int randomIndex = Random.Range(0, targetList.Count);
            QuestionTemplate template = targetList[randomIndex];

            GenerateAndSendQuestion(template, tileType);
        }
        else
        {
            Debug.LogWarning($"UYARI: {tileType} rengi için soru şablonu bulunamadı! Varsayılan soru soruluyor.");
            // Yedek soru (Hata vermemesi için)
            if (AnswerManager.instance != null)
                AnswerManager.instance.SetQuestion("Yedek Soru: 5 + 5 = ?", 10, tileType);
        }
    }

    // --- ŞABLONDAN SORU ÜRETME (HEPSİNİ BU YAPIYOR) ---
    // QuestionManager.cs içindeki GenerateAndSendQuestion fonksiyonunu bununla değiştir:

    void GenerateAndSendQuestion(QuestionTemplate tmpl, TileType type)
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

        // 2. Metni Oluştur
        string finalQuestionText = string.Format(tmpl.questionText, formatArgs);

        // 3. Formülü Oluştur
        string rawFormula = string.Format(tmpl.formula, formatArgs);

        // --- DÜZELTME BURADA BAŞLIYOR ---

        // Temizlik: Tırnak işaretlerini ve gereksiz boşlukları temizle
        string cleanFormula = rawFormula.Replace("\"", "").Replace("'", "").Trim();

        int calculatedAnswer = 0;

        try
        {
            // Hesaplama Motoru
            System.Data.DataTable dt = new System.Data.DataTable();
            var resultObj = dt.Compute(cleanFormula, "");

            // Sonucu Güvenli Çevir
            if (resultObj is int) calculatedAnswer = (int)resultObj;
            else if (resultObj is double) calculatedAnswer = (int)(double)resultObj;
            else if (resultObj is float) calculatedAnswer = (int)(float)resultObj;
            else if (resultObj is decimal) calculatedAnswer = (int)(decimal)resultObj;
            else calculatedAnswer = System.Convert.ToInt32(resultObj);

            Debug.Log($"✅ Soru Hazır: {cleanFormula} = {calculatedAnswer}");
        }
        catch (System.Exception e)
        {
            // HATA VARSA OYUN ÇÖKMESİN, LOG BASIP DEVAM ETSİN
            Debug.LogError($"🚨 FORMÜL HATASI! Şablon: {tmpl.name} \n" +
                           $"Hatalı Formül: '{cleanFormula}' (Orjinal: {tmpl.formula}) \n" +
                           $"Hata Mesajı: {e.Message}");

            // Acil durum cevabı (Oyun donmasın diye)
            calculatedAnswer = 0;
            finalQuestionText += " (Hata: Cevap 0)";
        }

        // 4. Gönder
        if (AnswerManager.instance != null)
        {
            AnswerManager.instance.SetQuestion(finalQuestionText, calculatedAnswer, type);
        }
    }

    // --- CEZA KÖŞESİ (RASTGELE SORU) ---
    public void AskRandomNormalQuestion()
    {
        // Joker, Start, Hard, Penalty HARİÇ diğerlerinden rastgele seç
        TileType[] validTypes = {
            TileType.Red,
            TileType.Blue,
            TileType.Green,
            TileType.Yellow,
            TileType.Purple,
            TileType.Orange
        };

        int randIndex = Random.Range(0, validTypes.Length);
        TileType selectedType = validTypes[randIndex];

        Debug.Log("Ceza Köşesi: Rastgele Soru Türü -> " + selectedType);

        SoruOlusturVeSor(selectedType);
    }
}