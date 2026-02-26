using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

namespace UI
{
    public class StorySwipeController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("Settings")]
        [Tooltip("Ne kadar kaydırınca diğer sayfaya geçsin?")]
        public float swipeThreshold = 50f;
        
        [Tooltip("Geçiş animasyonunun süresi")]
        public float snapDuration = 0.3f;
        
        [Tooltip("DOTween animasyon tipi")]
        public Ease snapEase = Ease.OutQuint;

        [Tooltip("Menü Butonlarının varsayılan Y konumu (Aşağı/Yukarı)")]
        public float defaultYOffset = -50f; // YENİ: Y ekseni ayarı

        [Header("Ignore Settings")]
        [Tooltip("Bu listeye eklediğin objelerin isimlerini içeren paneller kaydırılabilir sayfa olarak listeye ALINMAZ.")]
        public List<string> ignoreNames = new List<string> { "Background", "Scrollbar", "Mask", "Text" };

        private List<RectTransform> stories = new List<RectTransform>();
        private List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
        private int currentIndex = 0;
        
        private float dragStartX;
        private bool isDragging;
        private float screenDistance;
        private bool isInitialized = false;

        private Coroutine initCoroutine;

        // OnEnable tamamen kaldırıldı! 
        // Unity otomatik olarak listeyi aramaya kalkmayacak.
        // Asıl işi public Initialize() yapacak.
        
        void OnDisable()
        {
            KillAllTweens();
        }

        public void Initialize()
        {
            if (initCoroutine != null) StopCoroutine(initCoroutine);
            initCoroutine = StartCoroutine(SafeInitialize());
        }

        private IEnumerator SafeInitialize()
        {
            RectTransform myRect = GetComponent<RectTransform>();
            KillAllTweens();
            CleanInvalidEntries();

            int maxWaitFrames = 5;
            int framesWaited = 0;
            while ((myRect.rect.width <= 0.1f) && (framesWaited < maxWaitFrames))
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(myRect);
                yield return new WaitForEndOfFrame();
                framesWaited++;
            }

            screenDistance = myRect.rect.width;
            
            Debug.Log($"[StorySwipe] screenDistance: {screenDistance}, Screen.width: {Screen.width}, rect: {myRect.rect}");
            
            if (screenDistance <= 0.1f) screenDistance = Screen.width;

            // Diğer Scriptler objeleri ürettikten SONRA bu kod çalışacağı için listeyi şimdi alıyoruz.
            InitializeLists();
            
            Debug.Log($"[StorySwipe] Story count: {stories.Count}");

            ResetPositions();
            isInitialized = true;
        }

        private void KillAllTweens()
        {
            isDragging = false;
            for (int i = 0; i < stories.Count; i++)
            {
                if (stories[i] != null) stories[i].DOKill(true);
                if (canvasGroups[i] != null) canvasGroups[i].DOKill(true);
            }
        }

        private bool ShouldIgnoreChild(string childName)
        {
            foreach (string ignoreName in ignoreNames)
            {
                if (childName.ToLower().Contains(ignoreName.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private void InitializeLists()
        {
            stories.Clear();
            canvasGroups.Clear();

            // Sadece GERÇEK, Yok Edilmemiş (Destroy edilmeyi beklemeyen) Klonları al
            foreach (Transform child in transform)
            {
                if (ShouldIgnoreChild(child.name)) continue;

                RectTransform rt = child.GetComponent<RectTransform>();
                
                // Unity'de Destroy() çağrılan objeler hemen ölmez amakomponentleri/hiyerarşi davranışları değişebilir.
                // Kesin yeni yaratılmış "doğru" listeyi almak için güvenli ekleme:
                if (rt != null)
                {
                    rt.gameObject.SetActive(true); 
                    stories.Add(rt);
                    
                    CanvasGroup cg = child.GetComponent<CanvasGroup>();
                    if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                    canvasGroups.Add(cg);
                }
            }
        }

        private void CleanInvalidEntries()
        {
            for (int i = stories.Count - 1; i >= 0; i--)
            {
                if (stories[i] == null)
                {
                    stories.RemoveAt(i);
                    canvasGroups.RemoveAt(i);
                }
            }
        }

        private void ResetPositions()
        {
            if (stories.Count == 0) return;

            currentIndex = Mathf.Clamp(currentIndex, 0, stories.Count - 1);

            for (int i = 0; i < stories.Count; i++)
            {
                if (stories[i] == null) continue;

                if (i == currentIndex)
                {
                    // Eskiden Vector2.zero yapıyordu. Şimdi x=0, y=-50 yapacak.
                    stories[i].anchoredPosition = new Vector2(0, defaultYOffset);
                    if (canvasGroups[i] != null) canvasGroups[i].alpha = 1f;
                    stories[i].gameObject.SetActive(true);
                    Debug.Log($"[StorySwipe] Story[{i}] ACTIVE at (0,{defaultYOffset})");
                }
                else
                {
                    float defaultOffsetX = (i < currentIndex) ? -screenDistance : screenDistance;
                    stories[i].anchoredPosition = new Vector2(defaultOffsetX, defaultYOffset);
                    if (canvasGroups[i] != null) canvasGroups[i].alpha = 0f;
                    stories[i].gameObject.SetActive(false);
                    Debug.Log($"[StorySwipe] Story[{i}] HIDDEN at ({defaultOffsetX},{defaultYOffset})");
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isInitialized || stories.Count <= 1) return;
            
            for (int i = 0; i < stories.Count; i++)
            {
                if (stories[i] != null) stories[i].DOKill();
                if (canvasGroups[i] != null) canvasGroups[i].DOKill();
            }

            dragStartX = eventData.position.x;
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isInitialized || !isDragging || stories.Count <= 1) return;

            float dragDistance = eventData.position.x - dragStartX;
            float fadeRatio = Mathf.Abs(dragDistance) / screenDistance;

            if (stories[currentIndex] != null)
            {
                stories[currentIndex].anchoredPosition = new Vector2(dragDistance, defaultYOffset);
                if (canvasGroups[currentIndex] != null) canvasGroups[currentIndex].alpha = 1f - fadeRatio; 
            }

            if (dragDistance < 0 && currentIndex < stories.Count - 1)
            {
                RectTransform nextStory = stories[currentIndex + 1];
                if (nextStory != null)
                {
                    nextStory.gameObject.SetActive(true);
                    nextStory.anchoredPosition = new Vector2(screenDistance + dragDistance, defaultYOffset);
                    if (canvasGroups[currentIndex + 1] != null) canvasGroups[currentIndex + 1].alpha = fadeRatio; 
                }
            }
            else if (dragDistance > 0 && currentIndex > 0)
            {
                RectTransform prevStory = stories[currentIndex - 1];
                if (prevStory != null)
                {
                    prevStory.gameObject.SetActive(true);
                    prevStory.anchoredPosition = new Vector2(-screenDistance + dragDistance, defaultYOffset);
                    if (canvasGroups[currentIndex - 1] != null) canvasGroups[currentIndex - 1].alpha = fadeRatio; 
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isInitialized || !isDragging || stories.Count <= 1) return;
            isDragging = false;

            float dragDistance = eventData.position.x - dragStartX;

            if (dragDistance < -swipeThreshold && currentIndex < stories.Count - 1)
            {
                TransitionToStory(currentIndex + 1, -1);
            }
            else if (dragDistance > swipeThreshold && currentIndex > 0)
            {
                TransitionToStory(currentIndex - 1, 1);
            }
            else
            {
                CancelTransition();
            }
        }

        private void TransitionToStory(int newIndex, int direction)
        {
            int oldIndex = currentIndex;
            currentIndex = newIndex;

            float targetOldX = direction == -1 ? -screenDistance : screenDistance;
            
            RectTransform oldStory = stories[oldIndex];
            if (oldStory != null)
            {
                oldStory.DOAnchorPosX(targetOldX, snapDuration).SetEase(snapEase);
                if (canvasGroups[oldIndex] != null)
                {
                    canvasGroups[oldIndex].DOFade(0f, snapDuration)
                        .SetEase(snapEase)
                        .OnComplete(() => { if (oldStory != null) oldStory.gameObject.SetActive(false); });
                }
            }

            if (stories[currentIndex] != null)
            {
                stories[currentIndex].DOAnchorPosX(0, snapDuration).SetEase(snapEase);
                if (canvasGroups[currentIndex] != null) canvasGroups[currentIndex].DOFade(1f, snapDuration).SetEase(snapEase);
            }
        }

        private void CancelTransition()
        {
            if (stories[currentIndex] != null)
            {
                stories[currentIndex].DOAnchorPosX(0, snapDuration).SetEase(snapEase);
                if (canvasGroups[currentIndex] != null) canvasGroups[currentIndex].DOFade(1f, snapDuration).SetEase(snapEase);
            }

            if (currentIndex < stories.Count - 1 && stories[currentIndex + 1] != null)
            {
                RectTransform rightStory = stories[currentIndex + 1];
                rightStory.DOAnchorPosX(screenDistance, snapDuration).SetEase(snapEase);
                if (canvasGroups[currentIndex + 1] != null)
                {
                    canvasGroups[currentIndex + 1].DOFade(0f, snapDuration)
                        .SetEase(snapEase)
                        .OnComplete(() => { if (rightStory != null) rightStory.gameObject.SetActive(false); });
                }
            }

            if (currentIndex > 0 && stories[currentIndex - 1] != null)
            {
                RectTransform leftStory = stories[currentIndex - 1];
                leftStory.DOAnchorPosX(-screenDistance, snapDuration).SetEase(snapEase);
                if (canvasGroups[currentIndex - 1] != null)
                {
                    canvasGroups[currentIndex - 1].DOFade(0f, snapDuration)
                        .SetEase(snapEase)
                        .OnComplete(() => { if (leftStory != null) leftStory.gameObject.SetActive(false); });
                }
            }
        }
    }
}
