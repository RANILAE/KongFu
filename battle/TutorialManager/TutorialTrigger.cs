using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Manager")]
    public TutorialManager tutorialManager;

    [Header("Trigger Settings")]
    public KeyCode triggerKey = KeyCode.T; // 触发教程的按键
    public bool useUIButton = false; // 是否使用UI按钮触发
    public string triggerButtonName = "TutorialButton"; // UI按钮名称（如果使用UI触发）

    void Start()
    {
        // 如果没有指定tutorialManager，尝试自动查找
        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialManager>();
        }
    }

    void Update()
    {
        // 按键触发教程
        if (Input.GetKeyDown(triggerKey))
        {
            TriggerTutorial();
        }
    }

    // 公共方法：可以被UI按钮调用
    public void TriggerTutorial()
    {
        if (tutorialManager != null)
        {
            // 重新开始教程
            tutorialManager.SetCurrentStep(0);

            // 确保教程面板激活
            if (tutorialManager.tutorialPanel != null)
            {
                tutorialManager.tutorialPanel.SetActive(true);
            }

            Debug.Log("教程已重新触发！");
        }
        else
        {
            Debug.LogError("未找到TutorialManager！");
        }
    }

    // 重置并开始教程（可选的更彻底的重置方法）
    public void ResetAndStartTutorial()
    {
        if (tutorialManager != null)
        {
            // 重置所有状态
            tutorialManager.SetCurrentStep(0);

            // 重新开始教程
            tutorialManager.StartTutorial();

            Debug.Log("教程已重置并重新开始！");
        }
    }

    // 如果使用碰撞触发（可选）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTutorial();
        }
    }

    // 如果使用3D碰撞触发（可选）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTutorial();
        }
    }
}