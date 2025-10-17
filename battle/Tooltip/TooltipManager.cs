using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    [Header("Tooltip UI")]
    public TIPUI tipUI;

    void Start()
    {
        if (tipUI == null)
        {
            tipUI = FindObjectOfType<TIPUI>();
        }

        if (tipUI == null)
        {
            Debug.LogError("δ�ҵ�TIPUI�����");
            return;
        }
    }

    // ע�⣺�����Ƴ���position��������Ϊ����ʹ��Ԥ��λ��
    public void ShowTooltipAtPosition(string title, string description)
    {
        tipUI.ShowTooltip(title, description);
    }

    public void HideTooltip()
    {
        tipUI.HideTooltip();
    }

    public void UpdateCurrentTooltip(string title, string description)
    {
        tipUI.UpdateTooltipContent(title, description);
    }
}