using UnityEngine;
using System.Collections.Generic; // 添加此命名空间

public class EnemyManager : MonoBehaviour
{
    // 状态字段
    private bool IsDefending = false; // 防御状态（由 DefenseDuration 推断）
    private int DefenseDuration = 0; // 防御持续回合数 (私有，内部管理)
    private int DamageDealtCount = 0; // 用于触发第三个敌人蓄力
    private int DotDuration = 0; // DOT持续回合数 (现在在OnTurnStart中处理)
    private float AttackMultiplier = 1f; // 攻击倍率 (现在在ExecuteAttack中设置)
    private int DefenseUsedCount = 0; // 防御技能使用次数
    private int DamageCounter = 0; // 用于触发防御 (现在在TakeDamage中递增)

    // ========== 修改：追踪特定敌人效果的状态 ==========
    private bool NextAttackCharged = false; // 第二个敌人：标记下一次攻击是否蓄力
    private bool NextAttackAppliesDot = false; // 第三个敌人：标记下一次攻击是否附带蓄力DOT
    private int ThirdEnemyNextDefendDotBonus = 0; // 第三个敌人：下次防御DOT伤害加成
    // ========== 修改结束 ==========

    // ========== 新增：Inspector可修改的配置字段 ==========
    [Header("通用效果配置")]
    public int DefaultDefenseDuration = 1; // 默认防御持续回合数
    public int SecondEnemyDefenseDuration = 2; // 第二个敌人防御持续回合数
    public int ThirdEnemyDefenseDuration = 1; // 第三个敌人防御持续回合数 (新增)
    public int DotTurns = 2; // DOT持续回合数
    public float DotDamage = 1f; // DOT每回合伤害
    public float ChargedAttackMultiplier = 2.0f; // 蓄力攻击倍率
    public int ThirdEnemyChargeDotDamage = 3; // 第三个敌人蓄力攻击附带DOT伤害
    public int ThirdEnemyDefenseDotBaseDamage = 1; // 第三个敌人防御施加DOT的基础伤害
    // ========== 新增结束 ==========

    // 属性字段 (使用float)
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public float BaseAttack { get; private set; }
    public float CurrentAttack { get; set; }
    public float Defense { get; private set; }
    public float CurrentDefense { get; set; }

    public class EnemyAction
    {
        public enum Type { Attack, Defend, Charge }
        public Type type;
    }

    public enum EnemyType { Default, Second, Third }

    private EnemyType currentEnemyType = EnemyType.Default;
    private BattleConfig config;

    public void Initialize(BattleConfig config)
    {
        this.config = config;
        SetEnemyProperties(config);
        ResetEnemyState(config);
    }

    private void SetEnemyProperties(BattleConfig config)
    {
        // 根据当前敌人类型设置属性
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                MaxHealth = config.secondEnemyMaxHealth;
                BaseAttack = config.secondEnemyBaseAttack;
                Defense = config.secondEnemyBaseDefense;
                break;
            case EnemyType.Third:
                MaxHealth = config.thirdEnemyMaxHealth;
                BaseAttack = config.thirdEnemyBaseAttack;
                Defense = config.thirdEnemyBaseDefense;
                break;
            case EnemyType.Default:
            default:
                MaxHealth = config.enemyMaxHealth;
                BaseAttack = config.enemyBaseAttack;
                Defense = config.enemyBaseDefense;
                break;
        }
        Health = MaxHealth;
        CurrentAttack = BaseAttack;
        CurrentDefense = Defense;
    }

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

        // ========== 重置新增状态 ==========
        NextAttackCharged = false;
        NextAttackAppliesDot = false;
        ThirdEnemyNextDefendDotBonus = 0; // 重置防御DOT加成
        // ========== 重置结束 ==========

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
                        actualDamage *= (1f - BattleSystem.Instance.config.secondEnemyDefenseReduction);
                    }
                    break;
                case EnemyType.Third:
                    // 第三个敌人：完全格挡
                    actualDamage = 0f;
                    break;
                case EnemyType.Default:
                default:
                    // 默认敌人：减少25%伤害
                    actualDamage *= 0.75f;
                    break;
            }
        }

        Health = Mathf.Max(0, Health - actualDamage);
        if (actualDamage > 0)
        {
            DamageCounter++; // 增加受到伤害计数器
        }

        // 播放敌人受击动画的逻辑移到BattleSystem中

        // 更新UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public void EnableDefense()
    {
        // 设置防御状态
        IsDefending = true;
        // 根据敌人类型设置防御持续回合数
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                DefenseDuration = SecondEnemyDefenseDuration; // 使用Inspector中设置的值
                break;
            case EnemyType.Third: // **修复：为第三种敌人添加设置**
                DefenseDuration = ThirdEnemyDefenseDuration; // 使用Inspector中设置的值
                break;
            case EnemyType.Default:
            default:
                DefenseDuration = DefaultDefenseDuration; // 使用Inspector中设置的值
                break;
        }
        DefenseUsedCount++; // 增加防御使用次数

        // ========== 修改：第三个敌人防御效果 ==========
        if (currentEnemyType == EnemyType.Third)
        {
            // 立即对玩家施加DOT，基础伤害为1，加上永久加成
            int dotDamage = ThirdEnemyDefenseDotBaseDamage + ThirdEnemyNextDefendDotBonus; // 使用Inspector中设置的值
            if (BattleSystem.Instance != null && BattleSystem.Instance.effectManager != null)
            {
                BattleSystem.Instance.effectManager.AddPlayerDotEffect(dotDamage, DotTurns); // 使用Inspector中设置的值
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy applies {dotDamage} DOT damage to player for {DotTurns} turns!");
            }
            // 重置永久加成（因为它已经应用了）
            ThirdEnemyNextDefendDotBonus = 0;
        }
        // ========== 修改结束 ==========

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
                // ========== 修改：第二个敌人蓄力效果 ==========
                // 标记下一次攻击为蓄力攻击（伤害*2）
                NextAttackCharged = true;
                // 永久增加攻击力1点
                CurrentAttack += 1;
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy charged! Next attack will deal {ChargedAttackMultiplier}x damage and gains 1 permanent attack.");
                // ========== 修改结束 ==========
                break;
            case EnemyType.Third:
                // ========== 修改：第三个敌人蓄力效果 ==========
                // 标记下一次攻击附带DOT（3点伤害）
                NextAttackAppliesDot = true;
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy charged! Next attack will apply {ThirdEnemyChargeDotDamage} DOT.");
                // ========== 修改结束 ==========
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

    // ========== 修改：执行攻击动作（用于处理蓄力效果） ==========
    public void ExecuteAttack()
    {
        // 检查并应用蓄力效果
        if (currentEnemyType == EnemyType.Second && NextAttackCharged)
        {
            AttackMultiplier = ChargedAttackMultiplier; // 使用Inspector中设置的倍率
            NextAttackCharged = false; // 消耗蓄力效果
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy uses charged attack ({ChargedAttackMultiplier}x damage)!");
        }
        else
        {
            AttackMultiplier = 1f; // 重置倍率
        }

        // 检查并应用第三个敌人蓄力攻击附带的DOT
        if (currentEnemyType == EnemyType.Third && NextAttackAppliesDot)
        {
            // 攻击结算后，给玩家施加3点DOT，持续2回合
            if (BattleSystem.Instance != null && BattleSystem.Instance.effectManager != null)
            {
                BattleSystem.Instance.effectManager.AddPlayerDotEffect(ThirdEnemyChargeDotDamage, DotTurns); // 使用Inspector中设置的值
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy's attack applies {ThirdEnemyChargeDotDamage} DOT damage to player for {DotTurns} turns!");
            }
            NextAttackAppliesDot = false; // 消耗标记
        }
    }
    // ========== 修改结束 ==========

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

        // ========== 关键修改：在内部更新意图显示 ==========
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

    // ========== 修改：执行敌人动作后更新状态 ==========
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
                ExecuteAttack(); // 处理蓄力等效果
                break;
            case EnemyAction.Type.Defend:
                // 防御时重置受到伤害计数器
                DamageCounter = 0;
                DefenseUsedCount++; // 也增加防御使用次数
                // ========== 新增：第三个敌人防御技能永久DOT加成 ==========
                if (currentEnemyType == EnemyType.Third)
                {
                    ThirdEnemyNextDefendDotBonus += 1;
                    BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy's next defense DOT bonus increased by 1 (Total bonus: {ThirdEnemyNextDefendDotBonus}).");
                }
                // ========== 新增结束 ==========
                break;
            case EnemyAction.Type.Charge:
                // 蓄力动作本身不直接增加特定计数器，
                // 但其效果（如增加攻击力、延长防御、施加DOT）已在ChargeAttack中处理。
                // 如果需要，可以在这里添加特定于蓄力的计数器。
                break;
        }
        Debug.Log($"Enemy executed action: {actionType}. DamageDealtCount: {DamageDealtCount}, DamageCounter: {DamageCounter}, DefenseUsedCount: {DefenseUsedCount}");
    }
    // ========== 修改结束 ==========

    // 默认敌人AI
    private EnemyAction ChooseDefaultEnemyAction(int currentRound, float playerDamageTaken)
    {
        // 1. 蓄力技能：最高优先级（每4回合使用一次）
        if (currentRound % 4 == 1 && currentRound > 1) // 确保不是第一回合
        {
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // 2. 防御技能：每两回合使用一次（优先级第二）
        else if (currentRound % 2 == 0)
        {
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // 3. 伤害技能：其余状态均不满足时使用（优先级第三）
        else
        {
            return new EnemyAction { type = EnemyAction.Type.Attack };
        }
    }

    // ========== 修改：第二个敌人AI ==========
    private EnemyAction ChooseSecondEnemyAction(int currentRound, float playerDamageTaken)
    {
        // 优先级第一：蓄力技能（每3回合使用一次）最高优先级
        if (currentRound % 3 == 0 && currentRound > 0) // 例如第3, 6, 9...回合
        {
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // 优先级第二：防御技能（每4回合使用一次）（优先级第二）
        else if (currentRound % 4 == 0 && currentRound > 0) // 例如第4, 8, 12...回合
        {
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // 优先级第三：伤害技能（其余状态均不满足时使用）（优先级第三）
        else
        {
            return new EnemyAction { type = EnemyAction.Type.Attack };
        }
    }
    // ========== 修改结束 ==========

    // 第三个敌人AI
    private EnemyAction ChooseThirdEnemyAction(int currentRound, float playerDamageTaken)
    {
        // 优先级第一：蓄力技能（每3回合使用一次）最高优先级
        if (currentRound % 3 == 1 && currentRound > 1) // 例如第1, 4, 7...回合，但跳过第1回合
        {
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // 优先级第二：防御技能（每2回合使用一次）（优先级第二）
        else if (currentRound % 2 == 0)
        {
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // 优先级第三：伤害技能（其余状态均不满足时使用）（优先级第三）
        else
        {
            return new EnemyAction { type = EnemyAction.Type.Attack };
        }
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
                float damageThisTurn = DotDamage; // 使用Inspector中设置的值
                BattleSystem.Instance.playerManager.TakeDamage(damageThisTurn);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy applies {damageThisTurn} DOT to player! {DotDuration} turns remaining.");
                // 这里可以添加伤害计数逻辑
            }
        }
    }

    public void SetEnemyType(EnemyType type)
    {
        currentEnemyType = type;
        if (config != null)
        {
            SetEnemyProperties(config);
        }
    }

    public EnemyType GetCurrentEnemyType()
    {
        return currentEnemyType;
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