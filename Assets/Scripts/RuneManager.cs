using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneManager : Singleton<RuneManager>
{
    public Dictionary<string, int> runeEffects = new Dictionary<string, int>();
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    public int getEffectValue(string effect)
    {
        return runeEffects.ContainsKey(effect) ? runeEffects[effect] : 0;
    }

    public void AddRune(Rune rune)
    {
        AddRuneEffect(rune.info.effect,rune.info.value);
    }
    
    public void AddRuneEffect(string effectName, int effect)
    {
        if (runeEffects.ContainsKey(effectName))
        {
            runeEffects[effectName] += effect;
        }
        else
        {
            runeEffects.Add(effectName, effect);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
