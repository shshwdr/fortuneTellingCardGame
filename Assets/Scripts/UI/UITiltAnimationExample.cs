using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UITiltAnimation使用示例
/// 展示如何控制UI元素的倾斜动画
/// </summary>
public class UITiltAnimationExample : MonoBehaviour
{
    [Header("动画控制")]
    public UITiltAnimation tiltAnimation;
    
    [Header("测试按钮")]
    public Button startButton;
    public Button stopButton;
    public Button resetButton;
    public Button pauseResumeButton;
    
    [Header("参数调节")]
    public Slider angleSlider;
    public Slider speedSlider;
    
    private bool isPaused = false;
    
    void Start()
    {
        // 如果没有指定动画组件，尝试在当前对象上找到
        if (tiltAnimation == null)
        {
            tiltAnimation = GetComponent<UITiltAnimation>();
        }
        
        SetupButtons();
        SetupSliders();
    }
    
    private void SetupButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(() => {
                tiltAnimation?.StartTiltAnimation();
                Debug.Log("开始倾斜动画");
            });
        }
        
        if (stopButton != null)
        {
            stopButton.onClick.AddListener(() => {
                tiltAnimation?.StopTiltAnimation();
                Debug.Log("停止倾斜动画");
            });
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() => {
                tiltAnimation?.StopAndResetRotation();
                Debug.Log("重置旋转角度");
            });
        }
        
        if (pauseResumeButton != null)
        {
            pauseResumeButton.onClick.AddListener(() => {
                if (isPaused)
                {
                    tiltAnimation?.ResumeTiltAnimation();
                    Debug.Log("恢复动画");
                    isPaused = false;
                    
                    var buttonText = pauseResumeButton.GetComponentInChildren<Text>();
                    if (buttonText != null) buttonText.text = "暂停";
                }
                else
                {
                    tiltAnimation?.PauseTiltAnimation();
                    Debug.Log("暂停动画");
                    isPaused = true;
                    
                    var buttonText = pauseResumeButton.GetComponentInChildren<Text>();
                    if (buttonText != null) buttonText.text = "恢复";
                }
            });
        }
    }
    
    private void SetupSliders()
    {
        if (angleSlider != null)
        {
            angleSlider.minValue = 0f;
            angleSlider.maxValue = 45f;
            angleSlider.value = 10f; // 默认角度
            
            angleSlider.onValueChanged.AddListener((value) => {
                tiltAnimation?.SetTiltAngle(value);
                Debug.Log($"设置倾斜角度: {value}度");
            });
        }
        
        if (speedSlider != null)
        {
            speedSlider.minValue = 0.5f;
            speedSlider.maxValue = 5f;
            speedSlider.value = 1f; // 默认速度
            
            speedSlider.onValueChanged.AddListener((value) => {
                tiltAnimation?.SetTiltSpeed(value);
                Debug.Log($"设置倾斜速度: {value}秒");
            });
        }
    }
    
    void Update()
    {
        // 显示动画状态（可选）
        if (tiltAnimation != null && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"动画状态: {(tiltAnimation.IsPlaying() ? "播放中" : "已停止")}");
        }
    }
    
    void OnDestroy()
    {
        // 清理事件监听
        if (startButton != null) startButton.onClick.RemoveAllListeners();
        if (stopButton != null) stopButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (pauseResumeButton != null) pauseResumeButton.onClick.RemoveAllListeners();
        if (angleSlider != null) angleSlider.onValueChanged.RemoveAllListeners();
        if (speedSlider != null) speedSlider.onValueChanged.RemoveAllListeners();
    }
}