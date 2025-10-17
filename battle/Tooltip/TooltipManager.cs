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
            Debug.LogError("未找到TIPUI组件！");
            return;
        }
    }

    // 注意：这里移除了position参数，因为我们使用预设位置
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