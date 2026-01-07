using UnityEngine;

public class CanvasFixer : MonoBehaviour
{
    private Canvas myCanvas;

    void Awake()
    {
        myCanvas = GetComponent<Canvas>();

        // Oyun başladığı an modu otomatik değiştir
        if (myCanvas != null)
        {
            myCanvas.renderMode = RenderMode.ScreenSpaceCamera; // Modu Camera yap
            myCanvas.worldCamera = Camera.main;                 // Kamerayı bağla
            myCanvas.planeDistance = 1f;                        // Mesafeyi 1 yap
        }
    }
}