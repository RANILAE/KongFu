using TMPro;
using UnityEngine;

public class WheelSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject wheelUI;
    public SpriteRenderer yangWheel;
    public SpriteRenderer yinWheel;
    public TMP_Text yangPointsText;
    public TMP_Text yinPointsText;
    public TMP_Text stateText;
    public TMP_Text remainingPointsText;
    public TMP_Text maxPointsText;

    [Header("Settings")]
    public float rotationSpeed = 1f;
    public int decimalPlaces = 2;

    private float baseMaxPoints = 7f; // 基础点数上限（固定为7）
    private float retainedPoints = 0f; // 保留点数
    private float currentMaxPoints; // 当前总点数上限
    private float segmentAngle; // 每个点数的角度

    public float CurrentYangPoints { get; private set; }
    public float CurrentYinPoints { get; private set; }

    public float BaseMaxPoints => baseMaxPoints;
    public float RetainedPoints => retainedPoints;
    public float MaxPoints => currentMaxPoints;

    private float yangAngle;
    private float yinAngle;

    public void Initialize(float maxPoints)
    {
        baseMaxPoints = 7f; // 始终保持基础点数为7
        retainedPoints = 0f;
        currentMaxPoints = baseMaxPoints + retainedPoints;
        segmentAngle = 360f / currentMaxPoints;
        ResetPoints();
    }

    public void ResetPoints()
    {
        CurrentYangPoints = 0f;
        CurrentYinPoints = 0f;
        yangAngle = 0f;
        yinAngle = 0f;
        UpdateUI();
    }

    // 增加保留点数（用于保留点数系统）
    public void AddRetainedPoints(float amount)
    {
        retainedPoints += amount;
        currentMaxPoints = baseMaxPoints + retainedPoints;
        segmentAngle = 360f / currentMaxPoints;
        UpdateUI();
        Debug.Log($"Added retained points: +{amount}, Total retained: {retainedPoints}, New max points: {currentMaxPoints}");
    }

    // 重置保留点数到0
    public void ResetRetainedPoints()
    {
        retainedPoints = 0f;
        currentMaxPoints = baseMaxPoints + retainedPoints;
        segmentAngle = 360f / currentMaxPoints;
        UpdateUI();
        Debug.Log($"Reset retained points to 0, Max points: {currentMaxPoints}");
    }

    // 重置基础点数上限到默认值
    public void ResetBaseMaxPoints()
    {
        baseMaxPoints = 7f;
        currentMaxPoints = baseMaxPoints + retainedPoints;
        segmentAngle = 360f / currentMaxPoints;
        UpdateUI();
        Debug.Log($"Reset base max points to 7, Retained: {retainedPoints}, Total max: {currentMaxPoints}");
    }

    public void SetYangAngle(float angle)
    {
        yangAngle = Mathf.Clamp(angle, 0f, 360f);
        CurrentYangPoints = Mathf.Clamp(yangAngle / segmentAngle, 0f, currentMaxPoints);
        UpdateUI();
    }

    public void SetYinAngle(float angle)
    {
        yinAngle = Mathf.Clamp(angle, 0f, 360f);
        CurrentYinPoints = Mathf.Clamp(yinAngle / segmentAngle, 0f, currentMaxPoints);
        UpdateUI();
    }

    public void SetYangPoints(float points)
    {
        float newPoints = Mathf.Clamp(points, 0f, currentMaxPoints);
        if (newPoints + CurrentYinPoints > currentMaxPoints)
        {
            newPoints = currentMaxPoints - CurrentYinPoints;
        }

        CurrentYangPoints = newPoints;
        yangAngle = CurrentYangPoints * segmentAngle;
        UpdateUI();
    }

    public void SetYinPoints(float points)
    {
        float newPoints = Mathf.Clamp(points, 0f, currentMaxPoints);
        if (CurrentYangPoints + newPoints > currentMaxPoints)
        {
            newPoints = currentMaxPoints - CurrentYangPoints;
        }

        CurrentYinPoints = newPoints;
        yinAngle = CurrentYinPoints * segmentAngle;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (yangWheel != null)
        {
            yangWheel.transform.rotation = Quaternion.Euler(0, 0, -yangAngle);
        }

        if (yinWheel != null)
        {
            yinWheel.transform.rotation = Quaternion.Euler(0, 0, -yinAngle);
        }

        // 修改显示格式为 X/Y 格式，不显示文字
        if (yinPointsText != null)
        {
            yinPointsText.text = $"{CurrentYinPoints.ToString($"F{decimalPlaces}")}/{currentMaxPoints.ToString($"F{decimalPlaces}")}";
        }

        if (yangPointsText != null)
        {
            yangPointsText.text = $"{CurrentYangPoints.ToString($"F{decimalPlaces}")}/{currentMaxPoints.ToString($"F{decimalPlaces}")}";
        }

        // 剩余点数显示格式修改为 整数/最大点数（去掉括号）
        float remainingPoints = currentMaxPoints - (CurrentYangPoints + CurrentYinPoints);
        if (remainingPointsText != null)
        {
            // 显示为 整数/最大点数 格式（去掉括号，剩余点数向下取整）
            int remainingInteger = Mathf.FloorToInt(remainingPoints);
            int maxInteger = Mathf.FloorToInt(currentMaxPoints);
            remainingPointsText.text = $"{remainingInteger}/{maxInteger}";
        }

        // 最大点数显示格式修改为 (保留点数/最大点数)
        if (maxPointsText != null)
        {
            // 显示为 (保留点数/最大点数) 格式
            maxPointsText.text = $"({retainedPoints.ToString($"F{decimalPlaces}")}/{currentMaxPoints.ToString($"F{decimalPlaces}")})";
        }

        // 状态显示保持不变
        if (stateText != null)
        {
            float diff = CurrentYangPoints - CurrentYinPoints;
            stateText.text = GetCurrentStateName(diff);

            if (diff > 0) stateText.color = Color.red;
            else if (diff < 0) stateText.color = Color.blue;
            else stateText.color = Color.green;
        }
    }

    public string GetCurrentStateName(float diff)
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

    public void ShowWheelUI()
    {
        wheelUI.SetActive(true);
    }

    public void HideWheelUI()
    {
        wheelUI.SetActive(false);
    }
}