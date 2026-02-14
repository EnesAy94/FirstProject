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
    Orange,
    Hard,
    Start,
    Penalty,
    Joker
}

public class Tile : MonoBehaviour
{
    [Header("Ayarlar")]
    public TileType type;

    [Header("Bileşen Bağlantıları (3D/World)")]
    public MeshRenderer tileRenderer;
    public SpriteRenderer iconRenderer; // 3D dünyadaki ikon (Varsa)
    public TextMeshPro nameText;        // 3D dünyadaki yazı (Varsa)

    [Header("Bileşen Bağlantıları (UI/Canvas)")]
    public GameObject canvasObject;
    public TextMeshProUGUI titleText;   // Canvas üzerindeki yazı
    public Image iconImage;             // Canvas üzerindeki ikon

    [Header("Tema Ayarları")]
    public List<TileTheme> themes;

    void OnValidate()
    {
        UpdateVisuals(); // Editörde değişiklik yapınca standart hali gör
    }

    void Start()
    {
        UpdateVisuals(); // Oyun başlayınca önce standart hali yükle
    }

    // 1. VERSİYON: Standart Güncelleme (Hiçbir parametre almaz)
    // Sadece rengi ve varsayılan temayı ayarlar.
    public void UpdateVisuals()
    {
        TileTheme theme = themes.Find(x => x.type == type);

        if (theme != null)
        {
            // Materyal (Renk) Ayarı
            if (tileRenderer != null && theme.material != null)
            {
                tileRenderer.material = theme.material;
            }

            // Varsayılan İkon ve Yazı Ayarı
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

    // 2. VERSİYON: Bölüm Bazlı Güncelleme (LevelManager Çağırır)
    // Bu fonksiyon, yukarıdakinin üstüne yazar. Mekan ismini ve ikonunu değiştirir.
    public void UpdateVisuals(Sprite newIcon, string newName)
    {
        // A) İKON GÜNCELLEME
        if (newIcon != null)
        {
            // Hem UI hem SpriteRenderer hangisi varsa onu güncelle
            if (iconImage != null) iconImage.sprite = newIcon;
            if (iconRenderer != null) iconRenderer.sprite = newIcon;
        }

        // B) İSİM GÜNCELLEME
        if (!string.IsNullOrEmpty(newName))
        {
            // Hem UI hem TextMeshPro hangisi varsa onu güncelle
            if (titleText != null) titleText.text = newName;
            if (nameText != null) nameText.text = newName;
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