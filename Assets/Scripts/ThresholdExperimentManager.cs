using UnityEngine;
using System.Collections;

/// <summary>
/// 阈值测定实验的主管理器
/// 协调staircase、redirector、UI和数据记录
/// </summary>
public class ThresholdExperimentManager : MonoBehaviour
{
    [Header("Experiment Configuration")]
    [Tooltip("实验条件类型")]
    public ExperimentCondition condition = ExperimentCondition.Rotation;
    
    [Tooltip("年龄组 (0=儿童8-10岁, 1=成人20-25岁)")]
    public int ageGroup = 0;
    
    [Tooltip("参与者ID")]
    public string participantId = "P001";
    
    [Header("Rotation Gain Parameters")]
    [Tooltip("旋转增益初始值")]
    public float rotationInitialGain = 1.5f;
    
    [Tooltip("旋转增益初始步长")]
    public float rotationInitialStepSize = 0.05f;
    
    [Tooltip("旋转增益精细步长")]
    public float rotationFineStepSize = 0.02f;
    
    [Header("Curvature Gain Parameters")]
    [Tooltip("曲率半径初始值（米）")]
    public float curvatureInitialRadius = 6.0f;
    
    [Tooltip("曲率半径初始步长（米）")]
    public float curvatureInitialStepSize = 0.04f; // 对应2.3度/米
    
    [Tooltip("曲率半径精细步长（米）")]
    public float curvatureFineStepSize = 0.01f; // 对应0.6度/米
    
    [Header("References")]
    [Tooltip("Staircase控制器")]
    public StaircaseController staircaseController;
    
    [Tooltip("旋转增益Redirector（用于旋转条件）")]
    public ThresholdRotationRedirector rotationRedirector;
    
    [Tooltip("曲率增益Redirector（用于曲率条件）")]
    public ThresholdCurvatureRedirector curvatureRedirector;
    
    [Tooltip("数据记录器")]
    public ThresholdDataLogger dataLogger;
    
    [Tooltip("UI控制器")]
    public ThresholdUI uiController;
    
    [Tooltip("GlobalConfiguration引用")]
    public GlobalConfiguration globalConfiguration;
    
    [Tooltip("RedirectionManager引用")]
    public RedirectionManager redirectionManager;
    
    // 实验状态
    private ExperimentState currentState = ExperimentState.Waiting;
    private bool waitingForUserResponse = false;
    private float currentTrialGain = 1.0f;
    
    /// <summary>
    /// 实验条件枚举
    /// </summary>
    public enum ExperimentCondition
    {
        Rotation,    // 旋转增益
        Curvature    // 曲率增益
    }
    
    /// <summary>
    /// 实验状态枚举
    /// </summary>
    public enum ExperimentState
    {
        Waiting,           // 等待开始
        TrialInProgress,   // 试次进行中
        WaitingForResponse, // 等待用户响应
        TrialComplete,     // 试次完成
        ExperimentComplete // 实验完成
    }
    
    void Start()
    {
        InitializeExperiment();
    }
    
    /// <summary>
    /// 初始化实验
    /// </summary>
    public void InitializeExperiment()
    {
        // 初始化数据记录器
        if (dataLogger != null)
        {
            dataLogger.Initialize(participantId);
        }
        
        // 配置staircase参数
        if (staircaseController != null)
        {
            if (condition == ExperimentCondition.Rotation)
            {
                staircaseController.initialGain = rotationInitialGain;
                staircaseController.initialStepSize = rotationInitialStepSize;
                staircaseController.fineStepSize = rotationFineStepSize;
            }
            else // Curvature
            {
                // 对于曲率，我们需要将半径转换为增益值，或者直接在staircase中使用半径
                // 这里我们使用一个转换函数，或者直接使用半径值
                // 为了简化，我们可以让staircase直接使用半径值（较大的值对应较小的曲率）
                staircaseController.initialGain = curvatureInitialRadius;
                staircaseController.initialStepSize = curvatureInitialStepSize;
                staircaseController.fineStepSize = curvatureFineStepSize;
            }
            
            staircaseController.Initialize();
        }
        
        // 设置UI
        if (uiController != null)
        {
            uiController.Initialize(this);
        }
        
        currentState = ExperimentState.Waiting;
        
        Debug.Log("阈值测定实验初始化完成: " + condition.ToString());
    }
    
    /// <summary>
    /// 开始下一个试次
    /// </summary>
    public void StartNextTrial()
    {
        if (staircaseController == null || staircaseController.IsFinished())
        {
            CompleteExperiment();
            return;
        }
        
        // 获取下一个试次的增益值
        currentTrialGain = staircaseController.GetNextTrialGain();
        
        // 应用增益到对应的Redirector
        if (condition == ExperimentCondition.Rotation)
        {
            if (rotationRedirector != null)
            {
                rotationRedirector.SetRotationGain(currentTrialGain);
            }
        }
        else // Curvature
        {
            if (curvatureRedirector != null)
            {
                curvatureRedirector.SetCurvatureRadius(currentTrialGain);
                // 同时更新GlobalConfiguration（如果代码需要）
                if (globalConfiguration != null)
                {
                    globalConfiguration.CURVATURE_RADIUS = currentTrialGain;
                }
            }
        }
        
        currentState = ExperimentState.TrialInProgress;
        
        // 通知UI试次开始
        if (uiController != null)
        {
            uiController.OnTrialStarted(staircaseController.GetTrialCount() + 1, 
                staircaseController.GetStatusInfo());
        }
        
        Debug.Log(string.Format("试次 {0} 开始，增益值: {1:F4}", 
            staircaseController.GetTrialCount() + 1, currentTrialGain));
    }
    
    /// <summary>
    /// 试次完成，等待用户响应
    /// </summary>
    public void OnTrialComplete()
    {
        if (currentState != ExperimentState.TrialInProgress)
        {
            return;
        }
        
        currentState = ExperimentState.WaitingForResponse;
        waitingForUserResponse = true;
        
        // 显示响应UI
        if (uiController != null)
        {
            uiController.ShowResponseUI(condition);
        }
        
        Debug.Log("试次完成，等待用户响应");
    }
    
    /// <summary>
    /// 用户提交响应
    /// </summary>
    /// <param name="responseDetected">用户是否察觉到增益（true=察觉到，false=未察觉到）</param>
    public void SubmitUserResponse(bool responseDetected)
    {
        if (!waitingForUserResponse || staircaseController == null)
        {
            return;
        }
        
        waitingForUserResponse = false;
        
        // 判断响应是否正确
        // 对于正常trial：responseDetected=true表示用户察觉到增益（正确）
        // 对于catch trial：应该总是察觉（但我们仍然记录用户的响应）
        bool isCorrect = responseDetected;
        
        // 提交结果到staircase
        bool shouldContinue = staircaseController.SubmitTrialResult(isCorrect);
        
        // 更新UI
        if (uiController != null)
        {
            uiController.OnResponseSubmitted(isCorrect, staircaseController.GetStatusInfo());
        }
        
        Debug.Log(string.Format("用户响应: {0}, 正确: {1}", 
            responseDetected ? "察觉到" : "未察觉到", isCorrect));
        
        if (shouldContinue)
        {
            // 继续下一个试次
            currentState = ExperimentState.TrialComplete;
            StartCoroutine(WaitBeforeNextTrial(1.0f)); // 等待1秒后开始下一个试次
        }
        else
        {
            // 实验完成
            CompleteExperiment();
        }
    }
    
    /// <summary>
    /// 等待后开始下一个试次
    /// </summary>
    private IEnumerator WaitBeforeNextTrial(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartNextTrial();
    }
    
    /// <summary>
    /// 完成实验
    /// </summary>
    private void CompleteExperiment()
    {
        currentState = ExperimentState.ExperimentComplete;
        
        // 计算阈值
        float threshold = staircaseController.CalculateThreshold();
        
        // 记录数据
        if (dataLogger != null)
        {
            dataLogger.LogStaircaseData(
                condition.ToString(),
                ageGroup,
                staircaseController
            );
        }
        
        // 更新UI
        if (uiController != null)
        {
            uiController.OnExperimentComplete(threshold);
        }
        
        Debug.Log(string.Format("实验完成！条件: {0}, 阈值: {1:F6}", 
            condition.ToString(), threshold));
    }
    
    /// <summary>
    /// 获取当前状态
    /// </summary>
    public ExperimentState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 获取当前试次的增益值
    /// </summary>
    public float GetCurrentTrialGain()
    {
        return currentTrialGain;
    }
    
    /// <summary>
    /// 强制结束实验
    /// </summary>
    public void ForceEndExperiment()
    {
        if (currentState != ExperimentState.ExperimentComplete)
        {
            CompleteExperiment();
        }
    }
}