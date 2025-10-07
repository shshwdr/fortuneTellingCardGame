using System.Collections;
using System.Collections.Generic;
using Sinbad;
using UnityEngine;

public class CardInfo
{
    public string identifier;
    public string name;
    public string description;
    public List<string> upEffect;
    public List<string> downEffect;
    public bool isStart;
    public bool canBeDraw;
    public int cost;
}

public class CSVLoader : Singleton<CSVLoader>
{
    public Dictionary<string, CardInfo> cardInfoMap = new Dictionary<string, CardInfo>();
    public Dictionary<string, CustomerInfo> customerInfoMap = new Dictionary<string, CustomerInfo>();
    public Dictionary<string, DayInfo> dayInfoMap = new Dictionary<string, DayInfo>();
    public Dictionary<string, RuneInfo> runeInfoMap = new Dictionary<string, RuneInfo>();
    public Dictionary<string, SigilInfo> sigilInfoMap = new Dictionary<string, SigilInfo>();
    
    // Start is called before the first frame update
    public void Init()
    {
        LoadCardData();
        LoadCustomerData();
        LoadDayData();
        LoadRuneData();
        LoadSigilData();
    }
    
    private void LoadCardData()
    {
        var cardInfos = CsvUtil.LoadObjects<CardInfo>("card");
        foreach (var cardInfo in cardInfos)
        {
            cardInfoMap[cardInfo.identifier] = cardInfo;
        }
    }
    
    private void LoadCustomerData()
    {
        var customerInfos = CsvUtil.LoadObjects<CustomerInfo>("customer");
        foreach (var customerInfo in customerInfos)
        {
            customerInfoMap[customerInfo.identifier] = customerInfo;
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
        }
    }
    
    private void LoadSigilData()
    {
        var sigilInfos = CsvUtil.LoadObjects<SigilInfo>("sigil");
        foreach (var sigilInfo in sigilInfos)
        {
            sigilInfoMap[sigilInfo.identifier] = sigilInfo;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
