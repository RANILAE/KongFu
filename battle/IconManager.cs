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

    [Header("Icon Resources")]
    public Sprite yangStackIcon;
    public Sprite yinStackIcon;
    public Sprite counterStrikeIcon;
    public Sprite playerDotIcon;
    public Sprite attackDebuffIcon;
    public Sprite defenseDebuffIcon;
    public Sprite ultimateQiIcon;
    public Sprite attackIntentIcon;
    public Sprite defendIntentIcon;
    public Sprite chargeIntentIcon;
    public Sprite enemyDotIcon;
    public Sprite yangPenetrationIcon;
    public Sprite yinCoverIcon;

    private Dictionary<IconType, IconSlot> activePlayerIcons = new Dictionary<IconType, IconSlot>();
    private Dictionary<IconType, IconSlot> activeEnemyIcons = new Dictionary<IconType, IconSlot>();

    void Start()
    {
        InitializeSlots(playerIconSlots);
        InitializeSlots(enemyIconSlots);
    }

    void InitializeSlots(List<IconSlot> slots)
    {
        foreach (var slot in slots)
        {
            slot.iconImage.gameObject.SetActive(false);
            if (slot.stackText != null) slot.stackText.gameObject.SetActive(false);
            if (slot.durationText != null) slot.durationText.gameObject.SetActive(false);
            if (slot.tooltipPanel != null) slot.tooltipPanel.SetActive(false);
            slot.isActive = false;

            // 添加事件触发器
            if (slot.eventTrigger == null)
            {
                slot.eventTrigger = slot.iconImage.gameObject.AddComponent<EventTrigger>();
            }
            else
            {
                slot.eventTrigger.triggers.Clear();
            }

            // 添加鼠标悬停事件
            AddEventTrigger(slot, EventTriggerType.PointerEnter, OnPointerEnter);
            AddEventTrigger(slot, EventTriggerType.PointerExit, OnPointerExit);
        }
    }

    private void AddEventTrigger(IconSlot slot, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(action);
        slot.eventTrigger.triggers.Add(entry);
    }

    private void OnPointerEnter(BaseEventData eventData)
    {
        // 找到对应的IconSlot
        foreach (var slot in playerIconSlots)
        {
            if (eventData.selectedObject == slot.iconImage.gameObject && slot.tooltipPanel != null)
            {
                slot.tooltipPanel.SetActive(true);
                return;
            }
        }

        foreach (var slot in enemyIconSlots)
        {
            if (eventData.selectedObject == slot.iconImage.gameObject && slot.tooltipPanel != null)
            {
                slot.tooltipPanel.SetActive(true);
                return;
            }
        }
    }

    private void OnPointerExit(BaseEventData eventData)
    {
        // 找到对应的IconSlot
        foreach (var slot in playerIconSlots)
        {
            if (eventData.selectedObject == slot.iconImage.gameObject && slot.tooltipPanel != null)
            {
                slot.tooltipPanel.SetActive(false);
                return;
            }
        }

        foreach (var slot in enemyIconSlots)
        {
            if (eventData.selectedObject == slot.iconImage.gameObject && slot.tooltipPanel != null)
            {
                slot.tooltipPanel.SetActive(false);
                return;
            }
        }
    }

    // Add player icon
    public void AddPlayerIcon(IconType type, int stacks = 0, int duration = 0)
    {
        // If icon already exists, update it
        if (activePlayerIcons.ContainsKey(type))
        {
            UpdatePlayerIcon(type, stacks, duration);
            return;
        }

        // Find available slot
        IconSlot slot = GetAvailableSlot(playerIconSlots);
        if (slot == null)
        {
            Debug.LogWarning("No available player icon slots!");
            return;
        }

        // Set icon
        SetIcon(slot, type, stacks, duration);
        activePlayerIcons.Add(type, slot);
    }

    // Update player icon
    public void UpdatePlayerIcon(IconType type, int stacks = 0, int duration = 0)
    {
        if (!activePlayerIcons.ContainsKey(type)) return;

        IconSlot slot = activePlayerIcons[type];
        UpdateIcon(slot, stacks, duration);
    }

    // Remove player icon
    public void RemovePlayerIcon(IconType type)
    {
        if (!activePlayerIcons.ContainsKey(type)) return;

        IconSlot slot = activePlayerIcons[type];
        ClearSlot(slot);
        activePlayerIcons.Remove(type);
    }

    // Add enemy icon
    public void AddEnemyIcon(IconType type, int stacks = 0, int duration = 0)
    {
        // If icon already exists, update it
        if (activeEnemyIcons.ContainsKey(type))
        {
            UpdateEnemyIcon(type, stacks, duration);
            return;
        }

        // Find available slot
        IconSlot slot = GetAvailableSlot(enemyIconSlots);
        if (slot == null)
        {
            Debug.LogWarning("No available enemy icon slots!");
            return;
        }

        // Set icon
        SetIcon(slot, type, stacks, duration);
        activeEnemyIcons.Add(type, slot);
    }

    // Update enemy icon
    public void UpdateEnemyIcon(IconType type, int stacks = 0, int duration = 0)
    {
        if (!activeEnemyIcons.ContainsKey(type)) return;

        IconSlot slot = activeEnemyIcons[type];
        UpdateIcon(slot, stacks, duration);
    }

    // Remove enemy icon
    public void RemoveEnemyIcon(IconType type)
    {
        if (!activeEnemyIcons.ContainsKey(type)) return;

        IconSlot slot = activeEnemyIcons[type];
        ClearSlot(slot);
        activeEnemyIcons.Remove(type);
    }

    // Clear all player icons
    public void ClearPlayerIcons()
    {
        foreach (var type in activePlayerIcons.Keys)
        {
            ClearSlot(activePlayerIcons[type]);
        }
        activePlayerIcons.Clear();
    }

    // Clear all enemy icons
    public void ClearEnemyIcons()
    {
        foreach (var type in activeEnemyIcons.Keys)
        {
            ClearSlot(activeEnemyIcons[type]);
        }
        activeEnemyIcons.Clear();
    }

    // Set icon
    private void SetIcon(IconSlot slot, IconType type, int stacks, int duration)
    {
        slot.isActive = true;
        slot.iconImage.gameObject.SetActive(true);
        slot.iconImage.sprite = GetIconSprite(type);

        // Set stack text
        if (slot.stackText != null)
        {
            bool showStacks = stacks > 0;
            slot.stackText.gameObject.SetActive(showStacks);
            if (showStacks) slot.stackText.text = stacks.ToString();
        }

        // Set duration text
        if (slot.durationText != null)
        {
            bool showDuration = duration > 0;
            slot.durationText.gameObject.SetActive(showDuration);
            if (showDuration) slot.durationText.text = duration.ToString();
        }

        // Set tooltip
        if (slot.tooltipPanel != null)
        {
            string[] tooltip = GetTooltipInfo(type);
            if (slot.tooltipTitle != null) slot.tooltipTitle.text = tooltip[0];
            if (slot.tooltipDescription != null) slot.tooltipDescription.text = tooltip[1];
        }
    }

    // Update icon
    private void UpdateIcon(IconSlot slot, int stacks, int duration)
    {
        // Update stack text
        if (slot.stackText != null)
        {
            bool showStacks = stacks > 0;
            slot.stackText.gameObject.SetActive(showStacks);
            if (showStacks) slot.stackText.text = stacks.ToString();
        }

        // Update duration text
        if (slot.durationText != null)
        {
            bool showDuration = duration > 0;
            slot.durationText.gameObject.SetActive(showDuration);
            if (showDuration) slot.durationText.text = duration.ToString();
        }
    }

    // Clear slot
    private void ClearSlot(IconSlot slot)
    {
        slot.iconImage.gameObject.SetActive(false);
        if (slot.stackText != null) slot.stackText.gameObject.SetActive(false);
        if (slot.durationText != null) slot.durationText.gameObject.SetActive(false);
        if (slot.tooltipPanel != null) slot.tooltipPanel.SetActive(false);
        slot.isActive = false;
    }

    // Get available slot
    private IconSlot GetAvailableSlot(List<IconSlot> slots)
    {
        foreach (var slot in slots)
        {
            if (!slot.isActive) return slot;
        }
        return null;
    }

    // Get icon sprite
    private Sprite GetIconSprite(IconType type)
    {
        switch (type)
        {
            case IconType.YangStack: return yangStackIcon;
            case IconType.YinStack: return yinStackIcon;
            case IconType.CounterStrike: return counterStrikeIcon;
            case IconType.PlayerDot: return playerDotIcon;
            case IconType.AttackDebuff: return attackDebuffIcon;
            case IconType.DefenseDebuff: return defenseDebuffIcon;
            case IconType.UltimateQi: return ultimateQiIcon;
            case IconType.AttackIntent: return attackIntentIcon;
            case IconType.DefendIntent: return defendIntentIcon;
            case IconType.ChargeIntent: return chargeIntentIcon;
            case IconType.EnemyDot: return enemyDotIcon;
            case IconType.YangPenetration: return yangPenetrationIcon;
            case IconType.YinCover: return yinCoverIcon;
            default: return null;
        }
    }

    // Get tooltip info
    private string[] GetTooltipInfo(IconType type)
    {
        switch (type)
        {
            case IconType.YangStack:
                return new string[] { "Yang Stack", "Critical Yang stacks accumulated. Required for Extreme Yang effect." };
            case IconType.YinStack:
                return new string[] { "Yin Stack", "Critical Yin stacks accumulated. Required for Extreme Yin effect." };
            case IconType.CounterStrike:
                return new string[] { "Counter Strike", "Reflects part of damage taken when enemy attacks." };
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