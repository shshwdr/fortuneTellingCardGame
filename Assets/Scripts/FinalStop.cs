// Assets/Scripts/Commands/FinalStop.cs
using Naninovel;
using UnityEngine;

namespace Naninovel.Commands
{
    [Doc(
        @"
        明确标识一段对话的结束，触发FinalStop事件。
        与普通的@stop命令不同，@FinalStop专门用于标识对话的真正结束。
        ",
        null,
        @"
        ; 对话结束示例
        ...这是对话的最后一句。
        @FinalStop
        ",
        @"
        ; 带参数的消息
        @FinalStop ""对话已结束""
        "
    )]
    [CommandAlias("FinalStop")]
    public class FinalStop : Command
    {
        [Doc("可选的结束消息，用于标识对话结束的原因。")]
        [ParameterAlias(NamelessParameterAlias)]
        public StringParameter Message;

        public override async UniTask Execute(AsyncToken token = default)
        {
            var scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
            var currentScript = scriptPlayer.PlayedScript;
            var message = Assigned(Message) ? Message.Value : "对话已结束";

            // 触发FinalStop事件
            FinalStopEventManager.TriggerFinalStop(currentScript, message);

            // 停止脚本播放
            scriptPlayer.Stop();

            await UniTask.CompletedTask;
        }
    }
}