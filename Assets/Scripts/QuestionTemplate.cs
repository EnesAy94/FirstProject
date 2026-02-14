using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Template", menuName = "Quiz System/Question Template")]
public class QuestionTemplate : ScriptableObject
{
    [Header("Soru Metni")]
    [TextArea]
    public string questionText;

    [Header("Matematik Formülü")]
    [Tooltip("Örn: {0} + {1} * {2} - {3} / {4}")]
    public string formula; 

    [Header("Sayı Aralıkları")]
    public List<Vector2Int> variableRanges;

    [Header("Özel Kurallar")]
    [Tooltip("Aktif edilirse, üretilen sayı kendi Min değerinin katları şeklinde artar. Örn: Min 1000 ise (1000, 2000, 3000) üretir.")]
    public bool useMinMultiplierRule;

    [Header("Bölme Kuralları (Karmaşık İşlemler İçin)")]
    [Tooltip("Hangi sayı hangisine bölünüyor? X=Bölünenin İndeksi, Y=Bölenin İndeksi")]
    
    public List<Vector2Int> divisionPairs;
}