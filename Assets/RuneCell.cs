using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RuneCell : MonoBehaviour
{
    public string identifier;
    public TMP_Text text;
    public TMP_Text desc;

    public void SetData(RuneInfo info)
    {
        identifier = info.identifier;
        text.text = info.identifier;
        desc.text = info.description;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
