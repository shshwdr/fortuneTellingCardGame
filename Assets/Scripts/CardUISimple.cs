using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Simplified CardUI without DOTween dependency for testing
/// </summary>
public class CardUISimple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public TMP_Text cardNameText;
    public TMP_Text cardDescriptionText;
    public TMP_Text cardEffectText;
    public TMP_Text cardLevelText;
    public Button cardButton;
    public Image cardBack;
    //public GameObject cardFront;
    public GameObject fixedOb;
    
    [Header("Card Data")]
    public string cardId;
    public bool isUpright = false;
    public bool isFixed = false;
    public int cardIndex;
    public int cardLevel = 1;
    
    
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
        cardLevel = 1; // Default level for new cards
        
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
        cardLevel = card.level;
        cardBack.sprite = Resources.Load<Sprite>("tarot/" + card.info.identifier); 
        UpdateCardDisplay();
    }
    
    public void SetCardDataWithLevel(string id, int level, int index)
    {
        cardId = id;
        cardIndex = index;
        cardLevel = level;
        
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
            
        UpdateUpright();
        UpdateLevelDisplay();
        //UpdateEffectText();
        fixedOb.SetActive(isFixed);
    }
    
    private void UpdateLevelDisplay()
    {
        if (cardLevelText != null)
        {
            if (cardLevel > 1)
            {
                cardLevelText.text = $"Lv.{cardLevel}";
                cardLevelText.gameObject.SetActive(true);
            }
            else
            {
                cardLevelText.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateEffectText()
    {
        if (cardEffectText == null || cardInfo == null) return;
        
        var effects = isUpright ? cardInfo.UpEffect(cardLevel) : cardInfo.DownEffect(cardLevel);
        string effectText = "";//isUpright ? "Upright:\n" : "Reversed:\n";
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
        // if (cardBack != null && cardFront != null)
        // {
        //     cardFront.SetActive(isUpright);
        //     cardBack.SetActive(!isUpright);
        // }
        // else
        {
            // If no separate front/back objects, rotate the whole card
            oppositeTrans.localRotation = isUpright ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
        }
        
        cardButton.image.color = isUpright ? uprightColor : reversedColor;
        
        UpdateEffectText();
        UpdateLevelDisplay();
    }
    
    
    public void SetUpright(bool upright)
    {
        isUpright = upright;
        
        UpdateUpright();
    }
    
    /// <summary>
    /// 鼠标进入时显示卡牌详细信息
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardInfo != null && CardDetailDisplay.Instance != null)
        {
            CardDetailDisplay.Instance.ShowCardDetail(cardInfo,cardLevel, isUpright);
        }
    }
    
    /// <summary>
    /// 鼠标离开时隐藏卡牌详细信息
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardDetailDisplay.Instance != null)
        {
            CardDetailDisplay.Instance.HideDetail();
        }
    }
}