using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simplified CardUI without DOTween dependency for testing
/// </summary>
public class CardUISimple : MonoBehaviour
{
    [Header("UI References")]
    public Text cardNameText;
    public Text cardDescriptionText;
    public Text cardEffectText;
    public Button cardButton;
    public GameObject cardBack;
    public GameObject cardFront;
    
    [Header("Card Data")]
    public string cardId;
    public bool isUpright = true;
    public int cardIndex;
    
    private CardInfo cardInfo;
    
    public System.Action<int> OnCardClicked;
    
    void Start()
    {
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClick);
        }
    }
    
    public void SetCardData(string id, int index)
    {
        cardId = id;
        cardIndex = index;
        
        if (CSVLoader.Instance.cardInfoMap.ContainsKey(cardId))
        {
            cardInfo = CSVLoader.Instance.cardInfoMap[cardId];
            UpdateCardDisplay();
        }
    }
    
    private void UpdateCardDisplay()
    {
        if (cardInfo == null) return;
        
        if (cardNameText != null)
            cardNameText.text = cardInfo.name;
            
        if (cardDescriptionText != null)
            cardDescriptionText.text = cardInfo.description;
            
        UpdateEffectText();
    }
    
    private void UpdateEffectText()
    {
        if (cardEffectText == null || cardInfo == null) return;
        
        var effects = isUpright ? cardInfo.upEffect : cardInfo.downEffect;
        string effectText = isUpright ? "Upright:\n" : "Reversed:\n";
        
        foreach (string effect in effects)
        {
            effectText += FormatEffect(effect) + "\n";
        }
        
        cardEffectText.text = effectText;
    }
    
    private string FormatEffect(string effect)
    {
        if (effect.Contains('|'))
        {
            string[] parts = effect.Split('|');
            if (parts.Length == 2)
            {
                string attribute = parts[0];
                string value = parts[1];
                return $"{attribute} {(value.StartsWith("-") ? "" : "+")}{value}";
            }
        }
        
        // Special effects
        switch (effect)
        {
            case "allNegHalf": return "Halve all negative effects";
            case "allPosHalf": return "Halve all positive effects";
            default: return effect;
        }
    }
    
    public void OnCardClick()
    {
        FlipCard();
        OnCardClicked?.Invoke(cardIndex);
    }
    
    public void FlipCard()
    {
        isUpright = !isUpright;
        
        // Simple visual update without animation
        if (cardBack != null && cardFront != null)
        {
            cardFront.SetActive(isUpright);
            cardBack.SetActive(!isUpright);
        }
        else
        {
            // If no separate front/back objects, rotate the whole card
            transform.localRotation = isUpright ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
        }
        
        UpdateEffectText();
    }
    
    public void SetUpright(bool upright)
    {
        isUpright = upright;
        
        if (cardBack != null && cardFront != null)
        {
            cardFront.SetActive(isUpright);
            cardBack.SetActive(!isUpright);
        }
        else
        {
            transform.localRotation = isUpright ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
        }
        
        UpdateEffectText();
    }
}