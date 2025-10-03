using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Card
{
    public CardInfo info;
    public bool isUpright;
    public bool isFixed = false;
    public List<string> buffs = new List<string>(); // Future buff system
    
    public Card(CardInfo cardInfo, bool upright = true)
    {
        info = cardInfo;
        isUpright = upright;
    }
    
    public List<string> GetEffects()
    {
        return isUpright ? info.upEffect : info.downEffect;
    }
    
    public void FlipCard()
    {
        isUpright = !isUpright;
    }
    
    public Card Clone()
    {
        var clone = new Card(info, isUpright);
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
public class CustomerInfo
{
    public string identifier;
    public string name;
    public string description;
    public string target; // wealth, rela, sanity, power
}

[System.Serializable]
public class Customer
{
    public CustomerInfo info;
    public int power = 50;
   public int emotion = 50;
    public int wisdom = 50;
    public int sanity = 50;
    public int talkedTime = 0;
    public int mainAttribute=> GetAttribute(info.target);

    public int lastStory;
    public string identifier => info.identifier;
    public Customer(CustomerInfo customerInfo)
    {
        info = customerInfo;
        lastStory = -1;
    }
    
    public int GetAttribute(string attributeName)
    {
        switch (attributeName.ToLower())
        {
            case "wisdom": return wisdom;
            case "emotion": return emotion;
            case "sanity": return sanity;
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
            case "sanity": sanity = value; break;
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
}

[System.Serializable]
public class DayInfo
{
    public string identifier;
    public int day;
    public List<string> customers;
}

[System.Serializable]
public class RuneInfo
{
    public string identifier;
    public string name;
    public string description;
    public int cost;
    public string effect;
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
public class GameState
{
    public int currentDay = 1;
    public int money = 0;
    public int currentCustomerIndex = 0;
    public List<string> ownedRunes = new List<string>();
    public Dictionary<string, string> cardSigils = new Dictionary<string, string>(); // cardId -> sigilId
    public List<string> ownedUpgrades = new List<string>();
    public List<Card> availableCards = new List<Card>();
    public List<Card> usedCards = new List<Card>();
    
    public List<Customer> todayCustomers = new List<Customer>();
    
    // Persistent customer storage
    public Dictionary<string, Customer> persistentCustomers = new Dictionary<string, Customer>();
}