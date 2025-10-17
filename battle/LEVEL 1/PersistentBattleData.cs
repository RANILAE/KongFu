using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentBattleData : MonoBehaviour
{
    public static PersistentBattleData Instance { get; private set; }

    [Header("Player Configuration")]
    [Tooltip("Player's base health when starting a new game")]
    public int playerBaseHealth = 100; // ��һ���Ѫ��

    [Header("Health Bonus Configuration")]
    [Tooltip("Health bonus amount per level")]
    public int healthBonusPerLevel = 20; // ÿ�ص�Ѫ���ӳ�

    // ����ʱ����
    public int currentPlayerHealthBonus = 0; // ��ǰѪ���ӳ�
    public int currentEnemyId = 0; // ��ǰ����ID

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("PersistentBattleData initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentSceneName = scene.name;
        Debug.Log($"Scene loaded: {currentSceneName}");

        // ���ݵ�ǰ�ؿ���������Ѫ���ӳ�
        switch (currentSceneName)
        {
            case "Level1":
                // ��һ�أ�������������
                currentPlayerHealthBonus = 0;
                Debug.Log("Level1: Health bonus reset to 0");
                break;

            case "Level2":
                // �ڶ��أ����õ�һ���ӳ�
                currentPlayerHealthBonus = healthBonusPerLevel;
                Debug.Log($"Level2: Health bonus set to {currentPlayerHealthBonus}");
                break;

            case "Level3":
                // �����أ����õڶ����ӳ�
                currentPlayerHealthBonus = healthBonusPerLevel * 2;
                Debug.Log($"Level3: Health bonus set to {currentPlayerHealthBonus}");
                break;

            default:
                Debug.Log($"Unknown level: {currentSceneName}");
                break;
        }
    }

    public int GetPlayerTotalHealth()
    {
        return playerBaseHealth + currentPlayerHealthBonus;
    }

    public int GetPlayerBaseHealth()
    {
        return playerBaseHealth;
    }

    public int GetPlayerHealthBonus()
    {
        return currentPlayerHealthBonus;
    }

    public void SetNextEnemyId(int enemyId)
    {
        currentEnemyId = enemyId;
    }

    public int GetNextEnemyId()
    {
        return currentEnemyId;
    }

    // �����������ݣ�����Ϸ��ʼʱ���ã�
    public void ResetAllData()
    {
        currentPlayerHealthBonus = 0;
        currentEnemyId = 0;
        Debug.Log("All battle data reset");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}