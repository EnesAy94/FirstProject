using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public Image diceImage;      // Ekrandaki Zar Resmi
    public Sprite[] diceSprites; // 6 Tane Zar Resmi (Inspector'dan atacağız)

    private bool isRolling = false; // Şu an zar dönüyor mu?

    // Butona basınca bu çalışacak
    // Butona basınca bu çalışacak
    public void RollDice()
    {
        if (isRolling) return; // Zaten zar dönüyorsa basamasın

        // --- DÜZELTME BURADA ---
        // Önce sıradaki oyuncuyu buluyoruz
        PlayerMovement currentActivePlayer = GameManager.instance.GetActivePlayer();

        // Eğer o oyuncu hala yürüyorsa zar atma
        if (currentActivePlayer.isMoving) return;
        // -----------------------

        StartCoroutine(RollTheDice());
    }

    IEnumerator RollTheDice()
    {
        isRolling = true;

        // ... (Resim döndürme animasyonu aynı kalsın) ...
        for (int i = 0; i < 20; i++)
        {
            int randomFace = Random.Range(0, 6); // 0 ile 5 arası rastgele index
            diceImage.sprite = diceSprites[randomFace];

            // Her değişimde biraz bekle (Giderek yavaşlasın istersen süreyi artırabilirsin)
            yield return new WaitForSeconds(0.05f);
        }

        int result = Random.Range(1, 7);
        diceImage.sprite = diceSprites[result - 1];

        // --- DEĞİŞİKLİK BURADA ---
        // 1. GameManager'dan şu an sıranın kimde olduğunu öğren
        PlayerMovement activePlayer = GameManager.instance.GetActivePlayer();

        // 2. O oyuncuyu yürüt
        activePlayer.steps = result;
        activePlayer.StartMoving();

        isRolling = false;
    }
}