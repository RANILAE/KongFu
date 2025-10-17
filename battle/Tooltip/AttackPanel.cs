using UnityEngine;

public class AttackPanel : MonoBehaviour
{
    private YinYangTooltipManager tooltipManager;

    void Start()
    {
        tooltipManager = FindObjectOfType<YinYangTooltipManager>();
        if (tooltipManager == null)
        {
            Debug.LogError("未找到YinYangTooltipManager组件！");
        }
    }

    void OnEnable()
    {
        // 当面板启用时，确保显示正确的内容
        if (tooltipManager != null)
        {
            tooltipManager.UpdateAttackPanel();
        }
    }
}