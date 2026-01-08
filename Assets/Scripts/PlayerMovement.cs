using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Route currentRoute;

    int routePosition = 0;
    public int steps = 0;
    public bool isMoving = false;
    bool gameFinished = false;

    void Update()
    {
        if (gameFinished) return;
    }

    IEnumerator Move()
    {
        if (isMoving) yield break;
        isMoving = true;

        while (steps > 0)
        {
            // Bir sonraki kareye geçmek için indeksi artır
            routePosition++;

            // --- YENİ DÖNGÜ MANTIĞI (Işınlanmayı Çözen Kısım) ---
            
            // Eğer liste sonuna geldiysek (Örn: 40. kareye geldik ama liste 0-39 arası)
            // Hedefimiz 0. kare (Başlangıç) olmalı.
            // Ama routePosition'ı hemen 0 yapmıyoruz, önce oraya yürüsün istiyoruz.
            
            // Modulo (%) işlemi ile hedef indeksi buluyoruz.
            // Örn: routePosition 40 ise ve Count 40 ise -> 40 % 40 = 0 olur.
            int nextNodeIndex = routePosition % currentRoute.childNodes.Count;

            Vector3 nextPos = currentRoute.childNodes[nextNodeIndex].position;
            
            // Oraya kadar YÜRÜ (Işınlanma yok, while döngüsü ile kayarak gidiyor)
            while (MoveToNextNode(nextPos)) { yield return null; }

            // Yürüme bitti, şimdi eğer turu tamamladıysak ana değişkeni sıfırlayalım
            if (routePosition >= currentRoute.childNodes.Count)
            {
                routePosition = 0; 
                Debug.Log("🔄 Tur tamamlandı, başa dönüldü!");
            }

            // -----------------------------------------------------

            yield return new WaitForSeconds(0.1f); // Her karede minik bekleme
            steps--;
        }

        // --- HAREKET BİTTİ ---
        isMoving = false;
        
        CheckCurrentTile(); 
    }

    bool MoveToNextNode(Vector3 goal)
    {
        return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 5f * Time.deltaTime));
    }

    void CheckCurrentTile()
    {
        // Güvenlik kontrolü: Liste dışına taşma olmasın
        int safeIndex = routePosition % currentRoute.childNodes.Count;
        
        Tile currentTile = currentRoute.childNodes[safeIndex].GetComponent<Tile>();

        if (currentTile != null)
        {
            GameManager.instance.OnPlayerLanded(currentTile);
        }
        else
        {
            Debug.LogWarning("⚠️ HATA: Bu karede Tile scripti yok!");
        }
    }

    public void StartMoving()
    {
        if(!isMoving)
        {
            StartCoroutine(Move());
        }
    }

    public void BonusMove(int amount)
    {
        // İleride burayı dolduracağız (Geri gitme vs.)
        Debug.Log("Bonus Hareket: " + amount);
    }
}