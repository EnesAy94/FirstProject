using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Route currentRoute;
    public QuestionManager soruYoneticisi;

    int routePosition = 0;
    public int steps = 0;
    public bool isMoving = false;
    bool gameFinished = false;

    void Update()
    {
        if (gameFinished) return;

        /* Space tuşuna basınca test hareketi (Zar gelince burası değişecek)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !isMoving)
        {
            steps = 1; // Şimdilik 1 adım (İleride zardan gelecek)
            StartCoroutine(Move());
        }*/
    }

    IEnumerator Move()
    {
        if (isMoving) yield break;
        isMoving = true;

        while (steps > 0)
        {
            routePosition++;

            // Oyun Bitiş Kontrolü
            if (routePosition >= currentRoute.childNodes.Count)
            {
                routePosition = 0;
                Debug.Log("🎉 OYUN BİTTİ! KAZANDINIZ! 🎉");
                gameFinished = true;
                if (currentRoute.childNodes.Count > 0)
                {
                    Vector3 finishPos = currentRoute.childNodes[0].position;
                    while (MoveToNextNode(finishPos)) { yield return null; }
                }
                yield break;
            }

            Vector3 nextPos = currentRoute.childNodes[routePosition].position;
            while (MoveToNextNode(nextPos)) { yield return null; }

            yield return new WaitForSeconds(0.1f);
            steps--;
        }

        // --- HAREKET BİTTİ, ŞİMDİ KONTROL ZAMANI ---
        isMoving = false;
        CheckCurrentTile();
    }

    bool MoveToNextNode(Vector3 goal)
    {
        return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 5f * Time.deltaTime));
    }

    // YENİ EKLENEN FONKSİYON: Kareyi Analiz Et
    void CheckCurrentTile()
    {
        // Şu anki karenin (Node) içindeki 'Tile' scriptini bul
        Tile currentTile = currentRoute.childNodes[routePosition].GetComponent<Tile>();

        if (currentTile != null)
        {
            // Hangi tür olduğuna göre işlem yap (Switch-Case)
            switch (currentTile.type)
            {
                case TileType.Empty:
                    Debug.Log("⚪ BOŞ KARE: Bir şey yapma, sıra diğer oyuncuda.");
                    GameManager.instance.SwitchTurn(); // HEMEN SIRA DEĞİŞTİR
                    break;
                case TileType.Blue:
                    Debug.Log("🔵 MAVİ SORU: Matematik sorusu geliyor!");
                    soruYoneticisi.MaviSorusunuHazirla();
                    break;
                case TileType.Red:
                    Debug.Log("🔴 KIRMIZI SORU: Zor soru geliyor!");
                    soruYoneticisi.AntikaSorusunuHazirla();
                    break;
                case TileType.Green:
                    Debug.Log("🟢 YEŞİL SORU: Kolay soru geliyor!");
                    soruYoneticisi.YesilSorusunuHazirla();
                    break;
                case TileType.Yellow:
                    Debug.Log("🟡 SARI SORU: Mantık sorusu geliyor!");
                    soruYoneticisi.SariSorusunuHazirla();
                    break;
                case TileType.Purple:
                    Debug.Log("🟣 MOR SORU: Tarih sorusu geliyor!");
                    soruYoneticisi.MorSorusunuHazirla();
                    break;  
            }
        }
        else
        {
            Debug.LogWarning("⚠️ HATA: Bu karede Tile scripti yok!");
        }
    }
    // Ödül veya Ceza hareketi için
    public void BonusMove(int amount)
    {
        // Şimdilik sadece log atalım, Zar sistemini kurunca burası
        // piyonu fiziksel olarak hareket ettirecek.
        if (amount > 0)
            Debug.Log("Piyon " + amount + " kare ileri gidiyor...");
        else
            Debug.Log("Piyon " + Mathf.Abs(amount) + " kare geri gidiyor...");

        // Buraya ileride 'Move()' fonksiyonunu tekrar çağıracağız.
    }
    public void StartMoving()
    {
        if(!isMoving)
        {
            StartCoroutine(Move());
        }
    }
}