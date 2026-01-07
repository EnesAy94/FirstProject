using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [Header("Hedef ve Takip")]
    public Transform target; 
    public float smoothSpeed = 0.125f; 
    
    [Header("Çerçeve Kaydırma (İnce Ayar)")]
    // Oyuncuyu ekranda nereye iteklemek istiyorsan buraya yaz
    public Vector3 framingOffset; 

    [Header("Giriş Animasyonu")]
    public bool playIntro = true;
    public float introDuration = 3.0f;
    public float startZoomMultiplier = 2.0f;

    private Vector3 initialOffset; 
    private Vector3 velocity = Vector3.zero;
    private bool isIntroPlaying = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (target != null)
        {
            initialOffset = transform.position - target.position;
        }

        if (playIntro)
        {
            StartCoroutine(IntroAnimation());
        }
    }

    void LateUpdate()
    {
        if (target == null || isIntroPlaying) return;

        // Normal oyun sırasındaki hedef:
        Vector3 desiredPosition = target.position + initialOffset + framingOffset;
        
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        
        transform.position = smoothedPosition;
    }

    public void ChangeTarget(Transform newTarget)
    {
        target = newTarget;
    }

    IEnumerator IntroAnimation()
    {
        isIntroPlaying = true;

        // --- DÜZELTME BURADA YAPILDI ---
        
        // Animasyonun VARACAĞI nokta (Kaydırma payı DAHİL)
        Vector3 finalDestinationOffset = initialOffset + framingOffset;

        // Animasyonun BAŞLAYACAĞI nokta (Varış noktasına göre uzakta)
        // Böylece açı bozulmadan direkt oraya zoom yapar
        Vector3 startPositionOffset = finalDestinationOffset * startZoomMultiplier; 

        // Kamerayı başlangıç noktasına ışınla
        transform.position = target.position + startPositionOffset;

        float elapsedTime = 0f;

        while (elapsedTime < introDuration)
        {
            float t = elapsedTime / introDuration;
            // Smoothstep formülü (Daha yumuşak hızlanıp yavaşlama)
            t = t * t * (3f - 2f * t); 

            // Uzak noktadan -> Final (Framing dahil) noktaya süzül
            Vector3 currentOffset = Vector3.Lerp(startPositionOffset, finalDestinationOffset, t);
            
            transform.position = target.position + currentOffset;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Animasyon bitince tam yerine oturt
        transform.position = target.position + finalDestinationOffset;
        isIntroPlaying = false;
    }
}