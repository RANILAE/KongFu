using UnityEngine;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour
{
    [System.Serializable]
    public class DotEffect
    {
        public float damage;
        public int remainingTurns;
        public bool isPlayer;
    }

    [System.Serializable]
    public class DebuffEffect
    {
        public float amount;
        public int remainingTurns;
        public bool isAttackDebuff;
    }

    // 叠层系统
    private int yangPenetrationStacks = 0;
    private int yinCoverStacks = 0;
    private int balanceHealCooldown = 0;

    // 效果列表
    private List<DotEffect> activeDots = new List<DotEffect>();
    private List<DebuffEffect> activeDebuffs = new List<DebuffEffect>();

    private IconManager iconManager;

    public void Initialize(IconManager iconManager)
    {
        this.iconManager = iconManager;
        yangPenetrationStacks = 0;
        yinCoverStacks = 0;
        balanceHealCooldown = 0;
        activeDots.Clear();
        activeDebuffs.Clear();
    }

    public void ProcessAllEffects()
    {
        ProcessDotEffects();
        ProcessDebuffs();
        ProcessCooldowns(); // 确保处理CD
    }

    #region 叠层方法
    public void AddYangPenetrationStack()
    {
        yangPenetrationStacks++;
        iconManager.AddPlayerIcon(IconManager.IconType.YangStack, yangPenetrationStacks);
    }

    public int GetYangPenetrationStacks()
    {
        return yangPenetrationStacks;
    }

    public void ResetYangPenetrationStacks()
    {
        yangPenetrationStacks = 0;
        iconManager.RemovePlayerIcon(IconManager.IconType.YangStack);
    }

    public void AddYinCoverStack()
    {
        yinCoverStacks++;
        iconManager.AddPlayerIcon(IconManager.IconType.YinStack, yinCoverStacks);
    }

    public int GetYinCoverStacks()
    {
        return yinCoverStacks;
    }

    public void ResetYinCoverStacks()
    {
        yinCoverStacks = 0;
        iconManager.RemovePlayerIcon(IconManager.IconType.YinStack);
    }

    public void SetBalanceHealCooldown()
    {
        balanceHealCooldown = BattleSystem.Instance.config.balanceHealCooldown;
    }

    public bool CanHealFromBalance()
    {
        return balanceHealCooldown <= 0;
    }
    #endregion

    #region DOT处理
    public void AddPlayerDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = true });
        iconManager.AddPlayerIcon(IconManager.IconType.PlayerDot, damage, duration);
    }

    public void AddEnemyDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = false });
        iconManager.AddEnemyIcon(IconManager.IconType.EnemyDot, damage, duration);
    }

    private void ProcessDotEffects()
    {
        for (int i = activeDots.Count - 1; i >= 0; i--)
        {
            DotEffect dot = activeDots[i];
            if (dot.isPlayer)
            {
                BattleSystem.Instance.playerManager.TakeDamage(dot.damage);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Player takes {dot.damage:F1} DOT damage.");
            }
            else
            {
                BattleSystem.Instance.enemyManager.TakeDamage(dot.damage);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy takes {dot.damage:F1} DOT damage.");
            }

            dot.remainingTurns--;
            if (dot.remainingTurns <= 0)
            {
                activeDots.RemoveAt(i);
            }
        }
        // 更新图标显示
        iconManager.UpdateDotIcons(activeDots);
    }
    #endregion

    #region DEBUFF处理
    public void ApplyEnemyAttackDebuff(float amount, int duration)
    {
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = true });
        iconManager.AddEnemyIcon(IconManager.IconType.AttackDebuff, amount, duration);
    }

    public void ApplyPlayerDefenseDebuff(float amount, int duration)
    {
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = false });
        iconManager.AddPlayerIcon(IconManager.IconType.DefenseDebuff, amount, duration);
    }

    private void ProcessDebuffs()
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            DebuffEffect debuff = activeDebuffs[i];
            debuff.remainingTurns--;
            if (debuff.isAttackDebuff)
            {
                // 敌人攻击力降低
                BattleSystem.Instance.enemyManager.AdjustAttack(-debuff.amount);
            }
            else
            {
                // 玩家防御力降低
                BattleSystem.Instance.playerManager.AdjustDefense(-debuff.amount);
            }

            if (debuff.remainingTurns <= 0)
            {
                activeDebuffs.RemoveAt(i);
                // 已移除无效的图标更新调用
            }
        }
    }
    #endregion

    #region 冷却处理
    private void ProcessCooldowns()
    {
        // 处理Balance回血CD
        if (balanceHealCooldown > 0)
        {
            balanceHealCooldown--;
            Debug.Log($"Balance heal cooldown decreased to: {balanceHealCooldown}");
        }

        // 确保调用YinYangSystem的CD处理（关键修复）
        if (BattleSystem.Instance != null && BattleSystem.Instance.yinYangSystem != null)
        {
            BattleSystem.Instance.yinYangSystem.ProcessCooldowns();
        }
    }
    #endregion

    #region 敌人意图显示
    public void ShowEnemyIntent(EnemyManager.EnemyAction action)
    {
        // 清除所有意图图标
        iconManager.RemoveEnemyIcon(IconManager.IconType.AttackIntent);
        iconManager.RemoveEnemyIcon(IconManager.IconType.DefendIntent);
        iconManager.RemoveEnemyIcon(IconManager.IconType.ChargeIntent);

        // 添加新意图图标
        switch (action.type)
        {
            case EnemyManager.EnemyAction.Type.Attack:
                iconManager.AddEnemyIcon(IconManager.IconType.AttackIntent);
                break;
            case EnemyManager.EnemyAction.Type.Defend:
                iconManager.AddEnemyIcon(IconManager.IconType.DefendIntent);
                break;
            case EnemyManager.EnemyAction.Type.Charge:
                iconManager.AddEnemyIcon(IconManager.IconType.ChargeIntent);
                break;
        }
    }
    #endregion

    // 新增：获取当前Balance回血CD值（用于调试）
    public int GetBalanceHealCooldown()
    {
        return balanceHealCooldown;
    }

    // 新增：重置Balance回血CD（用于调试）
    public void ResetBalanceHealCooldown()
    {
        balanceHealCooldown = 0;
    }
}