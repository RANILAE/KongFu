using UnityEngine;

public class RetainedPointsSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private WheelSystem wheelSystem;
    private float calculatedRetainedPoints = 0f; // ��ǰ�غϼ�����ı�������
    private const float MAX_RETAINED_POINTS = 3f; // ������������

    public void Initialize(PlayerManager playerManager, WheelSystem wheelSystem)
    {
        this.playerManager = playerManager;
        this.wheelSystem = wheelSystem;
        calculatedRetainedPoints = 0f;
    }

    // ���㵱ǰ�غϵı�������������һغϽ���ʱ���ã�
    public void CalculateCurrentRetainedPoints()
    {
        if (playerManager == null || wheelSystem == null) return;

        // ���㵱ǰ�غ�ʣ������������ܵ�����
        float totalPoints = wheelSystem.MaxPoints;
        float usedPoints = wheelSystem.CurrentYangPoints + wheelSystem.CurrentYinPoints;
        float remainingPoints = totalPoints - usedPoints;

        // ���㱣��������ʣ�������һ�룬����ȡ��
        float calculatedPoints = Mathf.Floor(remainingPoints / 2f);

        // �����������ܳ�������
        calculatedRetainedPoints = Mathf.Min(calculatedPoints, MAX_RETAINED_POINTS);

        Debug.Log($"Calculated current retained points: Total={totalPoints}, Used={usedPoints}, Remaining={remainingPoints}, Calculated={calculatedPoints}, Stored={calculatedRetainedPoints}");
    }

    // Ӧ�ñ������������̣����»غϿ�ʼʱ���ã�
    public void ApplyRetainedPointsToWheel()
    {
        if (calculatedRetainedPoints > 0 && wheelSystem != null)
        {
            Debug.Log($"Applying retained points to wheel: {calculatedRetainedPoints}");
            wheelSystem.AddRetainedPoints(calculatedRetainedPoints); // ��ӱ�������
        }
    }

    // ��ȡ��ǰ����ı�������
    public float GetCalculatedRetainedPoints()
    {
        return calculatedRetainedPoints;
    }

    // ���õ�ǰ����ı�������
    public void ResetCalculatedRetainedPoints()
    {
        calculatedRetainedPoints = 0f;
    }
}