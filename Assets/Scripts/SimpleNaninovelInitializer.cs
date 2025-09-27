using UnityEngine;
using Naninovel;

/// <summary>
/// 简单的Naninovel引擎手动初始化器
/// 用于在关闭自动初始化后手动启动引擎
/// </summary>
public class SimpleNaninovelInitializer : MonoBehaviour
{
    [Header("初始化设置")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private string scriptToPlayAfterInit = ""; // 初始化后要播放的脚本
    
    private bool isInitialized = false;
    
    private async void Start()
    {
        if (initializeOnStart)
        {
            await InitializeNaninovel();
        }
    }
    
    /// <summary>
    /// 初始化Naninovel引擎
    /// </summary>
    [ContextMenu("初始化Naninovel")]
    public async UniTask InitializeNaninovel()
    {
        if (Engine.Initialized)
        {
            Debug.Log("Naninovel引擎已经初始化完成");
            isInitialized = true;
            return;
        }
        
        Debug.Log("开始初始化Naninovel引擎...");
        
        try
        {
            DisableTitleScript();
            // 使用RuntimeInitializer进行初始化
            await RuntimeInitializer.Initialize();
            
            isInitialized = true;
            Debug.Log("Naninovel引擎初始化完成！");
            
            // // 如果设置了要播放的脚本，则开始播放
            // if (!string.IsNullOrEmpty(scriptToPlayAfterInit))
            // {
            //     await PlayScript(scriptToPlayAfterInit);
            // }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Naninovel引擎初始化失败: {e.Message}");
        }
    }
    private void DisableTitleScript()
    {
        var scriptsConfig = Resources.Load<ScriptsConfiguration>("Naninovel/Configuration/ScriptsConfiguration");
        if (scriptsConfig != null)
        {
            scriptsConfig.TitleScript = ""; // 清空TitleScript
        }
    }
    private async UniTask InitializeWithModifiedConfig()
    {
        // 获取ScriptsConfiguration并修改TitleScript
        var scriptsConfig = Engine.GetConfiguration<ScriptsConfiguration>();
        if (scriptsConfig != null)
        {
            // 保存原始值
            var originalTitleScript = scriptsConfig.TitleScript;
            
            // 临时清空TitleScript
            scriptsConfig.TitleScript = "";
            
            try
            {
                // 初始化引擎
                await RuntimeInitializer.Initialize();
            }
            finally
            {
                // 恢复原始值（可选）
                // scriptsConfig.TitleScript = originalTitleScript;
            }
        }
        else
        {
            // 如果无法获取配置，直接初始化
            await RuntimeInitializer.Initialize();
        }
    }
    
    /// <summary>
    /// 播放指定脚本
    /// </summary>
    public async UniTask PlayScript(string scriptPath)
    {
        if (!Engine.Initialized)
        {
            Debug.LogError("引擎未初始化，无法播放脚本");
            return;
        }
        
        try
        {
            var scriptPlayer = Engine.GetService<IScriptPlayer>();
            if (scriptPlayer != null)
            {
                Debug.Log($"开始播放脚本: {scriptPath}");
                await scriptPlayer.LoadAndPlay(scriptPath);
            }
            else
            {
                Debug.LogError("无法获取ScriptPlayer服务");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"播放脚本失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 停止当前对话
    /// </summary>
    public void StopDialogue()
    {
        if (!Engine.Initialized) return;
        
        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        scriptPlayer?.Stop();
    }
    
    /// <summary>
    /// 检查引擎是否已初始化
    /// </summary>
    public bool IsEngineInitialized()
    {
        return Engine.Initialized && isInitialized;
    }
    
    /// <summary>
    /// 获取当前播放状态
    /// </summary>
    public string GetPlaybackStatus()
    {
        if (!Engine.Initialized)
            return "引擎未初始化";
        
        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        if (scriptPlayer == null)
            return "ScriptPlayer不可用";
        
        var currentScript = scriptPlayer.PlayedScript?.Path ?? "无";
        var isPlaying = scriptPlayer.Playing;
        
        return $"当前脚本: {currentScript}\n播放状态: {(isPlaying ? "进行中" : "已停止")}";
    }
    
    #region 调试UI
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 350, 200));
        
        GUILayout.Label("Naninovel引擎状态", GUI.skin.box);
        
        GUILayout.Space(10);
        
        // 显示状态
        GUILayout.Label($"引擎初始化: {Engine.Initialized}");
        GUILayout.Label($"本地状态: {isInitialized}");
        
        if (Engine.Initialized)
        {
            var scriptPlayer = Engine.GetService<IScriptPlayer>();
            if (scriptPlayer != null)
            {
                GUILayout.Label($"当前脚本: {scriptPlayer.PlayedScript?.Path ?? "无"}");
                GUILayout.Label($"播放状态: {(scriptPlayer.Playing ? "进行中" : "已停止")}");
            }
        }
        
        GUILayout.Space(10);
        
        // 控制按钮
        if (!Engine.Initialized)
        {
            if (GUILayout.Button("初始化引擎"))
                InitializeNaninovel().Forget();
        }
        else
        {
            if (GUILayout.Button("播放测试脚本"))
                PlayScript("test/基础/_测试脚本1").Forget();
            
            if (GUILayout.Button("停止对话"))
                StopDialogue();
        }
        
        GUILayout.EndArea();
    }
    
    #endregion
}
