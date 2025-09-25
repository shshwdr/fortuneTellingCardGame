using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to quickly setup a basic UI for testing the game
/// Run this in the editor to create a simple UI layout
/// </summary>
public class QuickUISetup : MonoBehaviour
{
    [Header("UI Setup Settings")]
    public bool createUI = false;
    public Font uiFont;
    
    [ContextMenu("Create Basic UI")]
    public void CreateBasicUI()
    {
        // Create Canvas if doesn't exist
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create GameUI object
        GameObject gameUIGO = new GameObject("GameUI");
        gameUIGO.transform.SetParent(canvas.transform, false);
        GameUI gameUI = gameUIGO.AddComponent<GameUI>();
        
        // Create main panel
        GameObject mainPanel = CreatePanel("MainPanel", canvas.transform);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;
        
        // Create top info panel
        GameObject topPanel = CreatePanel("TopPanel", mainPanel.transform);
        RectTransform topRect = topPanel.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 0.8f);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.offsetMin = Vector2.zero;
        topRect.offsetMax = Vector2.zero;
        
        // Create customer info
        gameUI.dayText = CreateText("DayText", topPanel.transform, "Day: 1", TextAnchor.UpperLeft).GetComponent<Text>();
        gameUI.moneyText = CreateText("MoneyText", topPanel.transform, "Money: 0", TextAnchor.UpperCenter).GetComponent<Text>();
        gameUI.currentCustomerIndexText = CreateText("CustomerIndexText", topPanel.transform, "Customer: 1/3", TextAnchor.UpperRight).GetComponent<Text>();
        
        // Create customer panel
        GameObject customerPanel = CreatePanel("CustomerPanel", mainPanel.transform);
        RectTransform customerRect = customerPanel.GetComponent<RectTransform>();
        customerRect.anchorMin = new Vector2(0, 0.4f);
        customerRect.anchorMax = new Vector2(1, 0.8f);
        customerRect.offsetMin = Vector2.zero;
        customerRect.offsetMax = Vector2.zero;
        
        // Customer info texts
        gameUI.customerNameText = CreateText("CustomerNameText", customerPanel.transform, "Customer Name", TextAnchor.UpperCenter).GetComponent<Text>();
        gameUI.customerTargetText = CreateText("CustomerTargetText", customerPanel.transform, "Wants: wealth", TextAnchor.MiddleCenter).GetComponent<Text>();
        
        // Attribute texts
        GameObject attributePanel = CreatePanel("AttributePanel", customerPanel.transform);
        RectTransform attrRect = attributePanel.GetComponent<RectTransform>();
        attrRect.anchorMin = new Vector2(0, 0);
        attrRect.anchorMax = new Vector2(1, 0.5f);
        attrRect.offsetMin = Vector2.zero;
        attrRect.offsetMax = Vector2.zero;
        
        // Layout for attributes
        HorizontalLayoutGroup attrLayout = attributePanel.AddComponent<HorizontalLayoutGroup>();
        attrLayout.spacing = 10;
        attrLayout.childControlWidth = true;
        attrLayout.childControlHeight = true;
        
        gameUI.wealthText = CreateText("WealthText", attributePanel.transform, "Wealth: 50", TextAnchor.MiddleCenter).GetComponent<Text>();
        gameUI.relationshipText = CreateText("RelationshipText", attributePanel.transform, "Relationship: 50", TextAnchor.MiddleCenter).GetComponent<Text>();
        gameUI.sanityText = CreateText("SanityText", attributePanel.transform, "Sanity: 50", TextAnchor.MiddleCenter).GetComponent<Text>();
        gameUI.powerText = CreateText("PowerText", attributePanel.transform, "Power: 50", TextAnchor.MiddleCenter).GetComponent<Text>();
        
        // Create cards panel
        GameObject cardsPanel = CreatePanel("CardsPanel", mainPanel.transform);
        RectTransform cardsRect = cardsPanel.GetComponent<RectTransform>();
        cardsRect.anchorMin = new Vector2(0, 0.1f);
        cardsRect.anchorMax = new Vector2(1, 0.4f);
        cardsRect.offsetMin = Vector2.zero;
        cardsRect.offsetMax = Vector2.zero;
        
        // Layout for cards
        HorizontalLayoutGroup cardsLayout = cardsPanel.AddComponent<HorizontalLayoutGroup>();
        cardsLayout.spacing = 20;
        cardsLayout.childControlWidth = true;
        cardsLayout.childControlHeight = true;
        
        // Create card slots
        for (int i = 0; i < 4; i++)
        {
            GameObject cardSlot = CreateCardSlot($"CardSlot{i}", cardsPanel.transform, i);
            gameUI.cardSlots[i] = cardSlot.GetComponent<CardUI>();
        }
        
        // Create buttons panel
        GameObject buttonsPanel = CreatePanel("ButtonsPanel", mainPanel.transform);
        RectTransform buttonsRect = buttonsPanel.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0, 0);
        buttonsRect.anchorMax = new Vector2(1, 0.1f);
        buttonsRect.offsetMin = Vector2.zero;
        buttonsRect.offsetMax = Vector2.zero;
        
        // Layout for buttons
        HorizontalLayoutGroup buttonsLayout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
        buttonsLayout.spacing = 10;
        buttonsLayout.childControlWidth = true;
        buttonsLayout.childControlHeight = true;
        
        gameUI.performDivinationButton = CreateButton("DivinationButton", buttonsPanel.transform, "Perform Divination").GetComponent<Button>();
        gameUI.skipCustomerButton = CreateButton("SkipButton", buttonsPanel.transform, "Skip Customer").GetComponent<Button>();
        gameUI.nextDayButton = CreateButton("NextDayButton", buttonsPanel.transform, "Next Day").GetComponent<Button>();
        
        Debug.Log("Basic UI created successfully!");
    }
    
    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        return panel;
    }
    
    private GameObject CreateText(string name, Transform parent, string text, TextAnchor alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        Text textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
        textComponent.alignment = alignment;
        
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return textGO;
    }
    
    private GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        
        Button button = buttonGO.AddComponent<Button>();
        
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        Text textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 12;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonGO;
    }
    
    private GameObject CreateCardSlot(string name, Transform parent, int index)
    {
        GameObject cardGO = new GameObject(name);
        cardGO.transform.SetParent(parent, false);
        
        Image image = cardGO.AddComponent<Image>();
        image.color = new Color(0.6f, 0.4f, 0.2f, 1f);
        
        Button button = cardGO.AddComponent<Button>();
        CardUI cardUI = cardGO.AddComponent<CardUI>();
        
        // Card name text
        GameObject nameTextGO = new GameObject("NameText");
        nameTextGO.transform.SetParent(cardGO.transform, false);
        Text nameText = nameTextGO.AddComponent<Text>();
        nameText.text = "Card Name";
        nameText.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 10;
        nameText.color = Color.white;
        nameText.alignment = TextAnchor.UpperCenter;
        
        RectTransform nameRect = nameTextGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.8f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // Card effect text
        GameObject effectTextGO = new GameObject("EffectText");
        effectTextGO.transform.SetParent(cardGO.transform, false);
        Text effectText = effectTextGO.AddComponent<Text>();
        effectText.text = "Effect";
        effectText.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        effectText.fontSize = 8;
        effectText.color = Color.white;
        effectText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform effectRect = effectTextGO.GetComponent<RectTransform>();
        effectRect.anchorMin = new Vector2(0, 0);
        effectRect.anchorMax = new Vector2(1, 0.8f);
        effectRect.offsetMin = Vector2.zero;
        effectRect.offsetMax = Vector2.zero;
        
        // Setup CardUI references
        cardUI.cardNameText = nameText;
        cardUI.cardEffectText = effectText;
        cardUI.cardButton = button;
        
        return cardGO;
    }
    
    void Update()
    {
        if (createUI)
        {
            createUI = false;
            CreateBasicUI();
        }
    }
}