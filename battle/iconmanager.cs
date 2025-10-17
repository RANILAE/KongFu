using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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

        [HideInInspector] public EffectCategory effectCategory;
        [HideInInspector] public bool isActive;

        // 设置事件触发器
        public void SetupEventTrigger()
        {
            if (iconImage == null) return;

            EventTrigger trigger = iconImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = iconImage.gameObject.AddComponent<EventTrigger>();
            }

            trigger.triggers.Clear();

            // 鼠标进入事件
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { ShowTooltip(); });
            trigger.triggers.Add(entryEnter);

            // 鼠标离开事件
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => { HideTooltip(); });
            trigger.triggers.Add(entryExit);
        }

        private void ShowTooltip()
        {
            if (tooltipPanel != null) tooltipPanel.SetActive(true);
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null) tooltipPanel.SetActive(false);
        }
    }

    public enum EffectCategory
    {
        YangStack,          // 阳叠层 (玩家)
        YinStack,            // 阴叠层 (玩家)
        CounterStrike,       // 反震效果 (玩家)
        PlayerDot,           // 玩家受到的DOT伤害 (玩家)
        AttackDebuff,        // 攻击下降 (玩家)
        DefenseDebuff,       // 防御下降 (玩家)
        EnemyDot,            // 敌人受到的DOT伤害 (敌人)
        YangPenetration,     // 阳穿透效果 (敌人)
        YinCover             // 阴覆盖效果 (敌人)
    }

    [Header("Player Icon Slots")]
    public List<IconSlot> playerIconSlots = new List<IconSlot>();

    [Header("Enemy Icon Slots")]
    public List<IconSlot> enemyIconSlots = new List<IconSlot>();

    [Header("Battle Config")]
    public BattleConfig config;

    [Header("Border Colors")]
    public Color yangStackColor = new Color(1f, 0.92f, 0.016f, 1f); // 黄色
    public Color yinStackColor = new Color(0.58f, 0f, 0.83f, 1f); // 紫色
    public Color counterStrikeColor = Color.cyan;
    public Color playerDotColor = Color.red;
    public Color attackDebuffColor = Color.red;
    public Color defenseDebuffColor = Color.blue;
    public Color enemyDotColor = Color.green;
    public Color yangPenetrationColor = Color.yellow;
    public Color yinCoverColor = Color.magenta;

    private Dictionary<EffectCategory, IconSlot> activePlayerEffects = new Dictionary<EffectCategory, IconSlot>();
    private Dictionary<EffectCategory, IconSlot> activeEnemyEffects = new Dictionary<EffectCategory, IconSlot>();

    private void Start()
    {
        // 初始化所有图标槽为隐藏状态
        InitializeSlots(playerIconSlots);
        InitializeSlots(enemyIconSlots);
    }

    private void InitializeSlots(List<IconSlot> slots)
    {
        foreach (var slot in slots)
        {
            slot.iconImage.gameObject.SetActive(false);
            if (slot.stackText != null) slot.stackText.gameObject.SetActive(false);
            if (slot.durationText != null) slot.durationText.gameObject.SetActive(false);
            if (slot.tooltipPanel != null) slot.tooltipPanel.SetActive(false);
            slot.isActive = false;
        }
    }

    // 添加玩家效果
    public void AddPlayerEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        // 如果效果已存在，更新它
        if (activePlayerEffects.ContainsKey(category))
        {
            UpdatePlayerEffect(category, stacks, duration);
            return;
        }

        // 找到空闲的图标槽
        IconSlot slot = GetAvailableSlot(playerIconSlots);
        if (slot == null)
        {
            Debug.LogWarning("No available player effect slots!");
            return;
        }

        // 设置效果
        SetEffect(slot, category, stacks, duration);
        activePlayerEffects.Add(category, slot);
    }

    // 更新玩家效果
    public void UpdatePlayerEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        if (!activePlayerEffects.ContainsKey(category)) return;

        IconSlot slot = activePlayerEffects[category];
        UpdateEffect(slot, stacks, duration);
    }

    // 移除玩家极效果
    public void RemovePlayerEffect(EffectCategory category)
    {
        if (!activePlayerEffects.ContainsKey(category)) return;

        IconSlot slot = activePlayerEffects[category];
        ClearSlot(slot);
        activePlayerEffects.Remove(category);
    }

    // 添加敌人效果
    public void AddEnemyEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        // 如果效果已存在，更新它
        if (activeEnemyEffects.ContainsKey(category))
        {
            UpdateEnemyEffect(category, stacks, duration);
            return;
        }

        // 找到空闲的图标槽
        IconSlot slot = GetAvailableSlot(enemyIconSlots);
        if (slot == null)
        {
            Debug.LogWarning("No available enemy effect slots!");
            return;
        }

        // 设置效果
        SetEffect(slot, category, stacks, duration);
        activeEnemyEffects.Add(category, slot);
    }

    // 更新敌人效果
    public void UpdateEnemyEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        if (!activeEnemyEffects.ContainsKey(category)) return;

        IconSlot slot = activeEnemyEffects[category];
        UpdateEffect(slot, stacks, duration);
    }

    // 移除敌人效果
    public void RemoveEnemyEffect(EffectCategory category)
    {
        if (!activeEnemyEffects.ContainsKey(category)) return;

        IconSlot slot = activeEnemyEffects[category];
        ClearSlot(slot);
        activeEnemyEffects.Remove(category);
    }

    // 清除所有玩家效果
    public void ClearPlayerEffects()
    {
        var keys = activePlayerEffects.Keys.ToList();
        foreach (var key in keys)
        {
            ClearSlot(activePlayerEffects[key]);
            activePlayerEffects.Remove(key);
        }
    }

    // 清除所有敌人效果
    public void ClearEnemyEffects()
    {
        var keys = activeEnemyEffects.Keys.ToList();
        foreach (var key in keys)
        {
            ClearSlot(activeEnemyEffects[key]);
            activeEnemyEffects.Remove(key);
        }
    }

    // 清除所有效果
    public void ClearAllEffects()
    {
        ClearPlayerEffects();
        ClearEnemyEffects();
    }

    // 找到空闲的图标槽
    private IconSlot GetAvailableSlot(List<IconSlot> slots)
    {
        foreach (var slot in slots)
        {
            if (!slot.isActive) return slot;
        }
        return null;
    }

    // 设置效果到图标槽
    private void SetEffect(IconSlot slot, EffectCategory category, int stacks = 0, int duration = 0)
    {
        slot.effectCategory = category;
        slot.isActive = true;

        // 设置图标
        SetIcon(slot, category);

        // 设置边框颜色
        SetBorderColor(slot, category);

        // 设置叠层数
        if (slot.stackText != null)
        {
            bool showStacks = stacks > 0;
            slot.stackText.gameObject.SetActive(showStacks);
            if (showStacks) slot.stackText.text = stacks.ToString();
        }

        // 设置持续时间
        if (slot.durationText != null)
        {
            bool showDuration = duration > 0;
            slot.durationText.gameObject.SetActive(showDuration);
            if (showDuration) slot.durationText.text = duration.ToString();
        }

        // 设置工具提示
        if (slot.tooltipTitle != null) slot.tooltipTitle.text = GetEffectName(category);
        if (slot.tooltipDescription != null) slot.tooltipDescription.text = GetEffectDescription(category);

        // 设置事件触发器
        slot.SetupEventTrigger();
    }

    // 设置图标
    private void SetIcon(IconSlot slot, EffectCategory category)
    {
        if (config == null) return;

        Sprite icon = null;

        switch (category)
        {
            case EffectCategory.YangStack: icon = config.yangStackIcon; break;
            case EffectCategory.YinStack: icon = config.yinStackIcon; break;
            case EffectCategory.CounterStrike: icon = config.counterStrikeIcon; break;
            case EffectCategory.PlayerDot: icon = config.playerDotIcon; break;
            case EffectCategory.AttackDebuff: icon = config.extremeYangDebuffIcon; break;
            case EffectCategory.DefenseDebuff: icon = config.extremeYinDebuffIcon; break;
            case EffectCategory.EnemyDot: icon = config.enemyDotIcon; break;
            case EffectCategory.YangPenetration: icon = config.yangPenetrationIcon; break;
            case EffectCategory.YinCover: icon = config.yinCoverIcon; break;
        }

        if (icon != null)
        {
            slot.iconImage.sprite = icon;
            slot.iconImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Icon for effect {category} not found in BattleConfig!");
        }
    }

    // 设置边框颜色
    private void SetBorderColor(IconSlot slot, EffectCategory category)
    {
        Image border = slot.iconImage.GetComponentInParent<Image>();
        if (border != null)
        {
            switch (category)
            {
                case EffectCategory.YangStack: border.color = yangStackColor; break;
                case EffectCategory.YinStack: border.color = yinStackColor; break;
                case EffectCategory.CounterStrike: border.color = counterStrikeColor; break;
                case EffectCategory.PlayerDot: border.color = playerDotColor; break;
                case EffectCategory.AttackDebuff: border.color = attackDebuffColor; break;
                case EffectCategory.DefenseDebuff: border.color = defenseDebuffColor; break;
                case EffectCategory.EnemyDot: border.color = enemyDotColor; break;
                case EffectCategory.YangPenetration: border.color = yangPenetrationColor; break;
                case EffectCategory.YinCover: border.color = yinCoverColor; break;
            }
        }
    }

    // 更新效果
    private void UpdateEffect(IconSlot slot, int stacks = 0, int duration = 0)
    {
        // 更新叠层数
        if (slot.stackText != null)
        {
            bool showStacks = stacks > 0;
            slot.stackText.gameObject.SetActive(showStacks);
            if (showStacks) slot.stackText.text = stacks.ToString();
        }

        // 更新持续时间
        if (slot.durationText != null)
        {
            bool showDuration = duration > 0;
            slot.durationText.gameObject.SetActive(showDuration);
            if (showDuration) slot.durationText.text = duration.ToString();
        }
    }

    // 获取效果名称
    private string GetEffectName(EffectCategory category)
    {
        switch (category)
        {
            case EffectCategory.YangStack: return "Yang Stack";
            case EffectCategory.YinStack: return "Yin Stack";
            case EffectCategory.CounterStrike: return "Counter Strike";
            case EffectCategory.PlayerDot: return "Player DOT";
            case EffectCategory.AttackDebuff: return "Attack Debuff";
            case EffectCategory.DefenseDebuff: return "Defense Debuff";
            case EffectCategory.EnemyDot: return "Enemy DOT";
            case EffectCategory.YangPenetration: return "Yang Penetration";
            case EffectCategory.YinCover: return "Yin Cover";
            default: return "Unknown Effect";
        }
    }

    // 获取效果描述
    private string GetEffectDescription(EffectCategory category)
    {
        switch (category)
        {
            case EffectCategory.YangStack: return "Yang attribute stacking effect";
            case EffectCategory.YinStack: return "Yin attribute stacking effect";
            case EffectCategory.CounterStrike: return "Reflects part of damage taken";
            case EffectCategory.PlayerDot: return "Player takes damage over time";
            case EffectCategory.AttackDebuff: return "Reduces attack power";
            case EffectCategory.DefenseDebuff: return "Reduces defense power";
            case EffectCategory.EnemyDot: return "Enemy takes damage over time";
            case EffectCategory.YangPenetration: return "Yang penetration effect, increases damage";
            case EffectCategory.YinCover: return "Yin cover effect, reduces defense";
            default: return "Unknown effect";
        }
    }

    // 清除图标槽
    private void ClearSlot(IconSlot slot)
    {
        slot.iconImage.gameObject.SetActive(false);
        if (slot.stackText != null) slot.stackText.gameObject.SetActive(false);
        if (slot.durationText != null) slot.durationText.gameObject.SetActive(false);
        if (slot.tooltipPanel != null) slot.tooltipPanel.SetActive(false);
        slot.isActive = false;
    }
}