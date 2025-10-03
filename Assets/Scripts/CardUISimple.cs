using System.Collections.Generic;
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
    public GameObject fixedOb;
    
    [Header("Card Data")]
    public string cardId;
    public bool isUpright = false;
    public bool isFixed = false;
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
        isFixed = card.isFixed;
        
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
fixedOb.SetActive(isFixed);
    }
    
    private void UpdateEffectText()
    {
        if (cardEffectText == null || cardInfo == null) return;
        
        var effects = isUpright ? cardInfo.upEffect : cardInfo.downEffect;
        string effectText = isUpright ? "Upright:\n" : "Reversed:\n";
        effectText += FormatEffect(effects);
        // foreach (string effect in effects)
        // {
        //     effectText += FormatEffect(effects) + "\n";
        // }
        
        cardEffectText.text = effectText;
    }
    
    private string FormatEffect(List<string> effects)
    {
        int i = 0;
        string effectText = "";
        for (; i < effects.Count; i++)
        {
            switch (effects[i])
            {
                case "when":
                {
                    i++;
                    effectText += "When: ";
                    switch (effects[i])
                    {
                        case "allUpCard":
                        {
                            effectText += "All Cards Are Upright: ";
                            break;
                        }
                        case "allDownCard":
                        {
                            effectText += "All Cards Are Reversed: ";
                            break;
                        }
                    }

                    i++;
                    effectText += attributeEffectText(effects.GetRange(i, effects.Count - i));
                }
                    return effectText;
                case "allNegHalf": return "Halve all negative effects";
                case "allPosAdd": return "All Postive effect add extra 1";
                case "allPosHalf": return "Halve all positive effects";
                default:
                    effectText += attributeEffectText(effects);
                    return effectText;
            }
            
        }
        return effectText;
    }

    string attributeEffectText(List<string> effects)
    {
        int i = 0;
        string effectText = "";
        for (; i < effects.Count; i++)
        {
            switch (effects[i])
            {
                case "wisdom":
                    effectText += "Wisdom ";
                    i++;
                    effectText += (effects[i].StartsWith("-") ? "" : "+") + effects[i]+ "\n";
                    
                    break;
                    case "power":
                        effectText += "Power ";
                        i++;
                        effectText += (effects[i].StartsWith("-") ? "" : "+") + effects[i]+ "\n";
                    
                        break;
                        case "emotion":
                            effectText += "Emotion ";
                            i++;
                            effectText += (effects[i].StartsWith("-") ? "" : "+") + effects[i]+ "\n";
                    
                            break;
                case "sanity":
                    effectText += "Sanity ";
                    i++;
                    effectText += (effects[i].StartsWith("-") ? "" : "+") + effects[i]+ "\n";
                    
                    break;
                case "allA":
                    effectText += "All Attributes ";
                    i++;
                    effectText += (effects[i].StartsWith("-") ? "" : "+") + effects[i] + "\n";
                    
                    break;
                                break;
            }
        }
        return effectText;
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