using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject wheelUI;
    public SpriteRenderer yangWheel;
    public SpriteRenderer yinWheel;
    public TMP_Text yangPointsText;
    public TMP_Text yinPointsText;
    public Image stateIcon; // 图标显示状态
    public TMP_Text remainingPointsText;
    public TMP_Text maxPointsText;

    [Header("State Icons")]
    public Sprite balanceIcon;
    public Sprite criticalYangIcon;
    public Sprite criticalYinIcon;
    public Sprite yangProsperityIcon;
    public Sprite yinProsperityIcon;
    public Sprite extremeYangIcon;
    public Sprite extremeYinIcon;
    public Sprite ultimateQiIcon;

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

        // 修改显示格式为 X/Y 格式，显示为整数
        if (yinPointsText != null)
        {
            yinPointsText.text = $"{Mathf.FloorToInt(CurrentYinPoints)}/{Mathf.FloorToInt(currentMaxPoints)}";
        }

        if (yangPointsText != null)
        {
            yangPointsText.text = $"{Mathf.FloorToInt(CurrentYangPoints)}/{Mathf.FloorToInt(currentMaxPoints)}";
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
            // 显示为 (保留点数/最大点数) 格式，都显示为整数
            maxPointsText.text = $"({Mathf.FloorToInt(retainedPoints)}/{Mathf.FloorToInt(currentMaxPoints)})";
        }

        // 状态显示改为图标
        if (stateIcon != null)
        {
            float diff = CurrentYangPoints - CurrentYinPoints;
            SetStateIcon(diff);
        }
    }

    private void SetStateIcon(float diff)
    {
        float absDiff = Mathf.Abs(diff);
        Sprite iconToUse = null;

        if (absDiff < 1f)
        {
            iconToUse = balanceIcon; // Balance
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            iconToUse = criticalYangIcon; // Critical Yang
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            iconToUse = criticalYinIcon; // Critical Yin
        }
        else if (diff > 2.5f && diff < 5f)
        {
            iconToUse = yangProsperityIcon; // Yang Prosperity
        }
        else if (diff < -2.5f && diff > -5f)
        {
            iconToUse = yinProsperityIcon; // Yin Prosperity
        }
        else if (diff >= 5f && diff <= 7f)
        {
            iconToUse = extremeYangIcon; // Extreme Yang
        }
        else if (diff <= -5f && diff >= -7f)
        {
            iconToUse = extremeYinIcon; // Extreme Yin
        }
        else if (absDiff > 7f && absDiff <= 10f)
        {
            iconToUse = ultimateQiIcon; // Ultimate Qi
        }

        if (iconToUse != null)
        {
            stateIcon.sprite = iconToUse;
            stateIcon.enabled = true;
        }
        else
        {
            stateIcon.enabled = false;
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

    // 修改：只隐藏状态图标，而不是整个轮盘UI
    public void HideWheelUI()
    {
        if (stateIcon != null)
        {
            stateIcon.enabled = false;
        }
    }

    // 修改：显示状态图标
    public void ShowWheelUI()
    {
        if (stateIcon != null)
        {
            stateIcon.enabled = true;
            // 更新图标显示
            float diff = CurrentYangPoints - CurrentYinPoints;
            SetStateIcon(diff);
        }
    }

    // 新增：完全隐藏整个轮盘UI（原来的逻辑）
    public void HideFullWheelUI()
    {
        if (wheelUI != null)
        {
            wheelUI.SetActive(false);
        }
    }

    // 新增：完全显示整个轮盘UI（原来的逻辑）
    public void ShowFullWheelUI()
    {
        if (wheelUI != null)
        {
            wheelUI.SetActive(true);
            // 同时显示状态图标
            ShowWheelUI();
        }
    }
}