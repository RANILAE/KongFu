using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelProgressController : MonoBehaviour
{
    public static LevelProgressController Instance { get; private set; }

    [System.Serializable]
    public class LevelInfo
    {
        public string scenePath;        // 场景路径
        public bool isUnlocked;         // 是否解锁
        public bool isCompleted;        // 是否完成
    }

    [Header("关卡列表设置")]
    public List<LevelInfo> levels = new List<LevelInfo>();

    private int currentActiveLevel = -1;
    private bool isInitialized = false; // 标记是否已初始化

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("关卡管理器初始化完成");

            // 初始化关卡状态
            InitializeLevelStates();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初始化关卡状态（关键修复）
    private void InitializeLevelStates()
    {
        // 确保只初始化一次
        if (isInitialized) return;
        isInitialized = true;

        // 如果没有关卡，直接返回
        if (levels.Count == 0) return;

        // 确保第一关解锁
        levels[0].isUnlocked = true;
        levels[0].isCompleted = false;

        // 锁定其他关卡
        for (int i = 1; i < levels.Count; i++)
        {
            levels[i].isUnlocked = false;
            levels[i].isCompleted = false;
        }

        Debug.Log("关卡状态初始化完成：第一关解锁，其他锁定");
    }

    // 添加新关卡
    public void AddLevel(string scenePath, bool isUnlocked = false, bool isCompleted = false)
    {
        if (!LevelExists(scenePath))
        {
            levels.Add(new LevelInfo
            {
                scenePath = scenePath,
                isUnlocked = isUnlocked,
                isCompleted = isCompleted
            });
            Debug.Log($"添加新关卡: {scenePath}");
        }
    }

    // 检查关卡是否存在
    public bool LevelExists(string scenePath)
    {
        return levels.Exists(l => l.scenePath == scenePath);
    }

    // 获取关卡信息
    public LevelInfo GetLevelInfo(string scenePath)
    {
        return levels.Find(l => l.scenePath == scenePath);
    }

    // 加载关卡
    public void LoadLevel(string scenePath)
    {
        LevelInfo level = GetLevelInfo(scenePath);

        // 关卡未配置时自动添加
        if (level == null)
        {
            // 新关卡默认锁定
            AddLevel(scenePath, false);
            level = GetLevelInfo(scenePath);
        }

        if (!level.isUnlocked)
        {
            Debug.LogWarning($"关卡 {scenePath} 尚未解锁");
            return;
        }

        if (level.isCompleted)
        {
            Debug.LogWarning($"关卡 {scenePath} 已完成");
            return;
        }

        // 标记当前激活关卡
        currentActiveLevel = levels.IndexOf(level);
        Debug.Log($"开始关卡: {scenePath}");

        // 加载场景
        GameManager.Instance.LoadLevel(scenePath);
    }

    // 完成当前关卡
    public void CompleteCurrentLevel()
    {
        if (currentActiveLevel >= 0 && currentActiveLevel < levels.Count)
        {
            LevelInfo level = levels[currentActiveLevel];
            level.isCompleted = true;

            // 解锁下一关（如果有）
            if (currentActiveLevel + 1 < levels.Count)
            {
                levels[currentActiveLevel + 1].isUnlocked = true;
                Debug.Log($"解锁新关卡: {levels[currentActiveLevel + 1].scenePath}");
            }

            Debug.Log($"完成关卡: {level.scenePath}");
            currentActiveLevel = -1;
        }
    }

    // 刷新所有按钮状态
    public void RefreshAllButtons()
    {
        LevelButton[] buttons = FindObjectsOfType<LevelButton>();
        foreach (var button in buttons)
        {
            if (button != null) button.UpdateButtonState();
        }
    }
}