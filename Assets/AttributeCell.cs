using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttributeCell : MonoBehaviour
{
    public TMP_Text attributeName;
    public TMP_Text currentValue;
    public TMP_Text changeValue;

    public void SetValue(string name, int value,int changeValue)
    {
        attributeName.text = name;
        currentValue.text = value.ToString();
        this.changeValue.text = changeValue==0?"":changeValue.ToString();
        
        if(changeValue!=0)
        this.changeValue.color = changeValue > 0 ? Color.green : Color.red;
    }
}
