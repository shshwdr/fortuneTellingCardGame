using UnityEngine;
using DG.Tweening;

/// <summary>
/// UI元素左右倾斜动画组件
/// 使用DOTween实现缓缓的左右倾斜循环动画
/// </summary>
public class UITiltAnimation : MonoBehaviour
{
    [Header("倾斜动画设置")]
    [SerializeField] private float tiltAngle = 10f; // 倾斜角度
    [SerializeField] private float tiltSpeed = 1f; // 倾斜速度（完成一次左右倾斜的时间）
    [SerializeField] private bool playOnStart = true; // 是否在Start时自动播放
    [SerializeField] private Ease easeType = Ease.InOutSine; // 缓动类型
    
    private Tween tiltTween;
    private Vector3 originalRotation;
    
    void Start()
    {
        // 记录原始旋转角度
        originalRotation = transform.eulerAngles;
        
        if (playOnStart)
        {
            StartTiltAnimation();
        }
    }
    
    /// <summary>
    /// 开始倾斜动画
    /// </summary>
    public void StartTiltAnimation()
    {
        StopTiltAnimation();
        
        // 创建左右倾斜的循环动画
        tiltTween = transform.DORotate(
            new Vector3(originalRotation.x, originalRotation.y, originalRotation.z + tiltAngle), 
            tiltSpeed / 2f
        )
        .SetEase(easeType)
        .SetLoops(-1, LoopType.Yoyo); // 无限循环，来回摆动
    }
    
    /// <summary>
    /// 停止倾斜动画
    /// </summary>
    public void StopTiltAnimation()
    {
        if (tiltTween != null)
        {
            tiltTween.Kill();
            tiltTween = null;
        }
    }
    
    /// <summary>
    /// 停止动画并重置到原始角度
    /// </summary>
    public void StopAndResetRotation()
    {
        StopTiltAnimation();
        transform.eulerAngles = originalRotation;
    }
    
    /// <summary>
    /// 设置倾斜角度
    /// </summary>
    /// <param name="angle">倾斜角度</param>
    public void SetTiltAngle(float angle)
    {
        tiltAngle = angle;
        
        // 如果正在播放动画，重新启动以应用新角度
        if (tiltTween != null && tiltTween.IsActive())
        {
            StartTiltAnimation();
        }
    }
    
    /// <summary>
    /// 设置倾斜速度
    /// </summary>
    /// <param name="speed">倾斜速度（完成一次左右倾斜的时间）</param>
    public void SetTiltSpeed(float speed)
    {
        tiltSpeed = speed;
        
        // 如果正在播放动画，重新启动以应用新速度
        if (tiltTween != null && tiltTween.IsActive())
        {
            StartTiltAnimation();
        }
    }
    
    /// <summary>
    /// 设置缓动类型
    /// </summary>
    /// <param name="ease">缓动类型</param>
    public void SetEaseType(Ease ease)
    {
        easeType = ease;
        
        // 如果正在播放动画，重新启动以应用新缓动
        if (tiltTween != null && tiltTween.IsActive())
        {
            StartTiltAnimation();
        }
    }
    
    /// <summary>
    /// 暂停动画
    /// </summary>
    public void PauseTiltAnimation()
    {
        if (tiltTween != null && tiltTween.IsActive())
        {
            tiltTween.Pause();
        }
    }
    
    /// <summary>
    /// 恢复动画
    /// </summary>
    public void ResumeTiltAnimation()
    {
        if (tiltTween != null && tiltTween.IsActive())
        {
            tiltTween.Play();
        }
    }
    
    /// <summary>
    /// 检查动画是否正在播放
    /// </summary>
    /// <returns>是否正在播放</returns>
    public bool IsPlaying()
    {
        return tiltTween != null && tiltTween.IsActive() && tiltTween.IsPlaying();
    }
    
    void OnDestroy()
    {
        // 清理动画
        StopTiltAnimation();
    }
    
    void OnDisable()
    {
        // 当对象被禁用时暂停动画
        PauseTiltAnimation();
    }
    
    void OnEnable()
    {
        // 当对象被启用时恢复动画（如果之前在播放）
        if (playOnStart && tiltTween != null)
        {
            ResumeTiltAnimation();
        }
    }
    
    #if UNITY_EDITOR
    void OnValidate()
    {
        // 在编辑器中修改参数时，如果正在播放模式，立即应用更改
        if (Application.isPlaying && tiltTween != null && tiltTween.IsActive())
        {
            StartTiltAnimation();
        }
    }
    #endif
}