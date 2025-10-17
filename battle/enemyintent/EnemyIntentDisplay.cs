using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // 必需命名空间

/// <summary>
/// 敌人意图显示组件 - 挂载在UI上，显示敌人下回合的行动意图
/// 使用 EventTrigger 实现鼠标悬停，兼容所有 Canvas Render Mode
/// </summary>
[RequireComponent(typeof(EventTrigger))]
public class EnemyIntentDisplay : MonoBehaviour
{
    [System.Serializable]
    public class EnemyIntentTooltips
    {
        [Header("Attack Tooltip")]
        [TextArea(2, 4)] public string attackTooltip = "进攻：敌人将对你造成伤害";

        [Header("Defend Tooltip")]
        [TextArea(2, 4)] public string defendTooltip = "防御：敌人将减少或免疫下一次伤害";

        [Header("Charge Tooltip")]
        [TextArea(2, 4)] public string chargeTooltip = "蓄力：敌人将强化自身能力";
    }

    [Header("UI References")]
    public Image intentIcon; // 显示意图图标的Image组件
    public GameObject tooltipPanel; // 鼠标悬停时显示的提示面板
    public TMP_Text tooltipText; // 提示文字（TextMeshPro）

    [Header("Intent Icons - 请在Inspector中分配")]
    public Sprite attackIcon;   // 进攻图标
    public Sprite defendIcon;   // 防御图标
    public Sprite chargeIcon;   // 蓄力图标

    [Header("Intent Tooltips for Different Enemies")]
    public EnemyIntentTooltips defaultEnemyTooltips;
    public EnemyIntentTooltips secondEnemyTooltips;
    public EnemyIntentTooltips thirdEnemyTooltips;

    private EventTrigger eventTrigger;
    private EnemyManager.EnemyAction.Type currentIntentType = EnemyManager.EnemyAction.Type.Attack; // 初始化为默认值
    private EnemyManager.EnemyType currentEnemyType = EnemyManager.EnemyType.Default; // 初始化为默认敌人类型

    private void Start()
    {
        // 初始化 EventTrigger
        InitializeEventTrigger();

        // 确保初始状态：隐藏图标和Tooltip
        if (intentIcon != null && intentIcon.gameObject != null)
        {
            intentIcon.gameObject.SetActive(false); // 隐藏图标 GameObject
        }
        else
        {
            Debug.LogWarning("EnemyIntentDisplay: intentIcon or its GameObject is null on Start.");
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("EnemyIntentDisplay: TooltipPanel未分配");
        }

        // 检查图标资源
        if (attackIcon == null || defendIcon == null || chargeIcon == null)
        {
            Debug.LogWarning("EnemyIntentDisplay: 请在Inspector中分配所有意图图标");
        }
    }

    private void InitializeEventTrigger()
    {
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        // 清空已有事件（避免重复添加）
        eventTrigger.triggers.Clear();

        // 添加 PointerEnter 事件（鼠标进入）
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnPointerEnterHandler((PointerEventData)data); }); // 明确转换数据类型
        eventTrigger.triggers.Add(enterEntry);

        // 添加 PointerExit 事件（鼠标离开）
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnPointerExitHandler((PointerEventData)data); }); // 明确转换数据类型
        eventTrigger.triggers.Add(exitEntry);
    }

    /// <summary>
    /// 更新显示的敌人意图 (指定敌人类型以获取正确文本)
    /// </summary>
    /// <param name="intentType">敌人的行动类型</param>
    /// <param name="enemyType">敌人的类型</param>
    public void UpdateIntent(EnemyManager.EnemyAction.Type intentType, EnemyManager.EnemyType enemyType)
    {
        Debug.Log($"<color=green>EnemyIntentDisplay: UpdateIntent called with Type: {intentType}, Enemy: {enemyType}</color>");
        currentIntentType = intentType;
        currentEnemyType = enemyType;

        // 强制更新图标和文本，然后显示
        if (intentIcon != null && intentIcon.gameObject != null)
        {
            Sprite targetSprite = attackIcon; // 默认图标
            string targetTooltip = "未知意图"; // 默认文本

            // 选择对应的文本配置
            EnemyIntentTooltips tooltipsToUse = defaultEnemyTooltips;
            switch (enemyType)
            {
                case EnemyManager.EnemyType.Second:
                    tooltipsToUse = secondEnemyTooltips;
                    Debug.Log("EnemyIntentDisplay: Using tooltips for Second Enemy.");
                    break;
                case EnemyManager.EnemyType.Third:
                    tooltipsToUse = thirdEnemyTooltips;
                    Debug.Log("EnemyIntentDisplay: Using tooltips for Third Enemy.");
                    break;
                case EnemyManager.EnemyType.Default:
                default:
                    tooltipsToUse = defaultEnemyTooltips;
                    Debug.Log("EnemyIntentDisplay: Using tooltips for Default Enemy.");
                    break;
            }

            // 根据意图类型选择图标和文本
            switch (intentType)
            {
                case EnemyManager.EnemyAction.Type.Attack:
                    targetSprite = attackIcon;
                    targetTooltip = tooltipsToUse.attackTooltip;
                    Debug.Log("EnemyIntentDisplay: Selected Attack icon and tooltip.");
                    break;
                case EnemyManager.EnemyAction.Type.Defend:
                    targetSprite = defendIcon;
                    targetTooltip = tooltipsToUse.defendTooltip;
                    Debug.Log("EnemyIntentDisplay: Selected Defend icon and tooltip.");
                    break;
                case EnemyManager.EnemyAction.Type.Charge:
                    targetSprite = chargeIcon;
                    targetTooltip = tooltipsToUse.chargeTooltip;
                    Debug.Log("EnemyIntentDisplay: Selected Charge icon and tooltip.");
                    break;
                default:
                    // 保持默认值
                    Debug.LogWarning($"EnemyIntentDisplay: Unknown intent type {intentType}, using Attack as default.");
                    break;
            }

            // 1. 设置图标
            intentIcon.sprite = targetSprite;
            // 2. 设置 Tooltip 文本
            if (tooltipText != null)
            {
                tooltipText.text = targetTooltip;
                Debug.Log($"EnemyIntentDisplay: Tooltip text set to '{targetTooltip}'");
            }
            else
            {
                Debug.LogWarning("EnemyIntentDisplay: tooltipText is null, cannot set tooltip.");
            }
            // 3. 显示图标 GameObject
            intentIcon.gameObject.SetActive(true);
            Debug.Log($"EnemyIntentDisplay: Icon GameObject activated. Sprite assigned: {(intentIcon.sprite != null ? intentIcon.sprite.name : "NULL")}");
        }
        else
        {
            Debug.LogError("EnemyIntentDisplay: Cannot update intent because intentIcon or its GameObject is null.");
        }
    }

    /// <summary>
    /// 清除意图显示
    /// </summary>
    public void ClearIntent()
    {
        if (intentIcon != null && intentIcon.gameObject != null)
        {
            intentIcon.gameObject.SetActive(false);
            Debug.Log("EnemyIntentDisplay: Intent icon cleared and hidden.");
        }
        currentIntentType = EnemyManager.EnemyAction.Type.Attack; // 重置为默认值
        currentEnemyType = EnemyManager.EnemyType.Default; // 重置为默认敌人
        Debug.Log("EnemyIntentDisplay: Intent type and enemy type reset to defaults.");
    }

    /// <summary>
    /// 获取当前显示的意图类型
    /// </summary>
    public EnemyManager.EnemyAction.Type GetCurrentIntent()
    {
        return currentIntentType;
    }

    /// <summary>
    /// 获取当前显示的敌人类型
    /// </summary>
    public EnemyManager.EnemyType GetCurrentEnemyType()
    {
        return currentEnemyType;
    }

    /// <summary>
    /// 鼠标进入时显示Tooltip（由EventTrigger调用）
    /// </summary>
    private void OnPointerEnterHandler(PointerEventData eventData) // 明确参数类型
    {
        Debug.Log("Mouse entered enemy intent icon (EventTrigger)");
        // 只有在图标可见时才显示 Tooltip
        if (tooltipPanel != null && intentIcon != null && intentIcon.gameObject.activeSelf)
        {
            tooltipPanel.SetActive(true);
            Debug.Log("EnemyIntentDisplay: Tooltip panel activated.");
        }
        else
        {
            Debug.Log($"EnemyIntentDisplay: Tooltip NOT shown. Panel Null: {tooltipPanel == null}, Icon Null: {intentIcon == null}, Icon Active: {(intentIcon != null ? intentIcon.gameObject.activeSelf.ToString() : "N/A")}");
        }
    }

    /// <summary>
    /// 鼠标离开时隐藏Tooltip（由EventTrigger调用）
    /// </summary>
    private void OnPointerExitHandler(PointerEventData eventData) // 明确参数类型
    {
        Debug.Log("Mouse exited enemy intent icon (EventTrigger)");
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            Debug.Log("EnemyIntentDisplay: Tooltip panel deactivated.");
        }
    }
}