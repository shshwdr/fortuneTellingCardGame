using System;
using System.Collections;
using System.Collections.Generic;
using Naninovel;
using Naninovel.Commands;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    private IScriptPlayer scriptPlayer;
    private ICharacterManager characterManager;
    private ITextPrinterManager printerManager;
    private Action dialogueStopAction;
    public async void Start()
    {
        //Debug.LogError("DialogueManager Start");
        
        //Debug.LogError("DialogueManager Async");
        try
        {
            // WebGL安全的等待Engine初始化
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // WebGL平台使用基于帧的等待，避免死锁
                int maxWaitFrames = 300; // 最多等待5秒（假设60FPS）
                int waitedFrames = 0;
                
                while (!Engine.Initialized && waitedFrames < maxWaitFrames)
                {
                    //Debug.LogError("DialogueManager waiting");
                    await UniTask.Yield(); // 等待一帧
                    waitedFrames++;
                }
                
                //Debug.LogError("DialogueManager finished");
                if (!Engine.Initialized)
                {
                    //Debug.LogError("Engine初始化超时！");
                    return;
                }
                //Debug.LogError("DialogueManager really finished");
            }
            else
            {
                // 非WebGL平台使用标准等待
                await UniTask.WaitUntil(() => Engine.Initialized);
            }
            
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            characterManager = Engine.GetService<ICharacterManager>();
            printerManager = Engine.GetService<ITextPrinterManager>();
            //scriptPlayer.OnFinalStop += OnDialogueStopped;
            //scriptPlayer.OnPlay += OnDialoguePlay;
            FinalStopEventManager.OnFinalStop += OnDialogueStopped;
            StartDialogue("start", () =>
            {
                GameSystem.Instance.StartNewGame();
            });
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DialogueManager 初始化异常: {e.Message}");
        }
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
    private void OnDialogueStopped(Script script,string message)
    {
        HideDialogueUI(message);
    }
    private void HideDialogueUI(string message)
    {
        if (message.Trim() == "stay")
        {
            // 隐藏所有文本打印器
            foreach (var printer in printerManager.Actors)
            {
                if (printer.Visible)
                {
                    printer.ChangeTintColor(Color.grey, new Tween(0.5f));
                }
            }
        }
        else
        {
            // 隐藏所有文本打印器
            foreach (var printer in printerManager.Actors)
            {
                if (printer.Visible)
                {
                    printer.ChangeVisibility(false, new Tween(0.5f)).Forget();
                }
            }
        }

        if (FindObjectOfType<RuntimeBehaviour>())
        {
            
            FindObjectOfType<RuntimeBehaviour>().GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        }
        if (dialogueStopAction != null)
        {
            dialogueStopAction();
        }
    }

    void show()
    {
        foreach (var printer in printerManager.Actors)
        {
            if (printer.Visible)
            {
                printer.ChangeTintColor(Color.white, new Tween(0.5f));
            }
        }
        if( FindObjectOfType<RuntimeBehaviour>() && FindObjectOfType<RuntimeBehaviour>().GetComponentInChildren<CanvasGroup>())
            FindObjectOfType<RuntimeBehaviour>().GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
    }
    public void StartDialogue(string name, string label, Action dialogueStopAction = null)
    {
        show();
        this.dialogueStopAction = dialogueStopAction;
        scriptPlayer.LoadAndPlayAtLabel($"{name}",label);
    }
    
    public void StartDialogue(string name, Action dialogueStopAction = null)
    {
        show();
        this.dialogueStopAction = dialogueStopAction;
        //Debug.LogError($"启动对话: {name}");
        scriptPlayer.LoadAndPlay($"{name}");
        // UniTask.Run(async () =>
        // {
        //     try
        //     {
        //         await scriptPlayer.LoadAndPlay($"Dialogue/{name}");
        //         Debug.Log("对话开始成功");
        //     }
        //     catch (System.Exception e)
        //     {
        //         Debug.LogError($"启动对话失败: {e.Message}");
        //     }
        // });
        //await scriptPlayer.LoadAndPlay("脚本路径");
    }
}
