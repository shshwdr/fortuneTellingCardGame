using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simplified CardUI without DOTween dependency for testing
/// </summary>
public class CardUISimple : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text cardNameText;
    public TMP_Text cardDescriptionText;
    public TMP_Text cardEffectText;
    public Button cardButton;
    public GameObject cardBack;
    public GameObject cardFront;
    
    [Header("Card Data")]
    public string cardId;
    public bool isUpright = false;
    public int cardIndex;
    
    private CardInfo cardInfo;
    public Transform oppositeTrans;
    public System.Action<int> OnCardClicked;

    public Color uprightColor;
    public Color reversedColor;
    
    
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
    
    public void SetCardData(Card card, int index)
    {
        cardId = card.info.identifier;
        cardIndex = index;
        cardInfo = card.info;
        isUpright = card.isUpright;
        UpdateCardDisplay();
    }
    
    private void UpdateCardDisplay()
    {
        if (cardInfo == null) return;
        
        if (cardNameText != null)
            cardNameText.text = cardInfo.name;
            
        if (cardDescriptionText != null)
            cardDescriptionText.text = cardInfo.description;
            
        UpdateUpright();
        //UpdateEffectText();

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
        UpdateUpright();
        
    }

    public void UpdateUpright()
    {
        
        // Simple visual update without animation
        if (cardBack != null && cardFront != null)
        {
            cardFront.SetActive(isUpright);
            cardBack.SetActive(!isUpright);
        }
        else
        {
            // If no separate front/back objects, rotate the whole card
            oppositeTrans.localRotation = isUpright ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
        }
        
        cardButton.image.color = isUpright ? uprightColor : reversedColor;
        
        UpdateEffectText();
    }
    
    
    public void SetUpright(bool upright)
    {
        isUpright = upright;
        
        UpdateUpright();
    }
}