using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ToastData
{
    public string message;
    public float displayDuration;
    
    public ToastData(string msg, float duration = 2f)
    {
        message = msg;
        displayDuration = duration;
    }
}

public class ToastManager : Singleton<ToastManager>
{
    [Header("Toast Settings")]
    public GameObject toastPrefab;
    public Transform toastContainer;
    public float toastInterval = 0.5f; // Time between showing toasts
    public float defaultDisplayDuration = 2f;
    public int maxToastsOnScreen = 5;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public Vector3 scaleInFrom = new Vector3(0.8f, 0.8f, 1f);
    public Vector3 scaleInTo = Vector3.one;
    public Ease fadeInEase = Ease.OutBack;
    public Ease fadeOutEase = Ease.InBack;
    
    private Queue<ToastData> toastQueue = new Queue<ToastData>();
    private List<ToastUI> activeToasts = new List<ToastUI>();
    private bool isProcessingQueue = false;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Validate components
        if (toastContainer == null)
        {
            Debug.LogWarning("ToastManager: No toast container assigned!");
        }
        
        if (toastPrefab == null)
        {
            Debug.LogWarning("ToastManager: No toast prefab assigned!");
        }
    }
    
    /// <summary>
    /// Show a toast message with default duration
    /// </summary>
    /// <param name="message">The message to display</param>
    public void ShowToast(string message)
    {
        ShowToast(message, defaultDisplayDuration);
    }
    
    /// <summary>
    /// Show a toast message with custom duration
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">How long to display the toast</param>
    public void ShowToast(string message, float duration)
    {
        if (string.IsNullOrEmpty(message))
            return;
            
        ToastData toastData = new ToastData(message, duration);
        toastQueue.Enqueue(toastData);
        
        // Start processing if not already running
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessToastQueue());
        }
    }
    
    /// <summary>
    /// Clear all pending toasts
    /// </summary>
    public void ClearQueue()
    {
        toastQueue.Clear();
    }
    
    /// <summary>
    /// Clear all active toasts immediately
    /// </summary>
    public void ClearActiveToasts()
    {
        for (int i = activeToasts.Count - 1; i >= 0; i--)
        {
            if (activeToasts[i] != null)
            {
                HideToast(activeToasts[i], true);
            }
        }
    }
    
    private IEnumerator ProcessToastQueue()
    {
        isProcessingQueue = true;
        
        while (toastQueue.Count > 0)
        {
            // Check if we can show more toasts
            if (activeToasts.Count < maxToastsOnScreen)
            {
                ToastData toastData = toastQueue.Dequeue();
                CreateAndShowToast(toastData);
            }
            
            yield return new WaitForSeconds(toastInterval);
        }
        
        isProcessingQueue = false;
    }
    
    private void CreateAndShowToast(ToastData toastData)
    {
        if (toastPrefab == null || toastContainer == null)
        {
            Debug.LogError("ToastManager: Missing prefab or container!");
            return;
        }
        
        // Instantiate toast
        GameObject toastObj = Instantiate(toastPrefab, toastContainer);
        ToastUI toastUI = toastObj.GetComponent<ToastUI>();
        
        if (toastUI == null)
        {
            Debug.LogError("ToastManager: Toast prefab must have ToastUI component!");
            Destroy(toastObj);
            return;
        }
        
        // Setup toast
        toastUI.SetMessage(toastData.message);
        activeToasts.Add(toastUI);
        
        // Position toast (stack them)
        PositionToasts();
        
        // Animate in
        AnimateToastIn(toastUI, toastData.displayDuration);
    }
    
    private void PositionToasts()
    {
        for (int i = 0; i < activeToasts.Count; i++)
        {
            if (activeToasts[i] != null)
            {
                // Move existing toasts up to make room for new ones
                activeToasts[i].transform.SetSiblingIndex(i);
            }
        }
    }
    
    private void AnimateToastIn(ToastUI toastUI, float displayDuration)
    {
        if (toastUI == null) return;
        
        // Set initial state
        CanvasGroup canvasGroup = toastUI.GetCanvasGroup();
        RectTransform rectTransform = toastUI.GetRectTransform();
        
        canvasGroup.alpha = 0f;
        rectTransform.localScale = scaleInFrom;
        
        // Create animation sequence
        Sequence sequence = DOTween.Sequence();
        
        // Fade in and scale animation
        sequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        sequence.Join(rectTransform.DOScale(scaleInTo, fadeInDuration).SetEase(fadeInEase));
        
        // Wait for display duration
        sequence.AppendInterval(displayDuration);
        
        // Fade out
        sequence.AppendCallback(() => HideToast(toastUI));
    }
    
    private void HideToast(ToastUI toastUI, bool immediate = false)
    {
        if (toastUI == null) return;
        
        if (immediate)
        {
            RemoveToastFromList(toastUI);
            if (toastUI.gameObject != null)
            {
                Destroy(toastUI.gameObject);
            }
            return;
        }
        
        CanvasGroup canvasGroup = toastUI.GetCanvasGroup();
        RectTransform rectTransform = toastUI.GetRectTransform();
        
        // Create fade out sequence
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));
        sequence.Join(rectTransform.DOScale(scaleInFrom, fadeOutDuration).SetEase(fadeOutEase));
        
        sequence.OnComplete(() => {
            RemoveToastFromList(toastUI);
            if (toastUI.gameObject != null)
            {
                Destroy(toastUI.gameObject);
            }
            RepositionRemainingToasts();
        });
    }
    
    private void RemoveToastFromList(ToastUI toastUI)
    {
        activeToasts.Remove(toastUI);
    }
    
    private void RepositionRemainingToasts()
    {
        for (int i = 0; i < activeToasts.Count; i++)
        {
            if (activeToasts[i] != null)
            {
                activeToasts[i].transform.SetSiblingIndex(i);
            }
        }
    }
    
    /// <summary>
    /// Get the number of toasts currently in queue
    /// </summary>
    public int GetQueueCount()
    {
        return toastQueue.Count;
    }
    
    /// <summary>
    /// Get the number of active toasts on screen
    /// </summary>
    public int GetActiveToastCount()
    {
        return activeToasts.Count;
    }
}