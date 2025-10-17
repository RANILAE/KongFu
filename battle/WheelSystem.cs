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
    public Image stateIcon; // ͼ����ʾ״̬
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

    private float baseMaxPoints = 7f; // �����������ޣ��̶�Ϊ7��
    private float retainedPoints = 0f; // ��������
    private float currentMaxPoints; // ��ǰ�ܵ�������
    private float segmentAngle; // ÿ�������ĽǶ�

    public float CurrentYangPoints { get; private set; }
    public float CurrentYinPoints { get; private set; }

    public float BaseMaxPoints => baseMaxPoints;
    public float RetainedPoints => retainedPoints;
    public float MaxPoints => currentMaxPoints;

    private float yangAngle;
    private float yinAngle;

    public void Initialize(float maxPoints)
    {
        baseMaxPoints = 7f; // ʼ�ձ��ֻ�������Ϊ7
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

    // ���ӱ������������ڱ�������ϵͳ��
    public void AddRetainedPoints(float amount)
    {
        retainedPoints += amount;
        currentMaxPoints = baseMaxPoints + retainedPoints;
        segmentAngle = 360f / currentMaxPoints;
        UpdateUI();
        Debug.Log($"Added retained points: +{amount}, Total retained: {retainedPoints}, New max points: {currentMaxPoints}");
    }

    // ���ñ���������0
    public void ResetRetainedPoints()
    {
        retainedPoints = 0f;
        currentMaxPoints = baseMaxPoints + retainedPoints;
        segmentAngle = 360f / currentMaxPoints;
        UpdateUI();
        Debug.Log($"Reset retained points to 0, Max points: {currentMaxPoints}");
    }

    // ���û����������޵�Ĭ��ֵ
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

        // �޸���ʾ��ʽΪ X/Y ��ʽ����ʾΪ����
        if (yinPointsText != null)
        {
            yinPointsText.text = $"{Mathf.FloorToInt(CurrentYinPoints)}/{Mathf.FloorToInt(currentMaxPoints)}";
        }

        if (yangPointsText != null)
        {
            yangPointsText.text = $"{Mathf.FloorToInt(CurrentYangPoints)}/{Mathf.FloorToInt(currentMaxPoints)}";
        }

        // ʣ�������ʾ��ʽ�޸�Ϊ ����/��������ȥ�����ţ�
        float remainingPoints = currentMaxPoints - (CurrentYangPoints + CurrentYinPoints);
        if (remainingPointsText != null)
        {
            // ��ʾΪ ����/������ ��ʽ��ȥ�����ţ�ʣ���������ȡ����
            int remainingInteger = Mathf.FloorToInt(remainingPoints);
            int maxInteger = Mathf.FloorToInt(currentMaxPoints);
            remainingPointsText.text = $"{remainingInteger}/{maxInteger}";
        }

        // ��������ʾ��ʽ�޸�Ϊ (��������/������)
        if (maxPointsText != null)
        {
            // ��ʾΪ (��������/������) ��ʽ������ʾΪ����
            maxPointsText.text = $"({Mathf.FloorToInt(retainedPoints)}/{Mathf.FloorToInt(currentMaxPoints)})";
        }

        // ״̬��ʾ��Ϊͼ��
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

    // �޸ģ�ֻ����״̬ͼ�꣬��������������UI
    public void HideWheelUI()
    {
        if (stateIcon != null)
        {
            stateIcon.enabled = false;
        }
    }

    // �޸ģ���ʾ״̬ͼ��
    public void ShowWheelUI()
    {
        if (stateIcon != null)
        {
            stateIcon.enabled = true;
            // ����ͼ����ʾ
            float diff = CurrentYangPoints - CurrentYinPoints;
            SetStateIcon(diff);
        }
    }

    // ��������ȫ������������UI��ԭ�����߼���
    public void HideFullWheelUI()
    {
        if (wheelUI != null)
        {
            wheelUI.SetActive(false);
        }
    }

    // ��������ȫ��ʾ��������UI��ԭ�����߼���
    public void ShowFullWheelUI()
    {
        if (wheelUI != null)
        {
            wheelUI.SetActive(true);
            // ͬʱ��ʾ״̬ͼ��
            ShowWheelUI();
        }
    }
}