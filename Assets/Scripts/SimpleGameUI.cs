using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simplified GameUI that works with both CardUI and CardUISimple
/// </summary>
public class SimpleGameUI : MonoBehaviour
{
    [Header("Customer Info UI")]
    public TMP_Text customerNameText;
    public TMP_Text customerTargetText;
    public AttributeCell wealthText;
    public AttributeCell relationshipText;
    public AttributeCell sanityText;
    public AttributeCell powerText;
    
    [Header("Game State UI")]
    public TMP_Text dayText;
    public TMP_Text moneyText;
    public TMP_Text currentCustomerIndexText;
    
    [Header("Card UI")]
    public Transform cardContainer;
    // Using MonoBehaviour array to support both CardUI and CardUISimple
    public CardUISimple[] cardSlots = new CardUISimple[4];
    
    [Header("Action Buttons")]
    public Button performDivinationButton;
    public Button skipCustomerButton;
    public Button nextDayButton;
    
    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public TMP_Text moneyEarnedText;
    
    private Customer currentCustomer;

    public Image customerImage;
    
    void Awake()
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
            
        // Setup card slots - works with both CardUI and CardUISimple
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null)
            {
                int index = i; // Capture for closure
                
                // Try CardUI first
                {
                    // Try CardUISimple
                    var cardUISimple = cardSlots[i] as CardUISimple;
                    if (cardUISimple != null)
                    {
                        cardUISimple.OnCardClicked += (cardIndex) => OnCardClicked(cardIndex);
                    }
                }
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

        customerImage.sprite = Resources.Load<Sprite>("Characters/" + currentCustomer.info.identifier);
        
        if (customer == null)
        {
            if (customerNameText != null) customerNameText.text = "No Customer";
            if (customerTargetText != null) customerTargetText.text = "";
            return;
        }
        
        if (customerNameText != null)
            customerNameText.text = customer.info.name;
            
            
        UpdateCustomerAttributes(customer);
    }
    
    
    
    private void UpdateCustomerAttributes(Customer customer)
    {
        var result = CardSystem.Instance.PerformDivination(currentCustomer,false);
        wealthText.SetValue("Wealth", customer.wealth, result.diffAttribute("wealth"));
        relationshipText .SetValue("Relationship", customer.relationship, result.diffAttribute("rela"));
        sanityText.SetValue("Sanity", customer.sanity, result.diffAttribute("sanity"));
        powerText.SetValue("Power", customer.power, result.diffAttribute("power"));

        var satisfiedText = result.isSatisfied ? "(Satisfied!)" : "";
        var satisfiedTextColor = result.isSatisfied ? "<color=green>":"";
        if (customerTargetText != null)
            customerTargetText.text = $"{satisfiedTextColor}Wants: {customer.info.target} {satisfiedText}";

        UpdateActions();
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
    
    private void UpdateCardDisplay(List<Card> cards)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null)
            {
                if (i < cards.Count)
                {
                    cardSlots[i].gameObject.SetActive(true);
                    
                    var cardUISimple = cardSlots[i] as CardUISimple;
                    if (cardUISimple != null)
                    {
                        cardUISimple.SetCardData(cards[i], i);
                    }
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
        UpdateCustomerAttributes(currentCustomer);
    }

    void UpdateActions()
    {
        if (CardSystem.Instance.reversedCardCount() != 2)
        {
            performDivinationButton.interactable = false;
            performDivinationButton.GetComponentInChildren<TMP_Text>().text = "Require 2 reversed cards";
        }
        else
        {
            performDivinationButton.interactable = true;
            performDivinationButton.GetComponentInChildren<TMP_Text>().text = "Fortune Telling";
        }
    }

    private void PerformDivination()
    {
        if (currentCustomer == null) return;

        if (CardSystem.Instance.reversedCardCount() != 2)
        {
            return;
        }
        
        var result = CardSystem.Instance.PerformDivination(currentCustomer,true);
        
        ShowResult(result);
        
        if (result.isSatisfied)
        {
            GameSystem.Instance.AddMoney(result.moneyEarned);
        }
        
        GameSystem.Instance.NextCustomer();
        CardSystem.Instance.DrawCardsForCustomer();
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
        GameSystem.Instance.NextCustomer();
        CardSystem.Instance.DrawCardsForCustomer();
    }
    
    private void NextDay()
    {
        GameSystem.Instance.StartNewDay();
        CardSystem.Instance.DrawCardsForCustomer();
    }
}