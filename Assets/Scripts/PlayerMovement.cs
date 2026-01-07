using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Route currentRoute;
    // public QuestionManager soruYoneticisi; // GEREK YOK: GameManager hallediyor

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
            routePosition++;

            // Oyun Bitiş / Tur Başa Dönüş Kontrolü
            if (routePosition >= currentRoute.childNodes.Count)
            {
                routePosition = 0;
                Debug.Log("🎉 TUR TAMAMLANDI! Başa dönülüyor...");
                // Buraya ileride Level Bitiş kontrolü eklenebilir
                
                // Piyonu fiziksel olarak başa ışınla veya yürüt
                transform.position = currentRoute.childNodes[0].position;
            }

            Vector3 nextPos = currentRoute.childNodes[routePosition].position;
            while (MoveToNextNode(nextPos)) { yield return null; }

            yield return new WaitForSeconds(0.1f);
            steps--;
        }

        // --- HAREKET BİTTİ ---
        isMoving = false;
        
        // YENİ SİSTEM:
        // Artık burada switch-case ile uğraşmıyoruz.
        // Topu GameManager'a atıyoruz, o ne yapacağını biliyor.
        CheckCurrentTile(); 
    }

    bool MoveToNextNode(Vector3 goal)
    {
        return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 5f * Time.deltaTime));
    }

    void CheckCurrentTile()
    {
        // 1. Durduğumuz kareyi bul
        if (routePosition < currentRoute.childNodes.Count)
        {
            Tile currentTile = currentRoute.childNodes[routePosition].GetComponent<Tile>();

            if (currentTile != null)
            {
                // 2. GameManager'a "Ben buraya indim, gereğini yap" de.
                GameManager.instance.OnPlayerLanded(currentTile);
            }
            else
            {
                Debug.LogWarning("⚠️ HATA: Bu karede Tile scripti yok!");
            }
        }
    }

    public void StartMoving()
    {
        if(!isMoving)
        {
            StartCoroutine(Move());
        }
    }

    // İleride görevlerden veya kartlardan gelen bonus hareketler için
    public void BonusMove(int amount)
    {
        // Şimdilik sadece log, ileride burayı dolduracağız
        Debug.Log("Piyon " + amount + " kare ileri/geri gidiyor.");
    }
}