// Assets/Scripts/Events/FinalStopEventManager.cs
using System;
using Naninovel;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// 管理FinalStop事件的静态类
    /// </summary>
    public static class FinalStopEventManager
    {
        /// <summary>
        /// FinalStop事件，当@FinalStop命令执行时触发
        /// </summary>
        public static event Action<Script, string> OnFinalStop;

        /// <summary>
        /// 触发FinalStop事件
        /// </summary>
        /// <param name="script">当前播放的脚本</param>
        /// <param name="message">结束消息</param>
        public static void TriggerFinalStop(Script script, string message)
        {
            Debug.Log($"[FinalStop] 脚本 '{script?.Path}' 已明确结束: {message}");
            OnFinalStop?.Invoke(script, message);
        }

        /// <summary>
        /// 清除所有事件监听器
        /// </summary>
        public static void ClearAllListeners()
        {
            OnFinalStop = null;
        }
    }
}