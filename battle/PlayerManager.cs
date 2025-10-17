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
    public int CounterStrikeDuration { get; set; } // 反震效果持续时间
    public bool NextTurnAttackDebuff { get; set; }
    public bool NextTurnDefenseDebuff { get; set; }
    public int DamageTakenLastTurn { get; set; }
    public float RetainedPoints { get; set; } // Retained points from previous turn

    // Stack system
    public int YangStacks { get; private set; } // Yang penetration stacks
    public int YinStacks { get; private set; } // Yin cover stacks

    private IconManager iconManager;

    public void Initialize(BattleConfig config, IconManager iconManager)
    {
        this.iconManager = iconManager;
        MaxHealth = config.playerBaseHealth; // 这里会包含跨关卡的生命上限加成
        Health = MaxHealth;
        Attack = 0;
        Defense = 0;
        DamageTakenLastTurn = 0;
        CounterStrikeActive = false;
        CounterStrikeMultiplier = 1.0f;
        CounterStrikeDuration = 0; // 初始化反震持续时间为0
        NextTurnAttackDebuff = false;
        NextTurnDefenseDebuff = false;
        RetainedPoints = 0f;
        YangStacks = 0;
        YinStacks = 0;
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        DamageTakenLastTurn += (int)damage;
        // 注意：这里不再直接调用动画，让BattleSystem来控制动画顺序
        // 播放玩家受击动画的逻辑移到BattleSystem中
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
        }
    }

    public void SetHealth(float newHealth)
    {
        Health = newHealth;
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
        }
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(MaxHealth, Health + amount);
        // 更新UI
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
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void AdjustDefense(float amount)
    {
        Defense = Mathf.Max(0, Defense + amount);
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void ApplyAttributes(float attack, float defense)
    {
        Attack = attack;
        Defense = defense;
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void AddYangStacks()
    {
        YangStacks++;
        iconManager.AddPlayerIcon(IconManager.IconType.YangStack, YangStacks);
    }

    public void AddYinStacks()
    {
        YinStacks++;
        iconManager.AddPlayerIcon(IconManager.IconType.YinStack, YinStacks);
    }

    public void ResetYangStacks()
    {
        YangStacks = 0;
        iconManager.RemovePlayerIcon(IconManager.IconType.YangStack);
    }

    public void ResetYinStacks()
    {
        YinStacks = 0;
        iconManager.RemovePlayerIcon(IconManager.IconType.YinStack);
    }

    public void ActivateCounterStrike(float multiplier, int duration = 1)
    {
        CounterStrikeActive = true;
        CounterStrikeMultiplier = multiplier;
        CounterStrikeDuration = duration; // 使用传入的持续时间
        iconManager.AddPlayerIcon(IconManager.IconType.CounterStrike, multiplier, duration);
        Debug.Log($"Activated Counter Strike: Multiplier={multiplier}, Duration={duration}");
    }

    public void ResetForNewTurn()
    {
        DamageTakenLastTurn = 0;
        // Points retention rule: keep half of unused points (floor)
        Attack = Mathf.Floor(Attack / 2f);
        Defense = Mathf.Floor(Defense / 2f);
        // 处理反震效果持续时间
        if (CounterStrikeDuration > 0)
        {
            CounterStrikeDuration--;
            if (CounterStrikeDuration > 0)
            {
                // 反震效果仍然有效
                CounterStrikeActive = true;
                Debug.Log($"Counter strike duration: {CounterStrikeDuration} turns remaining");
            }
            else
            {
                // 反震效果结束
                CounterStrikeActive = false;
                CounterStrikeMultiplier = 1.0f;
                if (iconManager != null)
                {
                    iconManager.RemovePlayerIcon(IconManager.IconType.CounterStrike);
                }
                Debug.Log("Counter strike effect expired");
            }
        }
        else
        {
            CounterStrikeActive = false;
            CounterStrikeMultiplier = 1.0f;
        }
        // Handle possible effects from previous turn
        if (NextTurnAttackDebuff)
        {
            Attack = Mathf.Floor(Attack * 0.5f);
            NextTurnAttackDebuff = false;
            // 移除图标
            if (iconManager != null)
            {
                iconManager.RemovePlayerIcon(IconManager.IconType.AttackDebuff);
            }
        }
        if (NextTurnDefenseDebuff)
        {
            Defense = Mathf.Floor(Defense * 0.5f);
            NextTurnDefenseDebuff = false;
            // 移除图标
            if (iconManager != null)
            {
                iconManager.RemovePlayerIcon(IconManager.IconType.DefenseDebuff);
            }
        }
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
        // 注意：移除重复的保留点数重置，让RetainedPointsSystem独立管理
    }
}