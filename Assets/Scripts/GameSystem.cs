using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameSystem : Singleton<GameSystem>
{
    public GameState gameState = new GameState();
    
    // Track daily statistics
    public int customersServedToday = 0;
    public int moneyEarnedToday = 0;
    
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
                var card = new Card(cardInfo);
                gameState.availableCards.Add(card);
                gameState.allCards.Add(card);
            }
        }
    }
    
    public void StartNewDay()
    {
        ShopMenu.OpenShop();
       // PopupDialog.Create("New Day", "test", "Continue to Next Day", () => StartNewDay());
        // Reset daily statistics
        customersServedToday = 0;
        moneyEarnedToday = 0;
        
        gameState.currentCustomerIndex = -1;
        gameState.todayCustomers.Clear();

        CardSystem.Instance.redrawTime = CardSystem.Instance.redrawTimePerDay;
        
        // Reset card deck for new day
        //InitializeAvailableCards();
        
        // Load customers for today
        LoadTodayCustomers();
        
        OnDayChanged?.Invoke(gameState.currentDay);
        
        //CardSystem.Instance.DrawCardsForCustomer();
        
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
        int previousDay = gameState.currentDay;
        gameState.currentDay++;
        
        // Get rent for the day that just ended
        string dayKey = previousDay.ToString();
        int rent = 0;
        if (CSVLoader.Instance.dayInfoMap.ContainsKey(dayKey))
        {
            rent = CSVLoader.Instance.dayInfoMap[dayKey].rent;
        }
        
        // Show day end summary
        ShowDayEndSummary(customersServedToday, moneyEarnedToday, rent);
    }
    
    private void ShowDayEndSummary(int customersServed, int moneyEarned, int rent)
    {
        string summary = $"Day {gameState.currentDay - 1} Summary:\n\n";
        summary += $"Customers served: {customersServed}\n";
        summary += $"Money earned: {moneyEarned}\n";
        summary += $"Rent due: {rent}";
        
        PopupDialog.Create(
            "Day End",
            summary,
            "Pay Rent",
            () => PayRentAndContinue(rent)
        );
    }
    
    private void PayRentAndContinue(int rent)
    {
        if (gameState.money >= rent)
        {
            // Can afford rent, pay it and go to shop
            gameState.money -= rent;
            OnMoneyChanged?.Invoke(gameState.money);
            ToastManager.Instance.ShowToast($"Paid {rent} rent");
            
            // Open shop
            ShopMenu.OpenShop();
        }
        else
        {
            // Can't afford rent, game over
            ShowGameOver($"You couldn't afford the rent of {rent}. You only had {gameState.money}.");
        }
    }
    
    public void ShowGameOver(string reason)
    {
        PopupDialog.Create(
            "Game Over",
            reason,
            "Restart Game",
            () => {
                var gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.RestartGame();
                }
            }
        );
    }
    
    public void AddMoney(int amount)
    {
        gameState.money += amount;
        moneyEarnedToday += amount; // Track daily earnings
        OnMoneyChanged?.Invoke(gameState.money);
        OnGameStateChanged?.Invoke();
    }
    
    public void CustomerServed()
    {
        customersServedToday++;
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
        return gameState.ownedRunes.Any(rune => rune.identifier == runeId);
    }
    
    public void AddRune(RuneInfo runeInfo, int status = 0)
    {
        if (!HasRune(runeInfo.identifier))
        {
            var rune = new Rune(runeInfo, status);
            gameState.ownedRunes.Add(rune);
            OnGameStateChanged?.Invoke();
        }
    }
    
    public Rune GetRune(string runeId)
    {
        return gameState.ownedRunes.FirstOrDefault(rune => rune.identifier == runeId);
    }
    
    public bool UpdateRuneStatus(string runeId, int newStatus)
    {
        var rune = GetRune(runeId);
        if (rune != null)
        {
            rune.status = newStatus;
            OnGameStateChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public int GetRuneStatus(string runeId)
    {
        var rune = GetRune(runeId);
        return rune?.status ?? -1; // Return -1 if rune not found
    }
    
    public string GetCardSigil(string cardId)
    {
        return gameState.cardSigils.ContainsKey(cardId) ? gameState.cardSigils[cardId] : null;
    }
}