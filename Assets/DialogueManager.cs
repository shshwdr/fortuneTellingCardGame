using System;
using System.Collections;
using System.Collections.Generic;
using Naninovel;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    private IScriptPlayer scriptPlayer;
    private ICharacterManager characterManager;
    private ITextPrinterManager printerManager;
    private Action dialogueStopAction;
    public void Init()
    {
        UniTask.Run(async () =>
        {
            try
            {
                await UniTask.WaitUntil(() => Engine.Initialized);
                
                scriptPlayer = Engine.GetService<IScriptPlayer>();
                characterManager = Engine.GetService<ICharacterManager>();
                printerManager = Engine.GetService<ITextPrinterManager>();
                scriptPlayer.OnFinalStop += OnDialogueStopped;
                scriptPlayer.OnPlay += OnDialoguePlay;
                StartDialogue("start");
                
            }
            catch (System.Exception e)
            {
            }
        });
    }
    private void OnDialoguePlay(Script script)
    {
        // Debug.Log($"对话开始: {script?.Path}");
        // // 执行对话开始前的操作
        // ShowDialogueUI();
        // if (FindObjectOfType<RuntimeBehaviour>())
        // {
        //     
        //     FindObjectOfType<RuntimeBehaviour>().GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
        // }
    }
    private void OnDialogueStopped(Script script)
    {
        HideDialogueUI();
    }
    private void HideDialogueUI()
    {
        // 隐藏所有文本打印器
        foreach (var printer in printerManager.Actors)
        {
            if (printer.Visible)
            {
                printer.ChangeVisibility(false, new Tween(0.5f)).Forget();
            }
        }

        if (FindObjectOfType<RuntimeBehaviour>())
        {
            
            FindObjectOfType<RuntimeBehaviour>().GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        }
    }
    public void StartDialogue(string name, Action dialogueStopAction = null)
    {
        FindObjectOfType<RuntimeBehaviour>().GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
        UniTask.Run(async () =>
        {
            try
            {
                await UniTask.WaitUntil(() => Engine.Initialized);
                this.dialogueStopAction = dialogueStopAction;
                await scriptPlayer.LoadAndPlay($"Dialogue/{name}");
                Debug.Log("对话开始成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"启动对话失败: {e.Message}");
            }
        });
        //await scriptPlayer.LoadAndPlay("脚本路径");
    }
}
