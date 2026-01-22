using UnityEngine;

/// <summary>
/// 阈值测定专用的曲率增益Redirector
/// 基于S2C算法，但精确控制曲率半径，用于曲率增益阈值测定实验
/// </summary>
public class ThresholdCurvatureRedirector : SteerToRedirector
{
    [Header("Curvature Gain Parameters")]
    [Tooltip("曲率半径（米），较小的半径产生更大的曲率")]
    public float curvatureRadius = 6.0f;
    
    [Tooltip("曲率增益上限（度/秒）")]
    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15f;
    
    [Tooltip("移动阈值（米/秒），低于此值的移动不应用曲率增益")]
    private const float MOVEMENT_THRESHOLD = 0.2f;
    
    private float lastCurvatureRadius = 6.0f;
    
    /// <summary>
    /// 从StaircaseController设置曲率半径
    /// </summary>
    public void SetCurvatureRadius(float radius)
    {
        curvatureRadius = Mathf.Max(0.1f, radius); // 确保半径不为0或负数
        lastCurvatureRadius = curvatureRadius;
        
        // 同时更新GlobalConfiguration中的CURVATURE_RADIUS（如果代码需要）
        if (globalConfiguration != null)
        {
            // 注意：这里只是临时使用，不应该永久修改GlobalConfiguration
            // 如果需要，可以添加一个临时参数来存储
        }
    }
    
    /// <summary>
    /// 获取当前曲率半径
    /// </summary>
    public float GetCurvatureRadius()
    {
        return curvatureRadius;
    }
    
    /// <summary>
    /// 将曲率半径转换为曲率增益值（度/米）
    /// </summary>
    public float RadiusToCurvatureGain(float radius)
    {
        if (radius <= 0) return float.MaxValue;
        // 曲率增益 = 1 / 半径 (弧度/米) = 180 / (π * 半径) (度/米)
        return Mathf.Rad2Deg / radius;
    }
    
    public override void PickRedirectionTarget()
    {
        // 使用S2C算法的目标选择逻辑
        Vector3 trackingAreaPosition = Utilities.FlattenedPos3D(redirectionManager.trackingSpace.position);
        Vector3 userToCenter = trackingAreaPosition - redirectionManager.currPos;

        float bearingToCenter = Vector3.Angle(userToCenter, redirectionManager.currDir);
        float directionToCenter = Mathf.Sign(Utilities.GetSignedAngle(redirectionManager.currDir, userToCenter));
        
        const float S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE = 160f;
        const float S2C_TEMP_TARGET_DISTANCE = 4f;
        
        if (bearingToCenter >= S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE)
        {
            // 生成临时目标
            if (noTmpTarget)
            {
                tmpTarget = new GameObject("ThresholdCurvature Temp Target");
                tmpTarget.transform.position = redirectionManager.currPos + S2C_TEMP_TARGET_DISTANCE * (Quaternion.Euler(0, directionToCenter * 90, 0) * redirectionManager.currDir);
                tmpTarget.transform.parent = transform;
                noTmpTarget = false;
            }
            currentTarget = tmpTarget.transform;
        }
        else
        {
            currentTarget = redirectionManager.trackingSpace;
            if (!noTmpTarget)
            {
                Destroy(tmpTarget);
                noTmpTarget = true;
            }
        }
    }
    
    public override void InjectRedirection()
    {
        PickRedirectionTarget();
        
        Vector3 deltaPos = redirectionManager.deltaPos;
        float deltaTime = redirectionManager.GetDeltaTime();
        
        // 计算曲率增益
        float rotationFromCurvatureGain = 0f;
        
        if (deltaPos.magnitude / deltaTime > MOVEMENT_THRESHOLD) // 用户正在移动
        {
            // 曲率增益 = 距离 / 半径 (弧度) = 距离 / 半径 * 180 / π (度)
            rotationFromCurvatureGain = Mathf.Rad2Deg * (deltaPos.magnitude / curvatureRadius);
            rotationFromCurvatureGain = Mathf.Min(rotationFromCurvatureGain, CURVATURE_GAIN_CAP_DEGREES_PER_SECOND * deltaTime);
            
            // 确定转向方向（S2C算法逻辑）
            Vector3 desiredFacingDirection = Utilities.FlattenedPos3D(currentTarget.position) - redirectionManager.currPos;
            int desiredSteeringDirection = (-1) * (int)Mathf.Sign(Utilities.GetSignedAngle(redirectionManager.currDir, desiredFacingDirection));
            
            float rotationProposed = desiredSteeringDirection * rotationFromCurvatureGain;
            
            // 应用曲率增益
            InjectCurvature(rotationProposed);
        }
    }
    
    /// <summary>
    /// 获取最后一次使用的曲率半径
    /// </summary>
    public float GetLastAppliedRadius()
    {
        return lastCurvatureRadius;
    }
    
    void OnDestroy()
    {
        // 清理临时目标
        if (!noTmpTarget && tmpTarget != null)
        {
            Destroy(tmpTarget);
        }
    }
}