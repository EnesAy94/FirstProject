using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    public Transform target;       // Takip edilen oyuncu
    public float followSpeed = 5f; // Takip hızı
    public float rotateSpeed = 2f; // Köşeleri dönme hızı

    // Başlangıçtaki ayarları "Kutsal" kabul edip saklıyoruz
    private Vector3 initialOffset;      // Başlangıçtaki konum farkı
    private Quaternion initialRotation; // Başlangıçtaki açı

    private float targetYRotation = 0f;  // Hedeflenen Y açısı (0, 90, 180...)
    private float currentYRotation = 0f; // Şu anki Y açısı

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (target != null)
        {
            // 1. Oyun başladığı an, kamera oyuncuya göre nerede duruyor?
            // Bunu kaydediyoruz. Artık "0. derece" burasıdır.
            initialOffset = transform.position - target.position;

            // 2. Kameranın başlangıçtaki açısı nedir?
            // Bunu da kaydediyoruz.
            initialRotation = transform.rotation;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // A. Açıyı Yumuşakça Hesapla (0 -> 90 -> 180...)
        currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, rotateSpeed * Time.deltaTime);

        // B. Dönüş İşlemi (En Kritik Nokta)
        // Başlangıç duruşunun üzerine "currentYRotation" kadar dönüş ekliyoruz.
        Quaternion turnRotation = Quaternion.Euler(0, currentYRotation, 0);

        // --- 1. POZİSYON HESABI ---
        // Başlangıçtaki mesafeyi (initialOffset), dönüş açımız kadar döndürüyoruz.
        // Böylece kamera oyuncunun etrafında yörüngede dönüyor.
        Vector3 rotatedOffset = turnRotation * initialOffset;
        
        Vector3 finalPosition = target.position + rotatedOffset;

        // --- 2. ROTASYON HESABI ---
        // Kameranın kendi açısını da (initialRotation) aynı miktarda döndürüyoruz.
        // Böylece kamera hem konum olarak dönüyor hem de bakış yönü olarak dönüyor.
        Quaternion finalRotation = turnRotation * initialRotation;

        // --- 3. UYGULAMA ---
        transform.position = Vector3.Lerp(transform.position, finalPosition, followSpeed * Time.deltaTime);
        transform.rotation = finalRotation;
    }

    public void ChangeTarget(Transform newTarget)
    {
        target = newTarget;
        // Hedef değişirse yeni hedefe göre offseti güncellemek gerekebilir
        // Ama şimdilik tek karakter olduğu için sorun yok.
    }

    // PlayerMovement buradan çağıracak (0, 90, 180...)
    public void SetRotation(float angle)
    {
        targetYRotation = angle;
    }
}