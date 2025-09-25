using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameSystem : Singleton<GameSystem>
{
    public GameState gameState = new GameState();
    
    public System.Action<Customer> OnCustomerChanged;
    public System.Action<int> OnMoneyChanged;
    public System.Action<int> OnDayChanged;
    public System.Action OnGameStateChanged;
    
    public void StartNewGame()
    {
        gameState = new GameState();
        InitializeAvailableCards();
        StartNewDay();
    }
    
    private void InitializeAvailableCards()
    {
        gameState.availableCards.Clear();
        foreach (var cardInfo in CSVLoader.Instance.cardInfoMap.Values)
        {
            gameState.availableCards.Add(cardInfo.identifier);
        }
    }
    
    public void StartNewDay()
    {
        gameState.currentCustomerIndex = 0;
        gameState.todayCustomers.Clear();
        
        // Reset card deck for new day
        InitializeAvailableCards();
        
        // Load customers for today
        LoadTodayCustomers();
        
        OnDayChanged?.Invoke(gameState.currentDay);
        
        if (gameState.todayCustomers.Count > 0)
        {
            OnCustomerChanged?.Invoke(gameState.todayCustomers[0]);
        }
        
        OnGameStateChanged?.Invoke();
    }
    
    private void LoadTodayCustomers()
    {
        string dayKey = gameState.currentDay.ToString();
        if (CSVLoader.Instance.dayInfoMap.ContainsKey(dayKey))
        {
            var dayInfo = CSVLoader.Instance.dayInfoMap[dayKey];
            foreach (string customerKey in dayInfo.customers)
            {
                if (customerKey == "r")
                {
                    // Random customer
                    var randomCustomer = GetRandomCustomer();
                    if (randomCustomer != null)
                    {
                        gameState.todayCustomers.Add(new Customer(randomCustomer));
                    }
                }
                else if (CSVLoader.Instance.customerInfoMap.ContainsKey(customerKey))
                {
                    var customerInfo = CSVLoader.Instance.customerInfoMap[customerKey];
                    gameState.todayCustomers.Add(new Customer(customerInfo));
                }
            }
        }
        
        // Ensure no duplicate customers on the same day
        var uniqueCustomers = new List<Customer>();
        var usedCustomerIds = new HashSet<string>();
        
        foreach (var customer in gameState.todayCustomers)
        {
            if (!usedCustomerIds.Contains(customer.info.identifier))
            {
                uniqueCustomers.Add(customer);
                usedCustomerIds.Add(customer.info.identifier);
            }
        }
        
        gameState.todayCustomers = uniqueCustomers;
    }
    
    private CustomerInfo GetRandomCustomer()
    {
        var allCustomers = CSVLoader.Instance.customerInfoMap.Values.ToList();
        if (allCustomers.Count > 0)
        {
            return allCustomers[Random.Range(0, allCustomers.Count)];
        }
        return null;
    }
    
    public Customer GetCurrentCustomer()
    {
        if (gameState.currentCustomerIndex < gameState.todayCustomers.Count)
        {
            return gameState.todayCustomers[gameState.currentCustomerIndex];
        }
        return null;
    }
    
    public void NextCustomer()
    {
        gameState.currentCustomerIndex++;
        
        if (gameState.currentCustomerIndex < gameState.todayCustomers.Count)
        {
            OnCustomerChanged?.Invoke(gameState.todayCustomers[gameState.currentCustomerIndex]);
        }
        else
        {
            // Day ended
            EndDay();
        }
        
        OnGameStateChanged?.Invoke();
    }
    
    private void EndDay()
    {
        gameState.currentDay++;
        // Here you could show end of day summary or shop
        Debug.Log("Day " + (gameState.currentDay - 1) + " ended. Starting day " + gameState.currentDay);
        StartNewDay();
    }
    
    public void AddMoney(int amount)
    {
        gameState.money += amount;
        OnMoneyChanged?.Invoke(gameState.money);
        OnGameStateChanged?.Invoke();
    }
    
    public bool SpendMoney(int amount)
    {
        if (gameState.money >= amount)
        {
            gameState.money -= amount;
            OnMoneyChanged?.Invoke(gameState.money);
            OnGameStateChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool HasUpgrade(string upgradeId)
    {
        return gameState.ownedUpgrades.Contains(upgradeId);
    }
    
    public bool HasRune(string runeId)
    {
        return gameState.ownedRunes.Contains(runeId);
    }
    
    public string GetCardSigil(string cardId)
    {
        return gameState.cardSigils.ContainsKey(cardId) ? gameState.cardSigils[cardId] : null;
    }
}