using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JokerItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Button useButton;

    private JokerType myType;

    // Manager bu fonksiyonu çağırıp kartı oluşturacak
    public void Setup(JokerData data, int count)
    {
        myType = data.type;

        // Görselleri ayarla
        if (iconImage != null) iconImage.sprite = data.icon;
        if (countText != null) countText.text = "x" + count;

        // Tıklama özelliği
        useButton.onClick.RemoveAllListeners();

        if (data.isUsableFromInventory)
        {
            // Eğer envanterden kullanılabiliyorsa (Streak gibi)
            useButton.interactable = true;
            useButton.onClick.AddListener(() => 
            {
                JokerManager.instance.UseJokerFromInventory(myType);
            });
        }
        else
        {
            // Kullanılamıyorsa (İkinci Şans gibi, pasif durur)
            useButton.interactable = false; 
        }
    }
}