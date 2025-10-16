using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sinbad;
using UnityEngine;

public class CardInfo
{
    public string identifier;
    public string name;
    public string description;
    public List<string> upEffect;
    public List<string> downEffect;
    public string icon;

    public Sprite Icon(bool isUpright)
    {
            var folder = "";
            if (isUpright)
            {
                folder = "cards_light/";
            }
            else
            {
                folder = "cards_dark/";
            }
            return Resources.Load<Sprite>("cards/"+folder + icon); 
    }

    public List<string> UpEffect(int level)
    {
return         GetEffectWithLevel(true, level);
    }
    public List<string> DownEffect(int level)
    {
        return GetEffectWithLevel(false, level);
    }

    public List<string> GetEffectWithLevel(bool isUp,int level)
    {
        var infoEffect = (isUp ? upEffect : downEffect).ToList();
        for (int i = 0; i < infoEffect.Count; i++)
        {
            var value = infoEffect[i];
            int v;
            if(int.TryParse( value, out v))
            {
                int finalv=v;
                // if (infoEffect[0] == "when")
                // {
                //     if (v > 0)
                //     {
                //         var basev = v - 1;
                //         finalv = basev + level;
                //     }else if (v < 0)
                //     {
                //         var basev = v + 1;
                //         finalv = basev - level;
                //     }
                // }
                // else
                {
                    finalv = v * level;
                }
               
                infoEffect[i] = finalv.ToString();
            }
        }
        return infoEffect;
    }
    public bool isStart;
    public bool canBeDraw;
    public int cost;
    public int maxLevel;
}

public class CSVLoader : Singleton<CSVLoader>
{
    public Dictionary<string, CardInfo> cardInfoMap = new Dictionary<string, CardInfo>();
    public Dictionary<string, CustomerInfo> customerInfoMap = new Dictionary<string, CustomerInfo>();
    public Dictionary<string, DayInfo> dayInfoMap = new Dictionary<string, DayInfo>();
    public Dictionary<string, RuneInfo> runeInfoMap = new Dictionary<string, RuneInfo>();
    public Dictionary<string, SigilInfo> sigilInfoMap = new Dictionary<string, SigilInfo>();

    public DayInfo getDayInfo(int day)
    {

        string str = day.ToString();
        return dayInfoMap.ContainsKey(str) ? dayInfoMap[str] : dayInfoMap.Values.Last();
    }
    // Start is called before the first frame update
    public void Init()
    {
        LoadCardData();
        LoadCustomerData();
        LoadDayData();
        LoadRuneData();
    }
    
    private void LoadCardData()
    {
        var cardInfos = CsvUtil.LoadObjects<CardInfo>("card");
        foreach (var cardInfo in cardInfos)
        {
            cardInfoMap[cardInfo.identifier] = cardInfo;
            if (cardInfo.canBeDraw)
            {
                if (cardInfo.Icon(true) == null)
                {
                    Debug.LogError($"{cardInfo.identifier} no upright icon");
                }else if (cardInfo.Icon(false) == null)
                {
                    Debug.LogError($"{cardInfo.identifier} no dark icon");
                }
            }
        }
    }
    
    private void LoadCustomerData()
    {
        var customerInfos = CsvUtil.LoadObjects<CustomerInfo>("customer");
        foreach (var customerInfo in customerInfos)
        {
            customerInfoMap[customerInfo.identifier] = customerInfo;
            if (customerInfo.icon== null)
            {
                Debug.LogError($"{customerInfo.identifier} no icon");
            }
        }
    }
    
    private void LoadDayData()
    {
        var dayInfos = CsvUtil.LoadObjects<DayInfo>("day");
        foreach (var dayInfo in dayInfos)
        {
            dayInfoMap[dayInfo.identifier] = dayInfo;
        }
    }
    
    private void LoadRuneData()
    {
        var runeInfos = CsvUtil.LoadObjects<RuneInfo>("rune");
        foreach (var runeInfo in runeInfos)
        {
            runeInfoMap[runeInfo.identifier] = runeInfo;
            if (runeInfo.canBeDraw && runeInfo.icon == null)
            {
                Debug.LogError($"{runeInfo.identifier} no icon");
            }
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
