using System.Collections.Generic;
using Naninovel;
using Naninovel.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Simplified GameUI that works with both CardUI and CardUISimple
/// </summary>
public class SimpleGameUI : MonoBehaviour
{
    [Header("Customer Info UI")]
    public TMP_Text customerNameText;
    public TMP_Text customerTargetText;
    public TMP_Text customerNextChatText;
    [FormerlySerializedAs("wealthText")] public AttributeCell wisdomText;
    public AttributeCell relationshipText;
    //public AttributeCell sanityText;
    public AttributeCell powerText;
    
    [Header("Game State UI")]
    public TMP_Text dayText;
    public TMP_Text moneyText;
    public TMP_Text currentCustomerIndexText;
    public TMP_Text sanityText;
    public TMP_Text sanityTempText;
    
    
    [Header("Card UI")]
    public Transform cardContainer;
    // Using MonoBehaviour array to support both CardUI and CardUISimple
    public CardUISimple[] cardSlots = new CardUISimple[4];
    
    [Header("Action Buttons")]
    public Button performDivinationButton;
    public Button skipCustomerButton;
    public Button nextDayButton;
    public Button redrawButton;
    
    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public TMP_Text moneyEarnedText;

    public GameObject desk;
    
    private Customer currentCustomer;

    public Image customerImage;

    public Button logButton;
    
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
        
        redrawButton.onClick.AddListener(() =>
        {
            CardSystem.Instance.Redraw();
            UpdateCustomerAttributes(currentCustomer);
        });
            
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
        logButton.onClick.AddListener(() =>
        {
            var uiManager = Engine.GetService<IUIManager>();
            uiManager.GetUI<IBacklogUI>()?.Show();
        });
    }
    
    private void SubscribeToEvents()
    {
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.OnCustomerShow += ShowCustomer;
            GameSystem.Instance.OnCustomerChanged += UpdateCustomerDisplay;
            GameSystem.Instance.OnAttributeChanged += UpdateCurrentCustomerAttributes;
            GameSystem.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            GameSystem.Instance.OnSanityChanged += UpdateSanityDisplay;
            
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
            GameSystem.Instance.OnCustomerShow -= ShowCustomer;
            GameSystem.Instance.OnCustomerChanged -= UpdateCustomerDisplay;
            GameSystem.Instance.OnAttributeChanged -= UpdateCurrentCustomerAttributes;
            GameSystem.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            GameSystem.Instance.OnDayChanged -= UpdateDayDisplay;
            GameSystem.Instance.OnGameStateChanged -= UpdateGameStateDisplay;
        }
        
        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.OnHandChanged -= UpdateCardDisplay;
        }
    }

    private void ShowCustomer(Customer customer)
    {
        currentCustomer = customer;
        customerImage.enabled = true;
        customerImage.sprite = Resources.Load<Sprite>("Characters/" + currentCustomer.info.identifier);
        customerImage.GetComponent<Animator>().SetTrigger("walkin");
        SFXManager.Instance.PlayMessage();
    }
    
    private void UpdateCustomerDisplay(Customer customer)
    {
        currentCustomer = customer;
        customerImage.enabled = true;
        customerImage.sprite = Resources.Load<Sprite>("Characters/" + currentCustomer.info.identifier);
        
        if (customer == null)
        {
            if (customerNameText != null) customerNameText.text = "";
            if (customerTargetText != null) customerTargetText.text = "";
            customerNextChatText.text = "";
            return;
        }
        
        if (customerNameText != null)
            customerNameText.text = customer.info.name;
            
            
        UpdateCustomerAttributes(customer);
    }



    public void UpdateCurrentCustomerAttributes()
    {
        UpdateCustomerAttributes(currentCustomer);
    }
    private void UpdateCustomerAttributes(Customer customer)
    {
        var result = CardSystem.Instance.PerformDivination(currentCustomer, false);
        powerText.SetValue("Power", customer.power, result.diffAttribute("power"));
        relationshipText.SetValue("Emotion", customer.emotion, result.diffAttribute("emotion"));
        wisdomText.SetValue("Wisdom", customer.wisdom, result.diffAttribute("wisdom"));
        
        // Update temporary sanity display
        if (sanityTempText != null)
        {
            if (result.tempSanityChange != 0)
            {
                string changeText = result.tempSanityChange > 0 ? $"+{result.tempSanityChange}" : result.tempSanityChange.ToString();
                sanityTempText.text = $"({changeText})";
                sanityTempText.color = result.tempSanityChange > 0 ? Color.green : Color.red;
            }
            else
            {
                sanityTempText.text = "";
            }
        }

        var satisfiedText = result.isSatisfied ? "(Satisfied!)" : "";
        var satisfiedTextColor = result.isSatisfied ? "<color=green>" : "<color=red>";
        var closeColor = result.isSatisfied ? "</color>" : "</color>";
        
        if (customerTargetText != null)
        {
            string requirementsText = customer.GetRequirementsText();
            customerTargetText.text = $"{satisfiedTextColor}Needs: {requirementsText} {satisfiedText}{closeColor}";
        }

        customerNextChatText.text = CustomFunctions.NextStoryRequest();

        UpdateActions();
    }
    
    private void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
            moneyText.text = $"Money: {money}";
    }
    
    private void UpdateSanityDisplay(int sanity)
    {
        if (sanityText != null)
            sanityText.text = $"Sanity: {sanity}";
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
        CardSystem.Instance.FixCard(cardIndex);
        //UpdateCustomerAttributes(currentCustomer);
    }

    void UpdateActions()
    {
        redrawButton.GetComponentInChildren<TMP_Text>().text = $"Redraw({CardSystem.Instance.redrawTime})";
        redrawButton.interactable = CardSystem.Instance.redrawTime>0;
        // if (CardSystem.Instance.reversedCardCount() != 2)
        // {
        //     performDivinationButton.interactable = false;
        //     performDivinationButton.GetComponentInChildren<TMP_Text>().text = "Require 2 reversed cards";
        // }
        // else
        // {
        //     performDivinationButton.interactable = true;
        //     performDivinationButton.GetComponentInChildren<TMP_Text>().text = "Fortune Telling";
        // }
    }

    private void PerformDivination()
    {
        if (currentCustomer == null) return;

        // if (CardSystem.Instance.reversedCardCount() != 2)
        // {
        //     return;
        // }
        
        var result = CardSystem.Instance.PerformDivination(currentCustomer,true);
        
        
        //final result

        if (GameSystem.Instance.GetSanity() <= 0)
        {
            GameSystem.Instance.ShowGameOver("You are too insane to continue.");
            return;
        }
        else
        {
            foreach (var attr in  CardSystem.Instance.allAttributes)
            {
                if (currentCustomer.GetAttribute(attr) <= 0)
                {
                    GameSystem.Instance.ShowGameOver($"Customer's {attr} is too low, they left upset.");
                    return;
                }
            }
        }
        
        ShowResult(result);
        
        var character = GameSystem.Instance.GetCurrentCustomer();
        character.talkedTime++;
        
        
        
        
        if (result.isSatisfied)
        {
            ToastManager.Instance.ShowToast($"Customer is happy! You earned {result.moneyEarned} gold");
            GameSystem.Instance.AddMoney(result.moneyEarned);
            GameSystem.Instance.CustomerServed(); // Track that we successfully served a customer
            
            DialogueManager.Instance.StartDialogue(character.identifier+"Request","goodResult", () =>
            {
                GameSystem.Instance.NextCustomer();
            });
        }
        else
        {
            
            ToastManager.Instance.ShowToast($"The customer is not satisfied...");
            DialogueManager.Instance.StartDialogue(character.identifier+"Request","badResult", () =>
            {
                GameSystem.Instance.NextCustomer();
            });
        }
        
        
        GameSystem.Instance.OnAttributeChanged?.Invoke();
        
        
        
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
    }
    
    private void SkipCustomer()
    {
        GameSystem.Instance.NextCustomer();
    }
    
    private void NextDay()
    {
        GameSystem.Instance.StartNewDay();
    }
}