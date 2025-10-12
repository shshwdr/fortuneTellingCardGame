using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PopupDialog : MenuBase
{
    [Header("Popup Components")]
    public TMP_Text titleText;
    public TMP_Text messageText;
    public Button button1;
    public Button button2;
    public TMP_Text button1Text;
    public TMP_Text button2Text;
    
    private Action onButton1Click;
    private Action onButton2Click;
    
    public static PopupDialog Create(string title, string message, string button1Text, Action button1Action, string button2Text = null, Action button2Action = null)
    {
        // Find existing popup or create one
        PopupDialog popup = FindFirstInstance<PopupDialog>();
        if (popup == null)
        {
            // Try to load from prefab first
            GameObject popupPrefab = Resources.Load<GameObject>("Prefabs/PopupDialog");
            if (popupPrefab != null)
            {
                GameObject popupObj = Instantiate(popupPrefab);
                popup = popupObj.GetComponent<PopupDialog>();
            }
            else
            {
                // Create popup dynamically if prefab not found
                popup = CreateDynamicPopup();
            }
        }
        
        if (popup != null)
        {
            popup.Setup(title, message, button1Text, button1Action, button2Text, button2Action);
        }
        return popup;
    }
    
    private static PopupDialog CreateDynamicPopup()
    {
        // Create a simple popup using UI elements
        GameObject canvasObj = new GameObject("PopupCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High sorting order to appear on top
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create popup dialog object
        GameObject popupObj = new GameObject("PopupDialog");
        popupObj.transform.SetParent(canvas.transform, false);
        
        PopupDialog popup = popupObj.AddComponent<PopupDialog>();
        
        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(popupObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        popup.menu = background;
        popup.animatedRect = bgRect;
        
        // Note: This is a basic implementation
        // For full functionality, you'll need to create the text and button elements
        // It's recommended to create the prefab in Unity Editor instead
        
        Debug.LogWarning("PopupDialog created dynamically with basic functionality. Create PopupDialog prefab for full features.");
        
        return popup;
    }
    
    public void Setup(string title, string message, string button1Text, Action button1Action, string button2Text = null, Action button2Action = null)
    {
        // Set title
        if (titleText != null)
            titleText.text = title;
        else
            Debug.Log($"Popup Title: {title}");
            
        // Set message
        if (messageText != null)
            messageText.text = message;
        else
            Debug.Log($"Popup Message: {message}");
        
        // Setup button 1
        if (button1 != null && this.button1Text != null)
        {
            this.button1Text.text = button1Text;
            button1.gameObject.SetActive(true);
            
            // Clear previous listeners
            button1.onClick.RemoveAllListeners();
            button1.onClick.AddListener(() => {
                button1Action?.Invoke();
                Hide(true);
            });
        }
        else
        {
            // Fallback: Use console for testing
            Debug.Log($"Popup Button 1: {button1Text} (Press 1 in console to simulate)");
            onButton1Click = button1Action;
        }
        
        // Setup button 2
        if (!string.IsNullOrEmpty(button2Text) && button2 != null && this.button2Text != null)
        {
            this.button2Text.text = button2Text;
            button2.gameObject.SetActive(true);
            
            // Clear previous listeners
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(() => {
                button2Action?.Invoke();
                Hide(true);
            });
        }
        else if (button2 != null)
        {
            button2.gameObject.SetActive(false);
        }
        else if (!string.IsNullOrEmpty(button2Text))
        {
            // Fallback: Use console for testing
            Debug.Log($"Popup Button 2: {button2Text} (Press 2 in console to simulate)");
            onButton2Click = button2Action;
        }
        
        Show();
    }
    
    protected override void Awake()
    {
        base.Awake();
        destroyWhenHide = false; // Destroy popup when hidden
    }
    
    void Update()
    {
        // Console testing fallback
        if (Input.GetKeyDown(KeyCode.Alpha1) && onButton1Click != null)
        {
            onButton1Click.Invoke();
            Hide(true);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && onButton2Click != null)
        {
            onButton2Click.Invoke();
            Hide(true);
        }
    }
}