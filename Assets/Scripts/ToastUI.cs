using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class ToastUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text messageText;
    public Image backgroundImage;
    
    [Header("Optional Components")]
    public Button closeButton;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    void Awake()
    {
        // Get required components
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        // Setup close button if present
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // Validate required components
        if (messageText == null)
        {
            Debug.LogWarning("ToastUI: No message text component assigned!");
        }
    }
    
    /// <summary>
    /// Set the message text for this toast
    /// </summary>
    /// <param name="message">The message to display</param>
    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
    
    /// <summary>
    /// Get the message currently displayed
    /// </summary>
    /// <returns>The current message text</returns>
    public string GetMessage()
    {
        return messageText != null ? messageText.text : string.Empty;
    }
    
    /// <summary>
    /// Set the background color of the toast
    /// </summary>
    /// <param name="color">The color to set</param>
    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
    
    /// <summary>
    /// Set the text color of the message
    /// </summary>
    /// <param name="color">The color to set</param>
    public void SetTextColor(Color color)
    {
        if (messageText != null)
        {
            messageText.color = color;
        }
    }
    
    /// <summary>
    /// Get the CanvasGroup component for alpha animations
    /// </summary>
    /// <returns>The CanvasGroup component</returns>
    public CanvasGroup GetCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        return canvasGroup;
    }
    
    /// <summary>
    /// Get the RectTransform component for scale/position animations
    /// </summary>
    /// <returns>The RectTransform component</returns>
    public RectTransform GetRectTransform()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        return rectTransform;
    }
    
    /// <summary>
    /// Called when the close button is clicked
    /// </summary>
    private void OnCloseButtonClicked()
    {
        // Notify ToastManager to hide this toast immediately
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.StartCoroutine(HideThisToast());
        }
    }
    
    private System.Collections.IEnumerator HideThisToast()
    {
        // This is a workaround since we don't have direct access to HideToast method
        // We'll just fade out quickly and destroy
        var cg = GetCanvasGroup();
        var rt = GetRectTransform();
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        Vector3 startScale = rt.localScale;
        float startAlpha = cg.alpha;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            rt.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            yield return null;
        }
        
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // Clean up button listener
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }
}