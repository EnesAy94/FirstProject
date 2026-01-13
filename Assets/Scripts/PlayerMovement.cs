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

    // YENİ: Kameranın o an hangi kenarda olduğunu bilmesi için hafıza
    private int currentSideIndex = -1; 

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
            
            int totalTiles = currentRoute.childNodes.Count;
            int nextNodeIndex = routePosition % totalTiles;
            Vector3 nextPos = currentRoute.childNodes[nextNodeIndex].position;

            // --- TİTREMEYİ ENGELLEYEN MANTIK ---
            
            // 1. Kenar uzunluğunu bul (Toplam / 4)
            int sideLength = totalTiles / 4; 

            // 2. Gideceğimiz karenin hangi kenarda olduğunu hesapla
            // (Mathf.Min kullanarak 4. kenar hatasını önlüyoruz, en fazla 3 olsun)
            int newSideIndex = Mathf.Min(nextNodeIndex / sideLength, 3);

            // 3. SADECE KENAR DEĞİŞTİYSE KAMERAYI DÖNDÜR
            // (Eğer zaten 0. kenardaysam ve yine 0. kenardaki bir kareye gidiyorsam kameraya dokunma)
            if (newSideIndex != currentSideIndex)
            {
                currentSideIndex = newSideIndex; // Yeni kenarı kaydet
                
                float targetAngle = currentSideIndex * 90f; // 0, 90, 180, 270
                
                if (CameraManager.instance != null)
                {
                    CameraManager.instance.SetRotation(targetAngle);
                }
            }
            // ------------------------------------

            while (MoveToNextNode(nextPos)) { yield return null; }

            // Turu tamamlama kontrolü
            if (routePosition >= totalTiles)
            {
                routePosition = 0; 
                Debug.Log("🔄 Tur tamamlandı, başa dönüldü!");
                
                // Tur bitince side index'i sıfırla veya güncelle ki karışmasın
                // (Gerekirse buraya özel bir kamera reset kodu eklenebilir ama şu anki mantık yeterli)
            }

            yield return new WaitForSeconds(0.1f);
            steps--;
        }

        isMoving = false;
        CheckCurrentTile(); 
    }

    bool MoveToNextNode(Vector3 goal)
    {
        return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 5f * Time.deltaTime));
    }

    void CheckCurrentTile()
    {
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
        Debug.Log("Bonus Hareket: " + amount);
    }
}