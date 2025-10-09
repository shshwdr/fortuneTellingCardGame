using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class RuneCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string identifier;
    public TMP_Text text;
    public TMP_Text desc;
    public TMP_Text statusText; // Optional: to display rune status
    public GameObject effecting;
    private RuneInfo info;
    public void SetData(Rune rune)
    {
        this.info = rune.info;
        identifier = rune.identifier;
        text.text = rune.info.name;
        desc.text = rune.info.description;
        
        // Display status if statusText is available
        if (statusText != null)
        {
            statusText.text = $"Status: {rune.status}";
        }
    }
    
    public void SetData(RuneInfo info)
    {
        this.info = info;
        identifier = info.identifier;
        text.text = info.name;
        desc.text = info.description;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetIsEffect(bool isEffect)
    {
        effecting.SetActive(isEffect);
    }
    
    public void PlayActivationAnimation()
    {
        // 左右扭动动画
        transform.DOKill(); // 停止之前的动画
        transform.DOShakeRotation(0.6f, new Vector3(0, 0, 15f), 10, 45f).SetEase(Ease.OutQuad);
    }
    
    /// <summary>
    /// 停止激活动画
    /// </summary>
    public void StopActivationAnimation()
    {
        // 停止DOTween动画
        transform.DOKill();
        // 重置旋转
        transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// 鼠标进入时显示卡牌详细信息
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CardDetailDisplay.Instance != null)
        {
            CardDetailDisplay.Instance.ShowRuneDetail(info);
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
