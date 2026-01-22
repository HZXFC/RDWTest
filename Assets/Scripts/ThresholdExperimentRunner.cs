using UnityEngine;

/// <summary>
/// 阈值测定实验的简单运行器
/// 提供简化的接口来运行实验
/// 可以作为参考实现
/// </summary>
public class ThresholdExperimentRunner : MonoBehaviour
{
    [Header("Experiment Setup")]
    [Tooltip("实验管理器引用")]
    public ThresholdExperimentManager experimentManager;
    
    [Tooltip("按键开始下一个试次（用于测试）")]
    public KeyCode startTrialKey = KeyCode.Space;
    
    [Tooltip("按键标记试次完成（用于测试）")]
    public KeyCode completeTrialKey = KeyCode.Return;
    
    [Tooltip("自动模式：自动开始试次和等待响应")]
    public bool autoMode = false;
    
    private void Start()
    {
        if (experimentManager == null)
        {
            experimentManager = FindObjectOfType<ThresholdExperimentManager>();
        }
        
        if (experimentManager != null)
        {
            experimentManager.InitializeExperiment();
            
            // 自动模式：立即开始第一个试次
            if (autoMode)
            {
                StartFirstTrial();
            }
        }
        else
        {
            Debug.LogError("ThresholdExperimentManager未找到！");
        }
    }
    
    private void Update()
    {
        if (experimentManager == null) return;
        
        // 键盘控制（用于测试）
        if (Input.GetKeyDown(startTrialKey))
        {
            if (experimentManager.GetCurrentState() == ThresholdExperimentManager.ExperimentState.Waiting ||
                experimentManager.GetCurrentState() == ThresholdExperimentManager.ExperimentState.TrialComplete)
            {
                experimentManager.StartNextTrial();
            }
        }
        
        if (Input.GetKeyDown(completeTrialKey))
        {
            if (experimentManager.GetCurrentState() == ThresholdExperimentManager.ExperimentState.TrialInProgress)
            {
                experimentManager.OnTrialComplete();
            }
        }
    }
    
    /// <summary>
    /// 开始第一个试次（用于自动模式）
    /// </summary>
    public void StartFirstTrial()
    {
        if (experimentManager != null)
        {
            experimentManager.StartNextTrial();
        }
    }
    
    /// <summary>
    /// 标记试次完成（用于任务完成时调用）
    /// </summary>
    public void MarkTrialComplete()
    {
        if (experimentManager != null)
        {
            experimentManager.OnTrialComplete();
        }
    }
    
    /// <summary>
    /// 提交用户响应（用于UI按钮调用）
    /// </summary>
    public void SubmitResponse(bool detected)
    {
        if (experimentManager != null)
        {
            experimentManager.SubmitUserResponse(detected);
        }
    }
}