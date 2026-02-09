using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// 1. ADIM: Listeye 'Joker' ekledik
public enum TileType 
{ 
    Empty, 
    Red,      
    Blue,   
    Yellow,   
    Purple,  
    Green,     
    Hard,
    Start,
    Penalty,
    Joker // YENİ: Artık Joker kendine ait bir kimliğe sahip!
}

public class Tile : MonoBehaviour
{
    [Header("Ayarlar")]
    public TileType type; 

    [Header("Bileşen Bağlantıları")]
    public MeshRenderer tileRenderer; 
    public GameObject canvasObject;   
    public TextMeshProUGUI titleText; 
    public Image iconImage;           

    [Header("Tema Ayarları")]
    public List<TileTheme> themes; 

    void OnValidate()
    {
        UpdateVisuals();
    }

    void Start()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        TileTheme theme = themes.Find(x => x.type == type);

        if (theme != null)
        {
            if (tileRenderer != null && theme.material != null)
            {
                tileRenderer.material = theme.material;
            }

            // GÖRSEL AYARLAMA MANTIĞI
            if (type == TileType.Empty)
            {
                // Empty ise yazıyı gizle, sadece ikon varsa göster (veya gizle)
                if (canvasObject != null) canvasObject.SetActive(true);
                if (titleText != null) titleText.text = ""; 
                if (iconImage != null) iconImage.sprite = theme.icon; 
            }
            else 
            {
                // JOKER BURAYA GİRER (Diğer renkler gibi)
                // Böylece "JOKER" yazısı ve Hediye Kutusu ikonunu gösterebilirsin.
                if (canvasObject != null) canvasObject.SetActive(true);
                if (titleText != null) titleText.text = theme.title;
                if (iconImage != null) iconImage.sprite = theme.icon;
            }
        }
    }
}

[System.Serializable]
public class TileTheme
{
    public string name; 
    public TileType type;
    public Material material;
    public Sprite icon;
    public string title;
}