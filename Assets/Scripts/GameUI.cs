using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Customer Info UI")]
    public Text customerNameText;
    public Text customerTargetText;
    public Text wealthText;
    public Text relationshipText;
    public Text sanityText;
    public Text powerText;
    
    [Header("Game State UI")]
    public Text dayText;
    public Text moneyText;
    public Text currentCustomerIndexText;
    
    [Header("Card UI")]
    public Transform cardContainer;
    public CardUI[] cardSlots = new CardUI[4];
    
    [Header("Action Buttons")]
    public Button performDivinationButton;
    public Button skipCustomerButton;
    public Button nextDayButton;
    
    [Header("Result UI")]
    public GameObject resultPanel;
    public Text resultText;
    public Text moneyEarnedText;
    
    private Customer currentCustomer;
    
    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeUI()
    {
        if (performDivinationButton != null)
            performDivinationButton.onClick.AddListener(PerformDivination);
            
        if (skipCustomerButton != null)
            skipCustomerButton.onClick.AddListener(SkipCustomer);
            
        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(NextDay);
            
        // Setup card slots
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null)
            {
                int index = i; // Capture for closure
                cardSlots[i].OnCardClicked += (cardIndex) => OnCardClicked(cardIndex);
            }
        }
        
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
    
    private void SubscribeToEvents()
    {
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.OnCustomerChanged += UpdateCustomerDisplay;
            GameSystem.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            GameSystem.Instance.OnDayChanged += UpdateDayDisplay;
            GameSystem.Instance.OnGameStateChanged += UpdateGameStateDisplay;
        }
        
        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.OnHandChanged += UpdateCardDisplay;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.OnCustomerChanged -= UpdateCustomerDisplay;
            GameSystem.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            GameSystem.Instance.OnDayChanged -= UpdateDayDisplay;
            GameSystem.Instance.OnGameStateChanged -= UpdateGameStateDisplay;
        }
        
        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.OnHandChanged -= UpdateCardDisplay;
        }
    }
    
    private void UpdateCustomerDisplay(Customer customer)
    {
        currentCustomer = customer;
        
        if (customer == null)
        {
            if (customerNameText != null) customerNameText.text = "No Customer";
            if (customerTargetText != null) customerTargetText.text = "";
            return;
        }
        
        if (customerNameText != null)
            customerNameText.text = customer.info.name;
            
        if (customerTargetText != null)
            customerTargetText.text = $"Wants: {customer.info.target}";
            
        UpdateCustomerAttributes(customer);
    }
    
    private void UpdateCustomerAttributes(Customer customer)
    {
        if (wealthText != null)
            wealthText.text = $"Wealth: {customer.wealth}";
            
        if (relationshipText != null)
            relationshipText.text = $"Relationship: {customer.relationship}";
            
        if (sanityText != null)
            sanityText.text = $"Sanity: {customer.sanity}";
            
        if (powerText != null)
            powerText.text = $"Power: {customer.power}";
    }
    
    private void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
            moneyText.text = $"Money: {money}";
    }
    
    private void UpdateDayDisplay(int day)
    {
        if (dayText != null)
            dayText.text = $"Day: {day}";
    }
    
    private void UpdateGameStateDisplay()
    {
        var gameState = GameSystem.Instance.gameState;
        
        if (currentCustomerIndexText != null)
        {
            int current = gameState.currentCustomerIndex + 1;
            int total = gameState.todayCustomers.Count;
            currentCustomerIndexText.text = $"Customer: {current}/{total}";
        }
    }
    
    private void UpdateCardDisplay(List<string> cards, List<bool> orientations)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null)
            {
                if (i < cards.Count)
                {
                    cardSlots[i].gameObject.SetActive(true);
                    cardSlots[i].SetCardData(cards[i], i);
                    cardSlots[i].SetUpright(orientations[i]);
                }
                else
                {
                    cardSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
    private void OnCardClicked(int cardIndex)
    {
        CardSystem.Instance.FlipCard(cardIndex);
    }
    
    private void PerformDivination()
    {
        if (currentCustomer == null) return;
        
        var result = CardSystem.Instance.PerformDivination(currentCustomer);
        
        ShowResult(result);
        
        if (result.isSatisfied)
        {
            GameSystem.Instance.AddMoney(result.moneyEarned);
        }
        
        // Wait for player to close result panel before proceeding
    }
    
    private void ShowResult(DivinationResult result)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            
            if (resultText != null)
            {
                string resultMessage = result.isSatisfied ? "Customer Satisfied!" : "Customer Disappointed...";
                resultMessage += "\n\nAttribute Changes:\n";
                
                foreach (var attr in result.initialAttributes.Keys)
                {
                    int initial = result.initialAttributes[attr];
                    int final = result.finalAttributes[attr];
                    int change = final - initial;
                    string changeText = change >= 0 ? $"+{change}" : change.ToString();
                    resultMessage += $"{attr}: {initial} -> {final} ({changeText})\n";
                }
                
                resultText.text = resultMessage;
            }
            
            if (moneyEarnedText != null)
            {
                moneyEarnedText.text = $"Money Earned: {result.moneyEarned}";
            }
        }
    }
    
    public void CloseResultPanel()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        GameSystem.Instance.NextCustomer();
        CardSystem.Instance.DrawCardsForCustomer();
    }
    
    private void SkipCustomer()
    {
        // Add small sanity boost for skipping
        if (currentCustomer != null)
        {
            GameSystem.Instance.AddMoney(1); // Small consolation
        }
        
        GameSystem.Instance.NextCustomer();
        CardSystem.Instance.DrawCardsForCustomer();
    }
    
    private void NextDay()
    {
        GameSystem.Instance.StartNewDay();
        CardSystem.Instance.DrawCardsForCustomer();
    }
}