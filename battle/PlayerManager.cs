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
    public int CounterStrikeDuration { get; set; } // ����Ч������ʱ��
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
        MaxHealth = config.playerBaseHealth; // ����������ؿ����������޼ӳ�
        Health = MaxHealth;
        Attack = 0;
        Defense = 0;
        DamageTakenLastTurn = 0;
        CounterStrikeActive = false;
        CounterStrikeMultiplier = 1.0f;
        CounterStrikeDuration = 0; // ��ʼ���������ʱ��Ϊ0
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
        // ע�⣺���ﲻ��ֱ�ӵ��ö�������BattleSystem�����ƶ���˳��
        // ��������ܻ��������߼��Ƶ�BattleSystem��
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
        }
    }

    public void SetHealth(float newHealth)
    {
        Health = newHealth;
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerStatus(Health, MaxHealth, Attack, Defense);
        }
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(MaxHealth, Health + amount);
        // ����UI
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
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void AdjustDefense(float amount)
    {
        Defense = Mathf.Max(0, Defense + amount);
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
    }

    public void ApplyAttributes(float attack, float defense)
    {
        Attack = attack;
        Defense = defense;
        // ����UI
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
        CounterStrikeDuration = duration; // ʹ�ô���ĳ���ʱ��
        iconManager.AddPlayerIcon(IconManager.IconType.CounterStrike, multiplier, duration);
        Debug.Log($"Activated Counter Strike: Multiplier={multiplier}, Duration={duration}");
    }

    public void ResetForNewTurn()
    {
        DamageTakenLastTurn = 0;
        // Points retention rule: keep half of unused points (floor)
        Attack = Mathf.Floor(Attack / 2f);
        Defense = Mathf.Floor(Defense / 2f);
        // ������Ч������ʱ��
        if (CounterStrikeDuration > 0)
        {
            CounterStrikeDuration--;
            if (CounterStrikeDuration > 0)
            {
                // ����Ч����Ȼ��Ч
                CounterStrikeActive = true;
                Debug.Log($"Counter strike duration: {CounterStrikeDuration} turns remaining");
            }
            else
            {
                // ����Ч������
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
            // �Ƴ�ͼ��
            if (iconManager != null)
            {
                iconManager.RemovePlayerIcon(IconManager.IconType.AttackDebuff);
            }
        }
        if (NextTurnDefenseDebuff)
        {
            Defense = Mathf.Floor(Defense * 0.5f);
            NextTurnDefenseDebuff = false;
            // �Ƴ�ͼ��
            if (iconManager != null)
            {
                iconManager.RemovePlayerIcon(IconManager.IconType.DefenseDebuff);
            }
        }
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(Attack, Defense);
        }
        // ע�⣺�Ƴ��ظ��ı����������ã���RetainedPointsSystem��������
    }
}