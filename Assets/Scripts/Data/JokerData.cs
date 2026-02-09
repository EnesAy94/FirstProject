using UnityEngine;

[CreateAssetMenu(fileName = "New Joker", menuName = "Joker System/Joker Data")]
public class JokerData : ScriptableObject
{
    public JokerType type;       // Hangi tür joker?
    public string jokerName;     // Örn: "Seri Kurtarıcı"
    public Sprite icon;          // Kartın resmi
    [TextArea]
    public string description;   // Örn: "Bozulan serini geri getirir."
    
    // Bu joker envanterden tıklanarak kullanılabilir mi?
    // (Mesela Renk jokeri hemen kullanılıyor, İkinci Şans ise sadece yanlış yapınca çıkıyor.
    // Ama Streak jokeri envanterden basınca çalışıyor.)
    public bool isUsableFromInventory; 
}