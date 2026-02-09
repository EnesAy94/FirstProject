using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public Route currentRoute;

    // DEĞİŞİKLİK 1: İsmi 'currentTileIndex' yaptık. 
    // Diğer scriptler (JokerManager, LevelManager) konumu bu isimle arıyor.
    public int currentTileIndex = 0;

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

    // NORMAL HAREKET (Zar veya Joker ile tetiklenir)
    IEnumerator Move()
    {
        if (isMoving) yield break;
        isMoving = true;

        while (steps > 0)
        {
            // İleri Git
            currentTileIndex++;

            // Harita sonuna gelince başa sar
            currentTileIndex %= currentRoute.childNodes.Count;

            Vector3 nextPos = currentRoute.childNodes[currentTileIndex].position;

            // Kamera Kontrolü
            CalculateSideAndRotate();

            // Fiziksel Hareket (Yürüme Efekti)
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

        int newSideIndex = Mathf.Min(currentTileIndex / sideLength, 3);

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

    // --- KARE KONTROLÜ (CheckCurrentTile) ---
    void CheckCurrentTile()
    {
        int safeIndex = currentTileIndex % currentRoute.childNodes.Count;
        Transform currentNode = currentRoute.childNodes[safeIndex];

        // Kutunun scriptine ulaşıyoruz
        Tile currentTile = currentNode.GetComponent<Tile>();

        if (currentTile != null)
        {
            // --- JOKER KONTROLÜ (YENİ VE HIZLI YÖNTEM) ---
            if (currentTile.type == TileType.Joker)
            {
                Debug.Log("🎁 Joker Kutusuna Geldin!");

                // JokerManager varsa jokeri ver
                if (JokerManager.instance != null)
                {
                    JokerManager.instance.EarnRandomJoker();
                }

                // Zarı tekrar aktif et (Soru sormayacağız)
                if (LevelManager.instance != null)
                    LevelManager.instance.SetDiceInteractable(true);

                return; // Çıkış yap, aşağı inip soru sorma
            }
            // ----------------------------------------------

            else if (currentTile.type == TileType.Penalty)
            {
                LevelManager.instance.EnterPenaltyZone();
            }
            else if (currentTile.type == TileType.Hard)
            {
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

    public void BonusMove(int amount)
    {
        // Boş bırakıyoruz (Puan sisteminde bonus hareket yok)
    }

    // --- JOKER HAREKET SİSTEMİ (En Temiz Hali) ---
    public void GoToNearestColor(TileType targetType)
    {
        // Elimizde zaten rota var (currentRoute), tekrar liste yapmaya gerek yok!
        var allNodes = currentRoute.childNodes;

        int targetIdx = -1;

        // Bulunduğumuz yerden sona kadar tara
        for (int i = currentTileIndex + 1; i < allNodes.Count; i++)
        {
            // Kutunun içindeki Tile scriptine bak
            Tile tile = allNodes[i].GetComponent<Tile>();

            if (tile != null && tile.type == targetType)
            {
                targetIdx = i;
                break;
            }
        }

        // Hedef bulunduysa
        if (targetIdx != -1)
        {
            // Kaç adım gitmesi gerektiğini hesapla
            int stepsToWalk = targetIdx - currentTileIndex;

            Debug.Log($"🏃 {targetType} rengine gidiliyor. Atılacak adım: {stepsToWalk}");

            // Zarı biz atmışız gibi ayarla ve yürüt
            this.steps = stepsToWalk;
            StartMoving();
        }
        else
        {
            // LevelManager üzerinden uyarı ver
            if (LevelManager.instance != null)
            {
                LevelManager.instance.ShowNotification("ÜZGÜNÜM", "İleride bu renkte kutu kalmadı!", () => { });
            }
        }
    }
}