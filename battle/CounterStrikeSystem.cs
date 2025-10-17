using UnityEngine;

public class CounterStrikeSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private EffectManager effectManager;
    // 移除了 private IconManager iconManager;

    // 修改了方法签名，移除了 IconManager 参数
    public void Initialize(PlayerManager playerManager, EnemyManager enemyManager,
                           EffectManager effectManager)
    {
        this.playerManager = playerManager;
        this.enemyManager = enemyManager;
        this.effectManager = effectManager;
        // 移除了 this.iconManager = iconManager;
    }

    public void HandleCounterStrike()
    {
        // 检查反震效果是否仍然有效（基于持续时间）
        if (!playerManager.CounterStrikeActive || playerManager.CounterStrikeDuration <= 0)
        {
            // 移除了图标移除逻辑
            return;
        }
        BattleSystem.Instance.uiManager.UpdateBattleLog("Attempting counter strike...");
        // 反震成功条件: 敌人攻击力 < 玩家防御力
        if (enemyManager.CurrentAttack < playerManager.Defense)
        {
            // 计算反震伤害 (攻击力 + 防御力)
            float counterDamage = playerManager.Attack + playerManager.Defense;
            // 应用倍率
            counterDamage *= playerManager.CounterStrikeMultiplier;
            enemyManager.TakeDamage(counterDamage);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Counter strike successful! Dealt {counterDamage:F1} damage");
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Counter strike failed! Player takes DOT damage");
            // 规则: 失败时受到2点DOT伤害持续2回合
            effectManager.AddPlayerDotEffect(2, 2);
        }
        // 注意：不再在这里移除反震效果，让PlayerManager.ResetForNewTurn()来处理持续时间
        // 反震效果会在持续时间结束后自动移除
    }
}