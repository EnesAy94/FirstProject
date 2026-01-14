using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public Route currentRoute;

    public int routePosition = 0;
    public int steps = 0;
    public bool isMoving = false;

    // Kameranın kenar takibi için hafıza
    private int currentSideIndex = -1;

    void Start()
    {
        // Oyun başladığında kameranın doğru açıda durması için
        if (currentRoute != null && currentRoute.childNodes.Count > 0)
        {
            CalculateSideAndRotate();
        }
    }

    // NORMAL ZAR HAREKETİ
    IEnumerator Move()
    {
        if (isMoving) yield break;
        isMoving = true;

        while (steps > 0)
        {
            // İleri Git
            routePosition++;

            // Harita sonuna gelince başa sar
            routePosition %= currentRoute.childNodes.Count;

            Vector3 nextPos = currentRoute.childNodes[routePosition].position;

            // Kamera Kontrolü
            CalculateSideAndRotate();

            // Fiziksel Hareket
            while (MoveToNextNode(nextPos)) { yield return null; }

            yield return new WaitForSeconds(0.1f);
            steps--;
        }

        isMoving = false;

        // Hareket bitince durduğumuz kareyi kontrol et
        CheckCurrentTile();
    }

    bool MoveToNextNode(Vector3 goal)
    {
        return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 10f * Time.deltaTime));
    }

    // --- KAMERA AÇISINI HESAPLAYAN FONKSİYON ---
    void CalculateSideAndRotate()
    {
        int totalTiles = currentRoute.childNodes.Count;
        int sideLength = totalTiles / 4;

        int newSideIndex = Mathf.Min(routePosition / sideLength, 3);

        if (newSideIndex != currentSideIndex)
        {
            currentSideIndex = newSideIndex;
            float targetAngle = currentSideIndex * 90f;

            if (CameraManager.instance != null)
            {
                CameraManager.instance.SetRotation(targetAngle);
            }
        }
    }

    // --- KARE KONTROLÜ ---
    void CheckCurrentTile()
    {
        int safeIndex = routePosition % currentRoute.childNodes.Count;
        Tile currentTile = currentRoute.childNodes[safeIndex].GetComponent<Tile>();

        if (currentTile != null)
        {
            if (currentTile.type == TileType.Penalty)
            {
                LevelManager.instance.EnterPenaltyZone();
            }
            else if (currentTile.type == TileType.Hard)
            {
                // DEĞİŞEN KISIM: Direkt soru sorma, LevelManager'a bildir
                LevelManager.instance.EnterHardZone();
            }
            else if (currentTile.type == TileType.Start || currentTile.type == TileType.Empty)
            {
                LevelManager.instance.SetDiceInteractable(true);
            }
            else
            {
                QuestionManager.instance.SoruOlusturVeSor(currentTile.type);
            }
        }
    }

    public void StartMoving()
    {
        if (!isMoving)
        {
            // Hareket başlayınca zarı kilitle
            if (LevelManager.instance != null) LevelManager.instance.SetDiceInteractable(false);

            StartCoroutine(Move());
        }
    }

    // --- DEĞİŞEN KISIM BURASI ---
    // Artık doğru/yanlış bilince piyonu oynatmıyoruz.
    // Fonksiyon duruyor (AnswerManager hata vermesin diye) ama içi boş.
    public void BonusMove(int amount)
    {
        // İÇİNİ BOŞALTTIK
        // İstersen log bırakabilirsin:
        // Debug.Log($"Bonus hareket (Puan sistemi aktif): {amount} birimlik hareket iptal edildi.");
    }
}