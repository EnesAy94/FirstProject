using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class JokerCardPickUI : MonoBehaviour
{
    [Header("Kart Bileşenleri")]
    public GameObject cardBackObj;
    public GameObject cardFrontObj;

    [Header("Ön Yüz Detayları")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    [Header("Sadece Envanter İçin")]
    public TextMeshProUGUI countText; // "x3" yazısı için (Prefab'a eklemen lazım)

    [Header("Durum")]
    public Button cardButton;
    private JokerData myData;
    private bool isFlipped = false;
    private bool isInventoryMode = false; // Hangi moddayız?

    // --- MOD 1: KART SEÇME EKRANI İÇİN KURULUM ---
    public void SetupForSelection(JokerData data, System.Action<JokerData> onCardClicked)
    {
        myData = data;
        isInventoryMode = false;
        isFlipped = false;
        cardButton.interactable = true;

        // Görünüm: Kapalı Başla
        cardBackObj.SetActive(true);
        cardFrontObj.SetActive(false);
        if (countText) countText.gameObject.SetActive(false); // Adet yazısını gizle

        // Düzgün rotasyon
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Verileri doldur
        FillData(data);

        // Tıklama: Kartı Çevir
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(() =>
        {
            if (!isFlipped)
            {
                FlipCard(onCardClicked);
            }
        });
    }

    // --- MOD 2: ENVANTER EKRANI İÇİN KURULUM ---
    public void SetupForInventory(JokerData data, int count)
    {
        myData = data;
        isInventoryMode = true;
        isFlipped = true; // Zaten açık sayılır
        cardButton.interactable = true;

        // Görünüm: Açık Başla
        cardBackObj.SetActive(false);
        cardFrontObj.SetActive(true);

        // Adet Yazısını Göster
        if (countText)
        {
            countText.gameObject.SetActive(true);
            countText.text = "x" + count;
        }

        // Düzgün rotasyon (Dönmüş kalmasın diye sıfırla)
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Verileri doldur
        FillData(data);

        // Tıklama: Kullanmaya Çalış
        cardButton.onClick.RemoveAllListeners();

        // Eğer envanterden kullanılabilir bir jokersa (Streak gibi)
        if (data.isUsableFromInventory)
        {
            cardButton.interactable = true;
            cardButton.onClick.AddListener(() =>
            {
                // Hafif tıklama efekti
                transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.1f);
                JokerManager.instance.UseJokerFromInventory(myData.type);
            });
        }
        else
        {
            // Pasif kart (Sadece bilgi verir)
            cardButton.interactable = false;
        }
    }

    void FillData(JokerData data)
    {
        if (iconImage) iconImage.sprite = data.icon;
        if (nameText) nameText.text = data.jokerName;
        if (descText) descText.text = data.description;
    }

    // --- ANİMASYON (Sadece Seçim Modunda Çalışır) ---
    void FlipCard(System.Action<JokerData> onComplete)
    {
        isFlipped = true;
        cardButton.interactable = false;

        transform.localRotation = Quaternion.Euler(0, 0, 0);

        transform.DOLocalRotate(new Vector3(0, 90, 0), 0.2f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                cardBackObj.SetActive(false);
                cardFrontObj.SetActive(true);

                transform.localEulerAngles = new Vector3(0, -90, 0);

                transform.DOLocalRotate(Vector3.zero, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        transform.localEulerAngles = Vector3.zero;
                        // Animasyon bitince Manager'a haber ver
                        if (onComplete != null) onComplete.Invoke(myData);
                    });
            });
    }
}