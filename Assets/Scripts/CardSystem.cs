using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    public List<Card> currentHand = new List<Card>();
    
    public System.Action<List<Card>> OnHandChanged;
    
    // Temporary sanity for preview calculations
    private int tempSanityChange = 0;
    public int redrawTimePerDay = 5;
    public int redrawTime = 5;

    public int reversedCardCount()
    {
        return currentHand.Count(card => !card.isUpright);
    }

    public void Redraw()
    {
        if (redrawTime <= 0)
        {
            Debug.LogError("Redraw: Redraw time is 0");
            return;
        }
        redrawTime--;
        
        // Collect unfixed cards to return to deck
        List<Card> redrawCards = new List<Card>();
        
        // Replace unfixed cards in their original positions
        for (int i = 0; i < currentHand.Count; i++)
        {
            if (!currentHand[i].isFixed)
            {
                // Add the unfixed card to redraw list
                redrawCards.Add(currentHand[i]);
                
                // Replace with a new card in the same position
                currentHand[i] = DrawNewCard();
            }
        }
        
        // Return the unfixed cards to used cards pile
        GameSystem.Instance.gameState.usedCards.AddRange(redrawCards);
        
        OnHandChanged?.Invoke(currentHand);
    }
    public void DrawCardsForCustomer()
    {
        // Move current hand to used cards
        foreach (var card in currentHand)
        {
                card.Reset();
        }
        
        GameSystem.Instance.gameState.usedCards.AddRange(currentHand);
        currentHand.Clear();
        
        // Draw 4 cards from available deck
        for (int i = 0; i < 4; i++)
        {
            DrawOneCard();
        }
        
        OnHandChanged?.Invoke(currentHand);
        
    }

    Card DrawNewCard()
    {
        if (GameSystem.Instance.gameState.availableCards.Count == 0)
        {
            Debug.Log("Deck is empty. Refilling...");
            RefillCardDeck();
        }
        int randomIndex = Random.Range(0, GameSystem.Instance.gameState.availableCards.Count);
        Card drawnCard = GameSystem.Instance.gameState.availableCards[randomIndex].Clone();
        drawnCard.isUpright = Random.Range(0, 2) == 0;
        
        GameSystem.Instance.gameState.availableCards.RemoveAt(randomIndex);
        return drawnCard;
    }
    
    void DrawOneCard()
    {
        currentHand.Add(DrawNewCard());
    }
    
    private void RefillCardDeck()
    {
        GameSystem.Instance.gameState.availableCards.AddRange(GameSystem.Instance.gameState.usedCards);
    }
    
    public void FlipCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < currentHand.Count)
        {
            currentHand[cardIndex].FlipCard();
            OnHandChanged?.Invoke(currentHand);
        }
    }
    public void FixCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < currentHand.Count)
        {
            currentHand[cardIndex].Fix();
            OnHandChanged?.Invoke(currentHand);
        }
    }
    
    
    public List<string> allAttributes = new List<string>()
    {
        "wisdom",
        "emotion",
        "power"
    };
    public DivinationResult PerformDivination(Customer customer, bool updateState)
    {
        var result = new DivinationResult();
        result.initialAttributes = new Dictionary<string, int>
        {
            {"wisdom", customer.wisdom},
            {"emotion", customer.emotion},
            {"power", customer.power}
        };
        
        // Initialize sanity values
        result.initialSanity = GameSystem.Instance.gameState.sanity;
        result.finalSanity = result.initialSanity;
        tempSanityChange = 0; // Reset temp sanity change
        
        // Create a copy for calculation
        var tempCustomer = new Customer(customer.info)
        {
            wisdom = customer.wisdom,
            emotion = customer.emotion,
            power = customer.power
        };
        
        // Apply card effects in order
        List<string> allEffects = new List<string>();
        
        allEffects.AddRange(currentHand.SelectMany(card => card.GetEffects()));
        
        for (int i = 0; i < currentHand.Count; i++)
        {
            Card card = currentHand[i];
            var effects = card.GetEffects();
            
            ApplyAttributeEffects(effects, tempCustomer, allEffects, updateState);
        }
        
        for (int i = 0; i < currentHand.Count; i++)
        {
            Card card = currentHand[i];
            var effects = card.GetEffects();
            
            ApplyWhenEffects(effects, tempCustomer, allEffects, currentHand, updateState);
        }
        
        result.finalAttributes = new Dictionary<string, int>
        {
            {"wisdom", tempCustomer.wisdom},
            {"emotion", tempCustomer.emotion},
            {"power", tempCustomer.power}
        };
        
        // Set sanity results
        result.finalSanity = result.initialSanity + tempSanityChange;
        result.tempSanityChange = tempSanityChange;
        
        // Check if customer requirements are satisfied
        Dictionary<string, int> attributeChanges = new Dictionary<string, int>();
        foreach (var attr in result.finalAttributes.Keys)
        {
            attributeChanges[attr] = result.diffAttribute(attr);
        }
        
        result.isSatisfied = customer.AreRequirementsSatisfied(attributeChanges);
        
        if (updateState)
        {
            // Apply the changes to the actual customer (persistent state)
            customer.wisdom = tempCustomer.wisdom;
            customer.emotion = tempCustomer.emotion;
            customer.power = tempCustomer.power;
            
            // Apply sanity changes to game state
            if (tempSanityChange != 0)
            {
                GameSystem.Instance.SubtractSanity(-tempSanityChange);
            }
        }
        
        if (result.isSatisfied)
        {
            // Calculate money reward based on total improvement across all requirements
            int totalImprovement = 0;
            foreach (var requirement in customer.requirements)
            {
                if (attributeChanges.ContainsKey(requirement.attributeName))
                {
                    totalImprovement += attributeChanges[requirement.attributeName];
                }
            }
            result.moneyEarned = Mathf.Max(1, totalImprovement);
        }
        
        return result;
    }
    
    private void ApplySpecialEffects(List<CardEffect> allEffects, Customer customer)
    {
        // Check for Moon card effects
        bool hasUprightMoon = allEffects.Any(e => e.cardId == "moon" && e.isUpright && e.effect == "allNegHalf");
        bool hasReversedMoon = allEffects.Any(e => e.cardId == "moon" && !e.isUpright && e.effect == "allPosHalf");
        
        // Check for Sun + Moon synergy rune
        bool hasSunMoonRune = GameSystem.Instance.HasRune("twin_lights");
        bool hasSun = allEffects.Any(e => e.cardId == "sun");
        bool hasMoon = allEffects.Any(e => e.cardId == "moon");
        
        if (hasSunMoonRune && hasSun && hasMoon)
        {
            // Remove all negative effects
            for (int i = allEffects.Count - 1; i >= 0; i--)
            {
                if (IsNegativeEffect(allEffects[i].effect))
                {
                    allEffects.RemoveAt(i);
                }
            }
        }
        else
        {
            // Apply Moon effects
            if (hasUprightMoon)
            {
                // Halve negative effects
                foreach (var effect in allEffects)
                {
                    if (IsNegativeEffect(effect.effect))
                    {
                        effect.isHalved = true;
                    }
                }
            }
            
            if (hasReversedMoon)
            {
                // Halve positive effects
                foreach (var effect in allEffects)
                {
                    if (IsPositiveEffect(effect.effect))
                    {
                        effect.isHalved = true;
                    }
                }
            }
        }
    }
    
    private void ApplyAttributeEffects(List<string> effects, Customer customer, List<string> allEffects, bool updateState)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            switch (effects[i])
            {
                case "allNegHalf":
                    break;
                case "allPosHalf":
                    break;
                case "wisdom":
                case "emotion":
                case "power":
                {
                    var key = effects[i];
                    i++;
                    var value = int.Parse(effects[i]);

                    adjustValue(key, value, allEffects, customer);
                }
                    break;

                case "sanity":
                {
                    i++;
                    var value = int.Parse(effects[i]);
                    changeSanity(value, updateState);
                    break;
                }
                case "allA":
                {
                    
                    i++;
                    var value = int.Parse(effects[i]);
                    foreach (var key in allAttributes)
                    {
                         adjustValue(key, value, allEffects, customer);
                    }

                    break;
                }
                case "when":
                {
                    return;
                }
            }
        }
    }
    private void ApplyWhenEffects(List<string> effects, Customer customer, List<string> allEffects, List<Card> currentHand, bool updateState)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            switch (effects[i])
            {
                case "when":
                {
                    i++;
                    switch (effects[i])
                    {
                        case "allUpCard":
                            bool allUp = true;
                            for (int j = 0; j < currentHand.Count; j++)
                            {
                                if (!currentHand[j].isUpright)
                                {
                                    allUp = false;
                                }
                            }
                            if (allUp)
                            {
                                i++;
                                ApplyAttributeEffects (effects.GetRange(i, effects.Count - i), customer, allEffects, updateState);
                            }
                            break;
                        case "allDownCard":
                            bool allDown = true;
                            for (int j = 0; j < currentHand.Count; j++)
                                if (currentHand[j].isUpright)
                                {
                                    allDown = false;
                                }

                            if (allDown)
                            {
                                i++;
                                ApplyAttributeEffects (effects.GetRange(i, effects.Count - i), customer, allEffects, updateState);
                            }
                            break;
                        case "total":
                            break;
                    }

                    break;
                }
                default:
                    return;
            }
        }
    }

    void adjustValue(string key, int value, List<string> allEffects, Customer customer)
    {
        if (value > 0)
        {
            if(allEffects.Contains("allPosHalf"))
            {
                value = Mathf.RoundToInt(value / 2f);
            }
            else if (allEffects.Contains("allPosAdd"))
            {
                value += 1;
            }
        }else if (value < 0 && allEffects.Contains("allNegHalf"))
        {
            value = Mathf.RoundToInt(value / 2f);
        }
                    
        customer.ModifyAttribute(key, value);
        
    }

    private void changeSanity(int value, bool updateState)
    {
        if (updateState)
        {
            GameSystem.Instance.SubtractSanity(-value);
        }
        else
        {
            tempSanityChange += value;
        }
    }
    
   
    private bool IsNegativeEffect(string effect)
    {
        if (effect.Contains('|'))
        {
            string[] parts = effect.Split('|');
            if (parts.Length == 2 && int.TryParse(parts[1], out int value))
            {
                return value < 0;
            }
        }
        return false;
    }
    
    private bool IsPositiveEffect(string effect)
    {
        if (effect.Contains('|'))
        {
            string[] parts = effect.Split('|');
            if (parts.Length == 2 && int.TryParse(parts[1], out int value))
            {
                return value > 0;
            }
        }
        return false;
    }
}

[System.Serializable]
public class CardEffect
{
    public string cardId;
    public string effect;
    public bool isUpright;
    public bool isHalved = false;
}

[System.Serializable]
public class DivinationResult
{
    public Dictionary<string, int> initialAttributes;
    public Dictionary<string, int> finalAttributes;
    public int initialSanity;
    public int finalSanity;
    public int tempSanityChange;

    public int diffAttribute(string attribute)
    {
         return finalAttributes[attribute] - initialAttributes[attribute];
    }
    
    public int diffSanity()
    {
        return finalSanity - initialSanity;
    }
    
    public bool isSatisfied;
    public int moneyEarned;
}