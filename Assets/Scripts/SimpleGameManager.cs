using UnityEngine;

/// <summary>
/// Super simple GameManager that just starts the core systems
/// No UI dependencies, just pure game logic
/// </summary>
public class SimpleGameManager : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("=== Simple Game Manager Starting ===");
        
        // Initialize CSV data
        CSVLoader.Instance.Init();
        Debug.Log("CSV data loaded successfully");
        
        // Start the game
        GameSystem.Instance.StartNewGame();
        Debug.Log("Game system initialized");
        
        // Draw initial cards
        CardSystem.Instance.DrawCardsForCustomer();
        Debug.Log("Initial cards drawn");
        
        // Add minimal test component
        gameObject.AddComponent<MinimalGameTest>();
        Debug.Log("Minimal test component added");
        
        Debug.Log("=== Game Ready! Use keyboard controls to play ===");
        LogControls();
    }
    
    void LogControls()
    {
        Debug.Log("CONTROLS:");
        Debug.Log("1-4: Flip cards 1-4");
        Debug.Log("Space: Perform divination");
        Debug.Log("N: Next customer");
        Debug.Log("R: Restart game");
    }
}