using UnityEngine;
using System.Collections.Generic; // Listeler için
using TMPro; // Ekrana "Sıra Mavi'de" yazdırmak istersen

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Her yerden ulaşmak için

    [Header("Oyuncular")]
    public PlayerMovement[] players; // Oyuncu listesi (P1, P2...)
    public int currentPlayerIndex = 0; // Şu an sıra kimde? (0=Kırmızı, 1=Mavi)

    [Header("UI")]
    public TextMeshProUGUI turnText; // Ekranda "Sıra: OYUNCU 1" yazsın

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (CameraManager.instance != null && players.Length > 0)
        {
            CameraManager.instance.target = players[currentPlayerIndex].transform;
        }
        UpdateTurnUI();
    }

    // Sıradaki oyuncuyu veren fonksiyon (Zar sistemi bunu kullanacak)
    public PlayerMovement GetActivePlayer()
    {
        return players[currentPlayerIndex];
    }

    // Sırayı diğerine geçiren fonksiyon
    public void SwitchTurn()
    {
        // Sırayı bir artır
        currentPlayerIndex++;

        // Eğer son oyuncuyu geçtiyse başa dön (Modülo işlemi)
        if (currentPlayerIndex >= players.Length)
        {
            currentPlayerIndex = 0;
        }

        Debug.Log("🔄 Sıra Değişti! Yeni Sıra: " + players[currentPlayerIndex].name);
        
        UpdateTurnUI();
        if (CameraManager.instance != null)
        {
            CameraManager.instance.ChangeTarget(players[currentPlayerIndex].transform);
        }
    }

    void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = "SIRA: " + players[currentPlayerIndex].name;
        }
    }
}