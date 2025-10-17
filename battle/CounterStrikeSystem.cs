using UnityEngine;

public class CounterStrikeSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private EffectManager effectManager;

    public void Initialize(PlayerManager playerManager, EnemyManager enemyManager,
                           EffectManager effectManager)
    {
        this.playerManager = playerManager;
        this.enemyManager = enemyManager;
        this.effectManager = effectManager;
    }

    /// <summary>
    /// 执行反震效果 - 严格按新规则
    /// </summary>
    /// <param name="isAttackBlocked">基础防御是否已经格挡了攻击</param>
    public void ExecuteCounterStrike(bool isAttackBlocked)
    {
        // --- 前置检查 ---
        // 1. 检查玩家是否激活了反震效果 (这个标记由YinYangSystem在特定状态下设置)
        //    BattleSystem 已经确认了 CounterStrikeActive 为 true 才会调用此方法
        //    但为了保险，内部再检查一次
        if (!playerManager.CounterStrikeActive)
        {
            // Debug.Log("CounterStrikeSystem: Counter strike not active, skipping execution.");
            return;
        }

        // --- 反震核心判定 ---
        // 2. 反震触发条件: 玩家防御力 > 敌人攻击力
        //    注意：即使基础攻击被格挡了 (isAttackBlocked=true)，只要防御力数值上大于攻击力，反震就成功。
        if (playerManager.Defense > enemyManager.CurrentAttack)
        {
            // --- 反震成功 ---
            // 3. 计算反震伤害
            float counterDamage = playerManager.Defense;

            // 4. 检查是否是极端阴状态，如果是则反震伤害乘以1.5倍
            //    playerManager.IsInExtremeYinState() 判断的是当前点数差是否符合极端阴范围
            //    playerManager.CounterStrikeActive 表示本回合激活了反震（可能是阴盛、极端阴或究极气）
            //    我们需要同时满足“处于极端阴点数范围”和“激活了反震”才能应用1.5倍伤害
            if (playerManager.IsInExtremeYinState())
            {
                counterDamage *= 1.5f;
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enhanced Counter strike! Dealt {counterDamage:F1} damage to enemy");
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Counter strike! Dealt {counterDamage:F1} damage to enemy");
            }

            // 5. 对敌人造成反震伤害
            enemyManager.TakeDamage(counterDamage);

            // 6. 播放反击特效
            if (BattleSystem.Instance.animationManager != null)
            {
                BattleSystem.Instance.animationManager.PlayCounterStrikeEffect();
            }

        }
        else
        {
            // --- 反震失败 ---
            BattleSystem.Instance.uiManager.UpdateBattleLog("Enemy's attack power is too high for counter strike!");

            // 7. 检查失败惩罚：只有在阴盛或极端阴状态下，反震失败才会受到DOT伤害
            //    playerManager.IsInYinProsperityState() 或 playerManager.IsInExtremeYinState()
            //    判断的是当前点数差是否符合这些范围，而 CounterStrikeActive 表示反震已激活
            //    这两个条件在 YinYangSystem 应用效果时是一致的，所以这里检查点数范围即可
            if (playerManager.IsInYinProsperityState() || playerManager.IsInExtremeYinState())
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog("Player suffers backlash! Takes DOT damage.");
                // 施加2点DOT伤害，持续2回合
                effectManager.AddPlayerDotEffect(2, 2);
            }
            else
            {
                // 如果是究极气状态激活的反震失败，根据规则不触发这个特定DOT
                // 普通伤害已在 BattleSystem 中处理
            }
        }

        // 8. 注意：反震的持续时间 (CounterStrikeDuration) 和状态 (CounterStrikeActive)
        //    的管理由 PlayerManager.ResetForNewTurn() 负责。
        //    本次调用结束后，本次攻击的反震判定就结束了。
        //    YinYangSystem 在每回合开始时根据点数差重新激活反震。
    }

    // --- 保持原有方法以兼容旧代码调用 (虽然可能不再直接使用) ---
    // 这些方法现在会调用核心的 ExecuteCounterStrike(bool) 方法

    public void HandleCounterStrike()
    {
        // 为了兼容，假设基础攻击未被格挡（因为如果格挡了，通常不会调用这个）
        // 但最准确的方式还是由 BattleSystem 传入正确的 isAttackBlocked 状态
        // 这里为了兼容性，我们假设未格挡
        ExecuteCounterStrike(false);
    }

    public void HandleCounterStrike(float damageTaken)
    {
        // damageTaken 参数在此逻辑中不再直接使用，因为判定基于防御力和攻击力
        // 同样假设未格挡
        ExecuteCounterStrike(false);
    }

    public void HandleCounterStrike(float damageTaken, bool isInCounterStrikeState)
    {
        // isInCounterStrikeState 参数在此逻辑下冗余，因为 PlayerManager.CounterStrikeActive 是权威源
        // 且 BattleSystem 已经检查了。为了兼容，我们调用核心逻辑。
        // 假设未格挡
        ExecuteCounterStrike(false);
    }
}