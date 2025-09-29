using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test script to demonstrate ToastManager functionality
/// </summary>
public class ToastTest : MonoBehaviour
{
    [Header("Test Buttons")]
    public Button showToastButton;
    public Button showMultipleToastsButton;
    public Button clearQueueButton;
    public Button clearActiveButton;
    
    [Header("Test Settings")]
    public string[] testMessages = {
        "Welcome to the game!",
        "Card drawn successfully",
        "Customer satisfied!",
        "Money earned: 10 gold",
        "New day started",
        "Achievement unlocked!",
        "Error: Something went wrong",
        "Warning: Low health"
    };
    
    void Start()
    {
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (showToastButton != null)
        {
            showToastButton.onClick.AddListener(ShowRandomToast);
        }
        
        if (showMultipleToastsButton != null)
        {
            showMultipleToastsButton.onClick.AddListener(ShowMultipleToasts);
        }
        
        if (clearQueueButton != null)
        {
            clearQueueButton.onClick.AddListener(ClearQueue);
        }
        
        if (clearActiveButton != null)
        {
            clearActiveButton.onClick.AddListener(ClearActive);
        }
    }
    
    public void ShowRandomToast()
    {
        if (ToastManager.Instance == null)
        {
            Debug.LogWarning("ToastManager not found!");
            return;
        }
        
        string randomMessage = testMessages[Random.Range(0, testMessages.Length)];
        float randomDuration = Random.Range(1f, 4f);
        
        ToastManager.Instance.ShowToast(randomMessage, randomDuration);
        
        Debug.Log($"Showed toast: {randomMessage} (Duration: {randomDuration:F1}s)");
    }
    
    public void ShowMultipleToasts()
    {
        if (ToastManager.Instance == null)
        {
            Debug.LogWarning("ToastManager not found!");
            return;
        }
        
        // Show 5 random toasts quickly
        for (int i = 0; i < 5; i++)
        {
            string message = testMessages[Random.Range(0, testMessages.Length)];
            ToastManager.Instance.ShowToast($"[{i + 1}] {message}");
        }
        
        Debug.Log("Showed 5 toasts in queue");
    }
    
    public void ClearQueue()
    {
        if (ToastManager.Instance == null)
        {
            Debug.LogWarning("ToastManager not found!");
            return;
        }
        
        int queueCount = ToastManager.Instance.GetQueueCount();
        ToastManager.Instance.ClearQueue();
        
        Debug.Log($"Cleared {queueCount} toasts from queue");
    }
    
    public void ClearActive()
    {
        if (ToastManager.Instance == null)
        {
            Debug.LogWarning("ToastManager not found!");
            return;
        }
        
        int activeCount = ToastManager.Instance.GetActiveToastCount();
        ToastManager.Instance.ClearActiveToasts();
        
        Debug.Log($"Cleared {activeCount} active toasts");
    }
    
    void Update()
    {
        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowRandomToast();
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowMultipleToasts();
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ClearQueue();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearActive();
        }
    }
    
    void OnGUI()
    {
        if (ToastManager.Instance == null) return;
        
        // Show debug info
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"Queue Count: {ToastManager.Instance.GetQueueCount()}");
        GUILayout.Label($"Active Toasts: {ToastManager.Instance.GetActiveToastCount()}");
        GUILayout.Label("Shortcuts:");
        GUILayout.Label("T - Show Random Toast");
        GUILayout.Label("M - Show Multiple Toasts");
        GUILayout.Label("Q - Clear Queue");
        GUILayout.Label("C - Clear Active Toasts");
        GUILayout.EndArea();
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (showToastButton != null)
            showToastButton.onClick.RemoveListener(ShowRandomToast);
        if (showMultipleToastsButton != null)
            showMultipleToastsButton.onClick.RemoveListener(ShowMultipleToasts);
        if (clearQueueButton != null)
            clearQueueButton.onClick.RemoveListener(ClearQueue);
        if (clearActiveButton != null)
            clearActiveButton.onClick.RemoveListener(ClearActive);
    }
}