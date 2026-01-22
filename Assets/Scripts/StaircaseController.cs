using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 实现1-up/2-down自适应阶梯法
/// 收敛于70.7%的探测概率阈值点
/// </summary>
public class StaircaseController : MonoBehaviour
{
    [Header("Staircase Parameters")]
    [Tooltip("初始增益值")]
    public float initialGain = 1.5f;
    
    [Tooltip("初始步长（较粗糙阶段）")]
    public float initialStepSize = 0.05f;
    
    [Tooltip("精细步长（收敛阶段）")]
    public float fineStepSize = 0.02f;
    
    [Tooltip("切换到精细步长所需的反转次数")]
    public int reversalsForFineStep = 2;
    
    [Tooltip("最大试次数量")]
    public int maxTrials = 25;
    
    [Tooltip("最小反转次数（达到此数量后可以终止）")]
    public int minReversals = 6;
    
    [Tooltip("Catch trial的概率（0-1）")]
    [Range(0f, 1f)]
    public float catchTrialProbability = 0.1f;
    
    [Tooltip("Catch trial使用的增益值（应该很容易察觉）")]
    public float catchTrialGain = 2.0f;
    
    // 当前状态
    private float currentGain;
    private float currentStepSize;
    private int trialCount = 0;
    private int reversalCount = 0;
    private int correctCount = 0; // 连续正确次数（用于2-down规则）
    private bool isCatchTrial = false;
    private bool isFinished = false;
    
    // 历史记录
    private List<TrialData> trialHistory = new List<TrialData>();
    
    // 反转点记录（用于计算阈值）
    private List<float> reversalPoints = new List<float>();
    
    /// <summary>
    /// 试次数据
    /// </summary>
    [System.Serializable]
    public class TrialData
    {
        public int trialNumber;
        public float gainValue;
        public bool isCatchTrial;
        public bool responseCorrect; // 用户是否察觉到增益
        public bool wasReversal;
        public float stepSizeUsed;
        
        public TrialData(int trialNum, float gain, bool catchTrial, bool correct, bool reversal, float step)
        {
            trialNumber = trialNum;
            gainValue = gain;
            isCatchTrial = catchTrial;
            responseCorrect = correct;
            wasReversal = reversal;
            stepSizeUsed = step;
        }
    }
    
    void Start()
    {
        Initialize();
    }
    
    /// <summary>
    /// 初始化staircase
    /// </summary>
    public void Initialize()
    {
        currentGain = initialGain;
        currentStepSize = initialStepSize;
        trialCount = 0;
        reversalCount = 0;
        correctCount = 0;
        isFinished = false;
        trialHistory.Clear();
        reversalPoints.Clear();
    }
    
    /// <summary>
    /// 获取下一个试次的增益值
    /// </summary>
    public float GetNextTrialGain()
    {
        if (isFinished)
        {
            Debug.LogWarning("Staircase已经完成，无法获取新的试次增益");
            return currentGain;
        }
        
        // 判断是否为catch trial
        isCatchTrial = Random.Range(0f, 1f) < catchTrialProbability;
        
        if (isCatchTrial)
        {
            return catchTrialGain;
        }
        
        return currentGain;
    }
    
    /// <summary>
    /// 提交试次结果并更新staircase
    /// </summary>
    /// <param name="responseCorrect">用户是否察觉到增益（对于catch trial，应该总是true）</param>
    /// <returns>是否应该继续实验</returns>
    public bool SubmitTrialResult(bool responseCorrect)
    {
        if (isFinished)
        {
            Debug.LogWarning("Staircase已经完成，无法提交新的结果");
            return false;
        }
        
        trialCount++;
        bool wasReversal = false;
        float previousGain = currentGain;
        
        // Catch trial处理：catch trial不计入staircase逻辑
        if (isCatchTrial)
        {
            // Catch trial只是用来检测用户是否在认真作答
            // 不计入staircase逻辑，gain值保持不变
            trialHistory.Add(new TrialData(trialCount, catchTrialGain, true, responseCorrect, false, 0));
            
            // 检查是否应该终止
            if (ShouldTerminate())
            {
                isFinished = true;
                return false;
            }
            return true; // 继续实验
        }
        
        // 正常trial的1-up/2-down规则
        if (responseCorrect)
        {
            correctCount++;
            if (correctCount >= 2) // 2-down: 连续2次正确，减小增益（变难）
            {
                float oldGain = currentGain;
                currentGain -= currentStepSize;
                
                // 检查是否发生反转（从增大变为减小，或从减小变为增大）
                if (trialHistory.Count > 0)
                {
                    var lastNormalTrial = GetLastNormalTrial();
                    if (lastNormalTrial != null && lastNormalTrial.responseCorrect == false)
                    {
                        // 发生了反转：上次是错误（增益增大），这次是正确（增益减小）
                        wasReversal = true;
                        reversalCount++;
                        reversalPoints.Add((oldGain + currentGain) / 2f); // 记录反转点
                        
                        // 切换到精细步长
                        if (reversalCount >= reversalsForFineStep)
                        {
                            currentStepSize = fineStepSize;
                        }
                    }
                }
                
                correctCount = 0;
            }
        }
        else // 1-up: 1次错误，增大增益（变易）
        {
            float oldGain = currentGain;
            currentGain += currentStepSize;
            
            // 检查是否发生反转
            if (trialHistory.Count > 0)
            {
                var lastNormalTrial = GetLastNormalTrial();
                if (lastNormalTrial != null && lastNormalTrial.responseCorrect == true)
                {
                    // 发生了反转：上次是正确（增益减小），这次是错误（增益增大）
                    wasReversal = true;
                    reversalCount++;
                    reversalPoints.Add((oldGain + currentGain) / 2f);
                    
                    // 切换到精细步长
                    if (reversalCount >= reversalsForFineStep)
                    {
                        currentStepSize = fineStepSize;
                    }
                }
            }
            
            correctCount = 0;
        }
        
        // 记录试次数据
        trialHistory.Add(new TrialData(trialCount, previousGain, false, responseCorrect, wasReversal, currentStepSize));
        
        // 检查是否应该终止
        if (ShouldTerminate())
        {
            isFinished = true;
            return false;
        }
        
        return true; // 继续实验
    }
    
    /// <summary>
    /// 获取最后一个非catch trial的数据
    /// </summary>
    private TrialData GetLastNormalTrial()
    {
        for (int i = trialHistory.Count - 1; i >= 0; i--)
        {
            if (!trialHistory[i].isCatchTrial)
            {
                return trialHistory[i];
            }
        }
        return null;
    }
    
    /// <summary>
    /// 判断是否应该终止实验
    /// </summary>
    private bool ShouldTerminate()
    {
        // 达到最大试次数量
        if (trialCount >= maxTrials)
        {
            return true;
        }
        
        // 达到最小反转次数
        if (reversalCount >= minReversals)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 计算检测阈值（使用反转点的平均值）
    /// </summary>
    public float CalculateThreshold()
    {
        if (reversalPoints.Count == 0)
        {
            Debug.LogWarning("没有足够的反转点来计算阈值");
            return currentGain;
        }
        
        // 排除前2个反转点（不稳定阶段），计算剩余反转点的平均值
        if (reversalPoints.Count <= 2)
        {
            float sum = 0f;
            foreach (float point in reversalPoints)
            {
                sum += point;
            }
            return sum / reversalPoints.Count;
        }
        
        // 排除前2个反转点
        float thresholdSum = 0f;
        for (int i = 2; i < reversalPoints.Count; i++)
        {
            thresholdSum += reversalPoints[i];
        }
        return thresholdSum / (reversalPoints.Count - 2);
    }
    
    /// <summary>
    /// 获取当前状态信息
    /// </summary>
    public string GetStatusInfo()
    {
        return string.Format("试次: {0}/{1}, 反转: {2}, 当前增益: {3:F3}, 步长: {4:F3}", 
            trialCount, maxTrials, reversalCount, currentGain, currentStepSize);
    }
    
    /// <summary>
    /// 获取所有试次历史
    /// </summary>
    public List<TrialData> GetTrialHistory()
    {
        return new List<TrialData>(trialHistory);
    }
    
    /// <summary>
    /// 获取反转点列表
    /// </summary>
    public List<float> GetReversalPoints()
    {
        return new List<float>(reversalPoints);
    }
    
    /// <summary>
    /// 检查是否已完成
    /// </summary>
    public bool IsFinished()
    {
        return isFinished;
    }
    
    /// <summary>
    /// 获取当前试次计数
    /// </summary>
    public int GetTrialCount()
    {
        return trialCount;
    }
    
    /// <summary>
    /// 获取反转次数
    /// </summary>
    public int GetReversalCount()
    {
        return reversalCount;
    }
}