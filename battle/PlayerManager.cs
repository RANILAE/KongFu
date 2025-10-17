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
        MaxHealth = config.playerBaseHealth;
        Health = MaxHealth;
        Attack = 0;
        Defense = 0;
        DamageTakenLastTurn = 0;
        CounterStrikeActive = false;
        CounterStrikeMultiplier = 1.0f;
        NextTurnAttackDebuff = false;
        NextTurnDefenseDebuff = false;
        RetainedPoints = 0f;
        YangStacks = 0;
        YinStacks = 0;
    }

    public void ApplyAttributes(float attack, float defense)
    {
        Attack = attack;
        Defense = defense;
        BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        DamageTakenLastTurn++;
        BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(Health + amount, MaxHealth);
        BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
    }

    public void SetHealth(float health)
    {
        Health = Mathf.Clamp(health, 0, MaxHealth);
        BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
    }

    public void AdjustDefense(float amount)
    {
        Defense += amount;
        BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
    }

    public void AdjustAttack(float amount)
    {
        Attack += amount;
        BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
    }

    public float CalculateDamage(float enemyDefense)
    {
        return Mathf.Max(0, Attack - enemyDefense);
    }

    public void AddYangStack()
    {
        YangStacks++;
        iconManager.UpdatePlayerIcon(IconManager.IconType.YangStack, YangStacks);
    }

    public void AddYinStack()
    {
        YinStacks++;
        iconManager.UpdatePlayerIcon(IconManager.IconType.YinStack, YinStacks);
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

    public void ActivateCounterStrike(float multiplier)
    {
        CounterStrikeActive = true;
        CounterStrikeMultiplier = multiplier;
        iconManager.AddPlayerIcon(IconManager.IconType.CounterStrike);
    }

    public void ResetForNewTurn()
    {
        DamageTakenLastTurn = 0;

        // Points retention rule: keep half of unused points (floor)
        Attack = Mathf.Floor(Attack / 2f);
        Defense = Mathf.Floor(Defense / 2f);

        CounterStrikeActive = false;
        CounterStrikeMultiplier = 1.0f;
        iconManager.RemovePlayerIcon(IconManager.IconType.CounterStrike);

        // Handle possible effects from previous turn
        if (NextTurnAttackDebuff)
        {
            Attack = Mathf.Floor(Attack * 0.5f);
            NextTurnAttackDebuff = false;
            iconManager.RemovePlayerIcon(IconManager.IconType.AttackDebuff);
        }

        if (NextTurnDefenseDebuff)
        {
            Defense = Mathf.Floor(Defense * 0.5f);
            NextTurnDefenseDebuff = false;
            iconManager.RemovePlayerIcon(IconManager.IconType.DefenseDebuff);
        }

        // Reset retained points (only last one turn)
        RetainedPoints = 0f;
    }
}