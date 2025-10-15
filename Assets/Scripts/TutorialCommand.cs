using Naninovel;
using UnityEngine;

namespace Naninovel.Commands
{
    [CommandAlias("tutorial")]
    public class TutorialCommand : Command
    {
        [ParameterAlias(NamelessParameterAlias)]
        public StringParameter Text;

        public override async UniTask Execute (AsyncToken token = default)
        {
            string message = Text?.Value ?? "default";
            
            // 确保TutorialManager实例存在
            if (TutorialManager.Instance != null)
            {
                if (message == "")
                {
                    TutorialManager.Instance.HideAllTutorialAreas();
                }
                else
                {
                    TutorialManager.Instance.ShowTutorialArea(message);
                }
            }
            else
            {
                Debug.LogWarning($"TutorialManager instance not found when executing tutorial command with text: {message}");
            }

            await UniTask.CompletedTask;
        }
    }
}