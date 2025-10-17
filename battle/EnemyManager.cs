using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // 属性字段 (使用float)
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public float BaseAttack { get; private set; }
    public float CurrentAttack { get; set; }
    public float Defense { get; private set; }
    public float CurrentDefense { get; set; }
    public int DamageCounter { get; private set; } // 用于触发防御

    public class EnemyAction
    {
        public enum Type { Attack, Defend, Charge }
        public Type type;
    }

    public void Initialize(BattleConfig config)
    {
        MaxHealth = config.enemyBaseHealth;
        Health = MaxHealth;
        BaseAttack = config.enemyBaseAttack;
        CurrentAttack = BaseAttack;
        Defense = 0;
        CurrentDefense = 0;
        DamageCounter = 0;
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        DamageCounter++;
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    public void EnableDefense()
    {
        CurrentDefense = BaseAttack * 0.2f; // 20%伤害减免
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    public void ChargeAttack()
    {
        CurrentAttack += 3; // 永久增加攻击力
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    public float CalculateDamage(float playerDefense)
    {
        return Mathf.Max(0, CurrentAttack - playerDefense);
    }

    public void AdjustAttack(float amount)
    {
        CurrentAttack = Mathf.Max(0, CurrentAttack + amount);
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    // 严格按照图片中的优先级实现敌人AI
    public EnemyAction ChooseAction(int currentRound, int playerDamageTaken)
    {
        // 1. 蓄力技能：最高优先级（每4回合）
        if (currentRound % 4 == 1)
        {
            // 如果不是第一回合
            if (currentRound > 1)
            {
                return new EnemyAction { type = EnemyAction.Type.Charge };
            }
        }

        // 2. 防御技能：当受到2次伤害后触发
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // 重置计数器
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }

        // 3. 伤害技能：默认行为
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }
}