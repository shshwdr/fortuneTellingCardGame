using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class RuneCell : MonoBehaviour
{
    public string identifier;
    public TMP_Text text;
    public TMP_Text desc;
    public TMP_Text statusText; // Optional: to display rune status
    public GameObject effecting;

    public void SetData(Rune rune)
    {
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
