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
        [Tooltip("��������Ŀ�ȣ����أ�")]
        public float width = 100f;
        [Tooltip("��������ĸ߶ȣ����أ�")]
        public float height = 50f;
        [Tooltip("�������������UIԪ�����ĵ�ƫ��")]
        public Vector2 offset = Vector2.zero;
        [Tooltip("�������������UIԪ�ص�λ��")]
        public Vector2 position = Vector2.zero;
        [Tooltip("�Ƿ�ʹ�þ���λ�ö����������UIԪ��")]
        public bool useAbsolutePosition = false;
    }

    [System.Serializable]
    public class PanelTimingSettings
    {
        [Header("Panel Timing Settings")]
        [Tooltip("�����ͣ�೤ʱ��󴥷���ʾ���룩")]
        public float triggerDelay = 0.3f;
        [Tooltip("����뿪���ӳ����ص�ʱ�䣨�룩")]
        public float hideDelay = 0.5f;
    }

    [Header("Target UI Components")]
    [Tooltip("���ڴ������������ʾ��UI���")]
    public RectTransform attackTarget;
    [Tooltip("���ڴ������������ʾ��UI���")]
    public RectTransform defenseTarget;
    [Tooltip("���ڴ���״̬�����ʾ��UI���")]
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
    public GameObject attackPanel;           // ���������
    public GameObject defensePanel;          // ���������
    public GameObject statePanel;            // ״̬���

    [Header("UI Elements")]
    public TMP_Text attackValueText;         // ��������ֵ�ı�
    public TMP_Text defenseValueText;        // ��������ֵ�ı�
    public TMP_Text yangPointsText;          // �������ı�
    public TMP_Text yinPointsText;           // �������ı�
    public TMP_Text stateNameText;           // ״̬�����ı�
    public TMP_Text stateDescriptionText;    // ״̬�����ı�

    [Header("Visual Debug")]
    public bool showTriggerArea = true;
    public bool showInGameView = false;
    public Color triggerAreaColor = new Color(1f, 1f, 0f, 0.3f);

    private WheelSystem wheelSystem;
    private Camera uiCamera;

    // ���������ر���
    private bool isAttackPanelVisible = false;
    private bool isMouseInAttackArea = false;
    private bool isWaitingToTriggerAttack = false;
    private float attackTriggerTimer = 0f;
    private float attackHideTimer = 0f;

    // ���������ر���
    private bool isDefensePanelVisible = false;
    private bool isMouseInDefenseArea = false;
    private bool isWaitingToTriggerDefense = false;
    private float defenseTriggerTimer = 0f;
    private float defenseHideTimer = 0f;

    // ״̬�����ر���
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
        // ��ȡWheelSystem���
        wheelSystem = FindObjectOfType<WheelSystem>();
        if (wheelSystem == null)
        {
            Debug.LogError("δ�ҵ�WheelSystem�����");
        }

        // ȷ����������ڿ�ʼʱ�������ص�
        HideAllPanels();
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            // ������λ��
            CheckMousePosition();

            // ���´��������ؼ�ʱ��
            UpdateTriggerTimers();
            UpdateHideTimers();
        }
    }

    /// <summary>
    /// �����������
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

        // �������м�ʱ��
        ResetAllTimers();
    }

    /// <summary>
    /// �������м�ʱ��
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

        // ��鹥����崥������
        bool wasInAttackArea = isMouseInAttackArea;
        isMouseInAttackArea = IsMouseInTriggerArea(mousePosition, attackTarget, attackTrigger);

        if (isMouseInAttackArea && !wasInAttackArea)
        {
            // �����빥�����򣬿�ʼ������ʱ
            StartAttackTriggerTimer();
        }
        else if (!isMouseInAttackArea && wasInAttackArea)
        {
            // ����뿪��������ȡ��������ʱ
            CancelAttackTriggerTimer();
        }

        // ��������崥������
        bool wasInDefenseArea = isMouseInDefenseArea;
        isMouseInDefenseArea = IsMouseInTriggerArea(mousePosition, defenseTarget, defenseTrigger);

        if (isMouseInDefenseArea && !wasInDefenseArea)
        {
            // ������������򣬿�ʼ������ʱ
            StartDefenseTriggerTimer();
        }
        else if (!isMouseInDefenseArea && wasInDefenseArea)
        {
            // ����뿪��������ȡ��������ʱ
            CancelDefenseTriggerTimer();
        }

        // ���״̬��崥������
        bool wasInStateArea = isMouseInStateArea;
        isMouseInStateArea = IsMouseInTriggerArea(mousePosition, stateTarget, stateTrigger);

        if (isMouseInStateArea && !wasInStateArea)
        {
            // ������״̬���򣬿�ʼ������ʱ
            StartStateTriggerTimer();
        }
        else if (!isMouseInStateArea && wasInStateArea)
        {
            // ����뿪״̬����ȡ��������ʱ
            CancelStateTriggerTimer();
        }
    }

    #region ������ʱ������

    private void StartAttackTriggerTimer()
    {
        if (attackTiming.triggerDelay > 0)
        {
            isWaitingToTriggerAttack = true;
            attackTriggerTimer = 0f;
        }
        else
        {
            // ���ӳ٣�������ʾ
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
            // ���ӳ٣�������ʾ
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
            // ���ӳ٣�������ʾ
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
        // ���¹�����崥����ʱ��
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

        // ���·�����崥����ʱ��
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

        // ����״̬��崥����ʱ��
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

    #region ���ؼ�ʱ������

    private void UpdateHideTimers()
    {
        // ���¹���������ؼ�ʱ��
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

        // ���·���������ؼ�ʱ��
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

        // ����״̬������ؼ�ʱ��
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

        // ��ȡUIԪ�����ĵ���������
        Vector2 elementCenter = target.position;

        // ���㴥�������������������
        Vector2 triggerCenter;
        if (triggerArea.useAbsolutePosition)
        {
            triggerCenter = triggerArea.position;
        }
        else
        {
            triggerCenter = elementCenter + triggerArea.offset;
        }

        // ������굽�����������ĵľ���
        float distanceX = Mathf.Abs(mousePosition.x - triggerCenter.x);
        float distanceY = Mathf.Abs(mousePosition.y - triggerCenter.y);

        // ����Ƿ��ھ���������
        return (distanceX <= triggerArea.width * 0.5f && distanceY <= triggerArea.height * 0.5f);
    }

    private void ShowAttackPanel()
    {
        if (attackPanel != null && !isAttackPanelVisible)
        {
            attackPanel.SetActive(true);
            isAttackPanelVisible = true;
            attackHideTimer = 0f;

            // ���¹����������
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

            // ���·����������
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

            // ����״̬�������
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

    // �������������¹����������
    public void UpdateAttackPanel()
    {
        if (wheelSystem == null || attackValueText == null || yangPointsText == null) return;

        // ��ȡ��ǰ��������������
        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;

        // �����ֵ
        float diff = yangPoints - yinPoints;

        // ����״̬���㹥������ʹ��ʵ��С��ֵ���㣩
        float finalAttack = CalculateFinalAttribute(yangPoints, yinPoints, diff, "attack");

        // ����UI����������ʾС������������ʾΪ������
        attackValueText.text = finalAttack.ToString("F2");
        yangPointsText.text = Mathf.FloorToInt(yangPoints).ToString() + "/" + Mathf.FloorToInt(wheelSystem.MaxPoints).ToString(); // ��ʾΪ������ʽ X/Y
    }

    // �������������·����������
    public void UpdateDefensePanel()
    {
        if (wheelSystem == null || defenseValueText == null || yinPointsText == null) return;

        // ��ȡ��ǰ��������������
        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;

        // �����ֵ
        float diff = yangPoints - yinPoints;

        // ����״̬�����������ʹ��ʵ��С��ֵ���㣩
        float finalDefense = CalculateFinalAttribute(yangPoints, yinPoints, diff, "defense");

        // ����UI����������ʾС������������ʾΪ������
        defenseValueText.text = finalDefense.ToString("F2");
        yinPointsText.text = Mathf.FloorToInt(yinPoints).ToString() + "/" + Mathf.FloorToInt(wheelSystem.MaxPoints).ToString(); // ��ʾΪ������ʽ X/Y
    }

    // ��������������״̬�������
    public void UpdateStatePanel()
    {
        if (wheelSystem == null || stateNameText == null || stateDescriptionText == null) return;

        // ��ȡ��ǰ��������������
        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;

        // �����ֵ
        float diff = yangPoints - yinPoints;

        // ��ȡ״̬���ƺ�����
        string stateName = GetStateName(diff);
        string stateDescription = GetStateDescription(diff, yangPoints, yinPoints);

        // ����UI
        stateNameText.text = stateName;
        stateDescriptionText.text = stateDescription;
    }

    private float CalculateFinalAttribute(float yangPoints, float yinPoints, float diff, string attributeType)
    {
        // ����״̬ѡ����
        float attackMultiplier = 1f;
        float defenseMultiplier = 1f;

        float absDiff = Mathf.Abs(diff);

        if (absDiff < 1f)
        {
            // ƽ��
            attackMultiplier = 1.25f;
            defenseMultiplier = 1.25f;
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            // �ٽ���
            attackMultiplier = 1.75f;
            defenseMultiplier = 1.25f;
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            // �ٽ���
            attackMultiplier = 1.25f;
            defenseMultiplier = 1.75f;
        }
        else if (diff > 2.5f && diff < 5f)
        {
            // ��ʢ
            attackMultiplier = 2.75f;
            defenseMultiplier = 1.25f;
        }
        else if (diff < -2.5f && diff > -5f)
        {
            // ��ʢ
            attackMultiplier = 1.0f;
            defenseMultiplier = 2.5f;
        }
        else if (diff >= 5f && diff <= 7f)
        {
            // ������
            attackMultiplier = 4.5f;
            defenseMultiplier = 0.5f;
        }
        else if (diff <= -5f && diff >= -7f)
        {
            // ������
            attackMultiplier = 1.0f;
            defenseMultiplier = 3.0f;
        }
        else if (absDiff > 7f && absDiff <= 10f)
        {
            // ������
            attackMultiplier = 7f;
            defenseMultiplier = 7f;
        }

        // ������Ӧ������ֵ��ʹ��ʵ��С��ֵ���㣩
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
        
        // ����UIԪ��ԭʼ�߽磨��ɫ��
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 center = target.position;
        Vector3 size = new Vector3(target.rect.width, target.rect.height, 0);
        Gizmos.DrawWireCube(center, size);
        
        // �����Զ��崥�����򣨻�ɫ��
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
        
        // �������
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
    
    // ��Scene��ͼ��ʵʱ����
    void OnValidate()
    {
        // �����ı�ʱˢ��Scene��ͼ
        if (!Application.isPlaying)
        {
            SceneView.RepaintAll();
        }
    }
#endif

    // ��Game��ͼ�л��Ƶ�����Ϣ
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

        // ��ȡUIԪ�����ĵ���������
        Vector2 elementCenter = target.position;

        // ���㴥�������������������
        Vector2 triggerCenter;
        if (triggerArea.useAbsolutePosition)
        {
            triggerCenter = triggerArea.position;
        }
        else
        {
            triggerCenter = elementCenter + triggerArea.offset;
        }

        // ת��Ϊ��Ļ����
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, triggerCenter);
        screenCenter.y = Screen.height - screenCenter.y; // Unity GUI��Y���Ƿ���

        // ���ƴ�������
        float halfWidth = triggerArea.width * 0.5f;
        float halfHeight = triggerArea.height * 0.5f;

        // ��������
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, triggerAreaColor);
        texture.Apply();

        // �����������
        GUI.color = triggerAreaColor;
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerArea.width, triggerArea.height), texture);

        // ���Ʊ߿�
        GUI.color = new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, 1f);
        // �ϱ߿�
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerArea.width, 2), Texture2D.whiteTexture);
        // �±߿�
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y + halfHeight - 2, triggerArea.width, 2), Texture2D.whiteTexture);
        // ��߿�
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, 2, triggerArea.height), Texture2D.whiteTexture);
        // �ұ߿�
        GUI.DrawTexture(new Rect(screenCenter.x + halfWidth - 2, screenCenter.y - halfHeight, 2, triggerArea.height), Texture2D.whiteTexture);

        // ����
        DestroyImmediate(texture);
    }
}