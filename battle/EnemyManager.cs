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
    public int DamageCounter { get; private set; } // 用于触发防御（现在改为受到3次攻击）

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

    // 新增：重置敌人状态方法
    public void ResetEnemyState(BattleConfig config)
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
        DamageCounter++; // 每次受到伤害都增加计数器（包括普通攻击和DOT伤害）

        // 注意：这里不再直接调用动画，让BattleSystem来控制动画顺序
        // 播放敌人受击动画的逻辑移到BattleSystem中

        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public void EnableDefense()
    {
        CurrentDefense = BaseAttack * 0.2f; // 20%伤害减免

        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public void ChargeAttack()
    {
        CurrentAttack += 3; // 永久增加攻击力

        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public float CalculateDamage(float playerDefense)
    {
        return Mathf.Max(0, CurrentAttack - playerDefense);
    }

    public void AdjustAttack(float amount)
    {
        CurrentAttack = Mathf.Max(0, CurrentAttack + amount);

        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    // 严格按照图片中的优先级实现敌人AI（修改防御触发条件）
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

        // 2. 防御技能：受到三次攻击时使用（优先级第二，从2次改为3次）
        if (DamageCounter >= 3)
        {
            DamageCounter = 0; // 重置计数器
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }

        // 3. 伤害技能：其余状态均不满足时使用（优先级第三）
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }
}