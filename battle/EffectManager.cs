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
        public bool isAttackDebuff; // true表示攻击Debuff, false表示防御Debuff
        public bool isPlayer; // true表示作用于玩家, false表示作用于敌人
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
        // 修正：使用 IconManager 中定义的正确类型 YangStack (原为 YangPenetrationStack)
        iconManager.AddPlayerIcon(IconManager.IconType.YangStack, yangPenetrationStacks);
    }

    public int GetYangPenetrationStacks()
    {
        return yangPenetrationStacks;
    }

    public void ResetYangPenetrationStacks()
    {
        yangPenetrationStacks = 0;
        // 修正：使用 IconManager 中定义的正确类型 YangStack (原为 YangPenetrationStack)
        iconManager.RemovePlayerIcon(IconManager.IconType.YangStack);
    }

    public void AddYinCoverStack()
    {
        yinCoverStacks++;
        // 修正：使用 IconManager 中定义的正确类型 YinStack (原为 YinCoverStack)
        iconManager.AddPlayerIcon(IconManager.IconType.YinStack, yinCoverStacks);
    }

    public int GetYinCoverStacks()
    {
        return yinCoverStacks;
    }

    public void ResetYinCoverStacks()
    {
        yinCoverStacks = 0;
        // 修正：使用 IconManager 中定义的正确类型 YinStack (原为 YinCoverStack)
        iconManager.RemovePlayerIcon(IconManager.IconType.YinStack);
    }

    public void SetBalanceHealCooldown()
    {
        balanceHealCooldown = BattleSystem.Instance.config.balanceHealCooldown;
        // 可以在这里添加 BalanceHealCD 图标，如果需要显示初始CD
        // iconManager.AddPlayerIcon(IconManager.IconType.BalanceHealCD, 0, balanceHealCooldown);
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
        // 修正：使用 IconManager 中定义的正确类型 PlayerDot (而不是 PlayerDot_DamageOverTime)
        iconManager.AddPlayerIcon(IconManager.IconType.PlayerDot, damage, duration);
    }

    public void AddEnemyDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = false });
        // 修正：使用 IconManager 中定义的正确类型 EnemyDot (而不是 EnemyDot_DamageOverTime)
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
        // 注意：这里 isAttackDebuff=true, isPlayer=false，表示这是作用于敌人的攻击Debuff
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = true, isPlayer = false });
        // 修正：使用 IconManager 中定义的正确类型 EnemyDebuff_DefenseDown (原为 EnemyDebuff_AttackDown)
        // 注意：这里的命名可能与逻辑不符，但为了匹配 IconManager.cs 中的定义，我们使用 EnemyDebuff_DefenseDown
        // *** 强烈建议检查 IconManager.cs 的 IconType 定义 ***
        iconManager.AddEnemyIcon(IconManager.IconType.EnemyDebuff_DefenseDown, amount, duration);
    }

    public void ApplyPlayerDefenseDebuff(float amount, int duration)
    {
        // 注意：这里 isAttackDebuff=false, isPlayer=true，表示这是作用于玩家的防御Debuff (来自极端阴)
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = false, isPlayer = true });
        // 修正：使用 IconManager 中定义的正确类型 ExtremeYangDebuff_AttackDown (原为 ExtremeYinDebuff_DefenseDown)
        // 注意：这里的命名可能与逻辑不符，但为了匹配 IconManager.cs 中的定义，我们使用 ExtremeYangDebuff_AttackDown
        // *** 强烈建议检查 IconManager.cs 的 IconType 定义 ***
        iconManager.AddPlayerIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown, amount, duration);
    }

    private void ProcessDebuffs()
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            DebuffEffect debuff = activeDebuffs[i];
            debuff.remainingTurns--;

            // 应用Debuff效果
            if (debuff.isAttackDebuff)
            {
                if (debuff.isPlayer)
                {
                    // 玩家攻击力降低
                    BattleSystem.Instance.playerManager.AdjustAttack(-debuff.amount);
                }
                else
                {
                    // 敌人攻击力降低 (来自极端阴效果)
                    BattleSystem.Instance.enemyManager.AdjustAttack(-debuff.amount);
                }
            }
            else
            {
                if (debuff.isPlayer)
                {
                    // 玩家防御力降低 (来自极端阳/阴效果)
                    BattleSystem.Instance.playerManager.AdjustDefense(-debuff.amount);
                }
                else
                {
                    // 敌人防御力降低 (来自其他效果，如果有的话)
                    BattleSystem.Instance.enemyManager.AdjustDefense(-debuff.amount);
                }
            }

            if (debuff.remainingTurns <= 0)
            {
                activeDebuffs.RemoveAt(i);
                // 移除对应的图标
                if (debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // 移除玩家的极端阳Debuff图标 (根据修正后的类型)
                    iconManager.RemovePlayerIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown);
                }
                else if (!debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // 移除敌人的攻击Debuff图标 (根据修正后的类型)
                    iconManager.RemoveEnemyIcon(IconManager.IconType.EnemyDebuff_DefenseDown);
                }
                // 如果还有其他组合（例如玩家攻击Debuff或敌人防御Debuff），也需要相应处理
                else if (debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // 例如，如果玩家也有攻击Debuff (目前没有这种情况，但保持逻辑完整)
                    iconManager.RemovePlayerIcon(IconManager.IconType.EnemyDebuff_DefenseDown); // 假设使用相同的类型或添加新类型
                }
                else if (!debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // 例如，如果敌人有防御Debuff (来自其他来源)
                    // 注意：根据需求，极端阴给敌人的是攻击Debuff，所以这里可能不会触发
                    // 但为了完整性，我们假设有 EnemyDefenseDebuff 类型
                    iconManager.RemoveEnemyIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown); // 假设使用相同的类型或添加新类型
                }
            }
            else
            {
                // 更新图标显示（动态更新数据）
                if (debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // 更新玩家的极端阳Debuff图标 (根据修正后的类型)
                    iconManager.AddPlayerIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown, debuff.amount, debuff.remainingTurns);
                }
                else if (!debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // 更新敌人的攻击Debuff图标 (根据修正后的类型)
                    iconManager.AddEnemyIcon(IconManager.IconType.EnemyDebuff_DefenseDown, debuff.amount, debuff.remainingTurns);
                }
                else if (debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // 更新玩家的攻击Debuff图标 (如果存在)
                    iconManager.AddPlayerIcon(IconManager.IconType.EnemyDebuff_DefenseDown, debuff.amount, debuff.remainingTurns); // 假设类型
                }
                else if (!debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // 更新敌人的防御Debuff图标 (如果存在)
                    iconManager.AddEnemyIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown, debuff.amount, debuff.remainingTurns); // 假设类型
                }
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
            // 如果需要动态更新 BalanceHealCD 图标，可以在这里添加
            // iconManager.AddPlayerIcon(IconManager.IconType.BalanceHealCD, 0, balanceHealCooldown);
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
        // *** 修正 IconType 名称 ***
        // 根据您的最新需求，使用统一的 EnemyIntent 图标
        iconManager.RemoveEnemyIcon(IconManager.IconType.EnemyIntent);
        // 移除对 AttackIntent, DefendIntent, ChargeIntent 的调用，因为现在只使用一个统一图标
        // iconManager.RemoveEnemyIcon(IconManager.IconType.AttackIntent);
        // iconManager.RemoveEnemyIcon(IconManager.IconType.DefendIntent);
        // iconManager.RemoveEnemyIcon(IconManager.IconType.ChargeIntent);

        // 添加新意图图标
        // *** 修正 IconType 名称 ***
        // 根据您的最新需求，使用统一的 EnemyIntent 图标
        iconManager.AddEnemyIcon(IconManager.IconType.EnemyIntent, (float)action.type); // 可以传递意图类型作为 value 供 Tooltip 使用
        // 移除对不同意图类型的分支调用，因为现在只使用一个统一图标
        /*
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
        */
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
        Debug.Log("EffectManager - Balance heal cooldown reset to 0");
        // 可能需要更新或移除 Balance CD 图标
        // iconManager.RemovePlayerIcon(IconManager.IconType.BalanceHealCD);
    }
}