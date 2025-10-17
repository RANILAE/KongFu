using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ���������ռ�

/// <summary>
/// ������ͼ��ʾ��� - ������UI�ϣ���ʾ�����»غϵ��ж���ͼ
/// ʹ�� EventTrigger ʵ�������ͣ���������� Canvas Render Mode
/// </summary>
[RequireComponent(typeof(EventTrigger))]
public class EnemyIntentDisplay : MonoBehaviour
{
    [System.Serializable]
    public class EnemyIntentTooltips
    {
        [Header("Attack Tooltip")]
        [TextArea(2, 4)] public string attackTooltip = "���������˽���������˺�";

        [Header("Defend Tooltip")]
        [TextArea(2, 4)] public string defendTooltip = "���������˽����ٻ�������һ���˺�";

        [Header("Charge Tooltip")]
        [TextArea(2, 4)] public string chargeTooltip = "���������˽�ǿ����������";
    }

    [Header("UI References")]
    public Image intentIcon; // ��ʾ��ͼͼ���Image���
    public GameObject tooltipPanel; // �����ͣʱ��ʾ����ʾ���
    public TMP_Text tooltipText; // ��ʾ���֣�TextMeshPro��

    [Header("Intent Icons - ����Inspector�з���")]
    public Sprite attackIcon;   // ����ͼ��
    public Sprite defendIcon;   // ����ͼ��
    public Sprite chargeIcon;   // ����ͼ��

    [Header("Intent Tooltips for Different Enemies")]
    public EnemyIntentTooltips defaultEnemyTooltips;
    public EnemyIntentTooltips secondEnemyTooltips;
    public EnemyIntentTooltips thirdEnemyTooltips;

    private EventTrigger eventTrigger;
    private EnemyManager.EnemyAction.Type currentIntentType = EnemyManager.EnemyAction.Type.Attack; // ��ʼ��ΪĬ��ֵ
    private EnemyManager.EnemyType currentEnemyType = EnemyManager.EnemyType.Default; // ��ʼ��ΪĬ�ϵ�������

    private void Start()
    {
        // ��ʼ�� EventTrigger
        InitializeEventTrigger();

        // ȷ����ʼ״̬������ͼ���Tooltip
        if (intentIcon != null && intentIcon.gameObject != null)
        {
            intentIcon.gameObject.SetActive(false); // ����ͼ�� GameObject
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
            Debug.LogWarning("EnemyIntentDisplay: TooltipPanelδ����");
        }

        // ���ͼ����Դ
        if (attackIcon == null || defendIcon == null || chargeIcon == null)
        {
            Debug.LogWarning("EnemyIntentDisplay: ����Inspector�з���������ͼͼ��");
        }
    }

    private void InitializeEventTrigger()
    {
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        // ��������¼��������ظ���ӣ�
        eventTrigger.triggers.Clear();

        // ��� PointerEnter �¼��������룩
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnPointerEnterHandler((PointerEventData)data); }); // ��ȷת����������
        eventTrigger.triggers.Add(enterEntry);

        // ��� PointerExit �¼�������뿪��
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnPointerExitHandler((PointerEventData)data); }); // ��ȷת����������
        eventTrigger.triggers.Add(exitEntry);
    }

    /// <summary>
    /// ������ʾ�ĵ�����ͼ (ָ�����������Ի�ȡ��ȷ�ı�)
    /// </summary>
    /// <param name="intentType">���˵��ж�����</param>
    /// <param name="enemyType">���˵�����</param>
    public void UpdateIntent(EnemyManager.EnemyAction.Type intentType, EnemyManager.EnemyType enemyType)
    {
        Debug.Log($"<color=green>EnemyIntentDisplay: UpdateIntent called with Type: {intentType}, Enemy: {enemyType}</color>");
        currentIntentType = intentType;
        currentEnemyType = enemyType;

        // ǿ�Ƹ���ͼ����ı���Ȼ����ʾ
        if (intentIcon != null && intentIcon.gameObject != null)
        {
            Sprite targetSprite = attackIcon; // Ĭ��ͼ��
            string targetTooltip = "δ֪��ͼ"; // Ĭ���ı�

            // ѡ���Ӧ���ı�����
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

            // ������ͼ����ѡ��ͼ����ı�
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
                    // ����Ĭ��ֵ
                    Debug.LogWarning($"EnemyIntentDisplay: Unknown intent type {intentType}, using Attack as default.");
                    break;
            }

            // 1. ����ͼ��
            intentIcon.sprite = targetSprite;
            // 2. ���� Tooltip �ı�
            if (tooltipText != null)
            {
                tooltipText.text = targetTooltip;
                Debug.Log($"EnemyIntentDisplay: Tooltip text set to '{targetTooltip}'");
            }
            else
            {
                Debug.LogWarning("EnemyIntentDisplay: tooltipText is null, cannot set tooltip.");
            }
            // 3. ��ʾͼ�� GameObject
            intentIcon.gameObject.SetActive(true);
            Debug.Log($"EnemyIntentDisplay: Icon GameObject activated. Sprite assigned: {(intentIcon.sprite != null ? intentIcon.sprite.name : "NULL")}");
        }
        else
        {
            Debug.LogError("EnemyIntentDisplay: Cannot update intent because intentIcon or its GameObject is null.");
        }
    }

    /// <summary>
    /// �����ͼ��ʾ
    /// </summary>
    public void ClearIntent()
    {
        if (intentIcon != null && intentIcon.gameObject != null)
        {
            intentIcon.gameObject.SetActive(false);
            Debug.Log("EnemyIntentDisplay: Intent icon cleared and hidden.");
        }
        currentIntentType = EnemyManager.EnemyAction.Type.Attack; // ����ΪĬ��ֵ
        currentEnemyType = EnemyManager.EnemyType.Default; // ����ΪĬ�ϵ���
        Debug.Log("EnemyIntentDisplay: Intent type and enemy type reset to defaults.");
    }

    /// <summary>
    /// ��ȡ��ǰ��ʾ����ͼ����
    /// </summary>
    public EnemyManager.EnemyAction.Type GetCurrentIntent()
    {
        return currentIntentType;
    }

    /// <summary>
    /// ��ȡ��ǰ��ʾ�ĵ�������
    /// </summary>
    public EnemyManager.EnemyType GetCurrentEnemyType()
    {
        return currentEnemyType;
    }

    /// <summary>
    /// ������ʱ��ʾTooltip����EventTrigger���ã�
    /// </summary>
    private void OnPointerEnterHandler(PointerEventData eventData) // ��ȷ��������
    {
        Debug.Log("Mouse entered enemy intent icon (EventTrigger)");
        // ֻ����ͼ��ɼ�ʱ����ʾ Tooltip
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
    /// ����뿪ʱ����Tooltip����EventTrigger���ã�
    /// </summary>
    private void OnPointerExitHandler(PointerEventData eventData) // ��ȷ��������
    {
        Debug.Log("Mouse exited enemy intent icon (EventTrigger)");
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            Debug.Log("EnemyIntentDisplay: Tooltip panel deactivated.");
        }
    }
}