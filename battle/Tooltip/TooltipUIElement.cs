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
    public string title = "UI元素标题";
    public string description = "UI元素描述内容";

    [Header("Trigger Area")]
    [Tooltip("触发区域的宽度（像素）")]
    public float triggerWidth = 100f;
    [Tooltip("触发区域的高度（像素）")]
    public float triggerHeight = 50f;
    [Tooltip("触发区域相对于UI元素中心的偏移")]
    public Vector2 triggerOffset = Vector2.zero;

    [Header("Display Settings")]
    [Tooltip("鼠标离开后延迟隐藏的时间（秒）")]
    public float hideDelay = 0.5f;

    [Header("Visual Debug")]
    public bool showTriggerArea = true;
    [Tooltip("在Game视图中也显示触发区域（仅在Play模式下）")]
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
                Debug.LogWarning($"未找到TooltipManager组件！请确保场景中有TooltipManager - {gameObject.name}");
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

    // 检测鼠标位置
    private void CheckMousePosition()
    {
        if (rectTransform == null || uiCamera == null) return;

        Vector2 mousePosition = Input.mousePosition;

        // 检查鼠标是否在触发区域内
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

    // 更新隐藏计时器
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

    // 开始隐藏计时
    private void StartHideTimer()
    {
        hideTimer = 0f;
    }

    // 取消隐藏计时
    private void CancelHideTimer()
    {
        hideTimer = 0f;
    }

    // 检查鼠标是否在自定义区域内
    private bool IsMouseInCustomArea(Vector2 mousePosition)
    {
        if (rectTransform == null) return false;

        // 获取UI元素中心的世界坐标
        Vector2 elementCenter = rectTransform.position;

        // 计算触发区域的世界坐标中心
        Vector2 triggerCenter = elementCenter + triggerOffset;

        // 计算鼠标到触发区域中心的距离
        float distanceX = Mathf.Abs(mousePosition.x - triggerCenter.x);
        float distanceY = Mathf.Abs(mousePosition.y - triggerCenter.y);

        // 检查是否在矩形区域内
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
        // 绘制UI元素原始边界（绿色）
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 center = rect.position;
        Vector3 size = new Vector3(rect.rect.width, rect.rect.height, 0);
        Gizmos.DrawWireCube(center, size);
        
        // 绘制自定义触发区域（黄色）
        Vector2 triggerCenter = (Vector2)center + triggerOffset;
        Vector3 triggerSize = new Vector3(triggerWidth, triggerHeight, 0);
        
        Gizmos.color = triggerAreaColor;
        Gizmos.DrawWireCube(triggerCenter, triggerSize);
        
        // 绘制填充
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

    // 在Game视图中绘制调试信息
    void OnGUI()
    {
        if (!Application.isPlaying || !showInGameView || !showTriggerArea) return;
        if (rectTransform == null || uiCamera == null) return;

        DrawTriggerAreaInGame();
    }

    private void DrawTriggerAreaInGame()
    {
        // 获取UI元素中心的世界坐标
        Vector2 elementCenter = rectTransform.position;

        // 计算触发区域的世界坐标中心
        Vector2 triggerCenter = elementCenter + triggerOffset;

        // 转换为屏幕坐标
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, triggerCenter);
        screenCenter.y = Screen.height - screenCenter.y; // Unity GUI的Y轴是反的

        // 绘制触发区域
        float halfWidth = triggerWidth * 0.5f;
        float halfHeight = triggerHeight * 0.5f;

        // 创建纹理
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, triggerAreaColor);
        texture.Apply();

        // 绘制填充区域
        GUI.color = triggerAreaColor;
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerWidth, triggerHeight), texture);

        // 绘制边框
        GUI.color = new Color(triggerAreaColor.r, triggerAreaColor.g, triggerAreaColor.b, 1f);
        // 上边框
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, triggerWidth, 2), Texture2D.whiteTexture);
        // 下边框
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y + halfHeight - 2, triggerWidth, 2), Texture2D.whiteTexture);
        // 左边框
        GUI.DrawTexture(new Rect(screenCenter.x - halfWidth, screenCenter.y - halfHeight, 2, triggerHeight), Texture2D.whiteTexture);
        // 右边框
        GUI.DrawTexture(new Rect(screenCenter.x + halfWidth - 2, screenCenter.y - halfHeight, 2, triggerHeight), Texture2D.whiteTexture);

        // 清理
        DestroyImmediate(texture);
    }
}