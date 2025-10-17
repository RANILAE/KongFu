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
        public string description = "�̳̲�������";
        public TutorialAction actionType = TutorialAction.WaitForClick;
        public KeyCode requiredKey = KeyCode.Space;
        public Vector2 clickPosition = Vector2.zero;
        public float waitTime = 1f;

        [Header("Text Settings")]
        public bool useCustomTextPosition = false;
        public Vector2 textPosition = Vector2.zero;
        public Vector2 textSize = new Vector2(400f, 100f);

        [Header("Custom Image Settings")]
        public bool useCustomImage = false;
        public Sprite customImage;
        public Vector2 imagePosition = Vector2.zero;
        public Vector2 imageSize = new Vector2(100f, 100f);
        public Color imageColor = Color.white;

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
    public bool enableAutoStart = true; // �Ƿ������Զ���ʼ����

    [Header("UI References")]
    public TMP_Text tutorialText;
    public GameObject tutorialPanel;
    public Button nextButton;
    public Image customImageDisplay;

    [Header("Tutorial Steps")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Editor Preview")]
    public bool enablePreview = false;
    public int previewStep = 0;

    private int currentStep = 0;
    private bool isWaitingForInput = false;
    private bool stepCompleted = false;
    private bool waitForButtonClick = false;

    void Start()
    {
        if (Application.isPlaying)
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextButtonClick);
            }

            // ����Ƿ�Ӧ���Զ���ʼ�̳�
            if (ShouldAutoStartTutorial())
            {
                StartTutorial();
            }
            else
            {
                // ���Զ���ʼʱ�������
                if (tutorialPanel != null)
                {
                    tutorialPanel.SetActive(false);
                }
            }
        }
    }

    // ����Ƿ�Ӧ���Զ���ʼ�̳�
    bool ShouldAutoStartTutorial()
    {
        if (!enableAutoStart) return false;

        // ��ȡ��ǰ��������
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // ֻ����level1ʱ�Զ���ʼ
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

            if (tutorialText != null)
            {
                tutorialText.text = currentStepData.description;
                HandleTextPosition(currentStepData);
            }

            HandleCustomImage(currentStepData);

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
                    Debug.Log("��������λ�ü���...");
                    break;
                case TutorialAction.WaitForKeyPress:
                    Debug.Log($"�밴�� {currentStepData.requiredKey} ��...");
                    break;
                case TutorialAction.WaitForPositionClick:
                    Debug.Log($"����ָ��λ��: {currentStepData.clickPosition}");
                    break;
                case TutorialAction.WaitForTime:
                    StartCoroutine(WaitForTimeCoroutine(currentStepData.waitTime));
                    break;
                case TutorialAction.WaitForButtonClick:
                    waitForButtonClick = true;
                    Debug.Log("������һ����ť...");
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

    void HandleCustomImage(TutorialStep step)
    {
        if (customImageDisplay != null)
        {
            if (step.useCustomImage && step.customImage != null)
            {
                customImageDisplay.gameObject.SetActive(true);
                customImageDisplay.sprite = step.customImage;
                customImageDisplay.color = step.imageColor;

                RectTransform imageRect = customImageDisplay.GetComponent<RectTransform>();
                imageRect.anchoredPosition = step.imagePosition;
                imageRect.sizeDelta = step.imageSize;
            }
            else
            {
                customImageDisplay.gameObject.SetActive(false);
            }
        }
    }

    void ShowPreview(int stepIndex)
    {
        if (stepIndex >= 0 && stepIndex < tutorialSteps.Count)
        {
            TutorialStep step = tutorialSteps[stepIndex];

            if (tutorialText != null)
            {
                tutorialText.text = step.description;
                HandleTextPosition(step);
            }

            HandleCustomImage(step);
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
                        Debug.Log("����ָ��λ�ã�");
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

        if (customImageDisplay != null)
        {
            customImageDisplay.gameObject.SetActive(false);
        }

        currentStep++;
        ShowCurrentStep();
    }

    void EndTutorial()
    {
        if (tutorialText != null)
            tutorialText.text = "�̳���ɣ�";

        Debug.Log("�̳���ɣ�");

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (customImageDisplay != null)
        {
            customImageDisplay.gameObject.SetActive(false);
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
        }
    }

    // ����������ǿ�ƿ�ʼ�̳̣�������level2��level3���ֶ�������
    public void ForceStartTutorial()
    {
        StartTutorial();
    }
}