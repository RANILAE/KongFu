using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VisualNovelController : MonoBehaviour
{
    [Header("视觉小说数据")]
    public List<GameObject> backgroundObjects; // 所有背景图 GameObject（带 SpriteRenderer）
    public List<string> dialogTexts;           // 对应每张图的文本

    [Header("场景引用")]
    public TMP_Text textComponent;            // 主对话文本（剧情）

    [Header("按钮控制")]
    public Button nextButton;                 // “下一张”按钮
    public Button endButton;                  // “结束”按钮（播放完显示）

    private int currentIndex = 0;

    void Start()
    {
        if (backgroundObjects.Count == 0)
        {
            Debug.LogError("请在 Inspector 中添加背景图 GameObject 列表！");
            return;
        }

        if (dialogTexts.Count == 0)
        {
            dialogTexts = new List<string>();
            for (int i = 0; i < backgroundObjects.Count; i++)
                dialogTexts.Add("");
        }

        while (dialogTexts.Count < backgroundObjects.Count)
            dialogTexts.Add("");

        // 初始化按钮状态
        if (endButton != null)
            endButton.gameObject.SetActive(false);

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextScene);
        }

        // 先全部隐藏
        HideAllBackgrounds();

        // 加载第一张
        LoadScene(0);
    }

    void HideAllBackgrounds()
    {
        foreach (var bg in backgroundObjects)
        {
            if (bg != null)
                bg.SetActive(false);
        }
    }

    void LoadScene(int index)
    {
        if (index >= backgroundObjects.Count)
        {
            Debug.Log("视觉小说播放完毕。");
            return;
        }

        // 先全部隐藏（保险起见）
        HideAllBackgrounds();

        // 激活当前背景图
        if (backgroundObjects[index] != null)
        {
            backgroundObjects[index].SetActive(true);
        }

        // 设置主对话文本
        string displayText = dialogTexts[index];
        if (textComponent != null)
        {
            textComponent.text = displayText;
        }

        // 🟢 如果是最后一张图，立即切换按钮状态
        bool isLastScene = (index == backgroundObjects.Count - 1);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(!isLastScene);
        }

        if (endButton != null)
        {
            endButton.gameObject.SetActive(isLastScene);
        }

        currentIndex = index;
    }

    public void NextScene()
    {
        LoadScene(currentIndex + 1);
    }
}