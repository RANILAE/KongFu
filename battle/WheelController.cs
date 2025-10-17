using UnityEngine;
using UnityEngine.UI;

public class WheelController : MonoBehaviour
{
    [Header("Wheel System")]
    public WheelSystem wheelSystem;

    [Header("Slider Controls")]
    public Slider yangSlider;
    public Slider yinSlider;

    private bool isUpdating = false; // ��ֹ�ݹ����

    void Start()
    {
        // �󶨻����¼�
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
            yangSlider.wholeNumbers = false; // ����С��
            yangSlider.value = 0; // ��ʼ��Ϊ0
        }

        if (yinSlider != null)
        {
            yinSlider.minValue = 0;
            yinSlider.maxValue = maxPoints;
            yinSlider.wholeNumbers = false; // ����С��
            yinSlider.value = 0; // ��ʼ��Ϊ0
        }
    }

    // ���û��鷽��
    public void ResetSliders()
    {
        if (yangSlider != null) yangSlider.value = 0;
        if (yinSlider != null) yinSlider.value = 0;
    }

    private void OnYangSliderChanged(float value)
    {
        if (isUpdating) return;
        isUpdating = true;

        // ����������
        wheelSystem.SetYangPoints(value);

        // �������������ֵ
        // ʹ�ù�������MaxPoints
        yinSlider.maxValue = wheelSystem.MaxPoints - wheelSystem.CurrentYangPoints;

        // ������������������ƣ�����������
        if (wheelSystem.CurrentYinPoints > yinSlider.maxValue)
        {
            wheelSystem.SetYinPoints(yinSlider.maxValue);
            yinSlider.value = yinSlider.maxValue;
        }

        isUpdating = false;
    }

    private void OnYinSliderChanged(float value)
    {
        if (isUpdating) return;
        isUpdating = true;

        // ����������
        wheelSystem.SetYinPoints(value);

        // �������������ֵ
        // ʹ�ù�������MaxPoints
        yangSlider.maxValue = wheelSystem.MaxPoints - wheelSystem.CurrentYinPoints;

        // ������������������ƣ�����������
        if (wheelSystem.CurrentYangPoints > yangSlider.maxValue)
        {
            wheelSystem.SetYangPoints(yangSlider.maxValue);
            yangSlider.value = yangSlider.maxValue;
        }

        isUpdating = false;
    }
}
