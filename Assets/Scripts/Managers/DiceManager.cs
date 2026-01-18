using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public Image diceImage;      // Ekrandaki Zar Resmi
    public Sprite[] diceSprites; // 6 Tane Zar Resmi

    [Header("Geliştirici / Test")]
    public int hileliZar = 0; // BURAYA 1-6 arası yazarsan o gelir. 0 yazarsan rastgele.

    private bool isRolling = false; // Şu an zar dönüyor mu?

    // Butona basınca bu çalışacak
    public void RollDice()
    {
        if (LevelManager.instance == null || LevelManager.instance.isLevelFinished) return;
        if (isRolling) return; // Zaten zar dönüyorsa basamasın

        // Artık "Sıradaki Oyuncu" yok, TEK OYUNCU var.
        PlayerMovement player = GameManager.instance.player;

        // Eğer player bulunamazsa veya zaten yürüyorsa işlem yapma
        if (player == null || player.isMoving) return;

        StartCoroutine(RollTheDice());
    }

    IEnumerator RollTheDice()
    {
        isRolling = true;

        // Animasyon (Rastgele resimler dönüyor - Görsel efekt)
        for (int i = 0; i < 20; i++)
        {
            int randomFace = Random.Range(0, 6);
            diceImage.sprite = diceSprites[randomFace];
            yield return new WaitForSeconds(0.05f);
        }

        // --- SONUÇ BELİRLEME (Hile Mantığı Burada) ---
        int result;

        // Eğer hileliZar kutusuna 0'dan büyük bir şey yazdıysan (1, 2, 3.. 6)
        if (hileliZar > 0 && hileliZar <= 6)
        {
            result = hileliZar; // Zorla o sayıyı yap
            Debug.Log("🎲 Hileli Zar Devrede: " + result);
            
            // İşimiz bitti, bir sonraki el tekrar rastgele olsun diye sıfırla
            // (Eğer sürekli 6 gelmesini istersen bu alt satırı sil)
            hileliZar = 0; 
        }
        else
        {
            // Kutu 0 ise normal rastgele sayı üret
            result = Random.Range(1, 7);
        }
        // ---------------------------------------------

        // Ekranda gelen sayının resmini göster
        diceImage.sprite = diceSprites[result - 1];

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