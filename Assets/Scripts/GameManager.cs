using System;
using System.Collections;
using System.Collections.Generic;
using Naninovel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public bool autoStartGame = true;
    public bool useMinimalTest = true; // Use minimal test instead of full UI
    
    void Awake()
    {
        CSVLoader.Instance.Init();
        
        //DialogueManager.Instance.Init();
    }

    private void Start()
    {
        if (autoStartGame)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        //GameSystem.Instance.StartNewGame();
        
        
    }
    
    void Update()
    {
        // Debug controls
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameSystem.Instance.NextCustomer();
            CardSystem.Instance.DrawCardsForCustomer();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
           // StartGame();
        }
    }

    public void RestartGame()
    {
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    void OnGUI()
    {
        // Debug info
        // var customer = GameSystem.Instance.GetCurrentCustomer();
        // if (customer != null)
        // {
        //     GUILayout.Label($"Day: {GameSystem.Instance.gameState.currentDay}");
        //     GUILayout.Label($"Money: {GameSystem.Instance.gameState.money}");
        //     GUILayout.Label($"Customer: {customer.info.name} (wants {customer.info.target})");
        //     GUILayout.Label($"Attributes: W:{customer.wealth} R:{customer.relationship} S:{customer.sanity} P:{customer.power}");
        //     
        //     if (GUILayout.Button("Perform Divination"))
        //     {
        //         var result = CardSystem.Instance.PerformDivination(customer);
        //         Debug.Log($"Divination result: Satisfied={result.isSatisfied}, Money={result.moneyEarned}");
        //         
        //         if (result.isSatisfied)
        //         {
        //             GameSystem.Instance.AddMoney(result.moneyEarned);
        //         }
        //         
        //         GameSystem.Instance.NextCustomer();
        //         CardSystem.Instance.DrawCardsForCustomer();
        //     }
        //     
        //     if (GUILayout.Button("Skip Customer"))
        //     {
        //         GameSystem.Instance.NextCustomer();
        //         CardSystem.Instance.DrawCardsForCustomer();
        //     }
        // }
    }
}
