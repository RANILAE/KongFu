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

    // ���㲢�洢��������
    public void CalculateAndStoreRetainedPoints()
    {
        float usedPoints = playerManager.Attack + playerManager.Defense;
        float remainingPoints = wheelSystem.MaxPoints - usedPoints;
        storedRetainedPoints = Mathf.Floor(remainingPoints / 2f);

        Debug.Log($"Calculated retained points: Used={usedPoints}, Remaining={remainingPoints}, Retained={storedRetainedPoints}");
    }

    // Ӧ�ô洢�ı�������
    public void ApplyStoredPoints()
    {
        if (storedRetainedPoints > 0)
        {
            Debug.Log($"Applying retained points: {storedRetainedPoints}");
            wheelSystem.IncreaseMaxPoints(storedRetainedPoints); // ʹ����ȷ�ķ�����
            playerManager.RetainedPoints = storedRetainedPoints;
        }
        storedRetainedPoints = 0f; // ���ô洢
    }

    // ���ñ�������
    public void ResetRetainedPoints()
    {
        storedRetainedPoints = 0f;
        playerManager.RetainedPoints = 0f;
        Debug.Log("Reset retained points");
    }
}