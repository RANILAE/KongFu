using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class YinYangTooltipManager : MonoBehaviour
{
    [System.Serializable]
    public class TriggerArea
    {
        [Header("Trigger Area Settings")]
        [Tooltip("触发区域的宽度（像素）")]
        public float width = 100f;
        [Tooltip("触发区域的高度（像素）")]
        public float height = 50f;
        [Tooltip("触发区域相对于UI元素中心的偏移")]
        public Vector2 offset = Vector2.zero;
        [Tooltip("触发区域相对于UI元素的位置")]
        public Vector2 position = Vector2.zero;
        [Tooltip("是否使用绝对位置而不是相对于UI元素")]
        public bool useAbsolutePosition = false;
    }

    [System.Serializable]
    public class PanelTimingSettings
    {
        [Header("Panel Timing Settings")]
        [Tooltip("鼠标悬停多长时间后触发显示（秒）")]
        public float triggerDelay = 0.3f;
        [Tooltip("鼠标离开后延迟隐藏的时间（秒）")]
        public float hideDelay = 0.5f;
    }

    [Header("Target UI Components")]
    [Tooltip("用于触发攻击面板显示的UI组件")]
    public RectTransform attackTarget;
    [Tooltip("用于触发防御面板显示的UI组件")]
    public RectTransform defenseTarget;
    [Tooltip("用于触发状态面板显示的UI组件")]
    public RectTransform stateTarget;

    [Header("Individual Trigger Areas")]
    public TriggerArea attackTrigger = new TriggerArea() { width = 120f, height = 60f, offset = Vector2.zero };
    public TriggerArea defenseTrigger = new TriggerArea() { width = 120f, height = 60f, offset = Vector2.zero };
    public TriggerArea stateTrigger = new TriggerArea() { width = 120f, height = 60f, offset = Vector2.zero };

    [Header("Individual Panel Timing Settings")]
    public PanelTimingSettings attackTiming = new PanelTimingSettings() { triggerDelay = 0.3f, hideDelay = 0.5f };
    public PanelTimingSettings defenseTiming = new PanelTimingSettings() { triggerDelay = 0.3f, hideDelay = 0.5f };
    public PanelTimingSettings stateTiming = new PanelTimingSettings() { triggerDelay = 0.3f, hideDelay = 0.5f };

    [Header("Tooltip Panels")]
    public GameObject attackPanel;           // 攻击力面板
    public GameObject defensePanel;          // 防御力面板
    public GameObject statePanel;            // 状态面板

    [Header("UI Elements")]
    public TMP_Text attackValueText;         // 攻击力数值文本
    public TMP_Text defenseValueText;        // 防御力数值文本
    public TMP_Text yangPointsText;          // 阳点数文本
    public TMP_Text yinPointsText;           // 阴点数文本
    public TMP_Text stateNameText;           // 状态名称文本
    public TMP_Text stateDescriptionText;    // 状态描述文本

    [Header("Visual Debug")]
    public bool showTriggerArea = true;
    public bool showInGameView = false;
    public Color triggerAreaColor = new Color(1f, 1f, 0f, 0.3f);

    private WheelSystem wheelSystem;
    private Camera uiCamera;

    // 攻击面板相关变量
    private bool isAttackPanelVisible = false;
    private bool isMouseInAttackArea = false;
    private bool isWaitingToTriggerAttack = false;
    private float attackTriggerTimer = 0f;
    private float attackHideTimer = 0f;

    // 防御面板相关变量
    private bool isDefensePanelVisible = false;
    private bool isMouseInDefenseArea = false;
    private bool isWaitingToTriggerDefense = false;
    private float defenseTriggerTimer = 0f;
    private float defenseHideTimer = 0f;

    // 状态面板相关变量
    private bool isStatePanelVisible = false;
    private bool isMouseInStateArea = false;
    private bool isWaitingToTriggerState = false;
    private float stateTriggerTimer = 0f;
    private float stateHideTimer = 0f;

    void Awake()
    {
        uiCamera = GetComponentInParent<Canvas>()?.worldCamera ?? Camera.main;
    }

    void Start()
    {
        // 获取WheelSystem组件
        wheelSystem = FindObjectOfType<WheelSystem>();
        if (wheelSystem == null)
        {
            Debug.LogError("未找到WheelSystem组件！");
        }

        // 确保所有面板在开始时都是隐藏的
        HideAllPanels();
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            // 检查鼠标位置
            CheckMousePosition();

            // 更新触发和隐藏计时器
            UpdateTriggerTimers();
            UpdateHideTimers();
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    private void HideAllPanels()
    {
        if (attackPanel != null)
        {
            attackPanel.SetActive(false);
            isAttackPanelVisible = false;
        }

        if (defensePanel != null)
        {
            defensePanel.SetActive(false);
            isDefensePanelVisible = false;
        }

        if (statePanel != null)
        {
            statePanel.SetActive(false);
            isStatePanelVisible = false;
        }

        // 重置所有计时器
        ResetAllTimers();
    }

    /// <summary>
    /// 重置所有计时器
    /// </summary>
    private void ResetAllTimers()
    {
        isWaitingToTriggerAttack = false;
        attackTriggerTimer = 0f;
        attackHideTimer = 0f;

        isWaitingToTriggerDefense = false;
        defenseTriggerTimer = 0f;
        defenseHideTimer = 0f;

        isWaitingToTriggerState = false;
        stateTriggerTimer = 0f;
        stateHideTimer = 0f;
    }

    private void CheckMousePosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        // 检查攻击面板触发区域
        bool wasInAttackArea = isMouseInAttackArea;
        isMouseInAttackArea = IsMouseInTriggerArea(mousePosition, attackTarget, attackTrigger);

        if (isMouseInAttackArea && !wasInAttackArea)
        {
            // 鼠标进入攻击区域，开始触发计时
            StartAttackTriggerTimer();
        }
        else if (!isMouseInAttackArea && wasInAttackArea)
        {
            // 鼠标离开攻击区域，取消触发计时
            CancelAttackTriggerTimer();
        }

        // 检查防御面板触发区域
        bool wasInDefenseArea = isMouseInDefenseArea;
        isMouseInDefenseArea = IsMouseInTriggerArea(mousePosition, defenseTarget, defenseTrigger);

        if (isMouseInDefenseArea && !wasInDefenseArea)
        {
            // 鼠标进入防御区域，开始触发计时
            StartDefenseTriggerTimer();
        }
        else if (!isMouseInDefenseArea && wasInDefenseArea)
        {
            // 鼠标离开防御区域，取消触发计时
            CancelDefenseTriggerTimer();
        }

        // 检查状态面板触发区域
        bool wasInStateArea = isMouseInStateArea;
        isMouseInStateArea = IsMouseInTriggerArea(mousePosition, stateTarget, stateTrigger);

        if (isMouseInStateArea && !wasInStateArea)
        {
            // 鼠标进入状态区域，开始触发计时
            StartStateTriggerTimer();
        }
        else if (!isMouseInStateArea && wasInStateArea)
        {
            // 鼠标离开状态区域，取消触发计时
            CancelStateTriggerTimer();
        }
    }

    #region 触发计时器管理

    private void StartAttackTriggerTimer()
    {
        if (attackTiming.triggerDelay > 0)
        {
            isWaitingToTriggerAttack = true;
            attackTriggerTimer = 0f;
        }
        else
        {
            // 无延迟，立即显示
            ShowAttackPanel();
        }
    }

    private void CancelAttackTriggerTimer()
    {
        isWaitingToTriggerAttack = false;
        attackTriggerTimer = 0f;
    }

    private void StartDefenseTriggerTimer()
    {
        if (defenseTiming.triggerDelay > 0)
        {
            isWaitingToTriggerDefense = true;
            defenseTriggerTimer = 0f;
        }
        else
        {
            // 无延迟，立即显示
            ShowDefensePanel();
        }
    }

    private void CancelDefenseTriggerTimer()
    {
        isWaitingToTriggerDefense = false;
        defenseTriggerTimer = 0f;
    }

    private void StartStateTriggerTimer()
    {
        if (stateTiming.triggerDelay > 0)
        {
            isWaitingToTriggerState = true;
            stateTriggerTimer = 0f;
        }
        else
        {
            // 无延迟，立即显示
            ShowStatePanel();
        }
    }

    private void CancelStateTriggerTimer()
    {
        isWaitingToTriggerState = false;
        stateTriggerTimer = 0f;
    }

    private void UpdateTriggerTimers()
    {
        // 更新攻击面板触发计时器
        if (isWaitingToTriggerAttack)
        {
            attackTriggerTimer += Time.deltaTime;
            if (attackTriggerTimer >= attackTiming.triggerDelay)
            {
                ShowAttackPanel();
                isWaitingToTriggerAttack = false;
                attackTriggerTimer = 0f;
            }
        }

        // 更新防御面板触发计时器
        if (isWaitingToTriggerDefense)
        {
            defenseTriggerTimer += Time.deltaTime;
            if (defenseTriggerTimer >= defenseTiming.triggerDelay)
            {
                ShowDefensePanel();
                isWaitingToTriggerDefense = false;
                defenseTriggerTimer = 0f;
            }
        }

        // 更新状态面板触发计时器
        if (isWaitingToTriggerState)
        {
            stateTriggerTimer += Time.deltaTime;
            if (stateTriggerTimer >= stateTiming.triggerDelay)
            {
                ShowStatePanel();
                isWaitingToTriggerState = false;
                stateTriggerTimer = 0f;
            }
        }
    }

    #endregion

    #region 隐藏计时器管理

    private void UpdateHideTimers()
    {
        // 更新攻击面板隐藏计时器
        if (!isMouseInAttackArea && isAttackPanelVisible)
        {
            attackHideTimer += Time.deltaTime;
            if (attackHideTimer >= attackTiming.hideDelay)
            {
                HideAttackPanel();
            }
        }
        else if (isMouseInAttackArea)
        {
            attackHideTimer = 0f;
        }

        // 更新防御面板隐藏计时器
        if (!isMouseInDefenseArea && isDefensePanelVisible)
        {
            defenseHideTimer += Time.deltaTime;
            if (defenseHideTimer >= defenseTiming.hideDelay)
            {
                HideDefensePanel();
            }
        }
        else if (isMouseInDefenseArea)
        {
            defenseHideTimer = 0f;
        }

        // 更新状态面板隐藏计时器
        if (!isMouseInStateArea && isStatePanelVisible)
        {
            stateHideTimer += Time.deltaTime;
            if (stateHideTimer >= stateTiming.hideDelay)
            {
                HideStatePanel();
            }
        }
        else if (isMouseInStateArea)
        {
            stateHideTimer = 0f;
        }
    }

    #endregion

    private bool IsMouseInTriggerArea(Vector2 mousePosition, RectTransform target, TriggerArea triggerArea)
    {
        if (target == null) return false;

        // 获取UI元素中心的世界坐标
        Vector2 elementCenter = target.position;

        // 计算触发区域的世界坐标中心
        Vector2 triggerCenter;
        if (triggerArea.useAbsolutePosition)
        {
            triggerCenter = triggerArea.position;
        }
        else
        {
            triggerCenter = elementCenter + triggerArea.offset;
        }

        // 计算鼠标到触发区域中心的距离
        float distanceX = Mathf.Abs(mousePosition.x - triggerCenter.x);
        float distanceY = Mathf.Abs(mousePosition.y - triggerCenter.y);

        // 检查是否在矩形区域内
        return (distanceX <= triggerArea.width * 0.5f && distanceY <= triggerArea.height * 0.5f);
    }

    private void ShowAttackPanel()
    {
        if (attackPanel != null && !isAttackPanelVisible)
        {
            attackPanel.SetActive(true);
            isAttackPanelVisible = true;
            attackHideTimer = 0f;

            // 更新攻击面板内容
            UpdateAttackPanel();
        }
    }

    private void HideAttackPanel()
    {
        if (attackPanel != null && isAttackPanelVisible)
        {
            attackPanel.SetActive(false);
            isAttackPanelVisible = false;
            attackHideTimer = 0f;
            CancelAttackTriggerTimer();
        }
    }

    private void ShowDefensePanel()
    {
        if (defensePanel != null && !isDefensePanelVisible)
        {
            defensePanel.SetActive(true);
            isDefensePanelVisible = true;
            defenseHideTimer = 0f;

            // 更新防御面板内容
            UpdateDefensePanel();
        }
    }

    private void HideDefensePanel()
    {
        if (defensePanel != null && isDefensePanelVisible)
        {
            defensePanel.SetActive(false);
            isDefensePanelVisible = false;
            defenseHideTimer = 0f;
            CancelDefenseTriggerTimer();
        }
    }

    private void ShowStatePanel()
    {
        if (statePanel != null && !isStatePanelVisible)
        {
            statePanel.SetActive(true);
            isStatePanelVisible = true;
            stateHideTimer = 0f;

            // 更新状态面板内容
            UpdateStatePanel();
        }
    }

    private void HideStatePanel()
    {
        if (statePanel != null && isStatePanelVisible)
        {
            statePanel.SetActive(false);
            isStatePanelVisible = false;
            stateHideTimer = 0f;
            CancelStateTriggerTimer();
        }
    }

    // 公开方法：更新攻击面板内容
    public void UpdateAttackPanel()
    {
        if (wheelSystem == null || attackValueText == null || yangPointsText == null) return;

        // 获取当前阳点数和阴点数
        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;

        // 计算差值
        float diff = yangPoints - yinPoints;

        // 根据状态计算攻击力（使用实际小数值计算）
        float finalAttack = CalculateFinalAttribute(yangPoints, yinPoints, diff, "attack");

        // 更新UI（攻击力显示小数，阳点数显示为整数）
        attackValueText.text = finalAttack.ToString("F2");
        yangPointsText.text = Mathf.FloorToInt(yangPoints).ToString() + "/" + Mathf.FloorToInt(wheelSystem.MaxPoints).ToString(); // 显示为整数格式 X/Y
    }

    // 公开方法：更新防御面板内容
    public void UpdateDefensePanel()
    {
        if (wheelSystem == null || defenseValueText == null || yinPointsText == null) return;

        // 获取当前阳点数和阴点数
        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;

        // 计算差值
        float diff = yangPoints - yinPoints;

        // 根据状态计算防御力（使用实际小数值计算）
        float finalDefense = CalculateFinalAttribute(yangPoints, yinPoints, diff, "defense");

        // 更新UI（防御力显示小数，阴点数显示为整数）
        defenseValueText.text = finalDefense.ToString("F2");
        yinPointsText.text = Mathf.FloorToInt(yinPoints).ToString() + "/" + Mathf.FloorToInt(wheelSystem.MaxPoints).ToString(); // 显示为整数格式 X/Y
    }

    // 公开方法：更新状态面板内容
    public void UpdateStatePanel()
    {
        if (wheelSystem == null || stateNameText == null || stateDescriptionText == null) return;

        // 获取当前阳点数和阴点数
        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;

        // 计算差值
        float diff = yangPoints - yinPoints;

        // 获取状态名称和描述
        string stateName = GetStateName(diff);
        string stateDescription = GetStateDescription(diff, yangPoints, yinPoints);

        // 更新UI
        stateNameText.text = stateName;
        stateDescriptionText.text = stateDescription;
    }

    private float CalculateFinalAttribute(float yangPoints, float yinPoints, float diff, string attributeType)
    {
        // 根据状态选择倍率
        float attackMultiplier = 1f;
        float defenseMultiplier = 1f;

        float absDiff = Mathf.Abs(diff);

        if (absDiff < 1f)
        {
            // 平衡
            attackMultiplier = 1.25f;
            defenseMultiplier = 1.25f;
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            // 临界阳
            attackMultiplier = 1.75f;
            defenseMultiplier = 1.25f;
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            // 临界阴
            attackMultiplier = 1.25f;
            defenseMultiplier = 1.75f;
        }
        else if (diff > 2.5f && diff < 5f)
        {
            // 阳盛
            attackMultiplier = 2.75f;
            defenseMultiplier = 1.25f;
        }
        else if (diff < -2.5f && diff > -5f)
        {
            // 阴盛
            attackMultiplier = 1.0f;
            defenseMultiplier = 2.5f;
        }
        else if (diff >= 5f && diff <= 7f)
        {
            // 极端阳
            attackMultiplier = 4.5f;
            defenseMultiplier = 0.5f;
        }
        else if (diff <= -5f && diff >= -7f)
        {
            // 极端阴
            attackMultiplier = 1.0f;
            defenseMultiplier = 3.0f;
        }
        else if (absDiff > 7f && absDiff <= 10f)
        {
            // 究极气
            attackMultiplier = 7f;
            defenseMultiplier = 7f;
        }

        // 返回相应的属性值（使用实际小数值计算）
        return attributeType == "attack" ? yangPoints * attackMultiplier : yinPoints * defenseMultiplier;
    }

    private string GetStateName(float diff)
    {
        float absDiff = Mathf.Abs(diff);

        if (absDiff < 1f) return "Balance";
        if (diff >= 1f && diff <= 2.5f) return "Critical Yang";
        if (diff <= -1f && diff >= -2.5f) return "Critical Yin";
        if (diff > 2.5f && diff < 5f) return "Yang Prosperity";
        if (diff < -2.5f && diff > -5f) return "Yin Prosperity";
        if (diff >= 5f && diff <= 7f) return "Extreme Yang";
        if (diff <= -5f && diff >= -7f) return "Extreme Yin";
        if (absDiff > 7f && absDiff <= 10f) return "Ultimate Qi";

        return "Unknown State";
    }

    private string GetStateDescription(float diff, float yangPoints, float yinPoints)
    {
        float absDiff = Mathf.Abs(diff);

        if (absDiff < 1f)
        {
            return "Player recovers 5 HP (Continuous use won't trigger healing effect) CD: 3 turns";
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            return "Apply Yang Penetration BUFF to enemy (Permanently retains stacks until player uses Extreme Yang to detonate)";
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            return "Apply Yin Cover BUFF to enemy (Permanently retains stacks until player uses Extreme Yin to detonate)";
        }
        else if (diff > 2.5f && diff < 5f)
        {
            float dotDamage = yangPoints / 2f;
            return $"Apply DOT damage of {dotDamage:F1} for 2 turns";
        }
        else if (diff < -2.5f && diff > -5f)
        {
            return "Add Counter Strike effect to defense (Counter Strike: If enemy's attack is less than player's defense, enemy takes damage equal to player's defense)";
        }
        else if (diff >= 5f && diff <= 7f)
        {
            return "Next turn attack reduced by half (lasts until next turn ends), requires at least 3 Yang Penetration stacks to use. Damage equals attack plus layers*2";
        }
        else if (diff <= -5f && diff >= -7f)
        {
            return "Next turn defense reduced by half (lasts until next turn ends), adds Counter Strike effect (only effective this turn). If counter strike occurs, damage is increased, and enemy's attack is reduced for 2 turns";
        }
        else if (absDiff > 7f && absDiff <= 10f)
        {
            return "Player's health reduces to 1 point and gains Counter Strike effect for 3 turns";
        }

        return "No special effect";
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showTriggerArea) return;
        
        DrawTriggerAreaGizmos(attackTarget, attackTrigger, "Attack Trigger");
        DrawTriggerAreaGizmos(defenseTarget, defenseTrigger, "Defense Trigger");
        DrawTriggerAreaGizmos(stateTarget, stateTrigger, "State Trigger");
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showTriggerArea) return;
        
        DrawTriggerAreaGizmos(attackTarget, attackTrigger, "Attack Trigger");
        DrawTriggerAreaGizmos(defenseTarget, defenseTrigger, "Defense Trigger");
        DrawTriggerAreaGizmos(stateTarget, stateTrigger, "State Trigger");
    }
    
    private void DrawTriggerAreaGizmos(RectTransform target, TriggerArea triggerArea, string label)
    {
        if (target == null) return;
        
        // 绘制UI元素原始边界（绿色）
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 center = target.position;
        Vector3 size = new Vector3(target.rect.width, target.rect.height, 0);
        Gizmos.DrawWireCube(center, size);
        
        // 绘制自定义触发区域（黄色）
        Vector2 triggerCenter;
        if (triggerArea.useAbsolutePosition)
        {
            triggerCenter = triggerArea.position;
        }
        else
        {
            triggerCenter = (Vector2)center + triggerArea.offset;
        }
        
        Vector3 triggerSize = new Vector3(triggerArea.width, triggerArea.height, 0);
        
        Gizmos.color = triggerAreaColor;
        Gizmos.DrawWireCube(triggerCenter, triggerSize);
        
        // 绘制填充
        Handles.color = new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, triggerAreaColor.a * 0.3f);
        Handles.DrawSolidRectangleWithOutline(
            new Vector3[] {
                new Vector3(triggerCenter.x - triggerArea.width/2, triggerCenter.y - triggerArea.height/2, 0),
                new Vector3(triggerCenter.x + triggerArea.width/2, triggerCenter.y - triggerArea.height/2, 0),
                new Vector3(triggerCenter.x + triggerArea.width/2, triggerCenter.y + triggerArea.height/2, 0),
                new Vector3(triggerCenter.x - triggerArea.width/2, triggerCenter.y + triggerArea.height/2, 0)
            },
            new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, triggerAreaColor.a * 0.3f),
            triggerAreaColor
        );
    }
    
    // 在Scene视图中实时更新
    void OnValidate()
    {
        // 参数改变时刷新Scene视图
        if (!Application.isPlaying)
        {
            SceneView.RepaintAll();
        }
    }
#endif

    // 在Game视图中绘制调试信息
    void OnGUI()
    {
        if (!Application.isPlaying || !showInGameView || !showTriggerArea) return;

        DrawTriggerAreaInGame(attackTarget, attackTrigger, "Attack");
        DrawTriggerAreaInGame(defenseTarget, defenseTrigger, "Defense");
        DrawTriggerAreaInGame(stateTarget, stateTrigger, "State");
    }

    private void DrawTriggerAreaInGame(RectTransform target, TriggerArea triggerArea, string label)
    {
        if (target == null || uiCamera == null) return;

        // 获取UI元素中心的世界坐标
        Vector2 elementCenter = target.position;

        // 计算触发区域的世界坐标中心
        Vector2 triggerCenter;
        if (triggerArea.useAbsolutePosition)
        {
            triggerCenter = triggerArea.position;
        }
        else
        {
            triggerCenter = elementCenter + triggerArea.offset;
        }

        // 转换为屏幕坐标
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, triggerCenter);
        screenCenter.y = Screen.height - screenCenter.y; // Unity GUI的Y轴是反的

        // 绘制触发区域
        float halfWidth = triggerArea.width * 0.5f;
        float halfHeight = triggerArea.height * 0.5f;

        // 创建纹理
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, triggerAreaColor);
        texture.Apply();

        // 绘制填充区域
        GUI.color = triggerAreaColor;
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerArea.width, triggerArea.height), texture);

        // 绘制边框
        GUI.color = new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, 1f);
        // 上边框
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerArea.width, 2), Texture2D.whiteTexture);
        // 下边框
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y + halfHeight - 2, triggerArea.width, 2), Texture2D.whiteTexture);
        // 左边框
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, 2, triggerArea.height), Texture2D.whiteTexture);
        // 右边框
        GUI.DrawTexture(new Rect(screenCenter.x + halfWidth - 2, screenCenter.y - halfHeight, 2, triggerArea.height), Texture2D.whiteTexture);

        // 清理
        DestroyImmediate(texture);
    }
}