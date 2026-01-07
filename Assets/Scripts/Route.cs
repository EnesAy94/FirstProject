using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Listeleri düzenlemek için gerekli

public class Route : MonoBehaviour
{
    // Yol üzerindeki kareleri tutacak listemiz
    [HideInInspector]
    public List<Transform> childNodes = new List<Transform>();

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        // Eğer liste doluysa çizgileri çiz
        if (childNodes != null && childNodes.Count > 0)
        {
            for (int i = 0; i < childNodes.Count - 1; i++)
            {
                if (childNodes[i] != null && childNodes[i+1] != null)
                {
                    Gizmos.DrawLine(childNodes[i].position, childNodes[i+1].position);
                }
            }
        }
    }

    // BU FONKSİYON SİHİR YAPACAK
    // Inspector'da scriptin adına sağ tıklayınca çıkacak
    [ContextMenu("Yolu Otomatik Doldur")]
    public void FillNodes()
    {
        childNodes.Clear(); // Eski listeyi temizle

        // Board'un altındaki TÜM objelere bak
        Transform[] tumCocuklar = GetComponentsInChildren<Transform>();

        foreach (Transform cocuk in tumCocuklar)
        {
            // 1. Kendisi (Board) olmasın
            // 2. İsminde "Tile" geçsin (Böylece CenterImage veya başka resimler karışmaz!)
            if (cocuk != this.transform && cocuk.name.Contains("Tile"))
            {
                childNodes.Add(cocuk);
            }
        }
        
        Debug.Log("✅ Yol başarıyla bulundu! Toplam Kare: " + childNodes.Count);
    }
}