using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardSystem : Singleton<CardSystem>
{
    public List<Card> currentHand = new List<Card>();
    
    public System.Action<List<Card>> OnHandChanged;
    
    // Temporary values for preview calculations
    private int tempSanityChange = 0;
    private int tempMoneyChange = 0;
    private int tempRerollChange = 0;

    public int redrawTimePerDay
    {
        get
        {
            return 7+ RuneManager.Instance.getEffectValue("redrawCount");
        }
    }
    public int redrawTime = 0;
    public void AddRedrawTime (int time)
    {
        redrawTime += time;
    }

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
                //redrawCards.Add(currentHand[i]);
                GameSystem.Instance.gameState.usedCards.Add(currentHand[i]);
                // Replace with a new card in the same position
                currentHand[i] = DrawNewCard();
            }
        }
        
        // Return the unfixed cards to used cards pile
        //GameSystem.Instance.gameState.usedCards.AddRange(redrawCards);
        
        OnHandChanged?.Invoke(currentHand);
        GameSystem.Instance.OnGameStateChanged?.Invoke();
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
        
        // Clear all temporary effects
        ClearTemporaryEffects();

        if (GameSystem.Instance.gameState.currentDay == 1 && GameSystem.Instance.gameState.currentCustomerIndex == 0)
        {
            DrawCard("sun",true);
            DrawCard("fool",false);
            DrawCard("magician",true);
            DrawCard("world",false);
            
        }
        else if (GameSystem.Instance.gameState.currentDay == 1 &&
                 GameSystem.Instance.gameState.currentCustomerIndex == 1)
        {
            if (TutorialManager.Instance.currentTutorial == "firstCustomerLeave")
            {
                DrawCard("lover",false);
                DrawCard("star",true);
                DrawCard("emperor",true);
                DrawCard("sun",true);
            }else if (TutorialManager.Instance.currentTutorial == "secondCustomerCome")
            {
                DrawCard("world",false);
                DrawCard("fool",false);
                DrawCard("sun",false);
            }
        }
        else
        {
            
            // Draw 4 cards from available deck
            for (int i = 0; i < 4; i++)
            {
                DrawOneCard();
            }

        }
        OnHandChanged?.Invoke(currentHand);
        GameSystem.Instance.OnAttributeChanged?.Invoke();

    }

    public void ClearHand()
    {
        GameSystem.Instance.gameState.usedCards.AddRange(currentHand);
        currentHand.Clear();
        
        // Clear all temporary effects
        ClearTemporaryEffects();
        
        OnHandChanged?.Invoke(currentHand);
    }
    
    /// <summary>
    /// 清除所有临时效果，包括卡牌计算的temp效果和符文点亮状态
    /// </summary>
    public void ClearTemporaryEffects()
    {
        // 重置临时值
        tempSanityChange = 0;
        tempMoneyChange = 0;
        tempRerollChange = 0;
        
        // 清除符文点亮状态
        ClearRuneActivationEffects();
    }
    
    /// <summary>
    /// 清除符文点亮状态和动画效果
    /// </summary>
    private void ClearRuneActivationEffects()
    {
        // 通知UI清除符文点亮状态
        var simpleGameUI = FindObjectOfType<SimpleGameUI>();
        if (simpleGameUI != null)
        {
            simpleGameUI.ClearRuneActivationStates();
        }
    }

    Card DrawCard(string cardName,bool isUpright)
    {
        if (GameSystem.Instance.gameState.availableCards.Count == 0)
        {
            Debug.Log("Deck is empty. Refilling...");
            RefillCardDeck();
        }
        var card = GameSystem.Instance.gameState.availableCards.Find(card => card.info.identifier == cardName);
        card.isUpright = isUpright;
        
        GameSystem.Instance.gameState.availableCards.Remove(card);

        currentHand.Add(card);
        return card;
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
        GameSystem.Instance.gameState.usedCards.Clear();
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
            if (currentHand[cardIndex].isFixed)
            {
              
            var prev = Math.Max(0, cardIndex - 1);
            var next =  Math.Min(currentHand.Count - 1, cardIndex + 1);
            if (currentHand[cardIndex].GetEffects().Contains("setAdjacentCardUpWhenLock"))
            {
                currentHand[prev].isUpright = true;
                currentHand[next].isUpright = true;
                OnHandChanged?.Invoke(currentHand);
                GameSystem.Instance.OnAttributeChanged?.Invoke();
            }
            if (currentHand[cardIndex].GetEffects().Contains("setAdjacentCardDownWhenLock"))
            {
                currentHand[prev].isUpright = false;
                currentHand[next].isUpright = false;
                OnHandChanged?.Invoke(currentHand);
                GameSystem.Instance.OnAttributeChanged?.Invoke();
            }
            
            }
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
        
        // Initialize money and reroll values
        result.initialMoney = GameSystem.Instance.gameState.money;
        result.initialRerolls = CardSystem.Instance.redrawTime;
        tempMoneyChange = 0; // Reset temp money change
        tempRerollChange = 0; // Reset temp reroll change
        
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

        result.finalAttributes = new Dictionary<string, int>
        {
            {"wisdom", tempCustomer.wisdom},
            {"emotion", tempCustomer.emotion},
            {"power", tempCustomer.power}
        };
        result.finalSanity = result.initialSanity + tempSanityChange;
        for (int o = 0; o < 3; o++)
        {
            
            for (int i = 0; i < currentHand.Count; i++)
            {
                Card card = currentHand[i];
                var effects = card.GetEffects();
            
                ApplyWhenEffects(effects, tempCustomer, allEffects, currentHand, updateState, result,o);
                
                result.finalAttributes = new Dictionary<string, int>
                {
                    {"wisdom", tempCustomer.wisdom},
                    {"emotion", tempCustomer.emotion},
                    {"power", tempCustomer.power}
                };
                
                result.finalSanity = result.initialSanity + tempSanityChange;
            }
        }

        ApplyRuneEffect(currentHand, updateState, result,customer);
        
        result.finalAttributes = new Dictionary<string, int>
        {
            {"wisdom", tempCustomer.wisdom},
            {"emotion", tempCustomer.emotion},
            {"power", tempCustomer.power}
        };
        
        // Set sanity results
        result.finalSanity = result.initialSanity + tempSanityChange;
        result.tempSanityChange = tempSanityChange;
        
        // Set money and reroll results
        result.finalMoney = result.initialMoney + tempMoneyChange;
        result.tempMoneyChange = tempMoneyChange; 
        result.finalRerolls = result.initialRerolls + tempRerollChange;
        result.tempRerollChange = tempRerollChange;
        
        // Check if customer requirements are satisfied
        Dictionary<string, int> attributeChanges = new Dictionary<string, int>();
        foreach (var attr in result.finalAttributes.Keys)
        {
            attributeChanges[attr] = result.diffAttribute(attr);
        }
        
        result.isSatisfied = customer.AreRequirementsSatisfied(attributeChanges);
        
        
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
            result.moneyEarned = totalImprovement + 2;
            tempMoneyChange +=  result.moneyEarned;
            result.tempMoneyChange = tempMoneyChange;
        }
        
        
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
            
            // Apply money changes to game state
            if (tempMoneyChange != 0)
            {
                GameSystem.Instance.AddMoney(tempMoneyChange);
            }
            
            // Apply reroll changes to card system
            if (tempRerollChange != 0)
            {
                AddRedrawTime(tempRerollChange);
            }
        }
        
        
        return result;
    }
    
    // private void ApplySpecialEffects(List<CardEffect> allEffects, Customer customer)
    // {
    //     // Check for Moon card effects
    //     bool hasUprightMoon = allEffects.Any(e => e.cardId == "moon" && e.isUpright && e.effect == "allNegHalf");
    //     bool hasReversedMoon = allEffects.Any(e => e.cardId == "moon" && !e.isUpright && e.effect == "allPosHalf");
    //     
    //     // Check for Sun + Moon synergy rune
    //     bool hasSunMoonRune = GameSystem.Instance.HasRune("twin_lights");
    //     bool hasSun = allEffects.Any(e => e.cardId == "sun");
    //     bool hasMoon = allEffects.Any(e => e.cardId == "moon");
    //     
    //     if (hasSunMoonRune && hasSun && hasMoon)
    //     {
    //         // Remove all negative effects
    //         for (int i = allEffects.Count - 1; i >= 0; i--)
    //         {
    //             if (IsNegativeEffect(allEffects[i].effect))
    //             {
    //                 allEffects.RemoveAt(i);
    //             }
    //         }
    //     }
    //     else
    //     {
    //         // Apply Moon effects
    //         if (hasUprightMoon)
    //         {
    //             // Halve negative effects
    //             foreach (var effect in allEffects)
    //             {
    //                 if (IsNegativeEffect(effect.effect))
    //                 {
    //                     effect.isHalved = true;
    //                 }
    //             }
    //         }
    //         
    //         if (hasReversedMoon)
    //         {
    //             // Halve positive effects
    //             foreach (var effect in allEffects)
    //             {
    //                 if (IsPositiveEffect(effect.effect))
    //                 {
    //                     effect.isHalved = true;
    //                 }
    //             }
    //         }
    //     }
    // }
    
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


    void ApplyRuneEffect(List<Card> currentHand, bool updateState, DivinationResult result,Customer customer)
    {
        if (currentHand.Count == 0)
        {
            return;
        }
        var effects = RuneManager.Instance.runeEffects;
        var isAllUpCard = true;
        var isAllDownCard = true;
        for (int j = 0; j < currentHand.Count; j++)
        {
            if (!currentHand[j].isUpright)
            {
                isAllUpCard = false;
            }
            else
            {
                isAllDownCard = false;
            }
        }

        foreach (var effect in effects.Keys)
        {
            switch (effect)
            {
                case "when|allUpCard|refreshCount":
                    if (isAllUpCard)
                    {
                        // Record activated rune
                        if (!result.activatedRunes.Contains("when|allUpCard|refreshCount"))
                        {
                            result.activatedRunes.Add("when|allUpCard|refreshCount");
                        }
                        
                        // Always use temp values, don't apply directly to game state here
                        tempRerollChange += effects[effect];
                    }

                    break;
                case "when|allDownCard|addGold":
                    if (isAllDownCard)
                    {
                        // Record activated rune
                        if (!result.activatedRunes.Contains("when|allDownCard|addGold"))
                        {
                            result.activatedRunes.Add(effect);
                        }
                        
                        // Always use temp values, don't apply directly to game state here
                        tempMoneyChange += effects[effect];
                    }

                    break;
                case "moreEmotionToSanity":
                {
                    
                    var required = customer.RequirementOnAttribute("emotion");
                    if (required > 0 && result.diffAttribute("emotion") > required)
                    {

                        tempSanityChange += result.diffAttribute("emotion") - required;
                        result.activatedRunes.Add(effect);
                    }
                }

                    break;
                case "morePowerToMoney":
                {
                    var required = customer.RequirementOnAttribute("power");
                    if (required > 0 && result.diffAttribute("power") > required)
                    {

                        tempMoneyChange += result.diffAttribute("power") - required;
                        result.activatedRunes.Add(effect);
                    }
                }
                    break;
                case "moreWisdomToRedraw":
                {
                    var required = customer.RequirementOnAttribute("wisdom");
                    if (required > 0 && result.diffAttribute("wisdom") > required)
                    {

                        tempRerollChange += result.diffAttribute("wisdom") - required;
                        result.activatedRunes.Add(effect);
                    }
                }

                    break;
            }
        }
    }
    private void ApplyWhenEffects(List<string> effects, Customer customer, List<string> allEffects, List<Card> currentHand, bool updateState,DivinationResult result,int order)
    {
        int upCount = 0;
        for (int j = 0; j < currentHand.Count; j++)
        {
            if (currentHand[j].isUpright)
            {
                upCount++;
            }
        }
        
        for (int i = 0; i < effects.Count; i++)
        {
            switch (effects[i])
            {
                case "when":
                {
                    i++;
                    if (order > 1)
                    {
                        return;
                    }
                    if ((effects[i] == "total" && order == 0) || (effects[i] != "total" && order == 1))
                    {
                        return;
                    }
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
                            i++;
                            var compareOriginValue = 0;
                            switch (effects[i])
                            {
                                case "power":
                                    compareOriginValue = result.diffAttribute("power");
                                    break;
                                case "anyA":
                                    
                                    var checkValueT = int.Parse(effects[i+1]);
                                    if (checkValueT > 0)
                                    {
                                        
                                        compareOriginValue =  Math.Max(Math.Max(result.diffAttribute("power"), result.diffAttribute("emotion")), result.diffAttribute("wisdom"));
                                    }
                                    else
                                    {
                                        compareOriginValue =  Math.Min(Math.Min(result.diffAttribute("power"), result.diffAttribute("emotion")), result.diffAttribute("wisdom"));
                                    }
                                    break;
                                case "sanity":
                                    compareOriginValue = result.diffSanity();
                                    break;
                            }

                            i++;
                            var checkValue = int.Parse(effects[i]);
                            var satisfied = false;
                            if (checkValue > 0)
                            {
                                satisfied = compareOriginValue >= checkValue;
                                
                            }
                            else
                            {
                                 satisfied = compareOriginValue <= checkValue;
                            }

                            i++;
                            if (satisfied)
                            {
                                
                                ApplyAttributeEffects (effects.GetRange(i, effects.Count - i), customer, allEffects, updateState);
                            }
                            break;
                    }

                    break;
                }
                case "wisdomS":
                    if (order <= 1)
                    {
                        return;
                    }
                    i++;
                    var valueString = effects[i];
                    
                    if (valueString == "emotionValue")
                    {
                        adjustValue("wisdom", result.diffAttribute("emotion"), allEffects, customer);
                    }else if (valueString == "powerValue")
                    {
                        adjustValue("wisdom", result.diffAttribute("power"), allEffects, customer);
                    }else if (valueString == "downCardCount")
                    {
                        i++;
                        int value = int .Parse(effects[i]);
                        adjustValue("wisdom", (4-upCount)*value, allEffects, customer);
                    }
                    else if (valueString == "upCardCount")
                    {
                        i++;
                        int value = int .Parse(effects[i]);
                        adjustValue("wisdom", upCount*value, allEffects, customer);
                    }
                    break;
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

        if (key == "power" && allEffects.Contains("doublePower"))
        {
            value *= 2;
        }else if (key == "emotion" && allEffects.Contains("doubleEmotion"))
        {
            value *= 2;
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
    
    // Money and reroll values
    public int initialMoney;
    public int finalMoney;
    public int tempMoneyChange;
    public int initialRerolls;
    public int finalRerolls;
    public int tempRerollChange;
    
    // Activated runes tracking
    public List<string> activatedRunes = new List<string>();

    public int diffAttribute(string attribute)
    {
         return finalAttributes[attribute] - initialAttributes[attribute];
    }
    
    public int diffSanity()
    {
        return finalSanity - initialSanity;
    }
    
    public int diffMoney()
    {
        return finalMoney - initialMoney;
    }
    
    public int diffRerolls()
    {
        return finalRerolls - initialRerolls;
    }
    
    public bool isSatisfied;
    public int moneyEarned;
}