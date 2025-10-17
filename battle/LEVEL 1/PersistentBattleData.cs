using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentBattleData : MonoBehaviour
{
    public static PersistentBattleData Instance { get; private set; }

    [Header("Player Configuration")]
    [Tooltip("Player's base health when starting a new game")]
    public int playerBaseHealth = 100; // 玩家基础血量

    [Header("Health Bonus Configuration")]
    [Tooltip("Health bonus amount per level")]
    public int healthBonusPerLevel = 20; // 每关的血量加成

    // 运行时数据
    public int currentPlayerHealthBonus = 0; // 当前血量加成
    public int currentEnemyId = 0; // 当前敌人ID

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

        // 根据当前关卡名称设置血量加成
        switch (currentSceneName)
        {
            case "Level1":
                // 第一关：重置所有数据
                currentPlayerHealthBonus = 0;
                Debug.Log("Level1: Health bonus reset to 0");
                break;

            case "Level2":
                // 第二关：设置第一级加成
                currentPlayerHealthBonus = healthBonusPerLevel;
                Debug.Log($"Level2: Health bonus set to {currentPlayerHealthBonus}");
                break;

            case "Level3":
                // 第三关：设置第二级加成
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

    // 重置所有数据（新游戏开始时调用）
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