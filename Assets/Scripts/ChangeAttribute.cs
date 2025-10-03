// 创建自定义命令
using Naninovel;

namespace Naninovel.Commands
{
    [CommandAlias("changeAttribute")] // 可选：设置别名
    public class ChangeAttribute : Command
    {
        [Doc("The message to display")]
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringParameter Message;
        
        [Doc("Duration in seconds")]
        public IntegerParameter Value = 0;

        public override async UniTask Execute(AsyncToken token = default)
        {
            // 调用Unity代码
            
            var character = GameSystem.Instance.GetCurrentCustomer();
            character.ModifyAttribute(Message.Value, Value.Value);
            ToastManager.Instance.ShowToast($"{Message.Value} +{Value.Value}");
            
            GameSystem.Instance.OnAttributeChanged?.Invoke();
           // ToastManager.Instance.ShowToast(Message.Value, Duration.Value);
            await UniTask.CompletedTask;
        }
    }
}