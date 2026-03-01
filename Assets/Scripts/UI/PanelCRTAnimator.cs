using UnityEngine;
using DG.Tweening;

public class PanelCRTAnimator : MonoBehaviour
{
    public enum AnimationStyle { CRT, SlideUpFade }

    [Header("CRT Animation Settings")]
    public AnimationStyle style = AnimationStyle.CRT;
    
    [Tooltip("Yatay çizgi olma süresi (Saniye) (Sadece CRT için)")]
    public float lineDuration = 0.15f; 
    
    [Tooltip("Dikey genişleme süresi veya Yukarı Kayma süresi (Saniye)")]
    public float expandDuration = 0.25f;

    [Tooltip("SlideUpFade İçin: Ne kadar aşağıdan başlasın?")]
    public float slideOffset = -150f;

    [Tooltip("Animasyonun uygulanacağı iç panel (Örn: Çerçeve). Boş bırakılırsa bu objenin kendisi canlandırılır.")]
    public RectTransform targetInnerPanel;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = targetInnerPanel != null ? targetInnerPanel : GetComponent<RectTransform>();
        canvasGroup = targetInnerPanel != null ? targetInnerPanel.GetComponent<CanvasGroup>() : GetComponent<CanvasGroup>();
        
        if (canvasGroup == null && rectTransform != null)
        {
            canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void OnEnable()
    {
        PlayOpenAnimation();
    }

    public void PlayOpenAnimation()
    {
        // Varsa devam eden eski animasyonları durdur
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);

        Sequence seq = DOTween.Sequence();

        if (style == AnimationStyle.CRT)
        {
            // Başlangıç durumu: Nokta kadar, ancak saydamlık var
            rectTransform.localScale = new Vector3(0.05f, 0.05f, 1f);
            rectTransform.anchoredPosition = Vector2.zero; // Ortala
            canvasGroup.alpha = 1f;

            // Eğer hedef panel varsa, ana panelin scale'ini bozmamak için 1 yap (arkaplan kararması için)
            if (targetInnerPanel != null)
            {
                transform.localScale = Vector3.one;
            }
            
            // İlk hareket: Yatay ince bir lazer çizgisine dönüş
            seq.Append(rectTransform.DOScaleX(1f, lineDuration).SetEase(Ease.OutExpo));
            
            // İkinci hareket: O ince çizgiden panelin tamamı fırlayarak açılsın
            seq.Append(rectTransform.DOScaleY(1f, expandDuration).SetEase(Ease.OutBack));
        }
        else if (style == AnimationStyle.SlideUpFade)
        {
            // Başlangıç durumu: Saydam ve normalden biraz aşağıda
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition = new Vector2(0, slideOffset);
            canvasGroup.alpha = 0f;

            // Ayn anda hem yukarı kayarak yerine otur (spring etkisi) hem de belirginleş
            seq.Join(rectTransform.DOAnchorPosY(0, expandDuration).SetEase(Ease.OutBack));
            seq.Join(canvasGroup.DOFade(1f, expandDuration * 0.8f).SetEase(Ease.Linear));
        }
    }

    public void ClosePanel()
    {
        // Kapanış başladıysa eskiyi öldür
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);
        
        Sequence seq = DOTween.Sequence();

        if (style == AnimationStyle.CRT)
        {
            // İlk hareket: Hızla sadece yatay ince bir çizgi haline kadar sıkış
            seq.Append(rectTransform.DOScaleY(0.05f, lineDuration).SetEase(Ease.InExpo));
            
            // İkinci hareket: Ortaya doğru nokta şekline gelip kaybol
            seq.Append(rectTransform.DOScaleX(0f, expandDuration).SetEase(Ease.InExpo));
        }
        else if (style == AnimationStyle.SlideUpFade)
        {
            // Normal yerine göre çok az aşağı kayarak ve şeffaflaşarak yok ol
            seq.Join(rectTransform.DOAnchorPosY(slideOffset * 0.5f, expandDuration * 0.7f).SetEase(Ease.InExpo));
            seq.Join(canvasGroup.DOFade(0f, expandDuration * 0.5f).SetEase(Ease.Linear));
        }
        
        // Animasyon bittiğinde ANA objeyi (Arka plan + İç panel) komple kapat
        seq.OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    public void PlayGlitchTransition()
    {
        // Sadece iç panelin alpha'sıyla veya rengiyle ufak bir titreme yapıp veriler değişiyormuş hissi veririz.
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);

        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;

        Sequence seq = DOTween.Sequence();
        
        // Şeffaflık titremesi
        seq.Append(canvasGroup.DOFade(0.2f, 0.05f));
        seq.Append(canvasGroup.DOFade(0.8f, 0.05f));
        seq.Append(canvasGroup.DOFade(0.1f, 0.05f));
        seq.Append(canvasGroup.DOFade(1f, 0.1f));
        
        // İsterseniz ufak bir sarsıntı (punch) dalgası da eklenebilir
        rectTransform.DOPunchScale(new Vector3(0.02f, -0.02f, 0), 0.2f, 10, 1f);
    }

    public void SetOpenedInstantly()
    {
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);
        
        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    public void KillTweens()
    {
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);
    }
}
