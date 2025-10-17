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
    public float CounterStrikeMultiplier { get; set; } // 可用于内部计算，如极端阴的1.5倍
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
        // 计算总血量：基础血量 + 加成血量
        int totalHealth = config.playerBaseHealth;
        if (PersistentBattleData.Instance != null)
        {
            totalHealth = PersistentBattleData.Instance.GetPlayerTotalHealth();
            Debug.Log($"Player total health calculated: Base {config.playerBaseHealth} + Bonus {PersistentBattleData.Instance.GetPlayerHealthBonus()} = {totalHealth}");
        }

        MaxHealth = totalHealth;
        Health = MaxHealth;

        // 初始化其他属性
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
        // 检查是否处于阴盛状态 (5 > 阴-阳 > 2.5)
        if (BattleSystem.Instance != null && BattleSystem.Instance.wheelSystem != null)
        {
            float yangPoints = BattleSystem.Instance.wheelSystem.CurrentYangPoints;
            float yinPoints = BattleSystem.Instance.wheelSystem.CurrentYinPoints;
            float diff = yinPoints - yangPoints; // 阴 - 阳
            return diff > 2.5f && diff < 5f; // (5 > 阴-阳 > 2.5)
        }
        return false;
    }

    public bool IsInExtremeYinState()
    {
        // 检查是否处于极端阴状态 (7 >= 阴-阳 >= 5)
        if (BattleSystem.Instance != null && BattleSystem.Instance.wheelSystem != null)
        {
            float yangPoints = BattleSystem.Instance.wheelSystem.CurrentYangPoints;
            float yinPoints = BattleSystem.Instance.wheelSystem.CurrentYinPoints;
            float diff = yinPoints - yangPoints; // 阴 - 阳
            return diff >= 5f && diff <= 7f; // (7 >= 阴-阳 >= 5)
        }
        return false;
    }

    public bool IsInUltimateQiState()
    {
        // 检查是否处于究级气状态 (10 >= |阳-阴| > 7)
        if (BattleSystem.Instance != null && BattleSystem.Instance.wheelSystem != null)
        {
            float yangPoints = BattleSystem.Instance.wheelSystem.CurrentYangPoints;
            float yinPoints = BattleSystem.Instance.wheelSystem.CurrentYinPoints;
            float absDiff = Mathf.Abs(yangPoints - yinPoints); // |阳-阴|
            return absDiff > 7f && absDiff <= 10f; // (10 >= |阳-阴| > 7)
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
        CounterStrikeMultiplier = multiplier; // 可存储倍率信息，供内部计算使用 (虽然当前逻辑未直接使用)
        CounterStrikeDuration = duration;
        Debug.Log($"PlayerManager: Activated Counter Strike. Multiplier: {multiplier}, Duration: {duration} turns.");
    }

    public void ResetForNewTurn()
    {
        DamageTakenLastTurn = 0;
        // Attack = Mathf.Floor(Attack / 2f); // 原始代码有这个，但根据新规则，只有特定Debuff才减半
        // Defense = Mathf.Floor(Defense / 2f); // 原始代码有这个，但根据新规则，只有特定Debuff才减半

        // --- 处理反震持续时间 ---
        if (CounterStrikeDuration > 0)
        {
            CounterStrikeDuration--;
            if (CounterStrikeDuration > 0)
            {
                CounterStrikeActive = true; // 保持激活
                Debug.Log($"PlayerManager: Counter strike duration: {CounterStrikeDuration} turns remaining");
            }
            else
            {
                CounterStrikeActive = false; // 失效
                CounterStrikeMultiplier = 1.0f;
                Debug.Log("PlayerManager: Counter strike effect expired");
            }
        }
        else
        {
            // 如果本来就是0或负数，确保状态是关闭的
            CounterStrikeActive = false;
            CounterStrikeMultiplier = 1.0f;
        }

        // --- 处理下回合攻击力Debuff ---
        if (NextTurnAttackDebuff)
        {
            Attack = Mathf.Floor(Attack * 0.5f);
            NextTurnAttackDebuff = false; // 重置标记
            Debug.Log("PlayerManager: Applied NextTurnAttackDebuff.");
        }

        // --- 处理下回合防御力Debuff ---
        if (NextTurnDefenseDebuff)
        {
            Defense = Mathf.Floor(Defense * 0.5f);
            NextTurnDefenseDebuff = false; // 重置标记
            Debug.Log("PlayerManager: Applied NextTurnDefenseDebuff.");
        }

        // --- 原始的Attack/Defense减半逻辑移除或注释掉 ---
        // Attack = Mathf.Floor(Attack / 2f);
        // Defense = Mathf.Floor(Defense / 2f);

        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }
}
