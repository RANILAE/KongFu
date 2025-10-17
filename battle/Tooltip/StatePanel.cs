using UnityEngine;

public class StatePanel : MonoBehaviour
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
            tooltipManager.UpdateStatePanel();
        }
    }
}