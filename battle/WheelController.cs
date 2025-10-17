using UnityEngine;
using UnityEngine.UI;

public class WheelController : MonoBehaviour
{
    [Header("Wheel System")]
    public WheelSystem wheelSystem;

    [Header("Slider Controls")]
    public Slider yangSlider;
    public Slider yinSlider;

    private bool isUpdating = false; // 防止递归更新

    void Start()
    {
        // 绑定滑块事件
        if (yangSlider != null)
        {
            yangSlider.onValueChanged.AddListener(OnYangSliderChanged);
        }

        if (yinSlider != null)
        {
            yinSlider.onValueChanged.AddListener(OnYinSliderChanged);
        }
    }

    public void Initialize(float maxPoints)
    {
        if (yangSlider != null)
        {
            yangSlider.minValue = 0;
            yangSlider.maxValue = maxPoints;
            yangSlider.wholeNumbers = false; // 允许小数
            yangSlider.value = 0; // 初始化为0
        }

        if (yinSlider != null)
        {
            yinSlider.minValue = 0;
            yinSlider.maxValue = maxPoints;
            yinSlider.wholeNumbers = false; // 允许小数
            yinSlider.value = 0; // 初始化为0
        }
    }

    // 重置滑块方法
    public void ResetSliders()
    {
        if (yangSlider != null) yangSlider.value = 0;
        if (yinSlider != null) yinSlider.value = 0;

        // 更新滑块最大值为当前轮盘的最大值
        UpdateSliderMaxValues();
    }

    // 新增：更新滑块最大值方法
    public void UpdateSliderMaxValues()
    {
        if (wheelSystem != null && yangSlider != null && yinSlider != null)
        {
            float currentMaxPoints = wheelSystem.MaxPoints;
            yangSlider.maxValue = currentMaxPoints;
            yinSlider.maxValue = currentMaxPoints;

            // 确保当前值不超过新的最大值
            if (yangSlider.value > currentMaxPoints)
            {
                yangSlider.value = currentMaxPoints;
                if (wheelSystem != null)
                {
                    wheelSystem.SetYangPoints(currentMaxPoints);
                }
            }

            if (yinSlider.value > currentMaxPoints)
            {
                yinSlider.value = currentMaxPoints;
                if (wheelSystem != null)
                {
                    wheelSystem.SetYinPoints(currentMaxPoints);
                }
            }
        }
    }

    private void OnYangSliderChanged(float value)
    {
        if (isUpdating || wheelSystem == null) return;
        isUpdating = true;

        // 设置阳点数
        wheelSystem.SetYangPoints(value);

        // 更新阴滑块最大值
        if (wheelSystem != null && yinSlider != null)
        {
            yinSlider.maxValue = wheelSystem.MaxPoints - wheelSystem.CurrentYangPoints;

            // 如果阴点数超过新限制，调整阴点数
            if (wheelSystem.CurrentYinPoints > yinSlider.maxValue)
            {
                wheelSystem.SetYinPoints(yinSlider.maxValue);
                yinSlider.value = yinSlider.maxValue;
            }
        }

        isUpdating = false;
    }

    private void OnYinSliderChanged(float value)
    {
        if (isUpdating || wheelSystem == null) return;
        isUpdating = true;

        // 设置阴点数
        wheelSystem.SetYinPoints(value);

        // 更新阳滑块最大值
        if (wheelSystem != null && yangSlider != null)
        {
            yangSlider.maxValue = wheelSystem.MaxPoints - wheelSystem.CurrentYinPoints;

            // 如果阳点数超过新限制，调整阳点数
            if (wheelSystem.CurrentYangPoints > yangSlider.maxValue)
            {
                wheelSystem.SetYangPoints(yangSlider.maxValue);
                yangSlider.value = yangSlider.maxValue;
            }
        }

        isUpdating = false;
    }
}