using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class TutorialManager : Singleton<TutorialManager>
{
    public bool showTutorial = true;

    HashSet<string> finishedTutorial = new HashSet<string>();
    public string currentTutorial;
    public void ShowTutorial(string tutorialName,Action action=null)
    {
        StartCoroutine(test(tutorialName, action));
    }

    IEnumerator test(string tutorialName,Action action=null)
    {
        yield return new WaitForSeconds(0.1f);
        if (!showTutorial)
        {
            yield break;
        }
        if (finishedTutorial.Contains(tutorialName))
        {
            yield break;
        }

        currentTutorial = tutorialName;
        finishedTutorial.Add(tutorialName);

        switch (tutorialName)
        {
            case "firstCustomerCome":
                GameSystem.Instance.gameState.canRedraw = false;
                GameSystem.Instance.gameState.canSkip = false;
                break;
            case "firstCustomerLeave":
                break;
            case "secondCustomerCome":
                GameSystem.Instance.gameState.canSkip = false;
                GameSystem.Instance.gameState.canTellForturn = false;
                break;
            case "redraw":
                GameSystem.Instance.gameState.canTellForturn = true;
                break;
            
        }
        
        GameSystem.Instance.OnUpdateActions.Invoke();
        DialogueManager.Instance.StartDialogue("start",tutorialName, () =>
        {
            action?.Invoke();
        });
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
