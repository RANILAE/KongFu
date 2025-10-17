using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("关卡设置")]
    public string targetScenePath; // 场景路径

    [Header("按钮状态颜色")]
    public Color unlockedColor = Color.green;
    public Color lockedColor = Color.gray;
    public Color completedColor = Color.blue;

    private Button button;
    private Image buttonImage;
    private Text buttonText;

    void Start()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogWarning($"{name}: 缺少Button组件!");
            return;
        }

        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<Text>();

        if (string.IsNullOrEmpty(targetScenePath))
        {
            Debug.LogWarning($"{name}: 未设置目标场景路径!");
            return;
        }

        // 绑定点击事件
        button.onClick.AddListener(() =>
        {
            if (LevelProgressController.Instance != null)
            {
                LevelProgressController.Instance.LoadLevel(targetScenePath);
            }
            else
            {
                // 如果没有关卡管理器，直接加载场景
                GameManager.Instance.LoadLevel(targetScenePath);
            }
        });

        // 初始化按钮状态
        UpdateButtonState();
    }

    public void UpdateButtonState()
    {
        // 如果没有关卡管理器，显示为可点击
        if (LevelProgressController.Instance == null)
        {
            SetButtonState(unlockedColor, true, "开始");
            return;
        }

        var levelInfo = LevelProgressController.Instance.GetLevelInfo(targetScenePath);

        // 如果关卡未配置，显示为可点击
        if (levelInfo == null)
        {
            SetButtonState(unlockedColor, true, "开始");
            return;
        }

        if (levelInfo.isCompleted)
        {
            SetButtonState(completedColor, false, "完成");
        }
        else if (!levelInfo.isUnlocked)
        {
            SetButtonState(lockedColor, false, "锁定");
        }
        else
        {
            SetButtonState(unlockedColor, true, "开始");
        }
    }

    private void SetButtonState(Color color, bool interactable, string text = "")
    {
        if (buttonImage != null) buttonImage.color = color;
        button.interactable = interactable;

        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }
}