using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JokerConfirmationPanel : MonoBehaviour
{
    public static JokerConfirmationPanel instance;

    [Header("UI")]
    public GameObject panelObj;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Button yesButton;
    public Button noButton;

    void Awake()
    {
        instance = this;
        if (panelObj) panelObj.SetActive(false);
    }

    public void ShowPanel(string title, string desc, System.Action onYes, System.Action onNo)
    {
        panelObj.SetActive(true);
        titleText.text = title;
        descText.text = desc;

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            panelObj.SetActive(false);
            onYes.Invoke();
        });

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() =>
        {
            panelObj.SetActive(false);
            onNo.Invoke();
        });
    }
}