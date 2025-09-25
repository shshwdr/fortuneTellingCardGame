using UnityEngine;

/// <summary>
/// Minimal test to verify the core game systems work without UI dependencies
/// </summary>
public class MinimalGameTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Starting Minimal Game Test ===");
        
        // Initialize CSV data
        CSVLoader.Instance.Init();
        Debug.Log("CSV Loader initialized");
        
        // Start game
        GameSystem.Instance.StartNewGame();
        Debug.Log("Game started");
        
        // Draw cards
        CardSystem.Instance.DrawCardsForCustomer();
        Debug.Log("Cards drawn");
        
        // Test divination
        TestDivination();
    }
    
    void TestDivination()
    {
        var customer = GameSystem.Instance.GetCurrentCustomer();
        if (customer != null)
        {
            Debug.Log($"Customer: {customer.info.name} wants {customer.info.target}");
            Debug.Log($"Initial stats - W:{customer.wealth} R:{customer.relationship} S:{customer.sanity} P:{customer.power}");
            
            // Show current cards
            var cards = CardSystem.Instance.currentHand;
            var orientations = CardSystem.Instance.cardOrientations;
            
            Debug.Log("Current cards:");
            for (int i = 0; i < cards.Count; i++)
            {
                string cardId = cards[i];
                bool isUpright = orientations[i];
                
                if (CSVLoader.Instance.cardInfoMap.ContainsKey(cardId))
                {
                    var cardInfo = CSVLoader.Instance.cardInfoMap[cardId];
                    string orientation = isUpright ? "Upright" : "Reversed";
                    Debug.Log($"  {i+1}: {cardInfo.name} ({orientation})");
                }
            }
            
            // Flip some cards for testing
            CardSystem.Instance.FlipCard(1); // Flip second card
            CardSystem.Instance.FlipCard(3); // Flip fourth card
            Debug.Log("Flipped cards 2 and 4");
            
            // Perform divination
            var result = CardSystem.Instance.PerformDivination(customer);
            
            Debug.Log("=== Divination Result ===");
            Debug.Log($"Customer satisfied: {result.isSatisfied}");
            Debug.Log($"Money earned: {result.moneyEarned}");
            
            Debug.Log("Attribute changes:");
            foreach (var attr in result.initialAttributes.Keys)
            {
                int initial = result.initialAttributes[attr];
                int final = result.finalAttributes[attr];
                int change = final - initial;
                string changeText = change >= 0 ? $"+{change}" : change.ToString();
                Debug.Log($"  {attr}: {initial} -> {final} ({changeText})");
            }
            
            if (result.isSatisfied)
            {
                GameSystem.Instance.AddMoney(result.moneyEarned);
                Debug.Log($"Added {result.moneyEarned} money. Total: {GameSystem.Instance.gameState.money}");
            }
        }
        else
        {
            Debug.LogError("No customer found!");
        }
    }
    
    void Update()
    {
        // Simple keyboard controls for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestDivination();
        }
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameSystem.Instance.NextCustomer();
            CardSystem.Instance.DrawCardsForCustomer();
            Debug.Log("=== Next Customer ===");
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameSystem.Instance.StartNewGame();
            CardSystem.Instance.DrawCardsForCustomer();
            Debug.Log("=== Game Restarted ===");
        }
        
        // Card flipping
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CardSystem.Instance.FlipCard(0);
            Debug.Log("Flipped card 1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CardSystem.Instance.FlipCard(1);
            Debug.Log("Flipped card 2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CardSystem.Instance.FlipCard(2);
            Debug.Log("Flipped card 3");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CardSystem.Instance.FlipCard(3);
            Debug.Log("Flipped card 4");
        }
    }
    
    void OnGUI()
    {
        // Simple debug GUI
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        
        var gameState = GameSystem.Instance.gameState;
        GUILayout.Label($"Day: {gameState.currentDay}");
        GUILayout.Label($"Money: {gameState.money}");
        GUILayout.Label($"Customer: {gameState.currentCustomerIndex + 1}/{gameState.todayCustomers.Count}");
        
        var customer = GameSystem.Instance.GetCurrentCustomer();
        if (customer != null)
        {
            GUILayout.Label($"Current: {customer.info.name} (wants {customer.info.target})");
            GUILayout.Label($"W:{customer.wealth} R:{customer.relationship} S:{customer.sanity} P:{customer.power}");
        }
        
        GUILayout.Label("Controls:");
        GUILayout.Label("1-4: Flip cards");
        GUILayout.Label("Space: Perform divination");
        GUILayout.Label("N: Next customer");
        GUILayout.Label("R: Restart game");
        
        GUILayout.EndArea();
    }
}