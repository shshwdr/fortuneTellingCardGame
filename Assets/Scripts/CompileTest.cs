using UnityEngine;

/// <summary>
/// Simple test script to verify all classes compile correctly
/// </summary>
public class CompileTest : MonoBehaviour
{
    void Start()
    {
        TestDataClasses();
        TestSystems();
        Debug.Log("All classes compiled successfully!");
    }
    
    void TestDataClasses()
    {
        // Test data structures
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
        
        Debug.Log("Data classes test passed");
    }
    
    void TestSystems()
    {
        // Test system singletons
        var csvLoader = CSVLoader.Instance;
        var gameSystem = GameSystem.Instance;
        var cardSystem = CardSystem.Instance;
        
        Debug.Log("System classes test passed");
    }
}