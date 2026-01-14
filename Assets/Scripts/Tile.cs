using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
    Penalty
}

public class Tile : MonoBehaviour
{
    [Header("Ayarlar")]
    // BURAYI DEĞİŞTİRDİK: 'currentType' yerine eski kodunla uyumlu olsun diye 'type' yaptık.
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
        // Burada da 'type' değişkenini kullanıyoruz
        TileTheme theme = themes.Find(x => x.type == type);

        if (theme != null)
        {
            if (tileRenderer != null && theme.material != null)
            {
                tileRenderer.material = theme.material;
            }

            // 'type' kontrolü
            if (type == TileType.Empty)
            {
                if (canvasObject != null) canvasObject.SetActive(true);
                if (titleText != null) titleText.text = ""; 
                if (iconImage != null) iconImage.sprite = theme.icon; 
            }
            else 
            {
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