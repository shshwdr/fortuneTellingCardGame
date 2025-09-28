using System.Collections.Generic;

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
    public int wealth = 50;
    public int relationship = 50;
    public int sanity = 50;
    public int power = 50;
    
    public Customer(CustomerInfo customerInfo)
    {
        info = customerInfo;
    }
    
    public int GetAttribute(string attributeName)
    {
        switch (attributeName.ToLower())
        {
            case "wealth": return wealth;
            case "rela": return relationship;
            case "sanity": return sanity;
            case "power": return power;
            default: return 0;
        }
    }
    
    public void SetAttribute(string attributeName, int value)
    {
        value = UnityEngine.Mathf.Clamp(value, 0, 100);
        switch (attributeName.ToLower())
        {
            case "wealth": wealth = value; break;
            case "rela": relationship = value; break;
            case "sanity": sanity = value; break;
            case "power": power = value; break;
        }
    }
    
    public void ModifyAttribute(string attributeName, int change)
    {
        int current = GetAttribute(attributeName);
        SetAttribute(attributeName, current + change);
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
    public List<string> availableCards = new List<string>();
    public List<string> usedCards = new List<string>();
    
    public List<Customer> todayCustomers = new List<Customer>();
}