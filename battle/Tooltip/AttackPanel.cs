using UnityEngine;

public class AttackPanel : MonoBehaviour
{
    private YinYangTooltipManager tooltipManager;

    void Start()
    {
        tooltipManager = FindObjectOfType<YinYangTooltipManager>();
        if (tooltipManager == null)
        {
            Debug.LogError("δ�ҵ�YinYangTooltipManager�����");
        }
    }

    void OnEnable()
    {
        // ���������ʱ��ȷ����ʾ��ȷ������
        if (tooltipManager != null)
        {
            tooltipManager.UpdateAttackPanel();
        }
    }
}