using System.Collections;
using System.Collections.Generic;
using Naninovel;
using Naninovel.UI;
using UnityEngine;
using UnityEngine.UI;

public class TopCanvas : MonoBehaviour
{
    public Button logButton;

    public Button allCardButton;
    // Start is called before the first frame update
    void Start()
    {
        
        logButton.onClick.AddListener(() =>
        {
            var uiManager = Engine.GetService<IUIManager>();
            uiManager.GetUI<IBacklogUI>()?.Show();
        });
        
        allCardButton.onClick.AddListener(() =>
        {
             CardDisplayMenu.ShowCards(GameSystem.Instance.gameState.allCards);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
