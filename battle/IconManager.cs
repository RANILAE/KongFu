using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 添加此引用以支持EventTrigger
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 代表一个图标槽位的数据结构。
/// </summary>
[System.Serializable]
public class IconSlot
{
    [Header("UI References")]
    [Tooltip("The Image component used to display the icon.")]
    public Image iconImage; // 图标图片

    [Tooltip("The TMP_Text component used to display the stack count or duration.")]
    public TMP_Text stackOrDurationText; // 显示叠层数量或持续时间

    [Header("Configuration")]
    [Tooltip("The type of icon this slot is designated for. Must match the IconType enum.")]
    public IconManager.IconType iconType; // 此槽位关联的图标类型

    // 注意：移除了 slot.tooltipPanel, slot.tooltipTitle, slot.tooltipDescription 的引用
    // 我们将使用 IconManager 类中定义的统一 Tooltip 引用

    [HideInInspector] public bool isActive = false; // 是否激活
    [HideInInspector] public float value; // 当前值（叠层数/持续时间/强度）
    [HideInInspector] public int duration; // 当前持续时间
}

/// <summary>
/// 管理战斗中所有图标（如 Buff, Debuff, Intent 等）的显示和交互。
/// </summary>
public class IconManager : MonoBehaviour
{
    /// <summary>
    /// 定义所有项目中实际使用的图标类型。
    /// </summary>
    public enum IconType
    {
        // --- Player Icons ---
        YangStack,              // Yang Stack (阳穿透叠层)
        YinStack,               // Yin Stack (阴覆盖叠层)
        CounterStrike,          // Counter Strike (反震激活)
        PlayerDot,              // Player DOT (玩家DOT伤害)
        AttackDebuff,           // Attack Debuff (攻击力下降Debuff)
        DefenseDebuff,           // Defense Debuff (防御力下降Debuff)
        BalanceHealCD,           // Balance Heal CD (平衡回血CD)

        // --- Enemy Icons ---
        // 注意：移除了 AttackIntent, DefendIntent, ChargeIntent
        // 改为使用一个统一的意图图标
        EnemyIntent,            // Enemy Intent (敌人意图 - 统一图标)
        EnemyDot,               // Enemy DOT (敌人DOT伤害)
        YangPenetration,        // Yang Penetration (阳穿透效果/层数)
        YinCover,               // Yin Cover (阴覆盖效果/层数)

        // --- Specific Debuff Types (为了解决 EffectManager.cs 中的编译错误) ---
        ExtremeYangDebuff_AttackDown,  // 极端阳攻击减半Debuff
        EnemyDebuff_DefenseDown        // 敌人防御下降Debuff
    }

    [Header("Player Icon Slots")]
    [Tooltip("List of icon slots available for displaying player-related icons.")]
    public List<IconSlot> playerIconSlots = new List<IconSlot>();

    [Header("Enemy Icon Slots")]
    [Tooltip("List of icon slots available for displaying enemy-related icons.")]
    public List<IconSlot> enemyIconSlots = new List<IconSlot>();

    // 内部字典用于快速查找和管理已激活的图标
    private Dictionary<IconType, IconSlot> activePlayerIcons = new Dictionary<IconType, IconSlot>();
    private Dictionary<IconType, IconSlot> activeEnemyIcons = new Dictionary<IconType, IconSlot>();

    // --- 统一 Tooltip 引用 ---
    [Header("Unified Tooltip Panel")]
    [Tooltip("The single GameObject that serves as the container for all icon tooltips.")]
    public GameObject tooltipPanel;

    [Tooltip("The TMP_Text component within the tooltip panel for the title.")]
    public TMP_Text tooltipTitle;

    [Tooltip("The TMP_Text component within the tooltip panel for the description.")]
    public TMP_Text tooltipDescription;
    // --- 统一 Tooltip 引用结束 ---

    // 当前显示Tooltip的图标槽 (用于OnPointerExit判断)
    private IconSlot currentTooltipSlot = null;

    void Awake()
    {
        // 初始化图标槽字典和UI状态
        InitializeIconSlots(playerIconSlots, activePlayerIcons);
        InitializeIconSlots(enemyIconSlots, activeEnemyIcons);

        // 初始化统一Tooltip面板为隐藏
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 初始化图标槽列表，设置初始状态和事件监听器
    /// </summary>
    private void InitializeIconSlots(List<IconSlot> slots, Dictionary<IconType, IconSlot> activeIcons)
    {
        foreach (var slot in slots)
        {
            if (slot.iconImage != null)
            {
                slot.iconImage.gameObject.SetActive(false);

                // --- 为图标图片添加 EventTrigger 以便处理鼠标悬停 ---
                // 确保有EventTrigger组件
                EventTrigger trigger = slot.iconImage.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = slot.iconImage.gameObject.AddComponent<EventTrigger>();
                }

                // 移除旧的监听器（如果有的话），防止重复添加
                trigger.triggers.Clear();

                // 添加PointerEnter事件
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                // 使用匿名方法捕获当前 slot 实例
                enterEntry.callback.AddListener((data) => { OnPointerEnter(slot); });
                trigger.triggers.Add(enterEntry);

                // 添加PointerExit事件
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                // 使用匿名方法捕获当前 slot 实例
                exitEntry.callback.AddListener((data) => { OnPointerExit(slot); });
                trigger.triggers.Add(exitEntry);
                // --- EventTrigger 添加结束 ---
            }

            slot.isActive = false;
            slot.value = 0f;
            slot.duration = 0;

            if (slot.stackOrDurationText != null)
            {
                slot.stackOrDurationText.text = "";
            }

            // 注意：不再需要初始化 slot.tooltipPanel, slot.tooltipTitle, slot.tooltipDescription
            // 因为我们使用统一的引用
        }
        // 注意：activeIcons 字典将在第一次调用 AddPlayerIcon/AddEnemyIcon 时填充
    }

    #region Player Icon Management

    /// <summary>
    /// 添加或更新一个玩家图标。
    /// </summary>
    public void AddPlayerIcon(IconType type, float value = 0, int duration = 0)
    {
        UpdateIcon(activePlayerIcons, playerIconSlots, type, value, duration, true);
    }

    /// <summary>
    /// 移除一个玩家图标。
    /// </summary>
    public void RemovePlayerIcon(IconType type)
    {
        RemoveIcon(activePlayerIcons, playerIconSlots, type, true);
    }

    #endregion

    #region Enemy Icon Management

    /// <summary>
    /// 添加或更新一个敌人图标。
    /// </summary>
    public void AddEnemyIcon(IconType type, float value = 0, int duration = 0)
    {
        // 特殊处理：敌人意图图标
        if (type == IconType.EnemyIntent)
        {
            // 对于意图图标，我们可能只想显示类型，而不关心 value 和 duration
            // 但为了保持一致性，我们仍然传递它们
            UpdateIcon(activeEnemyIcons, enemyIconSlots, type, value, duration, false);
        }
        else
        {
            UpdateIcon(activeEnemyIcons, enemyIconSlots, type, value, duration, false);
        }
    }

    /// <summary>
    /// 移除一个敌人图标。
    /// </summary>
    public void RemoveEnemyIcon(IconType type)
    {
        RemoveIcon(activeEnemyIcons, enemyIconSlots, type, false);
    }

    #endregion

    #region Core Icon Logic

    /// <summary>
    /// 通用方法：更新或添加一个图标
    /// </summary>
    private void UpdateIcon(Dictionary<IconType, IconSlot> activeIcons, List<IconSlot> slots, IconType type, float value, int duration, bool isPlayer)
    {
        IconSlot slot;

        // 检查图标是否已存在并激活
        if (activeIcons.TryGetValue(type, out slot))
        {
            // 如果存在，更新其数据
            slot.isActive = true;
            if (slot.iconImage != null)
            {
                slot.iconImage.gameObject.SetActive(true);
            }
            slot.value = value;
            slot.duration = duration;

            // 更新显示文本
            UpdateIconText(slot, value, duration);
        }
        else
        {
            // 如果不存在，寻找一个匹配类型且未被使用的槽位
            slot = FindAvailableSlot(slots, type);
            if (slot != null)
            {
                slot.isActive = true;
                // iconType 在Inspector中已设置，不需要运行时更改
                if (slot.iconImage != null)
                {
                    slot.iconImage.gameObject.SetActive(true);
                }
                slot.value = value;
                slot.duration = duration;

                // 更新显示文本
                UpdateIconText(slot, value, duration);

                // 将新激活的槽位加入字典
                activeIcons.Add(type, slot);
            }
            else
            {
                Debug.LogWarning($"No available icon slot found for {type} on {(isPlayer ? "Player" : "Enemy")}. Consider adding more slots in the inspector.");
            }
        }
    }

    /// <summary>
    /// 通用方法：移除一个图标
    /// </summary>
    private void RemoveIcon(Dictionary<IconType, IconSlot> activeIcons, List<IconSlot> slots, IconType type, bool isPlayer)
    {
        IconSlot slot;
        if (activeIcons.TryGetValue(type, out slot))
        {
            slot.isActive = false;
            if (slot.iconImage != null)
            {
                slot.iconImage.gameObject.SetActive(false);
            }
            slot.value = 0f;
            slot.duration = 0;
            if (slot.stackOrDurationText != null)
            {
                slot.stackOrDurationText.text = "";
            }

            // 如果当前Tooltip正显示在此图标上，则隐藏它
            if (currentTooltipSlot == slot)
            {
                HideUnifiedTooltip();
            }

            // 从激活字典中移除
            activeIcons.Remove(type);
        }
        // 如果图标不存在于激活字典中，说明它本来就未显示，无需操作。
    }

    /// <summary>
    /// 寻找一个指定类型且未被使用的图标槽
    /// </summary>
    private IconSlot FindAvailableSlot(List<IconSlot> slots, IconType requiredType)
    {
        foreach (var slot in slots)
        {
            // 寻找类型匹配且未激活的槽位
            if (slot.iconType == requiredType && !slot.isActive)
            {
                return slot;
            }
        }
        return null; // 没有找到可用槽位
    }

    /// <summary>
    /// 根据数值和持续时间更新UI文本
    /// </summary>
    private void UpdateIconText(IconSlot slot, float value, int duration)
    {
        if (slot.stackOrDurationText != null)
        {
            // 优先显示持续时间（如果大于0），否则显示数值（如果大于0）
            if (duration > 0)
            {
                slot.stackOrDurationText.text = duration.ToString();
            }
            else if (value > 0)
            {
                // 对于叠层，显示为整数
                if (IsStackType(slot.iconType))
                {
                    slot.stackOrDurationText.text = Mathf.FloorToInt(value).ToString();
                }
                else
                {
                    // 对于DOT伤害等，可以显示小数
                    slot.stackOrDurationText.text = value.ToString("F1");
                }
            }
            else
            {
                slot.stackOrDurationText.text = "";
            }
        }
    }

    /// <summary>
    /// 判断图标类型是否为叠层类型
    /// </summary>
    private bool IsStackType(IconType type)
    {
        return type == IconType.YangStack || type == IconType.YinStack ||
               type == IconType.YangPenetration || type == IconType.YinCover;
    }

    #endregion

    #region Unified Tooltip Handling

    /// <summary>
    /// 当鼠标进入图标区域时调用，显示统一Tooltip
    /// </summary>
    public void OnPointerEnter(IconSlot slot)
    {
        // 检查槽位是否激活且统一Tooltip面板引用存在
        if (!slot.isActive || tooltipPanel == null) return;

        // 记录当前显示Tooltip的槽位
        currentTooltipSlot = slot;

        // 从BattleSystem获取配置以获取Tooltip文本
        BattleConfig config = BattleSystem.Instance?.config;
        if (config == null)
        {
            Debug.LogWarning("IconManager: BattleConfig not found for tooltips.");
            // 即使没有配置，也可以显示基本信息
            // return;
        }

        // 获取Tooltip信息
        string[] tooltipInfo = GetTooltipInfo(slot.iconType, slot.value, slot.duration, config);

        // 更新统一Tooltip面板的内容
        if (tooltipInfo.Length == 2)
        {
            if (tooltipTitle != null)
            {
                tooltipTitle.text = tooltipInfo[0];
            }
            else
            {
                Debug.LogWarning("IconManager: tooltipTitle reference is not set in the inspector.");
            }

            if (tooltipDescription != null)
            {
                tooltipDescription.text = tooltipInfo[1];
            }
            else
            {
                Debug.LogWarning("IconManager: tooltipDescription reference is not set in the inspector.");
            }
        }
        else
        {
            // 如果 GetTooltipInfo 返回的数组长度不是2，说明有错误
            Debug.LogError($"IconManager: GetTooltipInfo for {slot.iconType} returned an unexpected array length.");
            if (tooltipTitle != null) tooltipTitle.text = "Error";
            if (tooltipDescription != null) tooltipDescription.text = "Failed to load tooltip data.";
        }

        // 显示统一Tooltip面板
        tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// 当鼠标离开图标区域时调用，隐藏统一Tooltip
    /// </summary>
    public void OnPointerExit(IconSlot slot)
    {
        // 只有当离开的图标是当前显示tooltip的图标时，才隐藏tooltip
        // 这样可以避免鼠标在图标间快速移动时tooltip闪烁
        if (currentTooltipSlot == slot)
        {
            HideUnifiedTooltip();
        }
    }

    /// <summary>
    /// 隐藏统一tooltip面板
    /// </summary>
    private void HideUnifiedTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        currentTooltipSlot = null;
    }

    /// <summary>
    /// 根据图标类型和数据获取Tooltip的标题和描述
    /// </summary>
    private string[] GetTooltipInfo(IconType type, float value, int duration, BattleConfig config)
    {
        // 如果没有配置，提供默认文本
        if (config == null)
        {
            return new string[] { type.ToString(), $"Value: {value:F1}, Duration: {duration}" };
        }

        switch (type)
        {
            // Player Icons
            case IconType.YangStack:
                return new string[] {
                    config.yangStackTooltipTitle ?? "Yang Stack",
                    string.Format(config.yangStackTooltipDescription ?? "Yang Penetration stacks: {0}", Mathf.FloorToInt(value))
                };
            case IconType.YinStack:
                return new string[] {
                    config.yinStackTooltipTitle ?? "Yin Stack",
                    string.Format(config.yinStackTooltipDescription ?? "Yin Cover stacks: {0}", Mathf.FloorToInt(value))
                };
            case IconType.CounterStrike:
                string csDurationText = duration > 0 ? $", Duration: {duration} turns" : "";
                return new string[] {
                    config.counterStrikeTooltipTitle ?? "Counter Strike",
                    string.Format(config.counterStrikeTooltipDescription ?? "Reflect damage when attacked{0}.", csDurationText)
                };
            case IconType.PlayerDot:
                return new string[] {
                    config.playerDotTooltipTitle ?? "Internal Bleeding",
                    string.Format(config.playerDotTooltipDescription ?? "Take {0:F1} damage at the start of your turn. Duration: {1} turns.", value, duration)
                };
            case IconType.AttackDebuff:
                string attackDurationText = duration > 0 ? $", Duration: {duration} turns" : "";
                return new string[] {
                    config.attackDebuffTooltipTitle ?? "Attack Down",
                    string.Format(config.attackDebuffTooltipDescription ?? "Your attack is reduced by {0:F1}{1}.", value, attackDurationText)
                };
            case IconType.DefenseDebuff:
                string defenseDurationText = duration > 0 ? $", Duration: {duration} turns" : "";
                return new string[] {
                    config.defenseDebuffTooltipTitle ?? "Defense Down",
                    string.Format(config.defenseDebuffTooltipDescription ?? "Your defense is reduced by {0:F1}{1}.", value, defenseDurationText)
                };
            case IconType.BalanceHealCD:
                return new string[] {
                    config.balanceHealCDTooltipTitle ?? "Balance Heal Cooldown",
                    string.Format(config.balanceHealCDTooltipDescription ?? "Cannot use Balance heal effect for {0} more turns.", duration)
                };

            // Enemy Icons
            // 注意：敌人意图现在是一个统一的图标类型
            case IconType.EnemyIntent:
                // 意图图标的Tooltip信息可能需要从EnemyManager或EffectManager获取更具体的描述
                // 这里提供一个基础实现，您可能需要根据实际意图类型（攻击/防御/蓄力）动态改变描述
                EnemyManager.EnemyAction.Type currentIntentType = GetEnemyIntentType();
                switch (currentIntentType)
                {
                    case EnemyManager.EnemyAction.Type.Attack:
                        return new string[] {
                            config.attackIntentTooltipTitle ?? "Attack Intent",
                            config.attackIntentTooltipDescription ?? "This enemy plans to attack next turn."
                        };
                    case EnemyManager.EnemyAction.Type.Defend:
                        return new string[] {
                            config.defendIntentTooltipTitle ?? "Defend Intent",
                            config.defendIntentTooltipDescription ?? "This enemy plans to defend next turn."
                        };
                    case EnemyManager.EnemyAction.Type.Charge:
                        return new string[] {
                            config.chargeIntentTooltipTitle ?? "Charge Intent",
                            config.chargeIntentTooltipDescription ?? "This enemy plans to charge up next turn."
                        };
                    default:
                        return new string[] { "Unknown Intent", "Enemy's next action is unknown." };
                }
            case IconType.EnemyDot:
                return new string[] {
                    config.enemyDotTooltipTitle ?? "Burning",
                    string.Format(config.enemyDotTooltipDescription ?? "Take {0:F1} damage at the start of enemy turn. Duration: {1} turns.", value, duration)
                };
            case IconType.YangPenetration:
                string yangDurationText = duration > 0 ? $", Duration: {duration} turns" : "";
                return new string[] {
                    config.yangPenetrationTooltipTitle ?? "Yang Penetration",
                    string.Format(config.yangPenetrationTooltipDescription ?? "Enemy Yang Penetration stacks: {0:F1}{1}.", value, yangDurationText)
                };
            case IconType.YinCover:
                string yinDurationText = duration > 0 ? $", Duration: {duration} turns" : "";
                return new string[] {
                    config.yinCoverTooltipTitle ?? "Yin Cover",
                    string.Format(config.yinCoverTooltipDescription ?? "Enemy Yin Cover stacks: {0:F1}{1}.", value, yinDurationText)
                };

            // Specific Debuff Types
            case IconType.ExtremeYangDebuff_AttackDown:
                string extremeYangDurationText = duration > 0 ? $", Duration: {duration} turns" : "";
                return new string[] {
                    "Extreme Yang Debuff",
                    $"Attack halved{extremeYangDurationText}."
                };
            case IconType.EnemyDebuff_DefenseDown:
                string enemyDebuffDurationText = duration > 0 ? $", Duration: {duration} turns" : "";
                return new string[] {
                    "Enemy Defense Down",
                    $"Enemy defense reduced{enemyDebuffDurationText}."
                };

            default:
                return new string[] { "Unknown Icon", "No description available for this icon." };
        }
    }

    /// <summary>
    /// 辅助方法：获取当前敌人的意图类型（需要根据您的其他脚本实现）
    /// </summary>
    private EnemyManager.EnemyAction.Type GetEnemyIntentType()
    {
        // 这里需要根据当前游戏状态获取敌人的下一个行动意图
        // 假设EffectManager存储了当前意图或可以通过某种方式查询
        // 由于具体实现依赖于您的其他脚本，这里提供一个占位符逻辑

        // 示例：如果EffectManager存储了当前意图
        // return BattleSystem.Instance.effectManager.GetCurrentEnemyIntent()?.type ?? EnemyManager.EnemyAction.Type.Attack;

        // 如果没有找到相关信息，默认返回攻击
        // 或者，您可以存储一个私有变量来跟踪当前显示的意图类型
        return EnemyManager.EnemyAction.Type.Attack;
    }

    #endregion

    #region Utility Methods for External Systems

    /// <summary>
    /// 更新DOT图标的显示
    /// </summary>
    public void UpdateDotIcons(List<EffectManager.DotEffect> dots)
    {
        // 清除所有玩家和敌人DOT图标
        RemovePlayerIcon(IconType.PlayerDot);
        RemoveEnemyIcon(IconType.EnemyDot);

        // 重新添加活跃的DOT图标（示例：只显示玩家和敌人的第一个DOT，或总和）
        if (dots != null)
        {
            EffectManager.DotEffect playerDot = null;
            EffectManager.DotEffect enemyDot = null;

            foreach (var dot in dots)
            {
                if (dot.isPlayer && playerDot == null)
                {
                    playerDot = dot;
                }
                else if (!dot.isPlayer && enemyDot == null)
                {
                    enemyDot = dot;
                }

                if (playerDot != null && enemyDot != null)
                {
                    break;
                }
            }

            if (playerDot != null)
            {
                AddPlayerIcon(IconType.PlayerDot, playerDot.damage, playerDot.remainingTurns);
            }
            if (enemyDot != null)
            {
                AddEnemyIcon(IconType.EnemyDot, enemyDot.damage, enemyDot.remainingTurns);
            }
        }
    }

    #endregion
}