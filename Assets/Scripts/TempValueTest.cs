using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Test script to verify temporary money and reroll functionality
/// </summary>
public class TempValueTest : MonoBehaviour
{
    [Header("UI References for Testing")]
    public TMP_Text moneyDisplayText;
    public TMP_Text moneyTempDisplayText;
    public TMP_Text rerollDisplayText;
    public TMP_Text rerollTempDisplayText;
    public TMP_Text sanityDisplayText;
    public TMP_Text sanityTempDisplayText;
    
    [Header("Test Buttons")]
    public Button testRuneEffectsButton;
    public Button performDivinationButton;
    public Button resetGameButton;
    
    void Start()
    {
        SetupButtons();
        InitializeDisplay();
    }
    
    private void SetupButtons()
    {
        if (testRuneEffectsButton != null)
            testRuneEffectsButton.onClick.AddListener(TestRuneEffects);
            
        if (performDivinationButton != null)
            performDivinationButton.onClick.AddListener(TestDivination);
            
        if (resetGameButton != null)
            resetGameButton.onClick.AddListener(ResetGame);
    }
    
    private void InitializeDisplay()
    {
        UpdateDisplay();
    }
    
    void Update()
    {
        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestRuneEffects();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestDivination();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ResetGame();
        }
        
        // Update display continuously
        UpdateDisplay();
    }
    
    private void TestRuneEffects()
    {
        Debug.Log("=== Testing Rune Effects ===");
        
        // Add test runes to simulate effects
        if (GameSystem.Instance != null && RuneManager.Instance != null)
        {
            // Clear existing rune effects
            RuneManager.Instance.runeEffects.Clear();
            
            // Add test rune effects that match our ApplyRuneEffect cases
            RuneManager.Instance.AddRuneEffect("when|allUpCard|refreshCount", 2);
            RuneManager.Instance.AddRuneEffect("when|allDownCard|addGold", 5);
            
            Debug.Log("Added test rune effects:");
            Debug.Log("- All Up Cards: +2 rerolls");
            Debug.Log("- All Down Cards: +5 gold");
            
            // Add corresponding test runes to game state
            var testUpRune = new RuneInfo
            {
                identifier = "test_up_rune",
                name = "Test Up Rune",
                description = "Gives rerolls when all cards are upright",
                effect = "when|allUpCard|refreshCount",
                value = 2
            };
            
            var testDownRune = new RuneInfo
            {
                identifier = "test_down_rune",
                name = "Test Down Rune", 
                description = "Gives gold when all cards are reversed",
                effect = "when|allDownCard|addGold",
                value = 5
            };
            
            GameSystem.Instance.AddRune(testUpRune);
            GameSystem.Instance.AddRune(testDownRune);
            
            // Set all cards to upright to test reroll effect
            if (CardSystem.Instance != null && CardSystem.Instance.currentHand.Count > 0)
            {
                for (int i = 0; i < CardSystem.Instance.currentHand.Count; i++)
                {
                    CardSystem.Instance.currentHand[i].isUpright = true;
                }
                Debug.Log("Set all cards to upright to trigger reroll effect");
            }
        }
    }
    
    private void TestDivination()
    {
        Debug.Log("=== Testing Divination with Temporary Values ===");
        
        var customer = GameSystem.Instance?.GetCurrentCustomer();
        if (customer != null && CardSystem.Instance != null)
        {
            // Test preview mode (updateState = false)
            var previewResult = CardSystem.Instance.PerformDivination(customer, false);
            Debug.Log("Preview Result:");
            Debug.Log($"- Temp Sanity Change: {previewResult.tempSanityChange}");
            Debug.Log($"- Temp Money Change: {previewResult.tempMoneyChange}");
            Debug.Log($"- Temp Reroll Change: {previewResult.tempRerollChange}");
            
            // Test actual application (updateState = true)
            Debug.Log("Applying changes...");
            var actualResult = CardSystem.Instance.PerformDivination(customer, true);
            Debug.Log("Changes applied to game state!");
        }
        else
        {
            Debug.LogWarning("No current customer or CardSystem not available");
        }
    }
    
    private void ResetGame()
    {
        Debug.Log("=== Resetting Game ===");
        
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.StartNewGame();
            Debug.Log("Game reset successfully");
        }
        
        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.DrawCardsForCustomer();
            Debug.Log("New cards drawn");
        }
    }
    
    private void UpdateDisplay()
    {
        if (GameSystem.Instance == null || CardSystem.Instance == null) return;
        
        // Update current values
        if (moneyDisplayText != null)
            moneyDisplayText.text = $"Money: {GameSystem.Instance.gameState.money}";
            
        if (rerollDisplayText != null)
            rerollDisplayText.text = $"Rerolls: {CardSystem.Instance.redrawTime}";
            
        if (sanityDisplayText != null)
            sanityDisplayText.text = $"Sanity: {GameSystem.Instance.gameState.sanity}";
        
        // Update temporary values (preview)
        var customer = GameSystem.Instance.GetCurrentCustomer();
        if (customer != null)
        {
            var result = CardSystem.Instance.PerformDivination(customer, false);
            
            // Update temp displays
            if (moneyTempDisplayText != null)
            {
                if (result.tempMoneyChange != 0)
                {
                    string changeText = result.tempMoneyChange > 0 ? $"+{result.tempMoneyChange}" : result.tempMoneyChange.ToString();
                    moneyTempDisplayText.text = $"({changeText})";
                    moneyTempDisplayText.color = result.tempMoneyChange > 0 ? Color.green : Color.red;
                }
                else
                {
                    moneyTempDisplayText.text = "";
                }
            }
            
            if (rerollTempDisplayText != null)
            {
                if (result.tempRerollChange != 0)
                {
                    string changeText = result.tempRerollChange > 0 ? $"+{result.tempRerollChange}" : result.tempRerollChange.ToString();
                    rerollTempDisplayText.text = $"({changeText})";
                    rerollTempDisplayText.color = result.tempRerollChange > 0 ? Color.green : Color.red;
                }
                else
                {
                    rerollTempDisplayText.text = "";
                }
            }
            
            if (sanityTempDisplayText != null)
            {
                if (result.tempSanityChange != 0)
                {
                    string changeText = result.tempSanityChange > 0 ? $"+{result.tempSanityChange}" : result.tempSanityChange.ToString();
                    sanityTempDisplayText.text = $"({changeText})";
                    sanityTempDisplayText.color = result.tempSanityChange > 0 ? Color.green : Color.red;
                }
                else
                {
                    sanityTempDisplayText.text = "";
                }
            }
        }
    }
    
    void OnGUI()
    {
        // Show debug information
        GUILayout.BeginArea(new Rect(10, 100, 400, 200));
        
        GUILayout.Label("=== Temporary Values Test ===");
        
        if (GameSystem.Instance != null && CardSystem.Instance != null)
        {
            var customer = GameSystem.Instance.GetCurrentCustomer();
            if (customer != null)
            {
                var result = CardSystem.Instance.PerformDivination(customer, false);
                
                GUILayout.Label($"Current Money: {GameSystem.Instance.gameState.money} | Preview: {result.tempMoneyChange}");
                GUILayout.Label($"Current Rerolls: {CardSystem.Instance.redrawTime} | Preview: {result.tempRerollChange}");
                GUILayout.Label($"Current Sanity: {GameSystem.Instance.gameState.sanity} | Preview: {result.tempSanityChange}");
                
                if (result.activatedRunes != null && result.activatedRunes.Count > 0)
                {
                    GUILayout.Label($"Activated Runes: {string.Join(", ", result.activatedRunes)}");
                }
                else
                {
                    GUILayout.Label("No Runes Activated");
                }
            }
        }
        
        GUILayout.Label("Controls:");
        GUILayout.Label("1 - Test Rune Effects");
        GUILayout.Label("2 - Test Divination");
        GUILayout.Label("3 - Reset Game");
        
        GUILayout.EndArea();
    }
}