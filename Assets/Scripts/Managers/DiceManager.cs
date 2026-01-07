using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public Image diceImage;      // Ekrandaki Zar Resmi
    public Sprite[] diceSprites; // 6 Tane Zar Resmi

    private bool isRolling = false; // Şu an zar dönüyor mu?

    // Butona basınca bu çalışacak
    public void RollDice()
    {
        if (isRolling) return; // Zaten zar dönüyorsa basamasın

        // --- DÜZELTME BURADA ---
        // Artık "Sıradaki Oyuncu" yok, TEK OYUNCU var.
        PlayerMovement player = GameManager.instance.player;

        // Eğer player bulunamazsa veya zaten yürüyorsa işlem yapma
        if (player == null || player.isMoving) return;
        // -----------------------

        StartCoroutine(RollTheDice());
    }

    IEnumerator RollTheDice()
    {
        isRolling = true;

        // Animasyon (Rastgele resimler dönüyor)
        for (int i = 0; i < 20; i++)
        {
            int randomFace = Random.Range(0, 6);
            diceImage.sprite = diceSprites[randomFace];
            yield return new WaitForSeconds(0.05f);
        }

        // Sonuç Belirleme (1-6 arası)
        int result = Random.Range(1, 7);
        diceImage.sprite = diceSprites[result - 1];

        // --- DEĞİŞİKLİK BURADA ---
        // Tek oyuncuya "Yürü" emri veriyoruz
        PlayerMovement player = GameManager.instance.player;
        
        if (player != null)
        {
            player.steps = result;
            player.StartMoving();
        }

        isRolling = false;
    }
}