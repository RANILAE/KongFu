using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ��Ӵ�������֧��EventTrigger
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ����һ��ͼ���λ�����ݽṹ��
/// </summary>
[System.Serializable]
public class IconSlot
{
    [Header("UI References")]
    [Tooltip("The Image component used to display the icon.")]
    public Image iconImage; // ͼ��ͼƬ

    [Tooltip("The TMP_Text component used to display the stack count or duration.")]
    public TMP_Text stackOrDurationText; // ��ʾ�������������ʱ��

    [Header("Configuration")]
    [Tooltip("The type of icon this slot is designated for. Must match the IconType enum.")]
    public IconManager.IconType iconType; // �˲�λ������ͼ������

    // ע�⣺�Ƴ��� slot.tooltipPanel, slot.tooltipTitle, slot.tooltipDescription ������
    // ���ǽ�ʹ�� IconManager ���ж����ͳһ Tooltip ����

    [HideInInspector] public bool isActive = false; // �Ƿ񼤻�
    [HideInInspector] public float value; // ��ǰֵ��������/����ʱ��/ǿ�ȣ�
    [HideInInspector] public int duration; // ��ǰ����ʱ��
}

/// <summary>
/// ����ս��������ͼ�꣨�� Buff, Debuff, Intent �ȣ�����ʾ�ͽ�����
/// </summary>
public class IconManager : MonoBehaviour
{
    /// <summary>
    /// ����������Ŀ��ʵ��ʹ�õ�ͼ�����͡�
    /// </summary>
    public enum IconType
    {
        // --- Player Icons ---
        YangStack,              // Yang Stack (����͸����)
        YinStack,               // Yin Stack (�����ǵ���)
        CounterStrike,          // Counter Strike (���𼤻�)
        PlayerDot,              // Player DOT (���DOT�˺�)
        AttackDebuff,           // Attack Debuff (�������½�Debuff)
        DefenseDebuff,           // Defense Debuff (�������½�Debuff)
        BalanceHealCD,           // Balance Heal CD (ƽ���ѪCD)

        // --- Enemy Icons ---
        // ע�⣺�Ƴ��� AttackIntent, DefendIntent, ChargeIntent
        // ��Ϊʹ��һ��ͳһ����ͼͼ��
        EnemyIntent,            // Enemy Intent (������ͼ - ͳһͼ��)
        EnemyDot,               // Enemy DOT (����DOT�˺�)
        YangPenetration,        // Yang Penetration (����͸Ч��/����)
        YinCover,               // Yin Cover (������Ч��/����)

        // --- Specific Debuff Types (Ϊ�˽�� EffectManager.cs �еı������) ---
        ExtremeYangDebuff_AttackDown,  // ��������������Debuff
        EnemyDebuff_DefenseDown        // ���˷����½�Debuff
    }

    [Header("Player Icon Slots")]
    [Tooltip("List of icon slots available for displaying player-related icons.")]
    public List<IconSlot> playerIconSlots = new List<IconSlot>();

    [Header("Enemy Icon Slots")]
    [Tooltip("List of icon slots available for displaying enemy-related icons.")]
    public List<IconSlot> enemyIconSlots = new List<IconSlot>();

    // �ڲ��ֵ����ڿ��ٲ��Һ͹����Ѽ����ͼ��
    private Dictionary<IconType, IconSlot> activePlayerIcons = new Dictionary<IconType, IconSlot>();
    private Dictionary<IconType, IconSlot> activeEnemyIcons = new Dictionary<IconType, IconSlot>();

    // --- ͳһ Tooltip ���� ---
    [Header("Unified Tooltip Panel")]
    [Tooltip("The single GameObject that serves as the container for all icon tooltips.")]
    public GameObject tooltipPanel;

    [Tooltip("The TMP_Text component within the tooltip panel for the title.")]
    public TMP_Text tooltipTitle;

    [Tooltip("The TMP_Text component within the tooltip panel for the description.")]
    public TMP_Text tooltipDescription;
    // --- ͳһ Tooltip ���ý��� ---

    // ��ǰ��ʾTooltip��ͼ��� (����OnPointerExit�ж�)
    private IconSlot currentTooltipSlot = null;

    void Awake()
    {
        // ��ʼ��ͼ����ֵ��UI״̬
        InitializeIconSlots(playerIconSlots, activePlayerIcons);
        InitializeIconSlots(enemyIconSlots, activeEnemyIcons);

        // ��ʼ��ͳһTooltip���Ϊ����
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ��ʼ��ͼ����б����ó�ʼ״̬���¼�������
    /// </summary>
    private void InitializeIconSlots(List<IconSlot> slots, Dictionary<IconType, IconSlot> activeIcons)
    {
        foreach (var slot in slots)
        {
            if (slot.iconImage != null)
            {
                slot.iconImage.gameObject.SetActive(false);

                // --- Ϊͼ��ͼƬ��� EventTrigger �Ա㴦�������ͣ ---
                // ȷ����EventTrigger���
                EventTrigger trigger = slot.iconImage.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = slot.iconImage.gameObject.AddComponent<EventTrigger>();
                }

                // �Ƴ��ɵļ�����������еĻ�������ֹ�ظ����
                trigger.triggers.Clear();

                // ���PointerEnter�¼�
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                // ʹ��������������ǰ slot ʵ��
                enterEntry.callback.AddListener((data) => { OnPointerEnter(slot); });
                trigger.triggers.Add(enterEntry);

                // ���PointerExit�¼�
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                // ʹ��������������ǰ slot ʵ��
                exitEntry.callback.AddListener((data) => { OnPointerExit(slot); });
                trigger.triggers.Add(exitEntry);
                // --- EventTrigger ��ӽ��� ---
            }

            slot.isActive = false;
            slot.value = 0f;
            slot.duration = 0;

            if (slot.stackOrDurationText != null)
            {
                slot.stackOrDurationText.text = "";
            }

            // ע�⣺������Ҫ��ʼ�� slot.tooltipPanel, slot.tooltipTitle, slot.tooltipDescription
            // ��Ϊ����ʹ��ͳһ������
        }
        // ע�⣺activeIcons �ֵ佫�ڵ�һ�ε��� AddPlayerIcon/AddEnemyIcon ʱ���
    }

    #region Player Icon Management

    /// <summary>
    /// ��ӻ����һ�����ͼ�ꡣ
    /// </summary>
    public void AddPlayerIcon(IconType type, float value = 0, int duration = 0)
    {
        UpdateIcon(activePlayerIcons, playerIconSlots, type, value, duration, true);
    }

    /// <summary>
    /// �Ƴ�һ�����ͼ�ꡣ
    /// </summary>
    public void RemovePlayerIcon(IconType type)
    {
        RemoveIcon(activePlayerIcons, playerIconSlots, type, true);
    }

    #endregion

    #region Enemy Icon Management

    /// <summary>
    /// ��ӻ����һ������ͼ�ꡣ
    /// </summary>
    public void AddEnemyIcon(IconType type, float value = 0, int duration = 0)
    {
        // ���⴦��������ͼͼ��
        if (type == IconType.EnemyIntent)
        {
            // ������ͼͼ�꣬���ǿ���ֻ����ʾ���ͣ��������� value �� duration
            // ��Ϊ�˱���һ���ԣ�������Ȼ��������
            UpdateIcon(activeEnemyIcons, enemyIconSlots, type, value, duration, false);
        }
        else
        {
            UpdateIcon(activeEnemyIcons, enemyIconSlots, type, value, duration, false);
        }
    }

    /// <summary>
    /// �Ƴ�һ������ͼ�ꡣ
    /// </summary>
    public void RemoveEnemyIcon(IconType type)
    {
        RemoveIcon(activeEnemyIcons, enemyIconSlots, type, false);
    }

    #endregion

    #region Core Icon Logic

    /// <summary>
    /// ͨ�÷��������»����һ��ͼ��
    /// </summary>
    private void UpdateIcon(Dictionary<IconType, IconSlot> activeIcons, List<IconSlot> slots, IconType type, float value, int duration, bool isPlayer)
    {
        IconSlot slot;

        // ���ͼ���Ƿ��Ѵ��ڲ�����
        if (activeIcons.TryGetValue(type, out slot))
        {
            // ������ڣ�����������
            slot.isActive = true;
            if (slot.iconImage != null)
            {
                slot.iconImage.gameObject.SetActive(true);
            }
            slot.value = value;
            slot.duration = duration;

            // ������ʾ�ı�
            UpdateIconText(slot, value, duration);
        }
        else
        {
            // ��������ڣ�Ѱ��һ��ƥ��������δ��ʹ�õĲ�λ
            slot = FindAvailableSlot(slots, type);
            if (slot != null)
            {
                slot.isActive = true;
                // iconType ��Inspector�������ã�����Ҫ����ʱ����
                if (slot.iconImage != null)
                {
                    slot.iconImage.gameObject.SetActive(true);
                }
                slot.value = value;
                slot.duration = duration;

                // ������ʾ�ı�
                UpdateIconText(slot, value, duration);

                // ���¼���Ĳ�λ�����ֵ�
                activeIcons.Add(type, slot);
            }
            else
            {
                Debug.LogWarning($"No available icon slot found for {type} on {(isPlayer ? "Player" : "Enemy")}. Consider adding more slots in the inspector.");
            }
        }
    }

    /// <summary>
    /// ͨ�÷������Ƴ�һ��ͼ��
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

            // �����ǰTooltip����ʾ�ڴ�ͼ���ϣ���������
            if (currentTooltipSlot == slot)
            {
                HideUnifiedTooltip();
            }

            // �Ӽ����ֵ����Ƴ�
            activeIcons.Remove(type);
        }
        // ���ͼ�겻�����ڼ����ֵ��У�˵����������δ��ʾ�����������
    }

    /// <summary>
    /// Ѱ��һ��ָ��������δ��ʹ�õ�ͼ���
    /// </summary>
    private IconSlot FindAvailableSlot(List<IconSlot> slots, IconType requiredType)
    {
        foreach (var slot in slots)
        {
            // Ѱ������ƥ����δ����Ĳ�λ
            if (slot.iconType == requiredType && !slot.isActive)
            {
                return slot;
            }
        }
        return null; // û���ҵ����ò�λ
    }

    /// <summary>
    /// ������ֵ�ͳ���ʱ�����UI�ı�
    /// </summary>
    private void UpdateIconText(IconSlot slot, float value, int duration)
    {
        if (slot.stackOrDurationText != null)
        {
            // ������ʾ����ʱ�䣨�������0����������ʾ��ֵ���������0��
            if (duration > 0)
            {
                slot.stackOrDurationText.text = duration.ToString();
            }
            else if (value > 0)
            {
                // ���ڵ��㣬��ʾΪ����
                if (IsStackType(slot.iconType))
                {
                    slot.stackOrDurationText.text = Mathf.FloorToInt(value).ToString();
                }
                else
                {
                    // ����DOT�˺��ȣ�������ʾС��
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
    /// �ж�ͼ�������Ƿ�Ϊ��������
    /// </summary>
    private bool IsStackType(IconType type)
    {
        return type == IconType.YangStack || type == IconType.YinStack ||
               type == IconType.YangPenetration || type == IconType.YinCover;
    }

    #endregion

    #region Unified Tooltip Handling

    /// <summary>
    /// ��������ͼ������ʱ���ã���ʾͳһTooltip
    /// </summary>
    public void OnPointerEnter(IconSlot slot)
    {
        // ����λ�Ƿ񼤻���ͳһTooltip������ô���
        if (!slot.isActive || tooltipPanel == null) return;

        // ��¼��ǰ��ʾTooltip�Ĳ�λ
        currentTooltipSlot = slot;

        // ��BattleSystem��ȡ�����Ի�ȡTooltip�ı�
        BattleConfig config = BattleSystem.Instance?.config;
        if (config == null)
        {
            Debug.LogWarning("IconManager: BattleConfig not found for tooltips.");
            // ��ʹû�����ã�Ҳ������ʾ������Ϣ
            // return;
        }

        // ��ȡTooltip��Ϣ
        string[] tooltipInfo = GetTooltipInfo(slot.iconType, slot.value, slot.duration, config);

        // ����ͳһTooltip��������
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
            // ��� GetTooltipInfo ���ص����鳤�Ȳ���2��˵���д���
            Debug.LogError($"IconManager: GetTooltipInfo for {slot.iconType} returned an unexpected array length.");
            if (tooltipTitle != null) tooltipTitle.text = "Error";
            if (tooltipDescription != null) tooltipDescription.text = "Failed to load tooltip data.";
        }

        // ��ʾͳһTooltip���
        tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// ������뿪ͼ������ʱ���ã�����ͳһTooltip
    /// </summary>
    public void OnPointerExit(IconSlot slot)
    {
        // ֻ�е��뿪��ͼ���ǵ�ǰ��ʾtooltip��ͼ��ʱ��������tooltip
        // �������Ա��������ͼ�������ƶ�ʱtooltip��˸
        if (currentTooltipSlot == slot)
        {
            HideUnifiedTooltip();
        }
    }

    /// <summary>
    /// ����ͳһtooltip���
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
    /// ����ͼ�����ͺ����ݻ�ȡTooltip�ı��������
    /// </summary>
    private string[] GetTooltipInfo(IconType type, float value, int duration, BattleConfig config)
    {
        // ���û�����ã��ṩĬ���ı�
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
            // ע�⣺������ͼ������һ��ͳһ��ͼ������
            case IconType.EnemyIntent:
                // ��ͼͼ���Tooltip��Ϣ������Ҫ��EnemyManager��EffectManager��ȡ�����������
                // �����ṩһ������ʵ�֣���������Ҫ����ʵ����ͼ���ͣ�����/����/��������̬�ı�����
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
    /// ������������ȡ��ǰ���˵���ͼ���ͣ���Ҫ�������������ű�ʵ�֣�
    /// </summary>
    private EnemyManager.EnemyAction.Type GetEnemyIntentType()
    {
        // ������Ҫ���ݵ�ǰ��Ϸ״̬��ȡ���˵���һ���ж���ͼ
        // ����EffectManager�洢�˵�ǰ��ͼ�����ͨ��ĳ�ַ�ʽ��ѯ
        // ���ھ���ʵ�����������������ű��������ṩһ��ռλ���߼�

        // ʾ�������EffectManager�洢�˵�ǰ��ͼ
        // return BattleSystem.Instance.effectManager.GetCurrentEnemyIntent()?.type ?? EnemyManager.EnemyAction.Type.Attack;

        // ���û���ҵ������Ϣ��Ĭ�Ϸ��ع���
        // ���ߣ������Դ洢һ��˽�б��������ٵ�ǰ��ʾ����ͼ����
        return EnemyManager.EnemyAction.Type.Attack;
    }

    #endregion

    #region Utility Methods for External Systems

    /// <summary>
    /// ����DOTͼ�����ʾ
    /// </summary>
    public void UpdateDotIcons(List<EffectManager.DotEffect> dots)
    {
        // ���������Һ͵���DOTͼ��
        RemovePlayerIcon(IconType.PlayerDot);
        RemoveEnemyIcon(IconType.EnemyDot);

        // ������ӻ�Ծ��DOTͼ�꣨ʾ����ֻ��ʾ��Һ͵��˵ĵ�һ��DOT�����ܺͣ�
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