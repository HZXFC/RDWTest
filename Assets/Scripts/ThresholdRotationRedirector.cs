using UnityEngine;

/// <summary>
/// 阈值测定专用的旋转增益Redirector
/// 精确控制旋转增益值，用于旋转增益阈值测定实验
/// </summary>
public class ThresholdRotationRedirector : Redirector
{
    [Header("Rotation Gain Parameters")]
    [Tooltip("旋转增益值（1.0 = 无增益，1.5 = 50%增益）")]
    public float rotationGain = 1.5f;
    
    [Tooltip("旋转阈值（度/秒），低于此值的旋转不应用增益")]
    public float rotationThreshold = 1.5f; // 度/秒
    
    [Tooltip("是否仅在快速旋转时应用增益（用于旋转任务）")]
    public bool applyOnFastRotationOnly = true;
    
    private float lastRotationGain = 1.0f;
    
    /// <summary>
    /// 从StaircaseController设置旋转增益值
    /// </summary>
    public void SetRotationGain(float gain)
    {
        rotationGain = gain;
    }
    
    /// <summary>
    /// 获取当前旋转增益值
    /// </summary>
    public float GetRotationGain()
    {
        return rotationGain;
    }
    
    public override void InjectRedirection()
    {
        float deltaDir = redirectionManager.deltaDir;
        float deltaTime = redirectionManager.GetDeltaTime();
        
        // 检查旋转速度是否超过阈值
        float rotationSpeed = Mathf.Abs(deltaDir / deltaTime);
        
        if (rotationSpeed >= rotationThreshold)
        {
            // 应用旋转增益
            // 增益公式: g_r = (rotationGain - 1.0) * deltaDir
            // 例如: rotationGain = 1.5, deltaDir = 90度
            // 应用增益: (1.5 - 1.0) * 90 = 45度额外旋转
            
            float rotationToApply = (rotationGain - 1.0f) * deltaDir;
            InjectRotation(rotationToApply);
            lastRotationGain = rotationGain;
        }
        else if (!applyOnFastRotationOnly)
        {
            // 如果不需要仅在快速旋转时应用，则对所有旋转应用增益
            float rotationToApply = (rotationGain - 1.0f) * deltaDir;
            InjectRotation(rotationToApply);
            lastRotationGain = rotationGain;
        }
    }
    
    /// <summary>
    /// 获取最后一次应用的增益值
    /// </summary>
    public float GetLastAppliedGain()
    {
        return lastRotationGain;
    }
}