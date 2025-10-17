using UnityEngine;

public class PersistentBattleData : MonoBehaviour
{
    public static PersistentBattleData Instance { get; private set; }

    // ��Ҫ��ؿ����������
    public int playerMaxHealthBonus = 0; // ���������������ֵ
    public int currentEnemyId = 0; // ��ǰ����ID

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // ÿ����Ϸ����ʱ��������
            ResetData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ������һ�صĵ���ID
    public void SetNextEnemyId(int enemyId)
    {
        currentEnemyId = enemyId;
    }

    // ���������������
    public void IncreasePlayerMaxHealth()
    {
        playerMaxHealthBonus += 20;
    }

    // ��ȡ��ҵ�ǰ�������޼ӳ�
    public int GetPlayerMaxHealthBonus()
    {
        return playerMaxHealthBonus;
    }

    // ��ȡ��һ�ص���ID
    public int GetNextEnemyId()
    {
        return currentEnemyId;
    }

    // �������ݣ���ѡ��
    public void ResetData()
    {
        playerMaxHealthBonus = 0;
        currentEnemyId = 0;
    }
}