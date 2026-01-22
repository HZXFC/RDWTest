using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 阈值测定实验的UI控制器
/// 实现2AFC（二选一强制选择）界面，儿童友好设计
/// </summary>
public class ThresholdUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("主Canvas")]
    public Canvas mainCanvas;
    
    [Tooltip("响应选择面板")]
    public GameObject responsePanel;
    
    [Tooltip("旋转任务 - 普通模式按钮（表情：笑脸）")]
    public Button rotationNormalButton;
    
    [Tooltip("旋转任务 - 加速模式按钮（表情：火焰）")]
    public Button rotationAcceleratedButton;
    
    [Tooltip("曲率任务 - 直线按钮（表情：铅笔）")]
    public Button curvatureStraightButton;
    
    [Tooltip("曲率任务 - 弯曲按钮（表情：香蕉）")]
    public Button curvatureCurvedButton;
    
    [Tooltip("状态信息文本")]
    public TextMeshProUGUI statusText;
    
    [Tooltip("试次信息文本")]
    public TextMeshProUGUI trialInfoText;
    
    [Tooltip("完成信息文本")]
    public TextMeshProUGUI completeText;
    
    [Header("UI Settings")]
    [Tooltip("按钮点击后隐藏UI的延迟时间（秒）")]
    public float hideUIDelay = 0.5f;
    
    private ThresholdExperimentManager experimentManager;
    private bool responseSubmitted = false;
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    public void Initialize(ThresholdExperimentManager manager)
    {
        experimentManager = manager;
        
        // 设置按钮事件
        if (rotationNormalButton != null)
        {
            rotationNormalButton.onClick.AddListener(() => OnRotationResponseSelected(false));
        }
        
        if (rotationAcceleratedButton != null)
        {
            rotationAcceleratedButton.onClick.AddListener(() => OnRotationResponseSelected(true));
        }
        
        if (curvatureStraightButton != null)
        {
            curvatureStraightButton.onClick.AddListener(() => OnCurvatureResponseSelected(false));
        }
        
        if (curvatureCurvedButton != null)
        {
            curvatureCurvedButton.onClick.AddListener(() => OnCurvatureResponseSelected(true));
        }
        
        // 初始隐藏响应面板
        if (responsePanel != null)
        {
            responsePanel.SetActive(false);
        }
        
        if (completeText != null)
        {
            completeText.gameObject.SetActive(false);
        }
        
        responseSubmitted = false;
    }
    
    /// <summary>
    /// 试次开始时的UI更新
    /// </summary>
    public void OnTrialStarted(int trialNumber, string statusInfo)
    {
        responseSubmitted = false;
        
        // 隐藏响应面板
        if (responsePanel != null)
        {
            responsePanel.SetActive(false);
        }
        
        // 更新状态信息
        if (trialInfoText != null)
        {
            trialInfoText.text = string.Format("试次 {0}", trialNumber);
        }
        
        if (statusText != null)
        {
            statusText.text = statusInfo;
        }
        
        Debug.Log("UI: 试次 " + trialNumber + " 开始");
    }
    
    /// <summary>
    /// 显示响应UI
    /// </summary>
    public void ShowResponseUI(ThresholdExperimentManager.ExperimentCondition condition)
    {
        if (responsePanel == null)
        {
            Debug.LogWarning("响应面板未设置");
            return;
        }
        
        responsePanel.SetActive(true);
        responseSubmitted = false;
        
        // 根据条件显示对应的按钮
        if (condition == ThresholdExperimentManager.ExperimentCondition.Rotation)
        {
            // 旋转任务：显示"普通"和"加速"按钮
            if (rotationNormalButton != null)
            {
                rotationNormalButton.gameObject.SetActive(true);
            }
            if (rotationAcceleratedButton != null)
            {
                rotationAcceleratedButton.gameObject.SetActive(true);
            }
            if (curvatureStraightButton != null)
            {
                curvatureStraightButton.gameObject.SetActive(false);
            }
            if (curvatureCurvedButton != null)
            {
                curvatureCurvedButton.gameObject.SetActive(false);
            }
        }
        else // Curvature
        {
            // 曲率任务：显示"直线"和"弯曲"按钮
            if (rotationNormalButton != null)
            {
                rotationNormalButton.gameObject.SetActive(false);
            }
            if (rotationAcceleratedButton != null)
            {
                rotationAcceleratedButton.gameObject.SetActive(false);
            }
            if (curvatureStraightButton != null)
            {
                curvatureStraightButton.gameObject.SetActive(true);
            }
            if (curvatureCurvedButton != null)
            {
                curvatureCurvedButton.gameObject.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// 旋转任务响应选择
    /// </summary>
    private void OnRotationResponseSelected(bool detectedAcceleration)
    {
        if (responseSubmitted || experimentManager == null)
        {
            return;
        }
        
        responseSubmitted = true;
        
        // 用户选择"加速模式"表示察觉到了旋转增益
        experimentManager.SubmitUserResponse(detectedAcceleration);
        
        // 延迟隐藏UI
        StartCoroutine(HideResponseUIAfterDelay());
    }
    
    /// <summary>
    /// 曲率任务响应选择
    /// </summary>
    private void OnCurvatureResponseSelected(bool detectedCurvature)
    {
        if (responseSubmitted || experimentManager == null)
        {
            return;
        }
        
        responseSubmitted = true;
        
        // 用户选择"弯曲"表示察觉到了曲率增益
        experimentManager.SubmitUserResponse(detectedCurvature);
        
        // 延迟隐藏UI
        StartCoroutine(HideResponseUIAfterDelay());
    }
    
    /// <summary>
    /// 响应提交后的UI更新
    /// </summary>
    public void OnResponseSubmitted(bool wasCorrect, string statusInfo)
    {
        // 更新状态信息
        if (statusText != null)
        {
            statusText.text = statusInfo;
            
            // 可选：显示反馈颜色
            // statusText.color = wasCorrect ? Color.green : Color.red;
        }
    }
    
    /// <summary>
    /// 实验完成时的UI更新
    /// </summary>
    public void OnExperimentComplete(float threshold)
    {
        if (responsePanel != null)
        {
            responsePanel.SetActive(false);
        }
        
        if (completeText != null)
        {
            completeText.gameObject.SetActive(true);
            completeText.text = string.Format("实验完成！\n检测阈值: {0:F4}", threshold);
        }
        
        if (trialInfoText != null)
        {
            trialInfoText.text = "实验已完成";
        }
        
        Debug.Log("UI: 实验完成，阈值 = " + threshold);
    }
    
    /// <summary>
    /// 延迟隐藏响应UI
    /// </summary>
    private System.Collections.IEnumerator HideResponseUIAfterDelay()
    {
        yield return new WaitForSeconds(hideUIDelay);
        
        if (responsePanel != null)
        {
            responsePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 显示消息（用于调试或提示）
    /// </summary>
    public void ShowMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log("UI Message: " + message);
    }
}