using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    public List<Card> currentHand = new List<Card>();
    
    public System.Action<List<Card>> OnHandChanged;

    public int reversedCardCount()
    {
        return currentHand.Count(card => !card.isUpright);
    }

    public void Redraw()
    {
        List<Card> redrawCards = new List<Card>();
        foreach (var card in currentHand)
        {
            if (!card.isFixed)
            {
                redrawCards.Add(card);
            }
        }
        GameSystem.Instance.gameState.usedCards.AddRange(redrawCards);
        currentHand.RemoveAll(card => !card.isFixed);
        int drawCount = 4 - currentHand.Count;
        for (int i = 0; i < drawCount; i++)
        {
            DrawOneCard();
        }
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

    void DrawOneCard()
    {
        if (GameSystem.Instance.gameState.availableCards.Count == 0)
        {
            Debug.Log("Deck is empty. Refilling...");
            RefillCardDeck();
        }
        int randomIndex = Random.Range(0, GameSystem.Instance.gameState.availableCards.Count);
        Card drawnCard = GameSystem.Instance.gameState.availableCards[randomIndex].Clone();
        drawnCard.isUpright = Random.Range(0, 2) == 0;
        currentHand.Add(drawnCard);
            
        GameSystem.Instance.gameState.availableCards.RemoveAt(randomIndex);
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
        "sanity",
        "power"
    };
    public DivinationResult PerformDivination(Customer customer, bool updateState)
    {
        var result = new DivinationResult();
        result.initialAttributes = new Dictionary<string, int>
        {
            {"wisdom", customer.wisdom},
            {"emotion", customer.emotion},
            {"sanity", customer.sanity},
            {"power", customer.power}
        };
        
        // Create a copy for calculation
        var tempCustomer = new Customer(customer.info)
        {
            wisdom = customer.wisdom,
            emotion = customer.emotion,
            sanity = customer.sanity,
            power = customer.power
        };
        
        // Apply card effects in order
        List<string> allEffects = new List<string>();
        
        allEffects.AddRange(currentHand.SelectMany(card => card.GetEffects()));
        
        for (int i = 0; i < currentHand.Count; i++)
        {
            Card card = currentHand[i];
            var effects = card.GetEffects();
            
            ApplyAttributeEffects(effects, tempCustomer,allEffects);
        }
        
        for (int i = 0; i < currentHand.Count; i++)
        {
            Card card = currentHand[i];
            var effects = card.GetEffects();
            
            ApplyWhenEffects(effects, tempCustomer,allEffects,currentHand);
        }
        
        // // Apply special effects first (Moon card effects)
        // ApplySpecialEffects(allEffects, tempCustomer);
        
        // Apply normal attribute changes

        
        result.finalAttributes = new Dictionary<string, int>
        {
            {"wisdom", tempCustomer.wisdom},
            {"emotion", tempCustomer.emotion},
            {"sanity", tempCustomer.sanity},
            {"power", tempCustomer.power}
        };
        // Check if customer is satisfied
        string targetAttribute = customer.info.target;
        int initialValue = result.initialAttributes[targetAttribute];
        int finalValue = result.finalAttributes[targetAttribute];
        
        result.isSatisfied = finalValue > initialValue;
        
        if (updateState)
        {
            // Apply the changes to the actual customer (persistent state)
            customer.wisdom = tempCustomer.wisdom;
            customer.emotion = tempCustomer.emotion;
            customer.sanity = tempCustomer.sanity;
            customer.power = tempCustomer.power;
        }
        
        if (result.isSatisfied)
        {
            // Calculate money reward based on improvement
            int improvement = finalValue - initialValue;
            result.moneyEarned = Mathf.Max(1, improvement);
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
    
    private void ApplyAttributeEffects(List<string> effects, Customer customer,List<string> allEffects)
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
                case "sanity":
                case "power":
                {
                    var key = effects[i];
                    i++;
                    var value = int.Parse(effects[i]);

                    adjustValue(key, value, allEffects, customer);
                }
                    break;
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
    private void ApplyWhenEffects(List<string> effects, Customer customer,List<string> allEffects, List<Card> currentHand)
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
                                ApplyAttributeEffects (effects.GetRange(i, effects.Count - i), customer,allEffects);
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
                                ApplyAttributeEffects (effects.GetRange(i, effects.Count - i), customer,allEffects);
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

    public int diffAttribute(string attribute)
    {
         return finalAttributes[attribute] - initialAttributes[attribute];
    }
    public bool isSatisfied;
    public int moneyEarned;
}