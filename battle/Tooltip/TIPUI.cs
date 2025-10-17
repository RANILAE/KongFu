using UnityEngine;
using TMPro;
using System.Collections;

public class TIPUI : MonoBehaviour
{
    [Header("Tooltip Panel")]
    public GameObject tooltipPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Display Settings")]
    [Tooltip("如果为true，使用面板的当前位置；如果为false，使用偏移位置")]
    public bool usePanelPosition = true;
    public Vector2 offset = new Vector2(10, 10);
    public float appearDelay = 0.3f;

    private RectTransform panelRectTransform;
    private Canvas canvas;
    private Vector2 defaultPosition; // 保存默认位置
    private Coroutine showCoroutine;

    void Start()
    {
        if (tooltipPanel != null)
        {
            panelRectTransform = tooltipPanel.GetComponent<RectTransform>();
            canvas = tooltipPanel.GetComponentInParent<Canvas>();

            // 保存面板的默认位置
            if (panelRectTransform != null)
            {
                defaultPosition = panelRectTransform.anchoredPosition;
            }
        }

        // 初始隐藏tooltip面板
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    public void ShowTooltip(string title, string description)
    {
        if (tooltipPanel == null) return;

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        if (titleText != null)
            titleText.text = title;
        if (descriptionText != null)
            descriptionText.text = description;

        // 确保游戏对象是激活的
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        showCoroutine = StartCoroutine(ShowTooltipDelayed());
    }

    private IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSeconds(appearDelay);

        // 设置位置
        SetTooltipPosition();

        // 显示面板
        tooltipPanel.SetActive(true);
        showCoroutine = null;
    }

    private void SetTooltipPosition()
    {
        if (panelRectTransform == null) return;

        if (usePanelPosition)
        {
            // 使用面板的默认位置（你在Inspector中摆放的位置）
            panelRectTransform.anchoredPosition = defaultPosition;
        }
        else
        {
            // 使用偏移位置（相对于默认位置）
            panelRectTransform.anchoredPosition = defaultPosition + offset;
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
    }

    public void UpdateTooltipContent(string title, string description)
    {
        if (titleText != null)
            titleText.text = title;
        if (descriptionText != null)
            descriptionText.text = description;
    }

    /// <summary>
    /// 更新默认位置（可以在运行时调用）
    /// </summary>
    public void UpdateDefaultPosition()
    {
        if (panelRectTransform != null)
        {
            defaultPosition = panelRectTransform.anchoredPosition;
        }
    }
}