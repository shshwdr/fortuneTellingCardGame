using System.Collections;
using System.Collections.Generic;
using Naninovel;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    public void Init()
    {
        //StartDialogue("start");
    }

    public void StartDialogue(string name)
    {
        UniTask.Run(async () =>
        {
            try
            {
                await UniTask.WaitUntil(() => Engine.Initialized);
                var scriptPlayer = Engine.GetService<IScriptPlayer>();
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
