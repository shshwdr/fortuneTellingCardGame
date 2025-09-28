using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    public List<string> currentHand = new List<string>();
    public List<bool> cardOrientations = new List<bool>(); // true = upright, false = reversed
    
    public System.Action<List<string>, List<bool>> OnHandChanged;

    public int reversedCardCount()
    {
        return cardOrientations.Count(o => !o) ;
    }
    public void DrawCardsForCustomer()
    {
        GameSystem.Instance.gameState.usedCards.AddRange(currentHand);
        currentHand.Clear();
        cardOrientations.Clear();
        
        // Draw 4 cards from available deck
        for (int i = 0; i < 4; i++)
        {
            if (GameSystem.Instance.gameState.availableCards.Count == 0)
            {
                Debug.Log("Deck is empty. Refilling...");
                RefillCardDeck();
            }
            int randomIndex = Random.Range(0, GameSystem.Instance.gameState.availableCards.Count);
            string drawnCard = GameSystem.Instance.gameState.availableCards[randomIndex];
            
            currentHand.Add(drawnCard);
            cardOrientations.Add(true); // Default to upright
            
            GameSystem.Instance.gameState.availableCards.RemoveAt(randomIndex);
        }
        
        // // If deck is empty, refill it
        // if (GameSystem.Instance.gameState.availableCards.Count == 0)
        // {
        //     RefillCardDeck();
        // }
        
        OnHandChanged?.Invoke(currentHand, cardOrientations);
    }
    
    private void RefillCardDeck()
    {
        GameSystem.Instance.gameState.availableCards.AddRange(GameSystem.Instance.gameState.usedCards);
    }
    
    public void FlipCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < cardOrientations.Count)
        {
            cardOrientations[cardIndex] = !cardOrientations[cardIndex];
            OnHandChanged?.Invoke(currentHand, cardOrientations);
        }
    }
    
    
    
    public DivinationResult PerformDivination(Customer customer)
    {
        var result = new DivinationResult();
        result.initialAttributes = new Dictionary<string, int>
        {
            {"wealth", customer.wealth},
            {"rela", customer.relationship},
            {"sanity", customer.sanity},
            {"power", customer.power}
        };
        
        // Create a copy for calculation
        var tempCustomer = new Customer(customer.info)
        {
            wealth = customer.wealth,
            relationship = customer.relationship,
            sanity = customer.sanity,
            power = customer.power
        };
        
        // Apply card effects in order
        List<CardEffect> allEffects = new List<CardEffect>();
        
        for (int i = 0; i < currentHand.Count; i++)
        {
            string cardId = currentHand[i];
            bool isUpright = cardOrientations[i];
            
            var cardInfo = CSVLoader.Instance.cardInfoMap[cardId];
            var effects = isUpright ? cardInfo.upEffect : cardInfo.downEffect;
            
            
            ApplyAttributeEffects(effects, tempCustomer);
        }
        
        // // Apply special effects first (Moon card effects)
        // ApplySpecialEffects(allEffects, tempCustomer);
        
        // Apply normal attribute changes
        
        result.finalAttributes = new Dictionary<string, int>
        {
            {"wealth", tempCustomer.wealth},
            {"rela", tempCustomer.relationship},
            {"sanity", tempCustomer.sanity},
            {"power", tempCustomer.power}
        };
        
        // Check if customer is satisfied
        string targetAttribute = customer.info.target;
        int initialValue = result.initialAttributes[targetAttribute];
        int finalValue = result.finalAttributes[targetAttribute];
        
        result.isSatisfied = finalValue > initialValue;
        
        if (result.isSatisfied)
        {
            // Calculate money reward based on improvement
            int improvement = finalValue - initialValue;
            result.moneyEarned = Mathf.Max(1, improvement / 2);
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
    
    private void ApplyAttributeEffects(List<string> allEffects, Customer customer)
    {
        for (int i = 0; i < allEffects.Count; i++)
        {
            switch (allEffects[i])
            {
                case "allNegHalf":
                    break;
                case "allPosHalf":
                    break;
                case "wealth":
                case "rela":
                case "sanity":
                case "power":

                    var key = allEffects[i];
                    i++;
                    var value = int.Parse(allEffects[i]);
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