using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Oyuncu Ayarları")]
    public PlayerMovement player; // Artık dizi [] yok, tek oyuncu var
    
    // Sıra (Turn) değişkenleri silindi çünkü tek kişiyiz.

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Kamera direkt oyuncuya kilitlensin
        if (CameraManager.instance != null && player != null)
        {
            CameraManager.instance.ChangeTarget(player.transform);
        }
    }

    // Diğer scriptler oyuncuya ulaşmak isterse bunu kullanacak
    public PlayerMovement GetPlayer()
    {
        return player;
    }

    // --- KRİTİK NOKTA: OYUNCU HAREKETİ BİTİNCE BU ÇAĞRILACAK ---
    // PlayerMovement scripti, hareket bitince burayı tetikleyecek.
    public void OnPlayerLanded(Tile currentTile)
    {
        if (currentTile == null) return;

        Debug.Log("Oyuncu şu karede durdu: " + currentTile.type);

        // 1. Eğer kare BOŞ ise (Ayak izi/Büyüteç)
        if (currentTile.type == TileType.Empty)
        {
            Debug.Log("Boş alan, bir şey olmuyor.");
            // Burada belki "Boş" sesi çalabilirsin.
            // Zarı tekrar aktif etmek gerekebilir (Bunu ileride DiceManager'da yaparız)
            return;
        }

        // 2. Eğer kare DOLU ise (Kırmızı, Mavi vb.)
        // Eski "Panel Aç" kodları yerine direkt QuestionManager'ı çağırıyoruz.
        QuestionManager.instance.SoruOlusturVeSor(currentTile.type);
    }
}