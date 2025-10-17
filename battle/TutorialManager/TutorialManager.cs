using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string description = "教程步骤描述";
        public TutorialAction actionType = TutorialAction.WaitForClick;
        public KeyCode requiredKey = KeyCode.Space;
        public Vector2 clickPosition = Vector2.zero;
        public float waitTime = 1f;

        [Header("Text Settings")]
        public bool useCustomTextPosition = false;
        public Vector2 textPosition = Vector2.zero;
        public Vector2 textSize = new Vector2(400f, 100f);

        [Header("Custom UI Component Settings")]
        public bool useCustomUIComponent = false;
        public GameObject customUIComponent; // 直接引用场景中的GameObject

        [Header("Preview")]
        public bool showPreview = false;
    }

    public enum TutorialAction
    {
        WaitForClick,
        WaitForKeyPress,
        WaitForTime,
        WaitForPositionClick,
        WaitForButtonClick
    }

    [Header("Level Settings")]
    public bool enableAutoStart = true;

    [Header("UI References")]
    public TMP_Text tutorialText;
    public GameObject tutorialPanel;
    public Button nextButton;

    [Header("Tutorial Steps")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Editor Preview")]
    public bool enablePreview = false;
    public int previewStep = 0;

    private int currentStep = 0;
    private bool isWaitingForInput = false;
    private bool stepCompleted = false;
    private bool waitForButtonClick = false;
    private GameObject lastShownComponent = null; // 记录上一个显示的组件

    void Start()
    {
        if (Application.isPlaying)
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextButtonClick);
            }

            if (ShouldAutoStartTutorial())
            {
                StartTutorial();
            }
            else
            {
                if (tutorialPanel != null)
                {
                    tutorialPanel.SetActive(false);
                }
            }
        }
    }

    bool ShouldAutoStartTutorial()
    {
        if (!enableAutoStart) return false;

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return currentSceneName.ToLower() == "level1";
    }

    public void StartTutorial()
    {
        currentStep = 0;
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        ShowCurrentStep();
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            if (isWaitingForInput && !stepCompleted)
            {
                HandleCurrentStep();
            }
        }
        else
        {
            // 编辑器模式下的预览
            if (enablePreview && tutorialSteps.Count > 0)
            {
                previewStep = Mathf.Clamp(previewStep, 0, tutorialSteps.Count - 1);
                ShowPreview(previewStep);
            }
        }
    }

    void ShowCurrentStep()
    {
        if (currentStep < tutorialSteps.Count)
        {
            TutorialStep currentStepData = tutorialSteps[currentStep];

            // 隐藏上一个显示的组件
            if (lastShownComponent != null && lastShownComponent != currentStepData.customUIComponent)
            {
                lastShownComponent.SetActive(false);
            }

            if (tutorialText != null)
            {
                tutorialText.text = currentStepData.description;
                HandleTextPosition(currentStepData);
            }

            // 显示当前步骤的UI组件
            HandleCustomUIComponent(currentStepData);

            isWaitingForInput = true;
            stepCompleted = false;
            waitForButtonClick = false;

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(currentStepData.actionType == TutorialAction.WaitForButtonClick);
            }

            switch (currentStepData.actionType)
            {
                case TutorialAction.WaitForClick:
                    Debug.Log("请点击任意位置继续...");
                    break;
                case TutorialAction.WaitForKeyPress:
                    Debug.Log($"请按下 {currentStepData.requiredKey} 键...");
                    break;
                case TutorialAction.WaitForPositionClick:
                    Debug.Log($"请点击指定位置: {currentStepData.clickPosition}");
                    break;
                case TutorialAction.WaitForTime:
                    StartCoroutine(WaitForTimeCoroutine(currentStepData.waitTime));
                    break;
                case TutorialAction.WaitForButtonClick:
                    waitForButtonClick = true;
                    Debug.Log("请点击下一步按钮...");
                    break;
            }
        }
        else
        {
            EndTutorial();
        }
    }

    void HandleTextPosition(TutorialStep step)
    {
        if (tutorialText != null)
        {
            RectTransform textRect = tutorialText.GetComponent<RectTransform>();

            if (step.useCustomTextPosition)
            {
                textRect.anchoredPosition = step.textPosition;
                textRect.sizeDelta = step.textSize;
            }
            else
            {
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = new Vector2(400f, 100f);
            }
        }
    }

    void HandleCustomUIComponent(TutorialStep step)
    {
        // 隐藏上一个组件
        if (lastShownComponent != null && lastShownComponent != step.customUIComponent)
        {
            lastShownComponent.SetActive(false);
        }

        // 显示当前步骤的组件
        if (step.useCustomUIComponent && step.customUIComponent != null)
        {
            step.customUIComponent.SetActive(true);
            lastShownComponent = step.customUIComponent;
        }
        else
        {
            lastShownComponent = null;
        }
    }

    void ShowPreview(int stepIndex)
    {
        if (stepIndex >= 0 && stepIndex < tutorialSteps.Count)
        {
            TutorialStep step = tutorialSteps[stepIndex];

            // 隐藏上一个显示的组件
            if (lastShownComponent != null && lastShownComponent != step.customUIComponent)
            {
                lastShownComponent.SetActive(false);
            }

            // 显示当前步骤的文本
            if (tutorialText != null)
            {
                tutorialText.text = step.description;
                HandleTextPosition(step);
            }

            // 显示当前步骤的UI组件
            if (step.useCustomUIComponent && step.customUIComponent != null)
            {
                step.customUIComponent.SetActive(true);
                lastShownComponent = step.customUIComponent;
            }
            else
            {
                lastShownComponent = null;
            }
        }
    }

    void HandleCurrentStep()
    {
        TutorialStep currentStepData = tutorialSteps[currentStep];

        switch (currentStepData.actionType)
        {
            case TutorialAction.WaitForClick:
                if (Input.GetMouseButtonDown(0))
                {
                    CompleteStep();
                }
                break;

            case TutorialAction.WaitForKeyPress:
                if (Input.GetKeyDown(currentStepData.requiredKey))
                {
                    CompleteStep();
                }
                break;

            case TutorialAction.WaitForPositionClick:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (Vector2.Distance(mousePos, currentStepData.clickPosition) <= 2f)
                    {
                        CompleteStep();
                    }
                    else
                    {
                        Debug.Log("请点击指定位置！");
                    }
                }
                break;

            case TutorialAction.WaitForButtonClick:
                break;
        }
    }

    void OnNextButtonClick()
    {
        if (waitForButtonClick && isWaitingForInput && !stepCompleted)
        {
            CompleteStep();
        }
    }

    IEnumerator WaitForTimeCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        CompleteStep();
    }

    void CompleteStep()
    {
        stepCompleted = true;
        isWaitingForInput = false;

        // 隐藏当前步骤的组件
        if (currentStep < tutorialSteps.Count)
        {
            TutorialStep currentStepData = tutorialSteps[currentStep];
            if (currentStepData.useCustomUIComponent && currentStepData.customUIComponent != null)
            {
                currentStepData.customUIComponent.SetActive(false);
            }
        }

        currentStep++;
        ShowCurrentStep();
    }

    void EndTutorial()
    {
        if (tutorialText != null)
            tutorialText.text = "教程完成！";

        Debug.Log("教程完成！");

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // 隐藏最后显示的组件
        if (lastShownComponent != null)
        {
            lastShownComponent.SetActive(false);
            lastShownComponent = null;
        }
    }

    public void SkipCurrentStep()
    {
        if (isWaitingForInput)
        {
            CompleteStep();
        }
    }

    public int GetCurrentStep()
    {
        return currentStep;
    }

    public void SetCurrentStep(int stepIndex)
    {
        if (stepIndex >= 0 && stepIndex < tutorialSteps.Count)
        {
            currentStep = stepIndex;
            if (Application.isPlaying)
            {
                ShowCurrentStep();
            }
            else
            {
                // 编辑器模式下直接预览
                ShowPreview(stepIndex);
            }
        }
    }

    public void ForceStartTutorial()
    {
        StartTutorial();
    }

    // 清除预览状态
    public void ClearPreview()
    {
        if (!Application.isPlaying && lastShownComponent != null)
        {
            lastShownComponent.SetActive(false);
            lastShownComponent = null;
        }
    }
}