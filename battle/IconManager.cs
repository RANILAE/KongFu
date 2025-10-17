using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class IconManager : MonoBehaviour
{
    [System.Serializable]
    public class IconSlot
    {
        public Image iconImage;
        public TMP_Text stackText;
        public TMP_Text durationText;
        public GameObject tooltipPanel;
        public TMP_Text tooltipTitle;
        public TMP_Text tooltipDescription;

        [HideInInspector] public bool isActive;
        [HideInInspector] public EventTrigger eventTrigger;
    }

    public enum IconType
    {
        // Player Icons
        YangStack,          // Yang Stack
        YinStack,            // Yin Stack
        CounterStrike,       // Counter Strike
        PlayerDot,           // Player DOT
        AttackDebuff,        // Attack Debuff
        DefenseDebuff,       // Defense Debuff
        UltimateQi,          // Ultimate Qi

        // Enemy Icons
        AttackIntent,        // Attack Intent
        DefendIntent,        // Defend Intent
        ChargeIntent,        // Charge Intent
        EnemyDot,            // Enemy DOT
        YangPenetration,     // Yang Penetration
        YinCover             // Yin Cover
    }

    [Header("Player Icon Slots")]
    public List<IconSlot> playerIconSlots = new List<IconSlot>();

    [Header("Enemy Icon Slots")]
    public List<IconSlot> enemyIconSlots = new List<IconSlot>();

    private Dictionary<IconType, IconSlot> playerIcons = new Dictionary<IconType, IconSlot>();
    private Dictionary<IconType, IconSlot> enemyIcons = new Dictionary<IconType, IconSlot>();

    void Start()
    {
        // 初始化字典
        InitializeIconSlots(playerIconSlots, playerIcons);
        InitializeIconSlots(enemyIconSlots, enemyIcons);
    }

    private void InitializeIconSlots(List<IconSlot> slots, Dictionary<IconType, IconSlot> icons)
    {
        foreach (var slot in slots)
        {
            slot.iconImage.gameObject.SetActive(false);
            slot.isActive = false;
            if (slot.tooltipPanel) slot.tooltipPanel.SetActive(false);

            // 为每个图标添加事件触发器
            if (slot.iconImage != null && slot.iconImage.GetComponent<EventTrigger>() == null)
            {
                slot.eventTrigger = slot.iconImage.gameObject.AddComponent<EventTrigger>();
                AddEventTrigger(slot.eventTrigger, EventTriggerType.PointerEnter, (data) => ShowTooltip(slot));
                AddEventTrigger(slot.eventTrigger, EventTriggerType.PointerExit, (data) => HideTooltip(slot));
            }
        }
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    public void AddPlayerIcon(IconType type, float stackOrAmount = 0, int duration = 0)
    {
        UpdateIcon(playerIcons, type, stackOrAmount, duration, true);
    }

    public void AddEnemyIcon(IconType type, float stackOrAmount = 0, int duration = 0)
    {
        UpdateIcon(enemyIcons, type, stackOrAmount, duration, true);
    }

    public void RemovePlayerIcon(IconType type)
    {
        RemoveIcon(playerIcons, type);
    }

    public void RemoveEnemyIcon(IconType type)
    {
        RemoveIcon(enemyIcons, type);
    }

    private void UpdateIcon(Dictionary<IconType, IconSlot> icons, IconType type, float stackOrAmount, int duration, bool show)
    {
        if (icons.ContainsKey(type))
        {
            IconSlot slot = icons[type];
            slot.isActive = show;
            slot.iconImage.gameObject.SetActive(show);
            if (slot.stackText) slot.stackText.text = stackOrAmount > 0 ? stackOrAmount.ToString("F1") : "";
            if (slot.durationText) slot.durationText.text = duration > 0 ? duration.ToString() : "";
        }
        else
        {
            // 如果图标槽不存在，找到一个空槽并分配
            IconSlot slot = FindFreeSlot(icons);
            if (slot != null)
            {
                icons.Add(type, slot);
                slot.isActive = show;
                slot.iconImage.gameObject.SetActive(show);
                if (slot.stackText) slot.stackText.text = stackOrAmount > 0 ? stackOrAmount.ToString("F1") : "";
                if (slot.durationText) slot.durationText.text = duration > 0 ? duration.ToString() : "";
            }
        }
    }

    private void RemoveIcon(Dictionary<IconType, IconSlot> icons, IconType type)
    {
        if (icons.ContainsKey(type))
        {
            IconSlot slot = icons[type];
            slot.isActive = false;
            slot.iconImage.gameObject.SetActive(false);
            if (slot.stackText) slot.stackText.text = "";
            if (slot.durationText) slot.durationText.text = "";
            icons.Remove(type);
        }
    }

    public void UpdateDotIcons(List<EffectManager.DotEffect> dots)
    {
        RemovePlayerIcon(IconType.PlayerDot);
        RemoveEnemyIcon(IconType.EnemyDot);
        foreach (var dot in dots)
        {
            if (dot.isPlayer)
            {
                AddPlayerIcon(IconType.PlayerDot, dot.damage, dot.remainingTurns);
            }
            else
            {
                AddEnemyIcon(IconType.EnemyDot, dot.damage, dot.remainingTurns);
            }
        }
    }

    private IconSlot FindFreeSlot(Dictionary<IconType, IconSlot> icons)
    {
        if (icons == playerIcons)
        {
            foreach (var slot in playerIconSlots)
            {
                if (!slot.isActive) return slot;
            }
        }
        else if (icons == enemyIcons)
        {
            foreach (var slot in enemyIconSlots)
            {
                if (!slot.isActive) return slot;
            }
        }
        return null;
    }

    private void ShowTooltip(IconSlot slot)
    {
        if (!slot.isActive || slot.tooltipPanel == null) return;

        // 获取图标类型
        IconType type = IconType.YangStack; // 默认值
        foreach (var kvp in playerIcons)
        {
            if (kvp.Value == slot) type = kvp.Key;
        }
        foreach (var kvp in enemyIcons)
        {
            if (kvp.Value == slot) type = kvp.Key;
        }

        string[] tooltipInfo = GetTooltipInfo(type);
        if (tooltipInfo.Length == 2)
        {
            slot.tooltipTitle.text = tooltipInfo[0];
            slot.tooltipDescription.text = tooltipInfo[1];
        }

        slot.tooltipPanel.SetActive(true);
    }

    private void HideTooltip(IconSlot slot)
    {
        if (slot.tooltipPanel) slot.tooltipPanel.SetActive(false);
    }

    private string[] GetTooltipInfo(IconType type)
    {
        switch (type)
        {
            case IconType.YangStack:
                return new string[] { "Yang Penetration", "Increases damage on attack." };
            case IconType.YinStack:
                return new string[] { "Yin Cover", "Reduces incoming damage." };
            case IconType.CounterStrike:
                return new string[] { "Counter Strike", "Reflects damage from enemy attacks." };
            case IconType.PlayerDot:
                return new string[] { "Player DOT", "Player takes damage over time." };
            case IconType.AttackDebuff:
                return new string[] { "Attack Debuff", "Player attack reduced next turn." };
            case IconType.DefenseDebuff:
                return new string[] { "Defense Debuff", "Player defense reduced next turn." };
            case IconType.UltimateQi:
                return new string[] { "Ultimate Qi", "Ultimate Qi effect activated. Player health set to 1." };
            case IconType.AttackIntent:
                return new string[] { "Attack Intent", "Enemy intends to attack next turn." };
            case IconType.DefendIntent:
                return new string[] { "Defend Intent", "Enemy intends to defend next turn." };
            case IconType.ChargeIntent:
                return new string[] { "Charge Intent", "Enemy intends to charge attack next turn." };
            case IconType.EnemyDot:
                return new string[] { "Enemy DOT", "Enemy takes damage over time." };
            case IconType.YangPenetration:
                return new string[] { "Yang Penetration", "Yang penetration effect applied to enemy." };
            case IconType.YinCover:
                return new string[] { "Yin Cover", "Yin cover effect applied to enemy." };
            default:
                return new string[] { "Unknown", "Unknown effect" };
        }
    }
}
