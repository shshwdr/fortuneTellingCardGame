using System;
using System.Collections.Generic;
using System.Linq;
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
    public TMP_Text moneyTempText;
    public TMP_Text currentCustomerIndexText;
    public TMP_Text sanityText;
    public TMP_Text sanityTempText;
    public TMP_Text rerollTempText;
    
    
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

    public Button usedCardButton;
    public Button availableCardButton;

    public Transform runeParent;
    public RuneCell[] runes;
    
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
        
        availableCardButton.onClick.AddListener(() =>
        { 
            CardDisplayMenu.ShowCards(GameSystem.Instance.gameState.availableCards, "Available Cards");
        });
        usedCardButton.onClick.AddListener(() =>
        {
            CardDisplayMenu.ShowCards(GameSystem.Instance.gameState.usedCards, "Used Cards");
        });
        runes = runeParent.GetComponentsInChildren<RuneCell>();
        
        // Initial rune display update
        UpdateRunes();
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
            GameSystem.Instance.OnGameStateChanged += UpdateRunes; // Update runes when game state changes
        }
        
        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.OnHandChanged += UpdateCardDisplay;
        }
    }

    void UpdateRunes()
    {
        if (runes == null || GameSystem.Instance == null) return;
        
        var ownedRunes = GameSystem.Instance.gameState.ownedRunes;
        
        // Get preview result to check activated runes
        DivinationResult result = null;
        if (currentCustomer != null)
        {
            result = CardSystem.Instance.PerformDivination(currentCustomer, false);
        }
        
        // Update existing rune cells
        for (int i = 0; i < runes.Length; i++)
        {
            if (i < ownedRunes.Count)
            {
                // Display owned rune
                runes[i].gameObject.SetActive(true);
                runes[i].SetData(ownedRunes[i]);
                
                // Check if this rune is activated in preview
                bool isActivated = false;
                if (result != null && result.activatedRunes != null)
                {
                    // Check if any activated effect belongs to this rune
                    foreach (var activatedEffect in result.activatedRunes)
                    {
                        // Match rune effects by checking if the effect corresponds to this rune
                        // This is a simplified check - in a real implementation, you might want
                        // to have a more robust mapping between runes and their effects
                        if (CheckIfEffectBelongsToRune(ownedRunes[i], activatedEffect))
                        {
                            isActivated = true;
                            break;
                        }
                    }
                }
                
                runes[i].SetIsEffect(isActivated);
            }
            else
            {
                // Hide empty slots
                runes[i].gameObject.SetActive(false);
            }
        }
    }
    
    private bool CheckIfEffectBelongsToRune(Rune rune, string effectName)
    {
        // Match rune effect with activated effect name
        // The rune.info.effect contains the effect type (like "when|allUpCard|refreshCount")
        // and the effectName in activatedRunes should match this
        return rune.info.effect == effectName;
    }
    
    public void PlayRuneActivationAnimations(List<string> activatedEffects)
    {
        if (runes == null || GameSystem.Instance == null || activatedEffects == null) return;
        
        var ownedRunes = GameSystem.Instance.gameState.ownedRunes;
        
        for (int i = 0; i < runes.Length && i < ownedRunes.Count; i++)
        {
            foreach (var effect in activatedEffects)
            {
                if (CheckIfEffectBelongsToRune(ownedRunes[i], effect))
                {
                    runes[i].PlayActivationAnimation();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 清除所有符文的点亮状态和动画效果
    /// </summary>
    public void ClearRuneActivationStates()
    {
        if (runes == null) return;
        
        for (int i = 0; i < runes.Length; i++)
        {
            if (runes[i] != null)
            {
                // 停止动画效果
                runes[i].StopActivationAnimation();
                // 设置为非激活状态
                runes[i].SetIsEffect(false);
            }
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
            GameSystem.Instance.OnGameStateChanged -= UpdateRunes; // Remove runes update listener
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

        // Update temporary money display
        if (moneyTempText != null)
        {
            if (result.tempMoneyChange != 0)
            {
                string changeText = result.tempMoneyChange > 0 ? $"+{result.tempMoneyChange}" : result.tempMoneyChange.ToString();
                moneyTempText.text = $"({changeText})";
                moneyTempText.color = result.tempMoneyChange > 0 ? Color.green : Color.red;
            }
            else
            {
                moneyTempText.text = "";
            }
        }

        // Update temporary reroll display
        if (rerollTempText != null)
        {
            if (result.tempRerollChange != 0)
            {
                string changeText = result.tempRerollChange > 0 ? $"+{result.tempRerollChange}" : result.tempRerollChange.ToString();
                rerollTempText.text = $"({changeText})";
                rerollTempText.color = result.tempRerollChange > 0 ? Color.green : Color.red;
            }
            else
            {
                rerollTempText.text = "";
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
        // Get preview result to show potential reroll changes
        DivinationResult result = null;
        if (currentCustomer != null)
        {
            result = CardSystem.Instance.PerformDivination(currentCustomer, false);
        }
        
        int currentRerolls = CardSystem.Instance.redrawTime;
        int displayRerolls = currentRerolls;
        
        // if (result != null && result.tempRerollChange != 0)
        // {
        //     displayRerolls += result.tempRerollChange;
        // }
        
        redrawButton.GetComponentInChildren<TMP_Text>().text = $"Redraw({displayRerolls})";
        redrawButton.interactable = currentRerolls > 0;
    }

    private void PerformDivination()
    {
        if (currentCustomer == null) return;

        // if (CardSystem.Instance.reversedCardCount() != 2)
        // {
        //     return;
        // }
        
        var result = CardSystem.Instance.PerformDivination(currentCustomer,true);
        
        // Play rune activation animations for activated effects
        if (result.activatedRunes != null && result.activatedRunes.Count > 0)
        {
            PlayRuneActivationAnimations(result.activatedRunes);
        }
        
        
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


        CardSystem.Instance.ClearHand();
        
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
        CardSystem.Instance.ClearHand();
        DialogueManager.Instance.StartDialogue(currentCustomer.identifier+"Request","skipCustomer", () =>
        {
            GameSystem.Instance.NextCustomer();
        });
    }
    
    private void NextDay()
    {
        GameSystem.Instance.StartNewDay();
    }

    private void Update()
    {
        
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var currentHandString =  CardSystem.Instance.currentHand.Select(card => card.info.identifier).ToList();
                Debug.Log( $"Current Hand: {string.Join(", ", currentHandString)}");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                var deckString = GameSystem.Instance.gameState.availableCards.Select(card => card.info.identifier).ToList();
                Debug.Log( $"Deck: {string.Join(", ", deckString)}");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                var usedString = GameSystem.Instance.gameState.usedCards.Select(card => card.info.identifier).ToList();
                Debug.Log( $"Used: {string.Join(", ", usedString)}");
            }
        
    }
}