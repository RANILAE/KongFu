using UnityEngine;

public class RetainedPointsSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private WheelSystem wheelSystem;
    private float calculatedRetainedPoints = 0f; // 当前回合计算出的保留点数
    private const float MAX_RETAINED_POINTS = 3f; // 保留点数上限

    public void Initialize(PlayerManager playerManager, WheelSystem wheelSystem)
    {
        this.playerManager = playerManager;
        this.wheelSystem = wheelSystem;
        calculatedRetainedPoints = 0f;
    }

    // 计算当前回合的保留点数（在玩家回合结束时调用）
    public void CalculateCurrentRetainedPoints()
    {
        if (playerManager == null || wheelSystem == null) return;

        // 计算当前回合剩余点数（基于总点数）
        float totalPoints = wheelSystem.MaxPoints;
        float usedPoints = wheelSystem.CurrentYangPoints + wheelSystem.CurrentYinPoints;
        float remainingPoints = totalPoints - usedPoints;

        // 计算保留点数：剩余点数的一半，向下取整
        float calculatedPoints = Mathf.Floor(remainingPoints / 2f);

        // 保留点数不能超过上限
        calculatedRetainedPoints = Mathf.Min(calculatedPoints, MAX_RETAINED_POINTS);

        Debug.Log($"Calculated current retained points: Total={totalPoints}, Used={usedPoints}, Remaining={remainingPoints}, Calculated={calculatedPoints}, Stored={calculatedRetainedPoints}");
    }

    // 应用保留点数到轮盘（在新回合开始时调用）
    public void ApplyRetainedPointsToWheel()
    {
        if (calculatedRetainedPoints > 0 && wheelSystem != null)
        {
            Debug.Log($"Applying retained points to wheel: {calculatedRetainedPoints}");
            wheelSystem.AddRetainedPoints(calculatedRetainedPoints); // 添加保留点数
        }
    }

    // 获取当前计算的保留点数
    public float GetCalculatedRetainedPoints()
    {
        return calculatedRetainedPoints;
    }

    // 重置当前计算的保留点数
    public void ResetCalculatedRetainedPoints()
    {
        calculatedRetainedPoints = 0f;
    }
}