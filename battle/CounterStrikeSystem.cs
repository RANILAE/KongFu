using UnityEngine;

public class CounterStrikeSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private EffectManager effectManager;
    private IconManager iconManager;

    public void Initialize(PlayerManager playerManager, EnemyManager enemyManager,
                           EffectManager effectManager, IconManager iconManager)
    {
        this.playerManager = playerManager;
        this.enemyManager = enemyManager;
        this.effectManager = effectManager;
        this.iconManager = iconManager;
    }

    public void HandleCounterStrike()
    {
        if (!playerManager.CounterStrikeActive) return;

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

        // 无论成功失败，反震效果只在本回合有效
        playerManager.CounterStrikeActive = false;
        iconManager.RemovePlayerIcon(IconManager.IconType.CounterStrike);
    }
}