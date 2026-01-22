using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 阈值测定实验的数据记录器
/// 扩展StatisticsLogger，专门记录staircase实验数据
/// </summary>
public class ThresholdDataLogger : MonoBehaviour
{
    [Header("Data Logging Settings")]
    [Tooltip("数据保存目录（相对于项目根目录）")]
    public string dataDirectory = "ThresholdExperimentResults";
    
    private string experimentDirectory;
    private string participantId;
    private System.DateTime experimentStartTime;
    
    // Staircase数据记录
    private List<StaircaseData> staircaseDataList = new List<StaircaseData>();
    
    /// <summary>
    /// Staircase数据结构
    /// </summary>
    [System.Serializable]
    public class StaircaseData
    {
        public string conditionName; // "Rotation" or "Curvature"
        public string participantId;
        public int ageGroup; // 0 = 儿童 (8-10岁), 1 = 成人 (20-25岁)
        public float threshold;
        public List<StaircaseController.TrialData> trialHistory;
        public List<float> reversalPoints;
        public int totalTrials;
        public int totalReversals;
        public System.DateTime timestamp;
    }
    
    /// <summary>
    /// 初始化数据记录器
    /// </summary>
    public void Initialize(string participantID)
    {
        participantId = participantID;
        experimentStartTime = System.DateTime.Now;
        
        // 创建实验目录
        string basePath = Application.dataPath + "/../" + dataDirectory;
        experimentDirectory = Path.Combine(basePath, participantId + "_" + experimentStartTime.ToString("yyyyMMdd_HHmmss"));
        
        if (!Directory.Exists(experimentDirectory))
        {
            Directory.CreateDirectory(experimentDirectory);
        }
        
        Debug.Log("数据记录器初始化完成，目录: " + experimentDirectory);
    }
    
    /// <summary>
    /// 记录一个staircase实验的数据
    /// </summary>
    public void LogStaircaseData(string conditionName, int ageGroup, StaircaseController staircase)
    {
        StaircaseData data = new StaircaseData
        {
            conditionName = conditionName,
            participantId = participantId,
            ageGroup = ageGroup,
            threshold = staircase.CalculateThreshold(),
            trialHistory = staircase.GetTrialHistory(),
            reversalPoints = staircase.GetReversalPoints(),
            totalTrials = staircase.GetTrialCount(),
            totalReversals = staircase.GetReversalCount(),
            timestamp = System.DateTime.Now
        };
        
        staircaseDataList.Add(data);
        
        // 立即保存到CSV
        SaveStaircaseDataToCSV(data);
        
        Debug.Log(string.Format("Staircase数据已记录: {0}, 阈值: {1:F4}, 试次: {2}, 反转: {3}", 
            conditionName, data.threshold, data.totalTrials, data.totalReversals));
    }
    
    /// <summary>
    /// 保存单个staircase数据到CSV文件
    /// </summary>
    private void SaveStaircaseDataToCSV(StaircaseData data)
    {
        string fileName = string.Format("{0}_{1}_staircase.csv", 
            participantId, data.conditionName);
        string filePath = Path.Combine(experimentDirectory, fileName);
        
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // 写入头部信息
            writer.WriteLine("Participant ID:," + data.participantId);
            writer.WriteLine("Condition:," + data.conditionName);
            writer.WriteLine("Age Group:," + (data.ageGroup == 0 ? "Child (8-10)" : "Adult (20-25)"));
            writer.WriteLine("Threshold:," + data.threshold.ToString("F6"));
            writer.WriteLine("Total Trials:," + data.totalTrials);
            writer.WriteLine("Total Reversals:," + data.totalReversals);
            writer.WriteLine("Timestamp:," + data.timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            writer.WriteLine();
            
            // 写入试次数据
            writer.WriteLine("Trial Data");
            writer.WriteLine("Trial Number,Gain Value,Is Catch Trial,Response Correct,Was Reversal,Step Size");
            
            foreach (var trial in data.trialHistory)
            {
                writer.WriteLine(string.Format("{0},{1:F6},{2},{3},{4},{5:F6}",
                    trial.trialNumber,
                    trial.gainValue,
                    trial.isCatchTrial ? "Yes" : "No",
                    trial.responseCorrect ? "Yes" : "No",
                    trial.wasReversal ? "Yes" : "No",
                    trial.stepSizeUsed));
            }
            
            writer.WriteLine();
            writer.WriteLine("Reversal Points");
            foreach (float point in data.reversalPoints)
            {
                writer.WriteLine(point.ToString("F6"));
            }
        }
        
        Debug.Log("数据已保存到: " + filePath);
    }
    
    /// <summary>
    /// 保存汇总数据（所有条件）
    /// </summary>
    public void SaveSummaryData()
    {
        string fileName = participantId + "_summary.csv";
        string filePath = Path.Combine(experimentDirectory, fileName);
        
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Participant ID:," + participantId);
            writer.WriteLine("Experiment Date:," + experimentStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
            writer.WriteLine();
            writer.WriteLine("Condition,Age Group,Threshold,Total Trials,Total Reversals");
            
            foreach (var data in staircaseDataList)
            {
                writer.WriteLine(string.Format("{0},{1},{2:F6},{3},{4}",
                    data.conditionName,
                    data.ageGroup == 0 ? "Child" : "Adult",
                    data.threshold,
                    data.totalTrials,
                    data.totalReversals));
            }
        }
        
        Debug.Log("汇总数据已保存到: " + filePath);
    }
    
    /// <summary>
    /// 获取数据目录路径
    /// </summary>
    public string GetDataDirectory()
    {
        return experimentDirectory;
    }
}