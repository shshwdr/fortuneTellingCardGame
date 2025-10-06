using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ShopMenu : MenuBase
{
    [Header("Shop Components")]
    public Transform runeContainer;
    public Transform cardContainer;
    public GameObject runeItemPrefab;
    public GameObject cardItemPrefab;
    public TMP_Text moneyText;
    public Button nextDayButton;
    
    private List<RuneInfo> availableRunes = new List<RuneInfo>();
    private List<CardInfo> availableCards = new List<CardInfo>();
    
    public override void Init()
    {
        base.Init();
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
        CreateRuneItems();
        CreateCardItems();
    }
    
    private void GenerateAvailableItems()
    {
        availableRunes.Clear();
        availableCards.Clear();
        
        // Get 3 random runes
        var allRunes = CSVLoader.Instance.runeInfoMap.Values.ToList();
        var randomRunes = allRunes.OrderBy(x => Random.value).Take(3).ToList();
        availableRunes.AddRange(randomRunes);
        
        // Get 3 random cards that can be drawn and are not already owned
        var allCards = CSVLoader.Instance.cardInfoMap.Values.Where(card => 
            card.canBeDraw && 
            !GameSystem.Instance.gameState.availableCards.Any(ownedCard => ownedCard.info.identifier == card.identifier)
        ).ToList();
        
        var randomCards = allCards.OrderBy(x => Random.value).Take(3).ToList();
        availableCards.AddRange(randomCards);
    }
    
    private void CreateRuneItems()
    {
        if (runeContainer == null || runeItemPrefab == null) return;
        
        // Clear existing items
        foreach (Transform child in runeContainer)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var rune in availableRunes)
        {
            //GameObject runeItem = Instantiate(runeItemPrefab, runeContainer);
            //SetupRuneItem(runeItem, rune);
        }
    }
    
    private void CreateCardItems()
    {
        if (cardContainer == null || cardItemPrefab == null) return;
        
        // Clear existing items
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var card in availableCards)
        {
            //GameObject cardItem = Instantiate(cardItemPrefab, cardContainer);
            //SetupCardItem(cardItem, card);
        }
    }
    
    private void SetupRuneItem(GameObject itemObj, RuneInfo rune)
    {
        var nameText = itemObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        var descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
        var costText = itemObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
        var buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
        
        if (nameText != null) nameText.text = rune.name;
        if (descText != null) descText.text = rune.description;
        if (costText != null) costText.text = $"Cost: {rune.cost}";
        
        if (buyButton != null)
        {
            bool canAfford = GameSystem.Instance.gameState.money >= rune.cost;
            buyButton.interactable = canAfford && !GameSystem.Instance.HasRune(rune.identifier);
            
            var buyButtonText = buyButton.GetComponentInChildren<TMP_Text>();
            if (buyButtonText != null)
            {
                if (GameSystem.Instance.HasRune(rune.identifier))
                    buyButtonText.text = "Owned";
                else if (!canAfford)
                    buyButtonText.text = "Can't Afford";
                else
                    buyButtonText.text = "Buy";
            }
            
            buyButton.onClick.AddListener(() => PurchaseRune(rune));
        }
    }
    
    private void SetupCardItem(GameObject itemObj, CardInfo card)
    {
        var nameText = itemObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        var descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
        var costText = itemObj.transform.Find("CostText")?.GetComponent<TMP_Text>();
        var buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
        
        if (nameText != null) nameText.text = card.name;
        if (descText != null) descText.text = card.description;
        
        // Cards cost is not defined in CardInfo, let's set a default cost based on rarity
        int cardCost = 10; // Default cost for cards
        if (costText != null) costText.text = $"Cost: {cardCost}";
        
        if (buyButton != null)
        {
            bool canAfford = GameSystem.Instance.gameState.money >= cardCost;
            bool alreadyOwned = GameSystem.Instance.gameState.availableCards.Any(ownedCard => ownedCard.info.identifier == card.identifier);
            
            buyButton.interactable = canAfford && !alreadyOwned;
            
            var buyButtonText = buyButton.GetComponentInChildren<TMP_Text>();
            if (buyButtonText != null)
            {
                if (alreadyOwned)
                    buyButtonText.text = "Owned";
                else if (!canAfford)
                    buyButtonText.text = "Can't Afford";
                else
                    buyButtonText.text = "Buy";
            }
            
            buyButton.onClick.AddListener(() => PurchaseCard(card, cardCost));
        }
    }
    
    private void PurchaseRune(RuneInfo rune)
    {
        if (GameSystem.Instance.SpendMoney(rune.cost))
        {
            GameSystem.Instance.gameState.ownedRunes.Add(rune.identifier);
            ToastManager.Instance.ShowToast($"Purchased {rune.name}!");
            
            // Refresh the shop display
            SetupShop();
            UpdateMoneyDisplay();
        }
    }
    
    private void PurchaseCard(CardInfo card, int cost)
    {
        if (GameSystem.Instance.SpendMoney(cost))
        {
            GameSystem.Instance.gameState.availableCards.Add(new Card(card));
            ToastManager.Instance.ShowToast($"Purchased {card.name}!");
            
            // Refresh the shop display
            SetupShop();
            UpdateMoneyDisplay();
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