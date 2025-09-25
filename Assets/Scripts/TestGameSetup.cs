using UnityEngine;
using UnityEngine.UI;

public class TestGameSetup : MonoBehaviour
{
    [Header("Test UI Elements")] public Text debugText;
    public Button startGameButton;
    public Button drawCardsButton;
    public Button testDivinationButton;

    void Start()
    {
        SetupTestButtons();
        UpdateDebugInfo();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestDivination();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CardSystem.Instance.FlipCard(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CardSystem.Instance.FlipCard(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CardSystem.Instance.FlipCard(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CardSystem.Instance.FlipCard(3);
        }

        UpdateDebugInfo();
    }

    private void SetupTestButtons()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(() =>
            {
                GameSystem.Instance.StartNewGame();
                CardSystem.Instance.DrawCardsForCustomer();
            });

        if (drawCardsButton != null)
            drawCardsButton.onClick.AddListener(() => { CardSystem.Instance.DrawCardsForCustomer(); });

        if (testDivinationButton != null)
            testDivinationButton.onClick.AddListener(TestDivination);
    }

    private void TestDivination()
    {
        var customer = GameSystem.Instance.GetCurrentCustomer();
        if (customer != null)
        {
            var result = CardSystem.Instance.PerformDivination(customer);

            Debug.Log($"Divination Result:");
            Debug.Log($"Customer: {customer.info.name} (wants {customer.info.target})");
            Debug.Log($"Satisfied: {result.isSatisfied}");
            Debug.Log($"Money Earned: {result.moneyEarned}");

            foreach (var attr in result.initialAttributes.Keys)
            {
                int initial = result.initialAttributes[attr];
                int final = result.finalAttributes[attr];
                Debug.Log($"{attr}: {initial} -> {final} ({final - initial})");
            }

            if (result.isSatisfied)
            {
                GameSystem.Instance.AddMoney(result.moneyEarned);
            }
        }
    }

    private void UpdateDebugInfo()
    {
        if (debugText == null) return;

        string info = "";

    }
}