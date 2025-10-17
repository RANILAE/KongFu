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
        public string scenePath;        // ����·��
        public bool isUnlocked;         // �Ƿ����
        public bool isCompleted;        // �Ƿ����
    }

    [Header("�ؿ��б�����")]
    public List<LevelInfo> levels = new List<LevelInfo>();

    private int currentActiveLevel = -1;
    private bool isInitialized = false; // ����Ƿ��ѳ�ʼ��

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("�ؿ���������ʼ�����");

            // ��ʼ���ؿ�״̬
            InitializeLevelStates();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ��ʼ���ؿ�״̬���ؼ��޸���
    private void InitializeLevelStates()
    {
        // ȷ��ֻ��ʼ��һ��
        if (isInitialized) return;
        isInitialized = true;

        // ���û�йؿ���ֱ�ӷ���
        if (levels.Count == 0) return;

        // ȷ����һ�ؽ���
        levels[0].isUnlocked = true;
        levels[0].isCompleted = false;

        // ���������ؿ�
        for (int i = 1; i < levels.Count; i++)
        {
            levels[i].isUnlocked = false;
            levels[i].isCompleted = false;
        }

        Debug.Log("�ؿ�״̬��ʼ����ɣ���һ�ؽ�������������");
    }

    // ����¹ؿ�
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
            Debug.Log($"����¹ؿ�: {scenePath}");
        }
    }

    // ���ؿ��Ƿ����
    public bool LevelExists(string scenePath)
    {
        return levels.Exists(l => l.scenePath == scenePath);
    }

    // ��ȡ�ؿ���Ϣ
    public LevelInfo GetLevelInfo(string scenePath)
    {
        return levels.Find(l => l.scenePath == scenePath);
    }

    // ���عؿ�
    public void LoadLevel(string scenePath)
    {
        LevelInfo level = GetLevelInfo(scenePath);

        // �ؿ�δ����ʱ�Զ����
        if (level == null)
        {
            // �¹ؿ�Ĭ������
            AddLevel(scenePath, false);
            level = GetLevelInfo(scenePath);
        }

        if (!level.isUnlocked)
        {
            Debug.LogWarning($"�ؿ� {scenePath} ��δ����");
            return;
        }

        if (level.isCompleted)
        {
            Debug.LogWarning($"�ؿ� {scenePath} �����");
            return;
        }

        // ��ǵ�ǰ����ؿ�
        currentActiveLevel = levels.IndexOf(level);
        Debug.Log($"��ʼ�ؿ�: {scenePath}");

        // ���س���
        GameManager.Instance.LoadLevel(scenePath);
    }

    // ��ɵ�ǰ�ؿ�
    public void CompleteCurrentLevel()
    {
        if (currentActiveLevel >= 0 && currentActiveLevel < levels.Count)
        {
            LevelInfo level = levels[currentActiveLevel];
            level.isCompleted = true;

            // ������һ�أ�����У�
            if (currentActiveLevel + 1 < levels.Count)
            {
                levels[currentActiveLevel + 1].isUnlocked = true;
                Debug.Log($"�����¹ؿ�: {levels[currentActiveLevel + 1].scenePath}");
            }

            Debug.Log($"��ɹؿ�: {level.scenePath}");
            currentActiveLevel = -1;
        }
    }

    // ˢ�����а�ť״̬
    public void RefreshAllButtons()
    {
        LevelButton[] buttons = FindObjectsOfType<LevelButton>();
        foreach (var button in buttons)
        {
            if (button != null) button.UpdateButtonState();
        }
    }
}