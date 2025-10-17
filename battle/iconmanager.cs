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

        // �����¼�������
        public void SetupEventTrigger()
        {
            if (iconImage == null) return;

            EventTrigger trigger = iconImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = iconImage.gameObject.AddComponent<EventTrigger>();
            }

            trigger.triggers.Clear();

            // �������¼�
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { ShowTooltip(); });
            trigger.triggers.Add(entryEnter);

            // ����뿪�¼�
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
        YangStack,          // ������ (���)
        YinStack,            // ������ (���)
        CounterStrike,       // ����Ч�� (���)
        PlayerDot,           // ����ܵ���DOT�˺� (���)
        AttackDebuff,        // �����½� (���)
        DefenseDebuff,       // �����½� (���)
        EnemyDot,            // �����ܵ���DOT�˺� (����)
        YangPenetration,     // ����͸Ч�� (����)
        YinCover             // ������Ч�� (����)
    }

    [Header("Player Icon Slots")]
    public List<IconSlot> playerIconSlots = new List<IconSlot>();

    [Header("Enemy Icon Slots")]
    public List<IconSlot> enemyIconSlots = new List<IconSlot>();

    [Header("Battle Config")]
    public BattleConfig config;

    [Header("Border Colors")]
    public Color yangStackColor = new Color(1f, 0.92f, 0.016f, 1f); // ��ɫ
    public Color yinStackColor = new Color(0.58f, 0f, 0.83f, 1f); // ��ɫ
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
        // ��ʼ������ͼ���Ϊ����״̬
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

    // ������Ч��
    public void AddPlayerEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        // ���Ч���Ѵ��ڣ�������
        if (activePlayerEffects.ContainsKey(category))
        {
            UpdatePlayerEffect(category, stacks, duration);
            return;
        }

        // �ҵ����е�ͼ���
        IconSlot slot = GetAvailableSlot(playerIconSlots);
        if (slot == null)
        {
            Debug.LogWarning("No available player effect slots!");
            return;
        }

        // ����Ч��
        SetEffect(slot, category, stacks, duration);
        activePlayerEffects.Add(category, slot);
    }

    // �������Ч��
    public void UpdatePlayerEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        if (!activePlayerEffects.ContainsKey(category)) return;

        IconSlot slot = activePlayerEffects[category];
        UpdateEffect(slot, stacks, duration);
    }

    // �Ƴ���Ҽ�Ч��
    public void RemovePlayerEffect(EffectCategory category)
    {
        if (!activePlayerEffects.ContainsKey(category)) return;

        IconSlot slot = activePlayerEffects[category];
        ClearSlot(slot);
        activePlayerEffects.Remove(category);
    }

    // ��ӵ���Ч��
    public void AddEnemyEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        // ���Ч���Ѵ��ڣ�������
        if (activeEnemyEffects.ContainsKey(category))
        {
            UpdateEnemyEffect(category, stacks, duration);
            return;
        }

        // �ҵ����е�ͼ���
        IconSlot slot = GetAvailableSlot(enemyIconSlots);
        if (slot == null)
        {
            Debug.LogWarning("No available enemy effect slots!");
            return;
        }

        // ����Ч��
        SetEffect(slot, category, stacks, duration);
        activeEnemyEffects.Add(category, slot);
    }

    // ���µ���Ч��
    public void UpdateEnemyEffect(EffectCategory category, int stacks = 0, int duration = 0)
    {
        if (!activeEnemyEffects.ContainsKey(category)) return;

        IconSlot slot = activeEnemyEffects[category];
        UpdateEffect(slot, stacks, duration);
    }

    // �Ƴ�����Ч��
    public void RemoveEnemyEffect(EffectCategory category)
    {
        if (!activeEnemyEffects.ContainsKey(category)) return;

        IconSlot slot = activeEnemyEffects[category];
        ClearSlot(slot);
        activeEnemyEffects.Remove(category);
    }

    // ����������Ч��
    public void ClearPlayerEffects()
    {
        var keys = activePlayerEffects.Keys.ToList();
        foreach (var key in keys)
        {
            ClearSlot(activePlayerEffects[key]);
            activePlayerEffects.Remove(key);
        }
    }

    // ������е���Ч��
    public void ClearEnemyEffects()
    {
        var keys = activeEnemyEffects.Keys.ToList();
        foreach (var key in keys)
        {
            ClearSlot(activeEnemyEffects[key]);
            activeEnemyEffects.Remove(key);
        }
    }

    // �������Ч��
    public void ClearAllEffects()
    {
        ClearPlayerEffects();
        ClearEnemyEffects();
    }

    // �ҵ����е�ͼ���
    private IconSlot GetAvailableSlot(List<IconSlot> slots)
    {
        foreach (var slot in slots)
        {
            if (!slot.isActive) return slot;
        }
        return null;
    }

    // ����Ч����ͼ���
    private void SetEffect(IconSlot slot, EffectCategory category, int stacks = 0, int duration = 0)
    {
        slot.effectCategory = category;
        slot.isActive = true;

        // ����ͼ��
        SetIcon(slot, category);

        // ���ñ߿���ɫ
        SetBorderColor(slot, category);

        // ���õ�����
        if (slot.stackText != null)
        {
            bool showStacks = stacks > 0;
            slot.stackText.gameObject.SetActive(showStacks);
            if (showStacks) slot.stackText.text = stacks.ToString();
        }

        // ���ó���ʱ��
        if (slot.durationText != null)
        {
            bool showDuration = duration > 0;
            slot.durationText.gameObject.SetActive(showDuration);
            if (showDuration) slot.durationText.text = duration.ToString();
        }

        // ���ù�����ʾ
        if (slot.tooltipTitle != null) slot.tooltipTitle.text = GetEffectName(category);
        if (slot.tooltipDescription != null) slot.tooltipDescription.text = GetEffectDescription(category);

        // �����¼�������
        slot.SetupEventTrigger();
    }

    // ����ͼ��
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

    // ���ñ߿���ɫ
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

    // ����Ч��
    private void UpdateEffect(IconSlot slot, int stacks = 0, int duration = 0)
    {
        // ���µ�����
        if (slot.stackText != null)
        {
            bool showStacks = stacks > 0;
            slot.stackText.gameObject.SetActive(showStacks);
            if (showStacks) slot.stackText.text = stacks.ToString();
        }

        // ���³���ʱ��
        if (slot.durationText != null)
        {
            bool showDuration = duration > 0;
            slot.durationText.gameObject.SetActive(showDuration);
            if (showDuration) slot.durationText.text = duration.ToString();
        }
    }

    // ��ȡЧ������
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

    // ��ȡЧ������
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

    // ���ͼ���
    private void ClearSlot(IconSlot slot)
    {
        slot.iconImage.gameObject.SetActive(false);
        if (slot.stackText != null) slot.stackText.gameObject.SetActive(false);
        if (slot.durationText != null) slot.durationText.gameObject.SetActive(false);
        if (slot.tooltipPanel != null) slot.tooltipPanel.SetActive(false);
        slot.isActive = false;
    }
}