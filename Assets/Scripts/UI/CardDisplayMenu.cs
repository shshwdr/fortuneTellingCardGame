using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A menu for displaying a list of cards, inheriting from MenuBase
/// </summary>
public class CardDisplayMenu : MenuBase
{
    [Header("Card Display Components")]
    public Transform cardContainer;
    public GameObject cardDisplayPrefab; // Prefab for displaying cards
    public TMP_Text titleText;
    public TMP_Text countText;
    public ScrollRect scrollRect;
    
    private List<Card> currentCards = new List<Card>();
    private List<GameObject> cardDisplayObjects = new List<GameObject>();
    
    public override void Init()
    {
        base.Init();
    }
    
    /// <summary>
    /// Display a list of cards in the menu
    /// </summary>
    /// <param name="cards">List of cards to display</param>
    /// <param name="title">Title text for the menu</param>
    public void DisplayCards(List<Card> cards, string title = "Cards")
    {
        currentCards = cards ?? new List<Card>();
        
        if (titleText != null)
            titleText.text = title;
            
        if (countText != null)
            countText.text = $"Count: {currentCards.Count}";
            
        RefreshCardDisplay();
        Show();
    }
    
    public override void Show(bool immediate = false)
    {
        base.Show(immediate);
        SFXManager.Instance.Menu();
    }
    
    /// <summary>
    /// Refresh the visual display of cards
    /// </summary>
    private void RefreshCardDisplay()
    {
        // Clear existing card displays
        ClearCardDisplays();
        
        if (cardContainer == null || cardDisplayPrefab == null)
        {
            Debug.LogWarning("CardDisplayMenu: cardContainer or cardDisplayPrefab not set up");
            return;
        }
        
        // Create card display objects
        foreach (var card in currentCards)
        {
            CreateCardDisplay(card);
        }
        
        // Reset scroll position
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    /// <summary>
    /// Create a display object for a single card
    /// </summary>
    private void CreateCardDisplay(Card card)
    {
        GameObject cardDisplay = Instantiate(cardDisplayPrefab, cardContainer);
        cardDisplayObjects.Add(cardDisplay);
        
        // Setup the card display using CardUISimple if available
        var cardUISimple = cardDisplay.GetComponent<CardUISimple>();
        if (cardUISimple != null)
        {
            cardUISimple.SetCardData(card, cardDisplayObjects.Count - 1);
            
            // Disable clicking for display-only mode (optional)
            if (cardUISimple.cardButton != null)
            {
                cardUISimple.cardButton.interactable = false;
            }
        }
        else
        {
            // Fallback: setup using text components if CardUISimple is not available
            SetupCardDisplayFallback(cardDisplay, card);
        }
    }
    
    /// <summary>
    /// Fallback method to setup card display using basic text components
    /// </summary>
    private void SetupCardDisplayFallback(GameObject cardDisplay, Card card)
    {
        var nameText = cardDisplay.transform.Find("NameText")?.GetComponent<TMP_Text>();
        var descText = cardDisplay.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
        var effectText = cardDisplay.transform.Find("EffectText")?.GetComponent<TMP_Text>();
        var statusText = cardDisplay.transform.Find("StatusText")?.GetComponent<TMP_Text>();
        
        if (nameText != null)
            nameText.text = card.info.name;
            
        if (descText != null)
            descText.text = card.info.description;
            
        if (effectText != null)
        {
            var effects = card.GetEffects();
            effectText.text = string.Join(", ", effects);
        }
        
        if (statusText != null)
        {
            string status = card.isUpright ? "Upright" : "Reversed";
            statusText.text = $"Status: {status}";
        }
    }
    
    /// <summary>
    /// Clear all card display objects
    /// </summary>
    private void ClearCardDisplays()
    {
        foreach (var cardDisplay in cardDisplayObjects)
        {
            if (cardDisplay != null)
                Destroy(cardDisplay);
        }
        cardDisplayObjects.Clear();
    }
    
    /// <summary>
    /// Static method to open the card display menu with specific cards
    /// </summary>
    /// <param name="cards">Cards to display</param>
    /// <param name="title">Title for the menu</param>
    public static void ShowCards(List<Card> cards, string title = "Cards")
    {
        var cardDisplayMenu = FindFirstInstance<CardDisplayMenu>();
        if (cardDisplayMenu != null)
        {
            cardDisplayMenu.DisplayCards(cards, title);
        }
        else
        {
            Debug.LogWarning("CardDisplayMenu not found in scene");
        }
    }
    
    public override void AfterHide()
    {
        base.AfterHide();
        // Clean up when menu is hidden
        ClearCardDisplays();
    }
    
    void OnDestroy()
    {
        ClearCardDisplays();
    }
}