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
    public void DrawCardsForCustomer()
    {
        // Move current hand to used cards
        GameSystem.Instance.gameState.usedCards.AddRange(currentHand);
        currentHand.Clear();
        
        // Draw 4 cards from available deck
        for (int i = 0; i < 4; i++)
        {
            if (GameSystem.Instance.gameState.availableCards.Count == 0)
            {
                Debug.Log("Deck is empty. Refilling...");
                RefillCardDeck();
            }
            int randomIndex = Random.Range(0, GameSystem.Instance.gameState.availableCards.Count);
            Card drawnCard = GameSystem.Instance.gameState.availableCards[randomIndex].Clone();
            drawnCard.isUpright = false;
            currentHand.Add(drawnCard);
            
            GameSystem.Instance.gameState.availableCards.RemoveAt(randomIndex);
        }
        
        OnHandChanged?.Invoke(currentHand);
        
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

                    var key = effects[i];
                    i++;
                    var value = int.Parse(effects[i]);

                    if (value > 0 && allEffects.Contains("allPosHalf"))
                    {
                        value = Mathf.RoundToInt(value / 2f);
                    }else if (value < 0 && allEffects.Contains("allNegHalf"))
                    {
                        value = Mathf.RoundToInt(value / 2f);
                    }
                    
                    customer.ModifyAttribute(key, value);
                    break;
            }
        }
    }
    
    private void ApplyEffect(CardEffect cardEffect, Customer customer)
    {
        string effect = cardEffect.effect;
        
        // Skip special effects (already handled)
        if (effect == "allNegHalf" || effect == "allPosHalf")
            return;
            
        // Parse attribute effects (format: "attribute|value")
        string[] parts = effect.Split('|');
        if (parts.Length == 2)
        {
            string attribute = parts[0];
            if (int.TryParse(parts[1], out int value))
            {
                if (cardEffect.isHalved)
                {
                    value = Mathf.RoundToInt(value / 2f);
                }
                
                customer.ModifyAttribute(attribute, value);
            }
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

    public int diffAttribute(string attribute)
    {
         return finalAttributes[attribute] - initialAttributes[attribute];
    }
    public bool isSatisfied;
    public int moneyEarned;
}