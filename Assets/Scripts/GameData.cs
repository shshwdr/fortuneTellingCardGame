using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Card
{
    public CardInfo info;
    public bool isUpright;
    public bool isFixed = false;
    public List<string> buffs = new List<string>(); // Future buff system
    public int level = 1;
    public Card(CardInfo cardInfo, int l = 1, bool upright = true)
    {
        info = cardInfo;
        isUpright = upright;
        level = l;
    }
    
    public List<string> GetEffects()
    {
        var infoEffect = isUpright ? info.UpEffect(level) : info.DownEffect(level);
        // for (int i = 0; i < infoEffect.Count; i++)
        // {
        //     var value = infoEffect[i];
        //     int v;
        //     if(int.TryParse( value, out v))
        //     {
        //         int finalv=v;
        //         if (infoEffect[0] == "when")
        //         {
        //             if (v > 0)
        //             {
        //                 var basev = v - 1;
        //                 finalv = basev + level;
        //             }else if (v < 0)
        //             {
        //                 var basev = v + 1;
        //                 finalv = basev - level;
        //             }
        //         }
        //         else
        //         {
        //             finalv = v * level;
        //         }
        //        
        //         infoEffect[i] = finalv.ToString();
        //     }
        // }
        
        return infoEffect;
    }
    
    public void FlipCard()
    {
        isUpright = !isUpright;
    }
    
    public Card Clone()
    {
        var clone = new Card(info,level, isUpright);
        clone.buffs = new List<string>(buffs);
        return clone;
    }

    public void Fix()
    {
        isFixed = !isFixed;
    }

    public void Reset()
    {
        isFixed = false;
    }
}

[System.Serializable]
public class AttributeRequirement
{
    public string attributeName;
    public int requiredIncrease;
    
    public AttributeRequirement(string attribute, int requirement)
    {
        attributeName = attribute;
        requiredIncrease = requirement;
    }
}

[System.Serializable]
public class CustomerInfo
{
    public string identifier;
    public string name;
    public string description;
    public string target; // wealth, rela, sanity, power
    public bool isStart;
}

[System.Serializable]
public class Customer
{
    public CustomerInfo info;
    public int power = 10;
    public int emotion = 10;
    public int wisdom = 10;
    public int talkedTime = 0;
    public int mainAttribute => GetAttribute(info.target);
    public int lastStory;
    public string identifier => info.identifier;
    public bool isAvailable;
    
    // Customer requirements based on day and attributes
    public List<AttributeRequirement> requirements = new List<AttributeRequirement>();
    
    public Customer(CustomerInfo customerInfo)
    {
        info = customerInfo;
        lastStory = -1;
        isAvailable = customerInfo.isStart;
    }
    
    public int GetAttribute(string attributeName)
    {
        switch (attributeName.ToLower())
        {
            case "wisdom": return wisdom;
            case "emotion": return emotion;
            //case "sanity": return sanity;
            case "power": return power;
            default: Debug.LogError("Unknown attribute name: " + attributeName); return 0;
        }
    }
    
    
    
    public void SetAttribute(string attributeName, int value)
    {
        value = UnityEngine.Mathf.Clamp(value, 0, 100);
        switch (attributeName.ToLower())
        {
            case "wisdom": wisdom = value; break;
            case "emotion": emotion = value; break;
           // case "sanity": sanity = value; break;
            case "power": power = value; break;
        }
        //GameSystem.Instance.OnAttributeChanged?.Invoke();
    }
    
    public void ModifyAttribute(string attributeName, int change)
    {
        int current = GetAttribute(attributeName);
        SetAttribute(attributeName, current + change);
        //GameSystem.Instance.OnAttributeChanged?.Invoke();
    }
    
    /// <summary>
    /// Generate customer requirements based on current day
    /// Main attribute: max(1, day/2)
    /// Secondary attribute (from day 2+): random(0, day/2) for one random non-main attribute
    /// </summary>
    public void GenerateRequirements(int currentDay)
    {
        requirements.Clear();
        
        // Main attribute requirement: max(1, day/2)
        int mainRequirement = Mathf.Max(1, currentDay / 3);
        requirements.Add(new AttributeRequirement(info.target, mainRequirement));
        
        // Secondary attribute requirement (from day 2 onwards)
        if (currentDay > 3)
        {
            // Get all attributes except the main one
            List<string> otherAttributes = new List<string> { "wisdom", "emotion", "power" };
            otherAttributes.Remove(info.target);
            
            // Randomly select one secondary attribute
            string secondaryAttribute = otherAttributes[Random.Range(0, otherAttributes.Count)];
            int secondaryRequirement = Random.Range(0, mainRequirement); // +1 to make it inclusive
            
            if (secondaryRequirement > 0)
            {
                requirements.Add(new AttributeRequirement(secondaryAttribute, secondaryRequirement));
            }
        }
    }
    
    /// <summary>
    /// Check if all requirements are satisfied based on attribute changes
    /// </summary>
    public bool AreRequirementsSatisfied(Dictionary<string, int> attributeChanges)
    {
        foreach (var requirement in requirements)
        {
            if (!attributeChanges.ContainsKey(requirement.attributeName))
                return false;
                
            int actualIncrease = attributeChanges[requirement.attributeName];
            if (actualIncrease < requirement.requiredIncrease)
                return false;
        }
        
        return true;
    }

    public int RequirementOnAttribute(string key)
    {
        return requirements.Find(r => r.attributeName == key)?.requiredIncrease ?? 0;
    }
    
    /// <summary>
    /// Get a formatted string describing the customer's requirements
    /// </summary>
    public string GetRequirementsText()
    {
        if (requirements.Count == 0)
            return "No requirements";
            
        List<string> requirementTexts = new List<string>();
        foreach (var requirement in requirements)
        {
            requirementTexts.Add($"{requirement.attributeName} +{requirement.requiredIncrease}");
        }
        
        return string.Join(", ", requirementTexts);
    }
}

[System.Serializable]
public class DayInfo
{
    public string identifier;
    public int day;
    public List<string> customers;
    public int rent;
}

[System.Serializable]
public class RuneInfo
{
    public string identifier;
    public string iconString;
    public string name;
    public string description;
    public int cost;
    public int value;
    public string effect;
    public bool canBeDraw;
    public bool isStart;
    public Sprite icon => Resources.Load<Sprite>("runes/" + iconString);
}

[System.Serializable]
public class SigilInfo
{
    public string identifier;
    public string name;
    public string description;
    public int cost;
    public string effect;
    public string cardTarget;
}

[System.Serializable]
public class UpgradeInfo
{
    public string identifier;
    public string name;
    public string description;
    public int cost;
    public string effect;
}

[System.Serializable]
public class Rune
{
    public RuneInfo info;
    public int status;
    
    public Rune(RuneInfo runeInfo, int runeStatus = 0)
    {
        info = runeInfo;
        status = runeStatus;
    }
    
    public string identifier => info.identifier;
}

[System.Serializable]
public class GameState
{
    public int currentDay = 1;
    public int sanity = 10;
    public int money = 0;
    public int currentCustomerIndex = 0;
    public List<Rune> ownedRunes = new List<Rune>();
    public Dictionary<string, string> cardSigils = new Dictionary<string, string>(); // cardId -> sigilId
    public List<string> ownedUpgrades = new List<string>();
    public List<Card> availableCards = new List<Card>();
    public List<Card> allCards = new List<Card>();
    public List<Card> usedCards = new List<Card>();

    public bool canRedraw;
    public bool canTellForturn;
    public bool canSkip;

    public int satisfiedCustomerCount = 0;
    
    public List<Customer> todayCustomers = new List<Customer>();
    
    // Persistent customer storage
    public Dictionary<string, Customer> persistentCustomers = new Dictionary<string, Customer>();
}