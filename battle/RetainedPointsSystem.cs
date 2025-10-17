using UnityEngine;

public class RetainedPointsSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private WheelSystem wheelSystem;
    private float storedRetainedPoints = 0f;

    public void Initialize(PlayerManager playerManager, WheelSystem wheelSystem)
    {
        this.playerManager = playerManager;
        this.wheelSystem = wheelSystem;
        storedRetainedPoints = 0f;
    }

    // 计算并存储保留点数
    public void CalculateAndStoreRetainedPoints()
    {
        float usedPoints = playerManager.Attack + playerManager.Defense;
        float remainingPoints = wheelSystem.MaxPoints - usedPoints;
        storedRetainedPoints = Mathf.Floor(remainingPoints / 2f);

        Debug.Log($"Calculated retained points: Used={usedPoints}, Remaining={remainingPoints}, Retained={storedRetainedPoints}");
    }

    // 应用存储的保留点数
    public void ApplyStoredPoints()
    {
        if (storedRetainedPoints > 0)
        {
            Debug.Log($"Applying retained points: {storedRetainedPoints}");
            wheelSystem.IncreaseMaxPoints(storedRetainedPoints); // 使用正确的方法名
            playerManager.RetainedPoints = storedRetainedPoints;
        }
        storedRetainedPoints = 0f; // 重置存储
    }

    // 重置保留点数
    public void ResetRetainedPoints()
    {
        storedRetainedPoints = 0f;
        playerManager.RetainedPoints = 0f;
        Debug.Log("Reset retained points");
    }
}