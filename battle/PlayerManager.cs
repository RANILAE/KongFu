using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Property fields
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public float Attack { get; set; }
    public float Defense { get; set; }

    // State fields
    public bool CounterStrikeActive { get; set; }
    public float CounterStrikeMultiplier { get; set; } // �������ڲ����㣬�缫������1.5��
    public int CounterStrikeDuration { get; set; }
    public bool NextTurnAttackDebuff { get; set; }
    public bool NextTurnDefenseDebuff { get; set; }
    public int DamageTakenLastTurn { get; set; }
    public float RetainedPoints { get; set; }

    // Stack system
    public int YangStacks { get; private set; }
    public int YinStacks { get; private set; }

    public void Initialize(BattleConfig config)
    {
        // ������Ѫ��������Ѫ�� + �ӳ�Ѫ��
        int totalHealth = config.playerBaseHealth;
        if (PersistentBattleData.Instance != null)
        {
            totalHealth = PersistentBattleData.Instance.GetPlayerTotalHealth();
            Debug.Log($"Player total health calculated: Base {config.playerBaseHealth} + Bonus {PersistentBattleData.Instance.GetPlayerHealthBonus()} = {totalHealth}");
        }

        MaxHealth = totalHealth;
        Health = MaxHealth;

        // ��ʼ����������
        Attack = 0;
        Defense = 0;
        DamageTakenLastTurn = 0;
        CounterStrikeActive = false;
        CounterStrikeMultiplier = 1.0f;
        CounterStrikeDuration = 0;
        NextTurnAttackDebuff = false;
        NextTurnDefenseDebuff = false;
        RetainedPoints = 0f;
        YangStacks = 0;
        YinStacks = 0;

        Debug.Log($"PlayerManager initialized - Health: {Health}/{MaxHealth}");
    }

    public bool IsInYinProsperityState()
    {
        // ����Ƿ�����ʢ״̬ (5 > ��-�� > 2.5)
        if (BattleSystem.Instance != null && BattleSystem.Instance.wheelSystem != null)
        {
            float yangPoints = BattleSystem.Instance.wheelSystem.CurrentYangPoints;
            float yinPoints = BattleSystem.Instance.wheelSystem.CurrentYinPoints;
            float diff = yinPoints - yangPoints; // �� - ��
            return diff > 2.5f && diff < 5f; // (5 > ��-�� > 2.5)
        }
        return false;
    }

    public bool IsInExtremeYinState()
    {
        // ����Ƿ��ڼ�����״̬ (7 >= ��-�� >= 5)
        if (BattleSystem.Instance != null && BattleSystem.Instance.wheelSystem != null)
        {
            float yangPoints = BattleSystem.Instance.wheelSystem.CurrentYangPoints;
            float yinPoints = BattleSystem.Instance.wheelSystem.CurrentYinPoints;
            float diff = yinPoints - yangPoints; // �� - ��
            return diff >= 5f && diff <= 7f; // (7 >= ��-�� >= 5)
        }
        return false;
    }

    public bool IsInUltimateQiState()
    {
        // ����Ƿ��ھ�����״̬ (10 >= |��-��| > 7)
        if (BattleSystem.Instance != null && BattleSystem.Instance.wheelSystem != null)
        {
            float yangPoints = BattleSystem.Instance.wheelSystem.CurrentYangPoints;
            float yinPoints = BattleSystem.Instance.wheelSystem.CurrentYinPoints;
            float absDiff = Mathf.Abs(yangPoints - yinPoints); // |��-��|
            return absDiff > 7f && absDiff <= 10f; // (10 >= |��-��| > 7)
        }
        return false;
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        DamageTakenLastTurn += (int)damage;
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
        }
    }

    public void SetHealth(float newHealth)
    {
        Health = newHealth;
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
        }
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(MaxHealth, Health + amount);
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
        }
    }

    public float CalculateDamage(float enemyDefense)
    {
        return Mathf.Max(0, Attack - enemyDefense);
    }

    public void AdjustAttack(float amount)
    {
        Attack = Mathf.Max(0, Attack + amount);
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void AdjustDefense(float amount)
    {
        Defense = Mathf.Max(0, Defense + amount);
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void ApplyAttributes(float attack, float defense)
    {
        Attack = attack;
        Defense = defense;
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void AddYangStacks()
    {
        YangStacks++;
    }

    public void AddYinStacks()
    {
        YinStacks++;
    }

    public void ResetYangStacks()
    {
        YangStacks = 0;
    }

    public void ResetYinStacks()
    {
        YinStacks = 0;
    }

    public void ActivateCounterStrike(float multiplier, int duration = 1)
    {
        CounterStrikeActive = true;
        CounterStrikeMultiplier = multiplier; // �ɴ洢������Ϣ�����ڲ�����ʹ�� (��Ȼ��ǰ�߼�δֱ��ʹ��)
        CounterStrikeDuration = duration;
        Debug.Log($"PlayerManager: Activated Counter Strike. Multiplier: {multiplier}, Duration: {duration} turns.");
    }

    public void ResetForNewTurn()
    {
        DamageTakenLastTurn = 0;
        // Attack = Mathf.Floor(Attack / 2f); // ԭʼ������������������¹���ֻ���ض�Debuff�ż���
        // Defense = Mathf.Floor(Defense / 2f); // ԭʼ������������������¹���ֻ���ض�Debuff�ż���

        // --- ���������ʱ�� ---
        if (CounterStrikeDuration > 0)
        {
            CounterStrikeDuration--;
            if (CounterStrikeDuration > 0)
            {
                CounterStrikeActive = true; // ���ּ���
                Debug.Log($"PlayerManager: Counter strike duration: {CounterStrikeDuration} turns remaining");
            }
            else
            {
                CounterStrikeActive = false; // ʧЧ
                CounterStrikeMultiplier = 1.0f;
                Debug.Log("PlayerManager: Counter strike effect expired");
            }
        }
        else
        {
            // �����������0������ȷ��״̬�ǹرյ�
            CounterStrikeActive = false;
            CounterStrikeMultiplier = 1.0f;
        }

        // --- �����»غϹ�����Debuff ---
        if (NextTurnAttackDebuff)
        {
            Attack = Mathf.Floor(Attack * 0.5f);
            NextTurnAttackDebuff = false; // ���ñ��
            Debug.Log("PlayerManager: Applied NextTurnAttackDebuff.");
        }

        // --- �����»غϷ�����Debuff ---
        if (NextTurnDefenseDebuff)
        {
            Defense = Mathf.Floor(Defense * 0.5f);
            NextTurnDefenseDebuff = false; // ���ñ��
            Debug.Log("PlayerManager: Applied NextTurnDefenseDebuff.");
        }

        // --- ԭʼ��Attack/Defense�����߼��Ƴ���ע�͵� ---
        // Attack = Mathf.Floor(Attack / 2f);
        // Defense = Mathf.Floor(Defense / 2f);

        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }
}
