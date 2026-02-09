using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Template", menuName = "Quiz System/Question Template")]
public class QuestionTemplate : ScriptableObject
{
    [Header("Soru Metni")]
    [Tooltip("Sayıların geleceği yerlere {0}, {1}, {2}... yaz. Örn: Ali'nin {0} elması, Veli'nin {1} armudu var. Toplam {2} kişiyle paylaştılar.")]
    [TextArea]
    public string questionText;

    [Header("Matematik Formülü")]
    [Tooltip("Metindeki sayılarla yapılacak işlemi buraya yaz. Örn: {0} + {1} * {2}")]
    public string formula;

    [Header("Sayı Aralıkları")]
    // Her bir {x} için ayrı min-max değeri girebileceksin
    public List<Vector2Int> variableRanges;
    // Örn: 
    // Element 0 ({0} için): Min 10, Max 50
    // Element 1 ({1} için): Min 1, Max 5
    // Element 2 ({2} için): Min 2, Max 4
}