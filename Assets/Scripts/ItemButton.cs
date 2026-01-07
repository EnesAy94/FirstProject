using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public TextMeshProUGUI isimText;
    public TextMeshProUGUI adetText;
    public TextMeshProUGUI miktarText;
    
    private Button myButton; // Buton bileşeni
    private CanvasGroup canvasGroup; // Soluklaştırmak için (Opsiyonel ama şık durur)

    // Arkada tutacağımız veriler
    private string myEsyaAdi;
    private string myDeger;
    private int myAdet;
    
    private QuestionManager manager;

    void Awake() // Start yerine Awake daha güvenli burada
    {
        myButton = GetComponent<Button>();
        manager = FindFirstObjectByType<QuestionManager>();
        myButton.onClick.AddListener(Secildi);
    }

    // --- YENİ GÜNCELLEME: Butonu Aktif/Pasif Yapma ---
    public void ButonuAyarla(string isim, int adet, string deger, bool isUsed)
    {
        myEsyaAdi = isim;
        myAdet = adet;
        myDeger = deger;

        isimText.text = isim;
        adetText.text = adet.ToString();
        miktarText.text = deger;

        // EĞER KULLANILDIYSA PASİF YAP
        if (isUsed)
        {
            myButton.interactable = false; // Tıklanamaz
            // Yazı rengini veya buton rengini gri yapabilirsin, interactable=false genelde yeter.
            isimText.color = Color.gray; 
        }
        else
        {
            myButton.interactable = true; // Tıklanabilir
            isimText.color = Color.black; // (Veya senin normal rengin neyse)
        }
    }

    void Secildi()
    {
        manager.CevabiHesapla(myEsyaAdi, myDeger, myAdet);
    }
}