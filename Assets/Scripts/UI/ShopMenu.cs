using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ShopMenu : MenuBase
{
    [Header("Shop Components")]
    public Transform itemContainer;
    public GameObject shopItemPrefab; // Base shop item prefab
    public GameObject runeDisplayPrefab; // Specific rune display prefab
    public GameObject cardDisplayPrefab; // Specific card display prefab
    public TMP_Text moneyText;
    public Button nextDayButton;
    public bool openAtBeginning = false;
    
    private List<RuneInfo> availableRunes = new List<RuneInfo>();
    private List<CardInfo> availableCards = new List<CardInfo>();
    
    // Track cards that have been upgraded in this shop session
    private HashSet<string> upgradedCardsThisSession = new HashSet<string>();
    
    public override void Init()
    {
        base.Init();
        
        // Clear upgraded cards tracking when initializing shop
        upgradedCardsThisSession.Clear();
        
        SetupShop();
        UpdateMoneyDisplay();
        
        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(ProceedToNextDay);
        }
    }
    
    private void SetupShop()
    {
        GenerateAvailableItems();
        CreateShopItems();
    }
    
    private void GenerateAvailableItems()
    {
        availableRunes.Clear();
        availableCards.Clear();
        
        // Get 3 random runes
        var allRunes = CSVLoader.Instance.runeInfoMap.Values.ToList();
        var randomRunes = allRunes.Where(rune => rune.canBeDraw && !GameSystem.Instance.HasRune(rune.identifier)).OrderBy(x => Random.value).Take(3).ToList();
        availableRunes.AddRange(randomRunes);
        
        // Get cards that can be drawn:
        // 1. Cards not owned yet
        // 2. Cards already owned but can be upgraded (level < maxLevel)
        var allCards = CSVLoader.Instance.cardInfoMap.Values.Where(card => card.canBeDraw).ToList();
        
        var availableCardsList = new List<CardInfo>();
        
        foreach (var card in allCards)
        {
            var ownedCard = GameSystem.Instance.gameState.allCards.FirstOrDefault(ownedCard => ownedCard.info.identifier == card.identifier);
            
            if (ownedCard == null)
            {
                // Card not owned, can be purchased
                availableCardsList.Add(card);
            }
            else if (ownedCard.level < card.maxLevel)
            {
                // Card owned but can be upgraded
                availableCardsList.Add(card);
            }
        }
        
        var randomCards = availableCardsList.OrderBy(x => Random.value).Take(3).ToList();
        availableCards.AddRange(randomCards);
    }
    
    private void CreateShopItems()
    {
        if (itemContainer == null || shopItemPrefab == null) 
        {
            Debug.LogWarning("Shop item container or prefab not set up. Using fallback.");
            return;
        }
        
        // Clear existing items
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create rune items
        foreach (var rune in availableRunes)
        {
            CreateShopItem(rune, ShopItemType.Rune);
        }
        
        // Create card items
        foreach (var card in availableCards)
        {
            CreateShopItem(card, ShopItemType.Card);
        }
    }
    
    private enum ShopItemType
    {
        Rune,
        Card
    }
    
    private void CreateShopItem(object itemData, ShopItemType itemType)
    {
        GameObject shopItem = Instantiate(shopItemPrefab, itemContainer);
        ShopItem shopItemComponent = shopItem.GetComponent<ShopItem>();
        
        if (shopItemComponent == null)
        {
            shopItemComponent = shopItem.AddComponent<ShopItem>();
        }
        
        // Get the display container from the shop item
        Transform displayContainer = shopItem.transform.Find("DisplayContainer");
        if (displayContainer == null)
        {
            Debug.LogWarning("DisplayContainer not found in shop item prefab");
            displayContainer = shopItem.transform;
        }
        
        // Create appropriate display prefab
        GameObject displayPrefab = null;
        switch (itemType)
        {
            case ShopItemType.Rune:
                if (runeDisplayPrefab != null)
                {
                    displayPrefab = Instantiate(runeDisplayPrefab, displayContainer);
                    displayPrefab.GetComponent<RuneCell>().SetData((RuneInfo)itemData);
                    //SetupRuneDisplay(displayPrefab, (RuneInfo)itemData);
                }
                shopItemComponent.SetupRune((RuneInfo)itemData, () => PurchaseRune((RuneInfo)itemData));
                break;
                
            case ShopItemType.Card:
                if (cardDisplayPrefab != null)
                {
                    displayPrefab = Instantiate(cardDisplayPrefab, displayContainer);
                    var cardUI = displayPrefab.GetComponent<CardUISimple>();
                    
                    // Check if this card is owned and show appropriate level
                    var cardInfo = (CardInfo)itemData;
                    var ownedCard = GameSystem.Instance.gameState.allCards.FirstOrDefault(ownedCard => ownedCard.info.identifier == cardInfo.identifier);
                    
                    if (ownedCard != null && ownedCard.level < cardInfo.maxLevel)
                    {
                        // Show as upgrade (next level)
                        var upgradeCard = new Card(cardInfo, ownedCard.level + 1);
                        cardUI.SetCardData(upgradeCard, 0);
                        
                        // Add upgrade indicator to the display
                        ShowUpgradeComparison(displayPrefab, ownedCard, upgradeCard);
                    }
                    else
                    {
                        // Show as new card (level 1)
                        var newCard = new Card(cardInfo, 1);
                        cardUI.SetCardData(newCard, 0);
                    }
                }
                shopItemComponent.SetupCard((CardInfo)itemData, () => PurchaseCard((CardInfo)itemData));
                break;
        }
        
        // Setup purchase button
        UpdateShopItemPurchaseButton(shopItemComponent, itemType, itemData);
    }
    
    private void ShowUpgradeComparison(GameObject displayPrefab, Card currentCard, Card upgradeCard)
    {
        // Find or create comparison display components
        var comparisonContainer = displayPrefab.transform.Find("UpgradeComparison");
        if (comparisonContainer == null)
        {
            // Create comparison container
            GameObject comparisonObj = new GameObject("UpgradeComparison");
            comparisonObj.transform.SetParent(displayPrefab.transform);
            
            // Add text component for showing comparison
            var comparisonText = comparisonObj.AddComponent<TMP_Text>();
            comparisonText.fontSize = 12;
            comparisonText.color = Color.green;
            comparisonText.alignment = TextAlignmentOptions.Center;
            
            // Position the comparison text
            var rectTransform = comparisonObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0.3f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            comparisonContainer = comparisonObj.transform;
        }
        
        var comparisonTextComponent = comparisonContainer.GetComponent<TMP_Text>();
        if (comparisonTextComponent != null)
        {
            comparisonTextComponent.text = GetUpgradeComparisonText(currentCard, upgradeCard);
        }
    }
    
    private string GetUpgradeComparisonText(Card currentCard, Card upgradeCard)
    {
        var currentEffects = currentCard.GetEffects();
        var upgradeEffects = upgradeCard.GetEffects();
        
        return $"Lv.{currentCard.level} → Lv.{upgradeCard.level}\nUpgrade Available!";
    }
    
    private void SetupRuneDisplay(GameObject displayObj, RuneInfo rune)
    {
        var nameText = displayObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        var descText = displayObj.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
        var costText = displayObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
        
        if (nameText != null) nameText.text = rune.name;
        if (descText != null) descText.text = rune.description;
        if (costText != null) costText.text = $"Cost: {rune.cost}";
        
        // Additional rune-specific display logic can go here
        // For example: rune icon, effect preview, etc.
    }
    
    private void SetupCardDisplay(GameObject displayObj, CardInfo card)
    {
        var nameText = displayObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        var descText = displayObj.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
        var costText = displayObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
        
        if (nameText != null) nameText.text = card.name;
        if (descText != null) descText.text = card.description;
        
        int cardCost = 10; // Default cost for cards
        if (costText != null) costText.text = $"Cost: {cardCost}";
        
        // Additional card-specific display logic can go here
        // For example: card artwork, effect preview, etc.
    }
    
    private void UpdateShopItemPurchaseButton(ShopItem shopItem, ShopItemType itemType, object itemData)
    {
        Button buyButton = shopItem.GetPurchaseButton();
        if (buyButton == null) return;
        
        bool canAfford = false;
        bool alreadyOwned = false;
        bool canUpgrade = false;
        bool upgradedThisSession = false;
        string itemName = "";
        int cost = 10;
        
        switch (itemType)
        {
            case ShopItemType.Rune:
                var rune = (RuneInfo)itemData;
                cost = rune.cost;
                canAfford = GameSystem.Instance.gameState.money >= rune.cost;
                alreadyOwned = GameSystem.Instance.HasRune(rune.identifier);
                itemName = rune.name;
                break;
                
            case ShopItemType.Card:
                var card = (CardInfo)itemData;
                var ownedCard = GameSystem.Instance.gameState.allCards.FirstOrDefault(ownedCard => ownedCard.info.identifier == card.identifier);
                
                cost = card.cost;
                canAfford = GameSystem.Instance.gameState.money >= cost;
                alreadyOwned = ownedCard != null;
                canUpgrade = alreadyOwned && ownedCard.level < card.maxLevel;
                upgradedThisSession = upgradedCardsThisSession.Contains(card.identifier);
                itemName = card.name;
                break;
        }
        
        buyButton.interactable = canAfford && (!alreadyOwned || canUpgrade) && !upgradedThisSession;
        
        var buyButtonText = buyButton.GetComponentInChildren<TMP_Text>();
        if (buyButtonText != null)
        {
            if (upgradedThisSession)
                buyButtonText.text = "Upgraded";
            else if (alreadyOwned && !canUpgrade)
                buyButtonText.text = "Max Level";
            else if (canUpgrade)
                buyButtonText.text = cost + " Upgrade";
            else if (!canAfford)
                buyButtonText.text = cost + " Can't Afford";
            else
                buyButtonText.text = cost + " Buy";
        }
    }
    
    private void PurchaseRune(RuneInfo rune)
    {
        if (GameSystem.Instance.SpendMoney(rune.cost))
        {
            GameSystem.Instance.AddRune(rune);
            ToastManager.Instance.ShowToast($"Purchased {rune.name}!");
            
            // Refresh all shop items
            RefreshAllShopItems();
            UpdateMoneyDisplay();
        }
        else
        {
            ToastManager.Instance.ShowToast($"Can't afford {rune.identifier}!");
        }
    }
    
    private void PurchaseCard(CardInfo card)
    {
        var ownedCard = GameSystem.Instance.gameState.allCards.FirstOrDefault(ownedCard => ownedCard.info.identifier == card.identifier);
        
        if (GameSystem.Instance.SpendMoney(card.cost))
        {
            if (ownedCard != null)
            {
                // Upgrade existing card
                ownedCard.level++;
                
                // Mark this card as upgraded in this session
                upgradedCardsThisSession.Add(card.identifier);
                
                ToastManager.Instance.ShowToast($"Upgraded {card.name} to level {ownedCard.level}!");
            }
            else
            {
                // Purchase new card
                var newCard = new Card(card);
                GameSystem.Instance.gameState.availableCards.Add(newCard);
                GameSystem.Instance.gameState.allCards.Add(newCard);
                ToastManager.Instance.ShowToast($"Purchased {card.name}!");
            }
            
            // Refresh all shop items
            RefreshAllShopItems();
            UpdateMoneyDisplay();
        }
        else
        {
             ToastManager.Instance.ShowToast($"Can't afford {card.identifier}!");
        }
    }
    
    public bool IsCardUpgradedThisSession(string cardIdentifier)
    {
        return upgradedCardsThisSession.Contains(cardIdentifier);
    }
    
    private void RefreshAllShopItems()
    {
        if (itemContainer == null) return;
        
        // Refresh all shop item button states
        ShopItem[] shopItems = itemContainer.GetComponentsInChildren<ShopItem>();
        foreach (var shopItem in shopItems)
        {
            shopItem.RefreshButtonState();
        }
    }
    
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: {GameSystem.Instance.gameState.money}";
        }
    }
    
    private void ProceedToNextDay()
    {
        Hide();
        GameSystem.Instance.StartNewDay();
    }
    
    public static void OpenShop()
    {
        var shop = FindFirstInstance<ShopMenu>();
        if (shop != null)
        {
            shop.Init();
            shop.Show();
        }
        else
        {
            Debug.LogError("ShopMenu not found in scene!");
            // Fallback: Show a simple message
            PopupDialog.Create(
                "Shop", 
                "Shop system requires ShopMenu prefab to be set up in the scene.", 
                "Continue to Next Day",
                () => GameSystem.Instance.StartNewDay()
            );
        }
    }
}