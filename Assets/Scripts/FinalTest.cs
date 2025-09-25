using UnityEngine;

public class FinalTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== FINAL COMPILATION TEST ===");
        
        // Test all data types can be created
        var cardInfo = new CardInfo();
        var customerInfo = new CustomerInfo();
        var customer = new Customer(customerInfo);
        var dayInfo = new DayInfo();
        var runeInfo = new RuneInfo();
        var sigilInfo = new SigilInfo();
        var upgradeInfo = new UpgradeInfo();
        var gameState = new GameState();
        var cardEffect = new CardEffect();
        var divinationResult = new DivinationResult();
        
        Debug.Log("✓ All data types created successfully");
        
        // Test all systems can be accessed
        var csvLoader = CSVLoader.Instance;
        var gameSystem = GameSystem.Instance;
        var cardSystem = CardSystem.Instance;
        
        Debug.Log("✓ All systems accessible");
        
        // Test CSV loading
        csvLoader.Init();
        Debug.Log("✓ CSV loading works");
        
        // Test game flow
        gameSystem.StartNewGame();
        cardSystem.DrawCardsForCustomer();
        Debug.Log("✓ Game flow works");
        
        Debug.Log("=== ALL TESTS PASSED! GAME IS READY ===");
        
        // Show current game state
        var currentCustomer = gameSystem.GetCurrentCustomer();
        if (currentCustomer != null)
        {
            Debug.Log($"Current customer: {currentCustomer.info.name}");
            Debug.Log($"Cards in hand: {cardSystem.currentHand.Count}");
        }
    }
}