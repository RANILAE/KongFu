using UnityEngine;

public class PersistentBattleData : MonoBehaviour
{
    public static PersistentBattleData Instance { get; private set; }

    // 需要跨关卡传输的数据
    public int playerMaxHealthBonus = 0; // 玩家生命上限提升值
    public int currentEnemyId = 0; // 当前敌人ID

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 每次游戏运行时重置数据
            ResetData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 设置下一关的敌人ID
    public void SetNextEnemyId(int enemyId)
    {
        currentEnemyId = enemyId;
    }

    // 增加玩家生命上限
    public void IncreasePlayerMaxHealth()
    {
        playerMaxHealthBonus += 20;
    }

    // 获取玩家当前生命上限加成
    public int GetPlayerMaxHealthBonus()
    {
        return playerMaxHealthBonus;
    }

    // 获取下一关敌人ID
    public int GetNextEnemyId()
    {
        return currentEnemyId;
    }

    // 重置数据（可选）
    public void ResetData()
    {
        playerMaxHealthBonus = 0;
        currentEnemyId = 0;
    }
}