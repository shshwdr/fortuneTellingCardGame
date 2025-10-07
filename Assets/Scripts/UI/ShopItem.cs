using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class ShopItem : MonoBehaviour
{
    [Header("Shop Item Components")]
    public Button purchaseButton;
    public TMP_Text purchaseButtonText;
    
    private RuneInfo runeData;
    private CardInfo cardData;
    private Action purchaseAction;
    private bool isRune;
    
    void Awake()
    {
        // Try to find purchase button if not assigned
        if (purchaseButton == null)
        {
            purchaseButton = GetComponentInChildren<Button>();
            if (purchaseButton == null)
            {
                purchaseButton = transform.Find("PurchaseButton")?.GetComponent<Button>();
            }
        }
        
        // Try to find purchase button text if not assigned
        if (purchaseButtonText == null && purchaseButton != null)
        {
            purchaseButtonText = purchaseButton.GetComponentInChildren<TMP_Text>();
        }
    }
    
    public void SetupRune(RuneInfo rune, Action onPurchase)
    {
        runeData = rune;
        cardData = null;
        purchaseAction = onPurchase;
        isRune = true;
        
        SetupPurchaseButton();
    }
    
    public void SetupCard(CardInfo card, Action onPurchase)
    {
        cardData = card;
        runeData = null;
        purchaseAction = onPurchase;
        isRune = false;
        
        SetupPurchaseButton();
    }
    
    private void SetupPurchaseButton()
    {
        if (purchaseButton == null) 
        {
            Debug.LogWarning("Purchase button not found in ShopItem");
            return;
        }
        
        // Clear previous listeners
        purchaseButton.onClick.RemoveAllListeners();
        
        // Add purchase action
        purchaseButton.onClick.AddListener(() => {
            purchaseAction?.Invoke();
            // Update button state after purchase
            UpdateButtonState();
        });
        
        UpdateButtonState();
    }
    
    private void UpdateButtonState()
    {
        if (purchaseButton == null) return;
        
        bool canAfford = false;
        bool alreadyOwned = false;
        
        if (isRune && runeData != null)
        {
            canAfford = GameSystem.Instance.gameState.money >= runeData.cost;
            alreadyOwned = GameSystem.Instance.HasRune(runeData.identifier);
        }
        else if (!isRune && cardData != null)
        {
            int cardCost = 10; // Default card cost
            canAfford = GameSystem.Instance.gameState.money >= cardCost;
            alreadyOwned = GameSystem.Instance.gameState.availableCards.Any(ownedCard => 
                ownedCard.info.identifier == cardData.identifier);
        }
        
        purchaseButton.interactable = canAfford && !alreadyOwned;
        
        if (purchaseButtonText != null)
        {
            if (alreadyOwned)
                purchaseButtonText.text = "Owned";
            else if (!canAfford)
                purchaseButtonText.text = "Can't Afford";
            else
                purchaseButtonText.text = "Buy";
        }
    }
    
    public Button GetPurchaseButton()
    {
        return purchaseButton;
    }
    
    public void RefreshButtonState()
    {
        UpdateButtonState();
    }
}