using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class TooltipUIElement : MonoBehaviour
{
    [Header("Tooltip Content")]
    public string title = "UIԪ�ر���";
    public string description = "UIԪ����������";

    [Header("Trigger Area")]
    [Tooltip("��������Ŀ�ȣ����أ�")]
    public float triggerWidth = 100f;
    [Tooltip("��������ĸ߶ȣ����أ�")]
    public float triggerHeight = 50f;
    [Tooltip("�������������UIԪ�����ĵ�ƫ��")]
    public Vector2 triggerOffset = Vector2.zero;

    [Header("Display Settings")]
    [Tooltip("����뿪���ӳ����ص�ʱ�䣨�룩")]
    public float hideDelay = 0.5f;

    [Header("Visual Debug")]
    public bool showTriggerArea = true;
    [Tooltip("��Game��ͼ��Ҳ��ʾ�������򣨽���Playģʽ�£�")]
    public bool showInGameView = false;
    public Color triggerAreaColor = new Color(1f, 1f, 0f, 0.3f);

    private TooltipManager tooltipManager;
    private RectTransform rectTransform;
    private Camera uiCamera;
    private bool isTooltipVisible = false;
    private bool isMouseInside = false;
    private float hideTimer = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        uiCamera = GetComponentInParent<Canvas>()?.worldCamera ?? Camera.main;
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            tooltipManager = FindObjectOfType<TooltipManager>();
            if (tooltipManager == null)
            {
                Debug.LogWarning($"δ�ҵ�TooltipManager�������ȷ����������TooltipManager - {gameObject.name}");
            }
        }
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            CheckMousePosition();
            UpdateHideTimer();
        }
    }

    // ������λ��
    private void CheckMousePosition()
    {
        if (rectTransform == null || uiCamera == null) return;

        Vector2 mousePosition = Input.mousePosition;

        // �������Ƿ��ڴ���������
        if (IsMouseInCustomArea(mousePosition))
        {
            if (!isMouseInside)
            {
                isMouseInside = true;
                CancelHideTimer();
                ShowTooltip();
            }
        }
        else
        {
            if (isMouseInside)
            {
                isMouseInside = false;
                StartHideTimer();
            }
        }
    }

    // �������ؼ�ʱ��
    private void UpdateHideTimer()
    {
        if (!isMouseInside && isTooltipVisible)
        {
            hideTimer += Time.deltaTime;
            if (hideTimer >= hideDelay)
            {
                HideTooltip();
            }
        }
    }

    // ��ʼ���ؼ�ʱ
    private void StartHideTimer()
    {
        hideTimer = 0f;
    }

    // ȡ�����ؼ�ʱ
    private void CancelHideTimer()
    {
        hideTimer = 0f;
    }

    // �������Ƿ����Զ���������
    private bool IsMouseInCustomArea(Vector2 mousePosition)
    {
        if (rectTransform == null) return false;

        // ��ȡUIԪ�����ĵ���������
        Vector2 elementCenter = rectTransform.position;

        // ���㴥�������������������
        Vector2 triggerCenter = elementCenter + triggerOffset;

        // ������굽�����������ĵľ���
        float distanceX = Mathf.Abs(mousePosition.x - triggerCenter.x);
        float distanceY = Mathf.Abs(mousePosition.y - triggerCenter.y);

        // ����Ƿ��ھ���������
        return (distanceX <= triggerWidth * 0.5f && distanceY <= triggerHeight * 0.5f);
    }

    public void ShowTooltip()
    {
        if (tooltipManager != null)
        {
            tooltipManager.ShowTooltipAtPosition(title, description);
            isTooltipVisible = true;
        }
    }

    public void HideTooltip()
    {
        if (tooltipManager != null)
        {
            tooltipManager.HideTooltip();
            isTooltipVisible = false;
            hideTimer = 0f;
        }
    }

    public void UpdateTooltipContent(string newTitle, string newDescription)
    {
        title = newTitle;
        description = newDescription;

        if (isTooltipVisible && tooltipManager != null)
        {
            tooltipManager.UpdateCurrentTooltip(title, description);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showTriggerArea) return;
        
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) return;
        
        DrawTriggerAreaGizmos(rect);
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showTriggerArea) return;
        
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) return;
        
        DrawTriggerAreaGizmos(rect);
    }
    
    private void DrawTriggerAreaGizmos(RectTransform rect)
    {
        // ����UIԪ��ԭʼ�߽磨��ɫ��
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 center = rect.position;
        Vector3 size = new Vector3(rect.rect.width, rect.rect.height, 0);
        Gizmos.DrawWireCube(center, size);
        
        // �����Զ��崥�����򣨻�ɫ��
        Vector2 triggerCenter = (Vector2)center + triggerOffset;
        Vector3 triggerSize = new Vector3(triggerWidth, triggerHeight, 0);
        
        Gizmos.color = triggerAreaColor;
        Gizmos.DrawWireCube(triggerCenter, triggerSize);
        
        // �������
        Handles.color = new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, triggerAreaColor.a * 0.3f);
        Handles.DrawSolidRectangleWithOutline(
            new Vector3[] {
                new Vector3(triggerCenter.x - triggerWidth/2, triggerCenter.y - triggerHeight/2, 0),
                new Vector3(triggerCenter.x + triggerWidth/2, triggerCenter.y - triggerHeight/2, 0),
                new Vector3(triggerCenter.x + triggerWidth/2, triggerCenter.y + triggerHeight/2, 0),
                new Vector3(triggerCenter.x - triggerWidth/2, triggerCenter.y + triggerHeight/2, 0)
            },
            new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, triggerAreaColor.a * 0.3f),
            triggerAreaColor
        );
    }
#endif

    // ��Game��ͼ�л��Ƶ�����Ϣ
    void OnGUI()
    {
        if (!Application.isPlaying || !showInGameView || !showTriggerArea) return;
        if (rectTransform == null || uiCamera == null) return;

        DrawTriggerAreaInGame();
    }

    private void DrawTriggerAreaInGame()
    {
        // ��ȡUIԪ�����ĵ���������
        Vector2 elementCenter = rectTransform.position;

        // ���㴥�������������������
        Vector2 triggerCenter = elementCenter + triggerOffset;

        // ת��Ϊ��Ļ����
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, triggerCenter);
        screenCenter.y = Screen.height - screenCenter.y; // Unity GUI��Y���Ƿ���

        // ���ƴ�������
        float halfWidth = triggerWidth * 0.5f;
        float halfHeight = triggerHeight * 0.5f;

        // ��������
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, triggerAreaColor);
        texture.Apply();

        // �����������
        GUI.color = triggerAreaColor;
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerWidth, triggerHeight), texture);

        // ���Ʊ߿�
        GUI.color = new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, 1f);
        // �ϱ߿�
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerWidth, 2), Texture2D.whiteTexture);
        // �±߿�
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y + halfHeight - 2, triggerWidth, 2), Texture2D.whiteTexture);
        // ��߿�
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, 2, triggerHeight), Texture2D.whiteTexture);
        // �ұ߿�
        GUI.DrawTexture(new Rect(screenCenter.x + halfWidth - 2, screenCenter.y - halfHeight, 2, triggerHeight), Texture2D.whiteTexture);

        // ����
        DestroyImmediate(texture);
    }
}