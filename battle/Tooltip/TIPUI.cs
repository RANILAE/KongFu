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
    [Tooltip("���Ϊtrue��ʹ�����ĵ�ǰλ�ã����Ϊfalse��ʹ��ƫ��λ��")]
    public bool usePanelPosition = true;
    public Vector2 offset = new Vector2(10, 10);
    public float appearDelay = 0.3f;

    private RectTransform panelRectTransform;
    private Canvas canvas;
    private Vector2 defaultPosition; // ����Ĭ��λ��
    private Coroutine showCoroutine;

    void Start()
    {
        if (tooltipPanel != null)
        {
            panelRectTransform = tooltipPanel.GetComponent<RectTransform>();
            canvas = tooltipPanel.GetComponentInParent<Canvas>();

            // ��������Ĭ��λ��
            if (panelRectTransform != null)
            {
                defaultPosition = panelRectTransform.anchoredPosition;
            }
        }

        // ��ʼ����tooltip���
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

        // ȷ����Ϸ�����Ǽ����
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        showCoroutine = StartCoroutine(ShowTooltipDelayed());
    }

    private IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSeconds(appearDelay);

        // ����λ��
        SetTooltipPosition();

        // ��ʾ���
        tooltipPanel.SetActive(true);
        showCoroutine = null;
    }

    private void SetTooltipPosition()
    {
        if (panelRectTransform == null) return;

        if (usePanelPosition)
        {
            // ʹ������Ĭ��λ�ã�����Inspector�аڷŵ�λ�ã�
            panelRectTransform.anchoredPosition = defaultPosition;
        }
        else
        {
            // ʹ��ƫ��λ�ã������Ĭ��λ�ã�
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
    /// ����Ĭ��λ�ã�����������ʱ���ã�
    /// </summary>
    public void UpdateDefaultPosition()
    {
        if (panelRectTransform != null)
        {
            defaultPosition = panelRectTransform.anchoredPosition;
        }
    }
}