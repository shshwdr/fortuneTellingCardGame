using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameSystem : Singleton<GameSystem>
{
    public GameState gameState = new GameState();
    
    public System.Action<Customer> OnCustomerChanged;
    public System.Action OnAttributeChanged;
    public System.Action<Customer> OnCustomerShow;
    public System.Action<int> OnMoneyChanged;
    public System.Action<int> OnSanityChanged;
    public System.Action<int> OnDayChanged;
    public System.Action OnGameStateChanged;
    
    public void StartNewGame()
    {
        //Debug.LogError("StartNewGame");
        gameState = new GameState();
        InitializeAvailableCards();
        InitializePersistentCustomers();
        StartNewDay();
        
        OnMoneyChanged?.Invoke(gameState.money);
        OnSanityChanged?.Invoke(gameState.sanity);
    }
    
    private void InitializePersistentCustomers()
    {
        gameState.persistentCustomers.Clear();
        foreach (var customerInfo in CSVLoader.Instance.customerInfoMap.Values)
        {
            gameState.persistentCustomers[customerInfo.identifier] = new Customer(customerInfo);
        }
    }
    
    private void InitializeAvailableCards()
    {
        gameState.availableCards.Clear();
        foreach (var cardInfo in CSVLoader.Instance.cardInfoMap.Values)
        {
            if (cardInfo.isStart)
            {
                gameState.availableCards.Add(new Card(cardInfo));
            }
        }
    }
    
    public void StartNewDay()
    {
        gameState.currentCustomerIndex = -1;
        gameState.todayCustomers.Clear();

        CardSystem.Instance.redrawTime = CardSystem.Instance.redrawTimePerDay;
        
        // Reset card deck for new day
        InitializeAvailableCards();
        
        // Load customers for today
        LoadTodayCustomers();
        
        OnDayChanged?.Invoke(gameState.currentDay);
        
        CardSystem.Instance.DrawCardsForCustomer();
        
        if (gameState.todayCustomers.Count > 0)
        {
            NextCustomer();
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
                    var randomCustomer = GetRandomPersistentCustomer();
                    if (randomCustomer != null)
                    {
                        gameState.todayCustomers.Add(randomCustomer);
                    }
                }
                else if (gameState.persistentCustomers.ContainsKey(customerKey))
                {
                    gameState.todayCustomers.Add(gameState.persistentCustomers[customerKey]);
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
        
        // Generate requirements for all customers based on current day
        foreach (var customer in gameState.todayCustomers)
        {
            customer.GenerateRequirements(gameState.currentDay);
        }
    }
    
    private Customer GetRandomPersistentCustomer()
    {
        var availableCustomers = gameState.persistentCustomers.Values.ToList();
        if (availableCustomers.Count > 0)
        {
            return availableCustomers[Random.Range(0, availableCustomers.Count)];
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

    IEnumerator customerShow()
    {
        OnCustomerShow?.Invoke(GetCurrentCustomer());
        yield return new WaitForSeconds(3);
        
        var character = GameSystem.Instance.GetCurrentCustomer();
        DialogueManager.Instance.StartDialogue(character.info.identifier+ "Request");
    }
    
    public void NextCustomer()
    {
        gameState.currentCustomerIndex++;
        
        if (gameState.currentCustomerIndex < gameState.todayCustomers.Count)
        {
            CardSystem.Instance.DrawCardsForCustomer();
            
            OnCustomerChanged?.Invoke(gameState.todayCustomers[gameState.currentCustomerIndex]);
            StartCoroutine(customerShow());
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

    public int GetSanity()
    {
        return gameState.sanity;
    }
    public void AddSanity(int amount)
    {
        gameState.sanity += amount;
        // if (gameState.sanity > 100)
        // {
        //     gameState.sanity = 100;
        // }
        OnSanityChanged?.Invoke(gameState.sanity);
        OnGameStateChanged?.Invoke();
    }
    
    public void SubtractSanity(int amount)
    { 
        gameState.sanity -= amount;
        if (gameState.sanity < 0)
        {
            gameState.sanity = 0;
            ToastManager.Instance.ShowToast("You've lose all your sanity...game over");
        }
        OnSanityChanged?.Invoke(gameState.sanity);
        OnGameStateChanged?.Invoke();
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