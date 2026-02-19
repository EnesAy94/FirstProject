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
    // --- MOD 2: ENVANTER EKRANI İÇİN KURULUM (GÜNCELLENDİ: Onay Paneli) ---
    public void SetupForInventory(JokerData data, int count)
    {
        myData = data;
        isInventoryMode = true;
        isFlipped = true;
        cardButton.interactable = true;

        cardBackObj.SetActive(false);
        cardFrontObj.SetActive(true);

        if (countText)
        {
            countText.gameObject.SetActive(true);
            countText.text = "x" + count;
        }

        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        FillData(data);

        cardButton.onClick.RemoveAllListeners();

        // --- BURASI DEĞİŞTİ ---

        // DURUM A: Kullanılabilir Jokerler (Renge Git, Can Doldur vb.)
        if (data.isUsableFromInventory)
        {
            cardButton.interactable = true;
            cardButton.onClick.AddListener(() =>
            {
                // Tıklama efekti
                transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.1f);

                // --- ÖZEL KONTROL: CAN DOLDURMA ---
                if (myData.type == JokerType.ScoreBoost)
                {
                    if (LevelManager.instance != null &&
                        LevelManager.instance.currentScore >= LevelManager.instance.currentChapter.startingScore)
                    {
                        LevelManager.instance.ShowNotification("CANIN DOLU!", "Turp gibisin, harcama!", () => { });
                        return;
                    }
                }

                // ONAY PANELİ AÇ
                if (JokerConfirmationPanel.instance != null)
                {
                    string desc = "Bu jokeri şimdi kullanmak istiyor musun?";
                    if (myData.type == JokerType.ColorMove)
                    {
                        desc = "İstediğin renkteki en yakın kutuya ışınlanmak ister misin?";
                    }
                    else if (myData.type == JokerType.ScoreBoost && LevelManager.instance != null)
                    {
                        int penalty = LevelManager.instance.GetCurrentPenalty();
                        int amount = penalty / 2;
                        if (amount < 1) amount = 1;
                        desc = $"Canını +{amount} puan artırmak istiyor musun?";
                    }

                    JokerConfirmationPanel.instance.ShowPanel(myData.jokerName.ToUpper(), desc,
                        () => { JokerManager.instance.UseJokerFromInventory(myData.type); }, // EVET
                        () => { } // HAYIR
                    );
                }
            });
        }
        // DURUM B: Pasif Eşyalar (Bilet gibi)
        else
        {
            cardButton.interactable = true; // Tıklanabilsin ama işlem yapmasın
            cardButton.onClick.AddListener(() =>
            {
                // Sadece bilgi ver
                if (RobotAssistant.instance != null)
                {
                    if (myData.type == JokerType.WheelTicket)
                        RobotAssistant.instance.Say("Bu bir Şans Bileti! Bölümü bitirince hesabına eklenecek.", 3f);
                    else
                        RobotAssistant.instance.Say("Bu eşya şu an kullanılamaz.", 2f);
                }
            });
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