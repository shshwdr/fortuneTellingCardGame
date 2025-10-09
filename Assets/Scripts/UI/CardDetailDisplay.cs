using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕中央显示卡牌详细信息的UI组件
/// </summary>
public class CardDetailDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject detailPanel;
    public TMP_Text cardNameText;
    public TMP_Text cardDescriptionText;
    public TMP_Text uprightEffectText;
    public TMP_Text reversedEffectText;
    public Image cardImage;
    public TMP_Text currentStatusText;
    public int level;
    private static CardDetailDisplay instance;
    public static CardDetailDisplay Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CardDetailDisplay>();
                if (instance == null)
                {
                    Debug.LogWarning("CardDetailDisplay not found in scene");
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
        HideDetail();
    }

    public void ShowRuneDetail(RuneInfo runeInfo)
    {
        cardNameText.text = runeInfo.name;
        cardDescriptionText.text = runeInfo.description;
        uprightEffectText.text = "";
        reversedEffectText.text = "";
        detailPanel.SetActive(true);
    }
    /// <summary>
    /// 显示卡牌详细信息
    /// </summary>
    /// <param name="cardInfo">卡牌信息</param>
    /// <param name="isCurrentlyUpright">当前是否为正位</param>
    public void ShowCardDetail(CardInfo cardInfo,int level, bool isCurrentlyUpright)
    {
        if (cardInfo == null || detailPanel == null) return;
        
        // 设置卡牌基本信息
        if (cardNameText != null)
            cardNameText.text = cardInfo.name;
            
        if (cardDescriptionText != null)
            cardDescriptionText.text = cardInfo.description;
            
        // 设置卡牌图片
        if (cardImage != null)
        {
            var sprite = Resources.Load<Sprite>("tarot/" + cardInfo.identifier);
            if (sprite != null)
                cardImage.sprite = sprite;
        }
        
        // 显示正位效果
        if (uprightEffectText != null)
        {
            string uprightText = "<color=green><b>Upright Effects:</b></color>\n";
            uprightText += FormatEffectList(cardInfo.UpEffect(level));
            uprightEffectText.text = uprightText;
        }
        
        // 显示逆位效果
        if (reversedEffectText != null)
        {
            string reversedText = "<color=red><b>Reversed Effects:</b></color>\n";
            reversedText += FormatEffectList(cardInfo.DownEffect(level));
            reversedEffectText.text = reversedText;
        }
        
        // 显示当前状态
        if (currentStatusText != null)
        {
            string statusColor = isCurrentlyUpright ? "green" : "red";
            string statusText = isCurrentlyUpright ? "Upright" : "Reversed";
            currentStatusText.text = $"<color={statusColor}><b>Current: {statusText}</b></color>";
        }
        
        detailPanel.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏卡牌详细信息
    /// </summary>
    public void HideDetail()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }
    
    /// <summary>
    /// 格式化效果列表为可读文本
    /// </summary>
    private string FormatEffectList(List<string> effects)
    {
        if (effects == null || effects.Count == 0)
            return "No effects";
            
        string result = "";
        int i = 0;
        
        while (i < effects.Count)
        {
            switch (effects[i])
            {
                case "when":
                    i++;
                    result += "When: ";
                    switch (effects[i])
                    {
                        case "allUpCard":
                            result += "All Cards Are Upright: ";
                            break;
                        case "allDownCard":
                            result += "All Cards Are Reversed: ";
                            break;
                    }
                    i++;
                    result += FormatAttributeEffects(effects.GetRange(i, effects.Count - i));
                    return result;
                    
                case "allNegHalf":
                    result += "• Halve all negative effects\n";
                    break;
                    
                case "allPosAdd":
                    result += "• All positive effects +1 extra\n";
                    break;
                    
                case "allPosHalf":
                    result += "• Halve all positive effects\n";
                    break;
                    
                case "wisdom":
                case "power":
                case "emotion":
                case "sanity":
                case "allA":
                    result += FormatAttributeEffects(effects.GetRange(i, effects.Count - i));
                    return result;
                    
                default:
                    result += $"• {effects[i]}\n";
                    break;
            }
            i++;
        }
        
        return result;
    }
    
    /// <summary>
    /// 格式化属性效果
    /// </summary>
    private string FormatAttributeEffects(List<string> effects)
    {
        string result = "";
        int i = 0;
        
        while (i < effects.Count)
        {
            string attributeName = "";
            switch (effects[i])
            {
                case "wisdom":
                    attributeName = "Wisdom";
                    break;
                case "power":
                    attributeName = "Power";
                    break;
                case "emotion":
                    attributeName = "Emotion";
                    break;
                case "sanity":
                    attributeName = "Sanity";
                    break;
                case "allA":
                    attributeName = "All Attributes";
                    break;
                default:
                    i++;
                    continue;
            }
            
            i++;
            if (i < effects.Count)
            {
                string value = effects[i];
                string sign = value.StartsWith("-") ? "" : "+";
                result += $"• {attributeName} {sign}{value}\n";
            }
            i++;
        }
        
        return result;
    }
}