using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        effecting .SetActive(isEffect);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
