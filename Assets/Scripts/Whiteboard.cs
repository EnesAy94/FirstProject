using UnityEngine;
using UnityEngine.InputSystem; 

public class Whiteboard : MonoBehaviour
{
    public RectTransform drawingArea; // Beyaz kağıdımız
    public Material lineMaterial;     
    
    private LineRenderer currentLine;
    private int sortingOrder = 1;

    void Update()
    {
        if (Mouse.current == null) return;

        // Farenin ekran üzerindeki yerini al
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // Tıklama anı
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Eğer fare kağıdın içindeyse çizmeye başla
            if (IsMouseInArea(mouseScreenPos))
            {
                CreateLine();
            }
        }

        // Basılı tutma anı
        if (Mouse.current.leftButton.isPressed && currentLine != null)
        {
            // --- SİHİRLİ KOD BURADA ---
            // Ekran tıklamasını, kağıdın üzerindeki (Local) koordinata çevir
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingArea, mouseScreenPos, Camera.main, out localPoint);
            
            // Çizgiye nokta ekle
            // Z eksenini -0.1f yapıyoruz ki kağıdın hafif önünde dursun (Gömülmesin)
            Vector3 pointPos = new Vector3(localPoint.x, localPoint.y, -1f);

            // Noktalar arası mesafe kontrolü (Çok sık nokta koyup kasmasın)
            if (currentLine.positionCount == 0 || Vector3.Distance(currentLine.GetPosition(currentLine.positionCount - 1), pointPos) > 0.1f)
            {
                currentLine.positionCount++;
                currentLine.SetPosition(currentLine.positionCount - 1, pointPos);
            }
        }

        // Bırakma anı
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            currentLine = null;
        }
    }

    void CreateLine()
    {
        GameObject lineObj = new GameObject("Line");
        // Çizgiyi "DrawingArea"nın çocuğu yapıyoruz, böylece kağıt nereye giderse çizgi de oraya gider
        lineObj.transform.SetParent(drawingArea.transform, false); 
        
        currentLine = lineObj.AddComponent<LineRenderer>();
        currentLine.material = lineMaterial;
        
        // --- ÖNEMLİ AYARLAR ---
        currentLine.useWorldSpace = false; // Dünya değil, kağıt koordinatı kullan
        currentLine.alignment = LineAlignment.TransformZ; // UI üzerinde düzgün görünsün
        
        currentLine.startWidth = 5f; // UI koordinatlarında 0.1 çok ince kalabilir, bunu 5 veya 10 yapıp dene
        currentLine.endWidth = 5f;
        
        currentLine.positionCount = 0;
        currentLine.sortingOrder = sortingOrder++; 
    }

    bool IsMouseInArea(Vector2 screenPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(drawingArea, screenPos, Camera.main);
    }

    public void ClearBoard()
    {
        foreach (Transform child in drawingArea.transform)
        {
            Destroy(child.gameObject);
        }
    }
}