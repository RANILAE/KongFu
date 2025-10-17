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
    public float CounterStrikeMultiplier { get; set; }
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

    // ... 其他方法保持不变 ...

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
        CounterStrikeMultiplier = multiplier;
        CounterStrikeDuration = duration;
        Debug.Log($"Activated Counter Strike: Multiplier={multiplier}, Duration={duration}");
    }

    public void ResetForNewTurn()
    {
        DamageTakenLastTurn = 0;
        Attack = Mathf.Floor(Attack / 2f);
        Defense = Mathf.Floor(Defense / 2f);

        if (CounterStrikeDuration > 0)
        {
            CounterStrikeDuration--;
            if (CounterStrikeDuration > 0)
            {
                CounterStrikeActive = true;
                Debug.Log($"Counter strike duration: {CounterStrikeDuration} turns remaining");
            }
            else
            {
                CounterStrikeActive = false;
                CounterStrikeMultiplier = 1.0f;
                Debug.Log("Counter strike effect expired");
            }
        }
        else
        {
            CounterStrikeActive = false;
            CounterStrikeMultiplier = 1.0f;
        }

        if (NextTurnAttackDebuff)
        {
            Attack = Mathf.Floor(Attack * 0.5f);
            NextTurnAttackDebuff = false;
        }
        if (NextTurnDefenseDebuff)
        {
            Defense = Mathf.Floor(Defense * 0.5f);
            NextTurnDefenseDebuff = false;
        }

        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }
}