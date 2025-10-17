using UnityEngine;
// ... 其他 using 语句保持不变 ...

public class EnemyManager : MonoBehaviour
{
    // 新增：敌人类型枚举
    public enum EnemyType { Default, Second, Third }
    private EnemyType currentEnemyType = EnemyType.Default;
    // 新增：状态字段
    public bool IsDefending { get; private set; }
    public int DefenseDuration { get; private set; } // 防御持续回合数
    public int DamageDealtCount { get; private set; } // 造成伤害计数器
    public int DotDuration { get; private set; } // DOT持续回合数
    public float AttackMultiplier { get; private set; } = 1f; // 攻击倍率
    public int DefenseUsedCount { get; private set; } // 防御技能使用次数
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
        // 重置所有状态
        IsDefending = false;
        DefenseDuration = 0;
        DamageDealtCount = 0;
        DotDuration = 0;
        AttackMultiplier = 1f;
        DefenseUsedCount = 0;
        DamageCounter = 0;
        // 根据敌人类型设置属性
        SetEnemyProperties(config);
    }

    // 新增：设置敌人类型
    public void SetEnemyType(EnemyType type)
    {
        currentEnemyType = type;
    }

    // 新增：获取当前敌人类型
    public EnemyType GetCurrentEnemyType()
    {
        return currentEnemyType;
    }

    // 新增：根据敌人类型设置属性
    private void SetEnemyProperties(BattleConfig config)
    {
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                MaxHealth = config.secondEnemy.health;
                Health = config.secondEnemy.health;
                BaseAttack = config.secondEnemy.attack;
                Defense = config.secondEnemy.defense;
                CurrentDefense = config.secondEnemy.defense;
                break;
            case EnemyType.Third:
                MaxHealth = config.thirdEnemy.health;
                Health = config.thirdEnemy.health;
                BaseAttack = config.thirdEnemy.attack;
                Defense = config.thirdEnemy.defense;
                CurrentDefense = config.thirdEnemy.defense;
                break;
            case EnemyType.Default:
            default:
                MaxHealth = config.defaultEnemy.health;
                Health = config.defaultEnemy.health;
                BaseAttack = config.defaultEnemy.attack;
                Defense = config.defaultEnemy.defense;
                CurrentDefense = config.defaultEnemy.defense;
                break;
        }
        CurrentAttack = BaseAttack;
    }

    // 新增：重置敌人状态方法（修复BUG）
    public void ResetEnemyState(BattleConfig config)
    {
        // 重置所有状态
        IsDefending = false;
        DefenseDuration = 0;
        DamageDealtCount = 0;
        DotDuration = 0;
        AttackMultiplier = 1f;
        DefenseUsedCount = 0;
        DamageCounter = 0;
        // 重新设置属性
        SetEnemyProperties(config);
    }

    public void TakeDamage(float damage)
    {
        float actualDamage = damage;
        // 如果正在防御，根据敌人类型应用不同的防御效果
        if (IsDefending)
        {
            switch (currentEnemyType)
            {
                case EnemyType.Second:
                    // 第二个敌人：减少50%伤害（可配置）
                    if (BattleSystem.Instance != null && BattleSystem.Instance.config != null)
                    {
                        float reduction = BattleSystem.Instance.config.secondEnemyDefenseReduction;
                        actualDamage = damage * (1f - reduction);
                        Debug.Log($"Enemy is defending and reducing damage by {reduction * 100}%! Original: {damage}, Reduced: {actualDamage}");
                    }
                    else
                    {
                        actualDamage = damage * 0.5f; // 默认50%减免
                    }
                    break;
                case EnemyType.Default:
                case EnemyType.Third:
                default:
                    // 其他敌人：完全免疫伤害
                    Debug.Log("Enemy is defending and ignoring attack!");
                    return;
            }
        }
        // 普通伤害计算
        Health = Mathf.Max(0, Health - actualDamage);
        DamageCounter++;
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
        IsDefending = true;
        DefenseDuration = 1; // 初始持续1回合
        DefenseUsedCount++; // 增加防御使用次数
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public void ChargeAttack()
    {
        // 根据敌人类型执行不同的蓄力逻辑
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                // 第二个敌人：延长防御时间
                if (DefenseDuration > 0)
                {
                    DefenseDuration++; // 延长防御持续时间
                }
                break;
            case EnemyType.Third:
                // 第三个敌人：给玩家上DOT
                DotDuration = 2; // DOT持续2回合
                break;
            case EnemyType.Default:
            default:
                // 默认敌人：增加攻击力
                CurrentAttack += 3; // 永久增加攻击力
                break;
        }
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public float CalculateDamage(float playerDefense)
    {
        // 应用攻击倍率
        float actualDamage = CurrentAttack * AttackMultiplier;
        return Mathf.Max(0, actualDamage - playerDefense);
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

    // 新增：调整敌人防御力方法（修复BUG）
    public void AdjustDefense(float amount)
    {
        CurrentDefense = Mathf.Max(0, CurrentDefense + amount);
        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    // ========== 修改：ChooseAction 方法 ==========
    // 严格按照不同敌人的优先级实现敌人AI
    public EnemyAction ChooseAction(int currentRound, float playerDamageTaken)
    {
        EnemyAction action;
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                action = ChooseSecondEnemyAction(currentRound, playerDamageTaken);
                break;
            case EnemyType.Third:
                action = ChooseThirdEnemyAction(currentRound, playerDamageTaken);
                break;
            case EnemyType.Default:
            default:
                action = ChooseDefaultEnemyAction(currentRound, playerDamageTaken);
                break;
        }

        // ========== 关键修改：在 ChooseAction 内部直接更新UI意图 ==========
        // 这样可以确保每次选择行动时，意图图标都会立即更新
        // 我们调用 EffectManager 的 ShowEnemyIntent 方法来处理意图显示
        if (BattleSystem.Instance != null && BattleSystem.Instance.effectManager != null)
        {
            // 调用 EffectManager 的方法来更新意图
            BattleSystem.Instance.effectManager.ShowEnemyIntent(action);
            Debug.Log($"[Enemy Intent Updated Internally] Round: {currentRound}, Action: {action.type}");
        }
        else
        {
            Debug.LogWarning("EnemyManager.ChooseAction: Cannot update intent display. BattleSystem.Instance or effectManager is null.");
        }
        // ========== 修改结束 ==========

        return action;
    }
    // ========== 修改结束 ==========

    // ========== 新增：执行敌人动作后更新状态 ==========
    /// <summary>
    /// 执行敌人动作后调用此方法更新相关状态计数器
    /// </summary>
    public void ExecuteAction(EnemyAction.Type actionType)
    {
        switch (actionType)
        {
            case EnemyAction.Type.Attack:
                // 攻击时增加造成伤害计数器
                DamageDealtCount++;
                break;
            case EnemyAction.Type.Defend:
                // 防御时重置受到伤害计数器
                DamageCounter = 0;
                DefenseUsedCount++; // 也增加防御使用次数
                break;
            case EnemyAction.Type.Charge:
                // 蓄力动作本身不直接增加特定计数器，
                // 但其效果（如增加攻击力、延长防御、施加DOT）已在ChargeAttack中处理。
                // 如果需要，可以在这里添加特定于蓄力的计数器。
                break;
        }
        Debug.Log($"Enemy executed action: {actionType}. DamageDealtCount: {DamageDealtCount}, DamageCounter: {DamageCounter}, DefenseUsedCount: {DefenseUsedCount}");
    }
    // ========== 新增结束 ==========

    // 默认敌人AI
    private EnemyAction ChooseDefaultEnemyAction(int currentRound, float playerDamageTaken)
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
        // 2. 防御技能：受到两次伤害时使用（优先级第二）
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // 重置计数器
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // 3. 伤害技能：其余状态均不满足时使用（优先级第三）
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }

    // 第二个敌人AI
    private EnemyAction ChooseSecondEnemyAction(int currentRound, float playerDamageTaken)
    {
        // 优先级第一：蓄力技能（当防御技能每使用2次之后使用该技能）
        if (DefenseUsedCount >= 2 && currentRound > 1)
        {
            DefenseUsedCount = 0; // 重置计数器
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // 优先级第二：防御技能（受到两次伤害时使用）
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // 重置计数器
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // 优先级第三：伤害技能
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }

    // 第三个敌人AI
    private EnemyAction ChooseThirdEnemyAction(int currentRound, float playerDamageTaken)
    {
        // 优先级第一：蓄力技能（每对玩家造成2次伤害后使用）
        if (DamageDealtCount >= 2)
        {
            DamageDealtCount = 0; // 重置计数器
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // 优先级第二：增伤技能（受到两次伤害时使用）
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // 重置计数器
            // 增加永久攻击力
            BaseAttack += 2;
            CurrentAttack = BaseAttack;
            // 增加攻击倍率
            AttackMultiplier = 1.5f;
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // 优先级第三：伤害技能
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }

    // 每回合开始时调用
    public void OnTurnStart()
    {
        // 处理防御持续时间
        if (DefenseDuration > 0)
        {
            DefenseDuration--;
            if (DefenseDuration <= 0)
            {
                IsDefending = false; // 防御结束
            }
        }
        // 处理DOT效果
        if (DotDuration > 0)
        {
            DotDuration--;
            // 对玩家造成DOT伤害
            if (BattleSystem.Instance != null && BattleSystem.Instance.playerManager != null)
            {
                BattleSystem.Instance.playerManager.TakeDamage(2f); // 2点DOT伤害
                // 更新UI
                if (BattleSystem.Instance.uiManager != null)
                {
                    BattleSystem.Instance.uiManager.UpdatePlayerStatus(
                        BattleSystem.Instance.playerManager.Health,
                        BattleSystem.Instance.playerManager.MaxHealth,
                        BattleSystem.Instance.playerManager.Attack,
                        BattleSystem.Instance.playerManager.Defense
                    );
                }
            }
        }
        // 处理攻击倍率衰减
        if (AttackMultiplier > 1f)
        {
            AttackMultiplier = Mathf.Max(1f, AttackMultiplier - 0.25f); // 每回合减少0.25倍率
        }
    }

    // 每回合结束时调用
    public void OnTurnEnd()
    {
        // 如果造成了伤害，增加伤害计数器
        if (BattleSystem.Instance != null && BattleSystem.Instance.playerManager != null)
        {
            // 这里可以添加伤害计数逻辑
        }
    }

    // 获取敌人ID（用于PersistentBattleData）
    public int GetEnemyId()
    {
        switch (currentEnemyType)
        {
            case EnemyType.Default: return 0;
            case EnemyType.Second: return 1;
            case EnemyType.Third: return 2;
            default: return 0;
        }
    }
}